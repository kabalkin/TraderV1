using System;
using System.Collections.Generic;
using System.Text;

namespace ProxyChecker
{
    public class Proxy
    {
        public string IP { get; set; }
        public string Port { get; set; }
        public long Ping { get; set; }
        public bool IsWork { get; set; }

        public override string ToString()
        {
            return IP + ":" + Port; 
        }
    }
}
