using System;
using System.Collections.Generic;
using System.Text;

namespace Addons
{
    public class BadProxyException:Exception
    {
        public BadProxyException(string message):base(message)
        {

        }
    }
}
