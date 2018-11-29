using System;

namespace Addons
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine("["+DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss")+"]\t"+message);
        }
    }
}
