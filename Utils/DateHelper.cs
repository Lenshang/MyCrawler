using System;
using System.Collections.Generic;
using System.Text;

namespace MyCrawler.Utils
{
    public class DateHelper
    {
        /// <summary>
        /// 获得13位的时间戳
        /// </summary>
        /// <param name="stime"></param>
        /// <returns></returns>
        public static string GetTimestamp(DateTime stime)
        {
            //TimeSpan sp = stime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return GetTimestampLong(stime).ToString();
        }
        /// <summary>
        /// 获得13位的时间戳Long类型
        /// </summary>
        /// <param name="stime"></param>
        /// <returns></returns>
        public static long GetTimestampLong(DateTime stime)
        {
            TimeSpan sp = stime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64(sp.TotalMilliseconds);
        }
        /// <summary>
        /// 获得10位时间戳
        /// </summary>
        /// <param name="stime"></param>
        /// <returns></returns>
        public static string GetTimestamp10(DateTime stime)
        {
            TimeSpan sp = stime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64(sp.TotalSeconds).ToString();
        }
        /// <summary>
        /// 时间戳转Datetime(utc)
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <param name="type">类型 1=毫秒 2=秒</param>
        /// <returns></returns>
        public static DateTime ConvertTimestamp(long timestamp,int type=1)
        {
            DateTime dt = new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc);
            if (type == 2)
            {
                dt=dt.AddSeconds(timestamp);
            }
            else
            {
                dt=dt.AddMilliseconds(timestamp);
            }
            
            return dt;
        }
        /// <summary>
        /// 反序列化日期
        /// </summary>
        /// <param name="date"></param>
        /// <param name="format"></param>
        /// <param name="defaultDate"></param>
        /// <returns></returns>
        public static DateTime DeserializeDate(string date,string format,DateTime? defaultDate=null)
        {
            try
            {
                DateTime dt = DateTime.ParseExact(date, format, System.Globalization.CultureInfo.InvariantCulture);
                return dt;
            }
            catch
            {
                if (defaultDate != null) return defaultDate.Value;
                return new DateTime(1970,1,1);
            }
        }
    }
}
