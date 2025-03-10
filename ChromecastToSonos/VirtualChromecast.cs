using Makaretu.Dns;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChromecastToSonos
{
    public class VirtualChromecast
    {
        private readonly Sonos _sonos;
        private readonly ServiceDiscovery _serviceDiscovery;

        public VirtualChromecast(Sonos sonos)
        {
            _sonos = sonos;
            _serviceDiscovery = new ServiceDiscovery();
        }

        public void Start()
        {
            AdvertiseChromecast();
            Task.Run(() => StartListener());
        }

        private void AdvertiseChromecast()
        {
            var serviceProfile = new ServiceProfile("VirtualChromecast", "_googlecast._tcp", 8080)
            {
                HostName = Dns.GetHostName()
            };
            serviceProfile.AddProperty("id", "1234567890");
            serviceProfile.AddProperty("md", "Virtual Chromecast");
            serviceProfile.AddProperty("fn", "Virtual Chromecast");
            serviceProfile.AddProperty("rs", "Google Cast");
            serviceProfile.AddProperty("bs", "true");
            serviceProfile.AddProperty("st", "0");
            serviceProfile.AddProperty("ca", "1234");
            serviceProfile.AddProperty("ic", "/setup/icon.png");
            serviceProfile.AddProperty("ve", "05");
            serviceProfile.AddProperty("groupname", _sonos.GroupName); // Add GroupName property

            _serviceDiscovery.Advertise(serviceProfile);
            Console.WriteLine("VirtualChromecast advertised on the network.");
        }

        private async Task StartListener()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"http://{_sonos.IpAddress}:8080/");
            listener.Start();
            Console.WriteLine($"VirtualChromecast started for Sonos device at {_sonos.IpAddress}, listening on port 8080...");

            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/media")
                {
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        string mediaUrl = await reader.ReadToEndAsync();
                        _sonos.Play(mediaUrl);
                    }

                    string responseString = "Media URL received";
                    byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    response.OutputStream.Close();
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Close();
                }
            }
        }
    }
}