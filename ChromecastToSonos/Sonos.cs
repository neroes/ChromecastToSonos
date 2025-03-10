using System.Net;
using System.Text;

namespace ChromecastToSonos
{
    public class Sonos
    {
        private readonly HttpClient _httpClient;
        public string IpAddress { get; private set; }
        public string GroupName { get; private set; } // Add GroupName property

        public Sonos(string ipAddress, string groupName)
        {
            _httpClient = new HttpClient();
            IpAddress = ipAddress;
            GroupName = groupName; // Initialize GroupName
        }

        public async void Play(string mediaUrl)
        {
            string sonosPort = "1400";
            string endpoint = $"http://{IpAddress}:{sonosPort}/MediaRenderer/AVTransport/Control";

            string soapRequest = $@"
                <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""
                            s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                    <s:Body>
                        <u:SetAVTransportURI xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1"">
                            <InstanceID>0</InstanceID>
                            <CurrentURI>{mediaUrl}</CurrentURI>
                            <CurrentURIMetaData></CurrentURIMetaData>
                        </u:SetAVTransportURI>
                    </s:Body>
                </s:Envelope>";

            var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();
                Console.WriteLine($"Playing media on Sonos: {mediaUrl}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing media on Sonos: {ex.Message}");
            }
        }
    }
}