using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCrawler.Utils
{
    public class LogHelper
    {
        private static object locker = new object();
        /// <summary>
        /// 全局字体颜色
        /// </summary>
        public static ConsoleColor Color = ConsoleColor.Gray;
        /// <summary>
        /// 控制台打印输出
        /// </summary>
        /// <param name="msg"></param>
        public static void Message(object msg, string type,ConsoleColor TypeColor, ConsoleColor? _color=null)
        {
            lock (locker)
            {
                if (_color == null)
                {
                    _color = Color;
                }
                Console.ForegroundColor = _color.Value;
                Console.Write($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}-<");
                Console.ForegroundColor = TypeColor;
                Console.Write(type);
                Console.ForegroundColor = _color.Value;
                Console.WriteLine($">: {msg.ToString()}");
                Console.ForegroundColor = Color;
            }
        }
        public static void Message(object msg, ConsoleColor? _color = null)
        {
            lock (locker)
            {
                if (_color == null)
                {
                    Console.ForegroundColor = Color;
                }
                else
                {
                    Console.ForegroundColor = _color.Value;
                }
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}-<Message>: {msg.ToString()}");
                Console.ForegroundColor = Color;
            }
        }
        /// <summary>
        /// 写LOG日志
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="type">类型</param>
        public static void WriteLog(object msg, LogType type)
        {
            //lock (locker)
            //{
            //    Console.ForegroundColor = Color;
            //    DbLiteHelper dbl = new DbLiteHelper($"log/{DateTime.Now.ToString("yyyyMMdd")}.db");
            //    Logger lg = new Logger()
            //    {
            //        Msg = msg.ToString(),
            //        Type = type,
            //        Date = DateTime.Now
            //    };
            //    dbl.Set<Logger>(lg);
            //    ShowMsg(lg);
            //}
            Message(msg);
        }
        /// <summary>
        /// LOG发生时显示消息
        /// </summary>
        /// <param name="lg"></param>
        private static void ShowMsg(Logger lg)
        {
            if (lg.Type == LogType.ERROR)
            {

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{lg.Date.ToString("yyyy-MM-dd HH:mm:ss")}-<{lg.Type}>: {lg.Msg}");
                Console.ForegroundColor = Color;
            }
            else
            {
                Console.WriteLine($"{lg.Date.ToString("yyyy-MM-dd HH:mm:ss")}-<{lg.Type}>: {lg.Msg}");
            }
        }
    }
    public class Logger
    {
        public int id { get; set; }
        public string Msg { get; set; }
        public LogType Type { get; set; }
        public DateTime Date { get; set; }
    }
    public enum LogType
    {
        INFO,
        ERROR
    }
}
