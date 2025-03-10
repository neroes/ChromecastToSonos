using Makaretu.Dns;

namespace ChromecastToSonos
{
    public class SonosService
    {
        private List<Sonos> _sonosDevices;
        private List<VirtualChromecast> _virtualChromecasts;

        public SonosService()
        {
            _sonosDevices = DiscoverSonosDevices().Result;
            _virtualChromecasts = new List<VirtualChromecast>();

            foreach (var sonos in _sonosDevices)
            {
                var virtualChromecast = new VirtualChromecast(sonos);
                _virtualChromecasts.Add(virtualChromecast);
                virtualChromecast.Start();
            }
        }

        private async Task<List<Sonos>> DiscoverSonosDevices()
        {
            Dictionary<string,string> sonosAddresses = [];
            Dictionary<string,string> HostToServerName = [];
            var serviceDiscovery = new ServiceDiscovery();
            var mdns = new MulticastService();
            var startTime = DateTime.Now;
            
            /*mdns.NetworkInterfaceDiscovered += (s, e) =>
            {

                // Ask for the name of all services.
                serviceDiscovery.QueryAllServices();
            };*/

            mdns.AnswerReceived += (s, e) =>
            {
                // Is this an answer to a service instance details?
                var servers = e.Message.Answers.OfType<SRVRecord>();
                foreach (var server in servers)
                {
                    if(server.Target.ToString().StartsWith("Sonos-"))
                    {
                        if (!server.Name.ToString().Contains("airplay"))
                        {
                            Console.WriteLine($"host '{server.Target}' for '{server.Name}'");
                            var groupName = server.Name.ToString()
                                .Split('.')[0]// extracting name
                                .Split('@')[1]
                                .Replace("\\032"," "); // put spaces back in
                            HostToServerName[server.Target.ToString()] = groupName;

                            // Ask for the host IP addresses.
                            mdns.SendQuery(server.Target, type: DnsType.A);
                            mdns.SendQuery(server.Target, type: DnsType.AAAA);
                        }
                            
                    }
                    
                }

                // Is this an answer to host addresses?
                var addresses = e.Message.Answers.OfType<AddressRecord>();
                foreach (var address in addresses)
                {
                    if (address.Name.ToString().StartsWith("Sonos-"))
                    {
                        sonosAddresses[address.Address.ToString()] = address.Name.ToString();
                        Console.WriteLine($"host '{address.Name}' at {address.Address}");
                        var timeElapsed = DateTime.Now - startTime;
                        Console.WriteLine($"TimeElapsed '{timeElapsed.TotalMilliseconds}'");
                    }
                }
            };

            try
            {
                mdns.Start();
                await Task.Delay(120000); // Wait for responses
            }
            finally
            {
                serviceDiscovery.Dispose();
                mdns.Stop();
            }


            if (sonosAddresses.Count == 0)
            {
                Console.WriteLine("No Sonos devices found on the network.");
                throw new Exception("No Sonos devices found on the network.");
            }

            var sonosDevices = sonosAddresses.Select(e => new Sonos(e.Key, HostToServerName[e.Value])).ToList();
            return sonosDevices;
        }

        public void PlayOnAllDevices(string mediaUrl)
        {
            foreach (var sonos in _sonosDevices)
            {
                sonos.Play(mediaUrl);
            }
        }
    }
}