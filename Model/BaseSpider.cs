using MyCrawler.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MyCrawler.Model
{
    public abstract class BaseSpider
    {
        public HttpEventClient http;

        public BaseSpider(int maxThread=1)
        {
            http = new HttpEventClient(maxThread);
            this.http.RegRequestPipelines(this.BeforeRequest);
            this.http.RegResponsePipelines(this.BeforeResponse);
            this.http.RegExceptionPipelines(this.BeforeException);
        }
        public abstract Task Run();
        /// <summary>
        /// 在发生Request之前执行，可修改Request内容
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <param name="meta"></param>
        /// <returns>返回request则继续执行该request，返回null则放弃该任务</returns>
        public virtual object BeforeRequest(BaseHttpClient client, RequestEntity request, object meta)
        {
            return request;
        }
        /// <summary>
        /// 在执行Response之前执行，可修改Response的内容
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="meta"></param>
        /// <returns>返回request则重新请求，返回response则继续，返回null则放弃</returns>
        public virtual object BeforeResponse(BaseHttpClient client, RequestEntity request, ResponseEntity response, object meta)
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
        /// <param name="meta"></param>
        /// <returns>返回Exception继续处理该异常，返回request重新请求，返回response则正常返回，返回NULL放弃该任务</returns>
        public virtual object BeforeException(BaseHttpClient client, Exception e, RequestEntity request, ResponseEntity response,object meta)
        {
            return e;
        }
    }
}
