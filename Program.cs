using System;

namespace TrayIconBuster
{
    static class Program
    {
        static void Main()
        {
            try
            {
                TrayIconBuster.RemoveZombieIcons();
            }
            catch (Exception ex)
            {
                Utilities.Log($"Tray icon buster error: {ex}");
            }
        }
    }
}