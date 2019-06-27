using MyCrawler.Model;
using MyCrawler.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyCrawler.Pipelines
{
    public class DefaultRequestPipelines
    {
        BaseSpider spider { get; set; }
        BloomFilterHelper bloomFilter { get; set; }
        public DefaultRequestPipelines(BaseSpider spider)
        {
            this.spider = spider;
            this.bloomFilter = new BloomFilterHelper(16, 20480000);
            spider.http.RegRequestPipelines(this.UrlFilter);
            spider.http.RegRequestPipelines(this.DeepControl);
        }
        public object DefaultHeader(BaseHttpClient client, RequestEntity request, MetaModel meta)
        {
            return null;
        }
        public object UrlFilter(BaseHttpClient client, RequestEntity request, MetaModel meta)
        {
            if (meta?.Get<bool>("dontFilter")==true)
            {
                return request;
            }
            if (this.bloomFilter.ContainsValue(request.Url.ToString()))
            {
                return null;
            }
            this.bloomFilter.AddValue(request.Url.ToString());
            return request;
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
