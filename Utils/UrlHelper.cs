using System;
using System.Collections.Generic;
using System.Text;

namespace MyCrawler.Utils
{
    class UrlHelper
    {
        public static string ConvertUrlParams(Dictionary<string,string> _param)
        {
            List<string> r = new List<string>();
            foreach(var key in _param.Keys)
            {
                string v = _param[key];
                r.Add(System.Web.HttpUtility.UrlEncode(key) + "=" + System.Web.HttpUtility.UrlEncode(v));
            }
            return string.Join('&', r);
        }
    }
}
