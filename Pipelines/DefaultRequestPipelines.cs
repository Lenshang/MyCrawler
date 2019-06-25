using MyCrawler.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyCrawler.Pipelines
{
    public class DefaultRequestPipelines
    {
        BaseSpider spider { get; set; }
        public DefaultRequestPipelines(BaseSpider spider)
        {
            this.spider = spider;
            spider.http.RegRequestPipelines(this.DeepControl);
        }
        public object DefaultHeader(BaseHttpClient client, RequestEntity request, Dictionary<string, object> meta)
        {
            return null;
        }
        public object DeepControl(BaseHttpClient client, RequestEntity request, Dictionary<string, object> meta)
        {
            if (meta == null)
            {
                meta = new Dictionary<string, object>();
            }
            if (meta.TryGetValue("deep", out object deep))
            {
                if (deep.GetType() == typeof(long)|| deep.GetType() == typeof(int))
                {
                    meta["deep"] = (long)deep + 1;
                }
                else
                {
                    meta["deep"] = (long)0;
                }
            }
            else
            {
                meta.Add("deep", 0);
            }

            if (this.spider.maxDeep >=0)
            {
                if((long)meta["deep"]> this.spider.maxDeep)
                {
                    return null;
                }
            }
            return request;
        }
    }
}
