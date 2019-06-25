using MyCrawler.Pipelines;
using MyCrawler.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyCrawler.Model
{
    public abstract class BaseSpider
    {
        public HttpEventClient http;
        private Object locker;
        private int status = 0;
        public int maxDeep = -1;
        public DefaultRequestPipelines defaultRequestPipelines { get; private set; }
        public BaseSpider(int maxThread=1)
        {
            this.locker = new object();
            http = new HttpEventClient(maxThread);
            defaultRequestPipelines = new DefaultRequestPipelines(this);

            this.http.RegRequestPipelines(this.BeforeRequest);
            this.http.RegResponsePipelines(this.BeforeResponse);
            this.http.RegExceptionPipelines(this.BeforeException);
        }
        public virtual Task RunAndWait(bool autoStop=true)
        {
            this.Run();
            if (autoStop)
            {
                return Task.Delay(5000).ContinueWith(r => {
                    int _count = 0;
                    while (true)
                    {
                        if (this.http.runningThread == null)
                        {
                            _count++;
                        }
                        else if (!this.http.runningThread.IsAlive)
                        {
                            _count++;
                        }
                        else
                        {
                            _count = 0;
                        }
                        if (_count > 3)
                        {
                            return;
                        }
                        Thread.Sleep(500);
                    }
                });
            }
            else
            {
                return Task.Run(() => {
                    while (true)
                    {
                        var _status = -1;
                        lock (locker)
                        {
                            _status = this.status;
                        }
                        if (_status == 2)
                        {
                            break;
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                });
            }
        }
        public virtual void End()
        {
            lock (locker)
            {
                status = 2;
            }
        }
        public abstract void Run();
        /// <summary>
        /// 在发生Request之前执行，可修改Request内容
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns>返回request则继续执行该request，返回null则放弃该任务</returns>
        public virtual object BeforeRequest(BaseHttpClient client, RequestEntity request, Dictionary<string, object> meta)
        {
            return request;
        }
        /// <summary>
        /// 在执行Response之前执行，可修改Response的内容
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns>返回request则重新请求，返回response则继续，返回null则放弃</returns>
        public virtual object BeforeResponse(BaseHttpClient client, RequestEntity request, ResponseEntity response, Dictionary<string, object> meta)
        {
            return response;
        }
        /// <summary>
        /// 在异常发生时执行
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns>返回Exception继续处理该异常，返回request重新请求，返回response则正常返回，返回NULL放弃该任务</returns>
        public virtual object BeforeException(BaseHttpClient client, Exception e, RequestEntity request, ResponseEntity response, Dictionary<string, object> meta)
        {
            return e;
        }
    }
}
