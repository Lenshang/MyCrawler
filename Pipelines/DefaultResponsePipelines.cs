using MyCrawler.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyCrawler.Pipelines
{
    public class DefaultResponsePipelines
    {
        BaseSpider spider { get; set; }
        public DefaultResponsePipelines(BaseSpider spider)
        {
            this.spider = spider;
            //this.spider.http.RegResponsePipelines(this.PageUrlGetter);
        }
    }
}
