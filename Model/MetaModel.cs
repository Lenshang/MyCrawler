using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace MyCrawler.Model
{
    public class MetaModel
    {
        private Dictionary<string,object> _meta { get; set; }
        public MetaModel()
        {
            this._meta = new Dictionary<string, object>();
        }
        private MetaModel(Dictionary<string, object> dicMeta)
        {
            this._meta = dicMeta;
        }
        public object this[string key]
        {
            set
            {
                if (this._meta.ContainsKey(key))
                {
                    this._meta[key]=value;
                }
                else
                {
                    this._meta.Add(key, value);
                }
            }
            get
            {
                if(this._meta.TryGetValue(key,out object val))
                {
                    return val;
                }
                return null;
            }
        }

        public T Get<T>(string key)
        {
            object r = this[key];
            if (r == null)
            {
                return default(T);
            }

            return (T)this[key];
        }
        public void Add(string key,object value)
        {
            this._meta.Add(key, value);
        }

        public MetaModel Copy()
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(memoryStream, this._meta);
            memoryStream.Position = 0;
            var newDict= formatter.Deserialize(memoryStream) as Dictionary<string,object>;
            return new MetaModel(newDict);

        }
    }
}
