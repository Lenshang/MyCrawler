using LvdunCrawler;
using MyCrawler.Model;
using MyCrawler.Spiders;
using MyCrawler.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MyCrawler
{
    class test
    {
        public test t1 { get; set; }
        public string id { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {

            BaseSpider spider = new CompanyCreditSpider();
            spider.Run();

            Console.ReadLine();
        }
    }
}
