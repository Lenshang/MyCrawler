using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace MyCrawler.Utils
{
    public class CopyHelper
    {
        public static T DeepCopy<T>(T obj)
        {
            //如果是字符串或值类型则直接返回
            if (obj is string || obj.GetType().IsValueType) return obj;
            JsonSerializerSettings setting = new JsonSerializerSettings();
            setting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            string jsonStr = JsonConvert.SerializeObject(obj,setting);

            return JsonConvert.DeserializeObject<T>(jsonStr);
        }
    }
}
