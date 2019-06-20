using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyCrawler.Utils
{
    class HttpECModel
    {
        public HttpRequestMessage request { get; set; }
        public HttpResponseMessage response { get; set; }
        public Action<HttpECModel> callback { get; set; }
        public Object meta { get; set; }
        public HttpECModel(HttpRequestMessage _request, Action<HttpECModel> callback,Object meta)
        {
            this.request = _request;
            this.callback = callback;
            this.meta = meta;
        }

        
    }
    class HttpEventClient
    {
        //public HttpClient client { get; set; }
        public ConcurrentBag<Func<HttpClient, HttpRequestMessage,HttpResponseMessage, object>> responsePipelines { get; set; }
        public ConcurrentBag<Func<HttpClient,HttpRequestMessage, object>> requestPipelines { get; set; }
        public List<HttpClient> clientsPool { get; set; }
        public ConcurrentStack<HttpClient> freeClientsQueue { get; set; }
        public ConcurrentStack<HttpECModel> tasksQueue { get; set; }
        public object locker { get; set; }
        public Thread runningThread { get; set; } = null;
        public HttpEventClient(int maxThread=1)
        {
            clientsPool = new List<HttpClient>();
            freeClientsQueue = new ConcurrentStack<HttpClient>();
            tasksQueue = new ConcurrentStack<HttpECModel>();
            ServicePointManager.DefaultConnectionLimit = maxThread+1;
            for (int i = 0; i < maxThread; i++)
            {
                clientsPool.Add(new HttpClient());
            }
            foreach(var client in clientsPool)
            {
                freeClientsQueue.Push(client);
            }
            locker = new object();
            //client = new HttpClient();
            responsePipelines = new ConcurrentBag<Func<HttpClient, HttpRequestMessage,HttpResponseMessage, object>>();
            requestPipelines = new ConcurrentBag<Func<HttpClient,HttpRequestMessage, object>>();
        }
        private void Run()
        {
            bool isAlive = true;
            lock (locker)
            {
                if (runningThread == null)
                {
                    isAlive = false;
                }
                else
                {
                    isAlive = runningThread.IsAlive;
                }
                if (!isAlive)
                {
                    runningThread = new Thread(() => {
                        while (tasksQueue.Count > 0)
                        {
                            HttpClient client;
                            if (freeClientsQueue.TryPop(out client))
                            {
                                HttpECModel task;
                                if (tasksQueue.TryPop(out task))
                                {
                                    this.Send(client, task, task.callback).ContinueWith(r => {
                                        //freeClientsQueue.Enqueue(client);
                                    });
                                }
                                else
                                {
                                    freeClientsQueue.Push(client);
                                }
                            }
                            Thread.Sleep(10);
                        }
                    });
                    runningThread.Start();
                }
            }
        }
        /// <summary>
        /// 注册一个Response管道，管道返回HttpResponseMessage 则继续，返回HttpRequestMessage 则重新请求，返回Null 则放弃请求
        /// </summary>
        /// <param name="pipeline"></param>
        public void RegResponsePipelines(Func<HttpClient, HttpRequestMessage,HttpResponseMessage, object> pipeline)
        {
            this.responsePipelines.Add(pipeline);
        }
        /// <summary>
        /// 注册一个Request管道，管道返回HttpRequestMessage 则继续，返回Null 则放弃请求
        /// </summary>
        /// <param name="pipeline"></param>
        public void RegRequestPipelines(Func<HttpClient,HttpRequestMessage, object> pipeline)
        {
            this.requestPipelines.Add(pipeline);
        }
        public void Get(string url,Action<HttpECModel> callback,Object meta=null) 
        {
            tasksQueue.Push(new HttpECModel(this.CreateGetRequest(url), callback, meta));
            this.Run();
            //return await this.Send(request, callback);
        }

        public void Post(string url,string data, Action<HttpECModel> callback, string contentType = "application/json",Object meta=null)
        {

            //var r = await this.client.PostAsync(url, httpContent);
            tasksQueue.Push(new HttpECModel(this.CreatePostRequest(url,data,contentType), callback, meta));
            this.Run();
        }
        public HttpRequestMessage CreateGetRequest(string url)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri(url);
            return request;
        }
        public HttpRequestMessage CreatePostRequest(string url, string data, string contentType = "application/json")
        {
            HttpContent httpContent = new StringContent(data);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.Content = httpContent;
            request.RequestUri = new Uri(url);
            return request;
        }
        private async Task<bool> Send(HttpClient client, HttpECModel model, Action<HttpECModel> callback)
        {
            try
            {
                foreach (var pipeline in requestPipelines)
                {
                    var _request= pipeline.Invoke(client, model.request);

                    model.request = _request is HttpRequestMessage ? _request as HttpRequestMessage : null;
                    if (model.request==null)
                    {
                        break;
                    }
                }
                if (model.request == null)
                {
                    freeClientsQueue.Push(client);
                    return false;
                }
                HttpResponseMessage r=null;
                r = await client.SendAsync(model.request);

                foreach (var pipeline in responsePipelines)
                {
                    var rpip = pipeline.Invoke(client,model.request,r);
                    if(rpip is HttpRequestMessage)
                    {
                        model.request = rpip as HttpRequestMessage;
                        return await this.Send(client, model, callback);
                    }
                    else if(rpip is null)
                    {
                        freeClientsQueue.Push(client);
                        return false;
                    }
                }
                model.response = r;
                callback.Invoke(model);
                freeClientsQueue.Push(client);//放在此处添加队列，解析过程也在线程限制数里
                return r.IsSuccessStatusCode;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}
