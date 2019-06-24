using System;
using System.Collections.Generic;
using System.Text;

namespace MyCrawler.Model
{
    class CatchHttpCodeException:Exception
    {
        public CatchHttpCodeException(int code)
        {
            this.HttpStatusCode = code;
        }
        public int HttpStatusCode { get; set; }
    }
}
