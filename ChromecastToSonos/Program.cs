using System;

namespace ChromecastToSonos
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var sonosService = new SonosService();
                Console.WriteLine("SonosService started. Virtual Chromecasts are now running for each Sonos device.");

                // Keep the application running
                Console.WriteLine("Press Enter to exit...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    public class MediaChangedEventArgs : EventArgs
    {
        public string MediaUrl { get; set; }
    }
}