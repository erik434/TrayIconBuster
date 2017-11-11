using System;

namespace TrayIconBuster
{
    public static class Utilities
    {
        public static void Log(string message)
        {
            Console.WriteLine(DateTime.Now.ToLongTimeString() + " " + message);
        }
    }
}
