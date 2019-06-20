using LvdunCrawler;
using System;
using System.Threading;

namespace MyCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            LvdunSpider spider = new LvdunSpider();
            spider.Run();

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
