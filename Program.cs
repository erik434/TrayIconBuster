using System;

namespace TrayIconBuster
{
    static class Program
    {
        static void Main()
        {
            try
            {
                TrayIconBuster.RemovePhantomIcons();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tray icon buster error: {ex}");
            }
        }
    }
}