﻿using LvdunCrawler;
using MyCrawler.Model;
using MyCrawler.Spiders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MyCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            BaseSpider spider = new IosStoreSpider();
            spider.RunAndWait().Wait();
        }
    }
}
