using System.Text.Json;
using System.Web;
using Newtonsoft.Json;
using OrderAPI.Dtos;

namespace OrderAPI.Services
{
    public class GoogleAPIService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        public GoogleAPIService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GoogleApi:ApiKey"];
        }

        public async Task<DeliveryTimeResponse> GetDeliveryTimeAsync(string origem, string destino)
        {
            Console.WriteLine(_apiKey);
         
            string url = $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={origem}&destinations={destino}&key={_apiKey}";
            Console.WriteLine(url);
            var response = await _httpClient.GetStringAsync(url);
            Console.WriteLine(response);

            return JsonConvert.DeserializeObject<DeliveryTimeResponse>(response);

        }
    }
}