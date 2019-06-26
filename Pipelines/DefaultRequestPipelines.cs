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
        public object DefaultHeader(BaseHttpClient client, RequestEntity request, MetaModel meta)
        {
            return null;
        }
        public object DeepControl(BaseHttpClient client, RequestEntity request, MetaModel meta)
        {
            if (meta == null)
            {
                meta = new MetaModel();
            }
            if (meta["deep"] != null)
            {
                if (meta["deep"].GetType() == typeof(int))
                {
                    meta["deep"] = (int)meta["deep"] + 1;
                }
                else if (meta["deep"].GetType() == typeof(int))
                {
                    meta["deep"] = Convert.ToInt32(meta["deep"]) + 1;
                }
                else
                {
                    meta["deep"] = (int)0;
                }
            }
            else
            {
                meta["deep"]= (int)0;
            }

            if (this.spider.maxDeep >=0)
            {
                int _deep = 0;
                if (meta["deep"].GetType() == typeof(int))
                {
                    _deep = (int)meta["deep"];
                }
                else
                {
                    _deep = Convert.ToInt32(meta["deep"]);
                }
                if (_deep > this.spider.maxDeep)
                {
                    return null;
                }
            }
            return request;
        }
    }
}
