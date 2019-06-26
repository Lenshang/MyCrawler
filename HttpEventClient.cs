using MyCrawler.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyCrawler
{
    public class HttpContentModel
    {
        public RequestEntity request { get; set; }
        public ResponseEntity response { get; set; }
        public Action<HttpContentModel> callback { get; set; }
        public MetaModel meta { get; set; }
        public HttpContentModel(RequestEntity _request, Action<HttpContentModel> callback, MetaModel meta)
        {
            this.request = _request;
            this.callback = callback;
            this.meta = meta;
        }

        
    }
    public class HttpEventClient<T> where T:BaseHttpClient
    {
        //public HttpClient client { get; set; }
        public ConcurrentBag<Func<BaseHttpClient, Exception, RequestEntity, ResponseEntity, MetaModel, object>> exceptionPipelines { get; set; }
        public ConcurrentBag<Func<BaseHttpClient, RequestEntity,ResponseEntity, MetaModel, object>> responsePipelines { get; set; }
        public ConcurrentBag<Func<BaseHttpClient, RequestEntity, MetaModel, object>> requestPipelines { get; set; }

        public List<BaseHttpClient> clientsPool { get; set; }
        public ConcurrentStack<BaseHttpClient> freeClientsQueue { get; set; }
        public ConcurrentStack<HttpContentModel> tasksQueue { get; set; }
        public List<int> CatchHttpCode { get; set; }
        public object locker { get; set; }
        public Thread runningThread { get; set; } = null;
        public HttpEventClient(int maxThread=1)
        {
            CatchHttpCode = new List<int>() {503,500 };
            clientsPool = new List<BaseHttpClient>();
            freeClientsQueue = new ConcurrentStack<BaseHttpClient>();
            tasksQueue = new ConcurrentStack<HttpContentModel>();
            ServicePointManager.DefaultConnectionLimit = maxThread+1;
            for (int i = 0; i < maxThread; i++)
            {
                clientsPool.Add(new DefaultHttpClient(i));
            }
            foreach(var client in clientsPool)
            {
                freeClientsQueue.Push(client);
            }
            locker = new object();
            //client = new HttpClient();
            exceptionPipelines = new ConcurrentBag<Func<BaseHttpClient, Exception, RequestEntity, ResponseEntity, MetaModel, object>>();
            responsePipelines = new ConcurrentBag<Func<BaseHttpClient, RequestEntity,ResponseEntity, MetaModel, object>>();
            requestPipelines = new ConcurrentBag<Func<BaseHttpClient, RequestEntity, MetaModel, object>>();
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
                            BaseHttpClient client;
                            if (freeClientsQueue.TryPop(out client))
                            {
                                HttpContentModel task;
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
        /// 注册一个Exception管道用来处理异常。管道返回RequestEntity则重新请求，管道返回ResponseEntity则继续，管道返回Null则放弃请求
        /// </summary>
        /// <param name="pipeline"></param>
        public void RegExceptionPipelines(Func<BaseHttpClient, Exception, RequestEntity, ResponseEntity, MetaModel, object> pipeline)
        {
            this.exceptionPipelines.Add(pipeline);
        }
        /// <summary>
        /// 注册一个Response管道，管道返回ResponseEntity 则继续，返回RequestEntity 则重新请求，返回Null 则放弃请求
        /// </summary>
        /// <param name="pipeline"></param>
        public void RegResponsePipelines(Func<BaseHttpClient, RequestEntity,ResponseEntity, MetaModel, object> pipeline)
        {
            this.responsePipelines.Add(pipeline);
        }
        /// <summary>
        /// 注册一个Request管道，管道返回RequestEntity 则继续，返回Null 则放弃请求,返回Exception则继续处理异常
        /// </summary>
        /// <param name="pipeline"></param>
        public void RegRequestPipelines(Func<BaseHttpClient, RequestEntity, MetaModel, object> pipeline)
        {
            this.requestPipelines.Add(pipeline);
        }
        public void Get(string url,Action<HttpContentModel> callback, MetaModel meta =null) 
        {
            var model = new HttpContentModel(this.CreateGetRequest(url), callback, meta);
            try
            {
                tasksQueue.Push(model);
                this.Run();
            }
            catch(Exception e)
            {
                ProcessException(null, e, model).ContinueWith(r=> { });
            }
        }

        public void Post(string url,string data, Action<HttpContentModel> callback, string contentType = "application/json", MetaModel meta =null)
        {

            //var r = await this.client.PostAsync(url, httpContent);
            var model = new HttpContentModel(null, callback, meta);
            try
            {
                model.request = this.CreatePostRequest(url, data, contentType);
                tasksQueue.Push(model);
                this.Run();
            }
            catch (Exception e)
            {
                ProcessException(null, e, model).ContinueWith(r => { });
            }
        }
        public RequestEntity CreateGetRequest(string url)
        {
            var request = new RequestEntity(url);
            request.Method = RequestMethod.GET;
            return request;
        }
        public RequestEntity CreatePostRequest(string url, string data, string contentType = "application/json")
        {
            HttpContent httpContent = new StringContent(data);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            var request = new RequestEntity(url);
            request.Method = RequestMethod.POST;
            request.SetSendData(data);
            return request;
        }
        private async Task<bool> Send(BaseHttpClient client, HttpContentModel model, Action<HttpContentModel> callback)
        {
            try
            {
                var _requestPip = requestPipelines.ToArray();
                foreach (var pipeline in _requestPip)
                {
                    var _request= pipeline.Invoke(client, model.request,model.meta);

                    model.request = _request is RequestEntity ? _request as RequestEntity : null;
                    if (model.request==null)
                    {
                        break;
                    }
                }
                if (model.request == null)
                {
                    //freeClientsQueue.Push(client);
                    return false;
                }
                ResponseEntity r=null;
                if (client == null)
                {
#if DEBUG
                    Console.WriteLine("HttpClient is null");
#endif
                    return false;
                }
                r = await client.CreateHttpRequestSend(model);
                //针对StatusCode 做异常处理
                if(this.CatchHttpCode.Contains(r.StatusCode))
                {
                    model.response = r;
                    throw new CatchHttpCodeException(r.StatusCode);
                }

                Func<BaseHttpClient, RequestEntity, ResponseEntity, MetaModel, object>[] _responsePip= responsePipelines.ToArray();
                foreach (var pipeline in _responsePip)
                {
                    var rpip = pipeline.Invoke(client,model.request,r,model.meta);
                    if(rpip is RequestEntity)
                    {
                        model.request = rpip as RequestEntity;
                        return await this.Send(client, model, callback);
                    }
                    else if(rpip is null)
                    {
                        //freeClientsQueue.Push(client);
                        return false;
                    }
                }
                model.response = r;
                callback.Invoke(model);
                //freeClientsQueue.Push(client);//放在此处添加队列，解析过程也在线程限制数里
                return r.StatusCode==200;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return await ProcessException(client, e, model);
            }
            finally
            {
                freeClientsQueue.Push(client);//放在此处添加队列，解析过程也在线程限制数里
            }
        }
        private async Task<bool> ProcessException(BaseHttpClient client,Exception e, HttpContentModel model)
        {
            var exceptionPip = exceptionPipelines.ToArray();
            foreach (var pipeline in exceptionPip)
            {
                var rpip = pipeline.Invoke(client, e, model.request, model.response,model.meta);
                if (rpip is Exception)
                {
                    e = rpip as Exception;
                    continue;
                }
                else if (rpip is RequestEntity)
                {
                    model.request = rpip as RequestEntity;
                    return await this.Send(client, model, model.callback);
                }
                else if (rpip is ResponseEntity)
                {
                    var resp = rpip as ResponseEntity;
                    model.response = resp;
                    model.callback.Invoke(model);
                    return resp.StatusCode==200;
                }
            }
            return false;
        }
    }

    public class HttpEventClient : HttpEventClient<DefaultHttpClient>
    {
        public HttpEventClient(int maxThread = 1) : base(maxThread)
        {
        }
    }
}
