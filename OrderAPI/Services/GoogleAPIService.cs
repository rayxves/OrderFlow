using System.Text.Json;
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
           _apiKey = configuration["GooglePlacesApi:ApiKey"];
        }

        public async Task<DeliveryTimeResponse> GetDeliveryTimeAsync(string origem, string destino){
        string url = $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={origem}&destinations={destino}&key={_apiKey}";

        var response = await _httpClient.GetStringAsync(url);
        return JsonConvert.DeserializeObject<DeliveryTimeResponse>(response);
        }
    }
}