using CitizenFX.Core;
using Flurl.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleSync.Server
{
    /// <summary>
    /// Weather information of a City.
    /// </summary>
    public class WeatherData
    {
        [JsonProperty("id")]
        public int ID { get; set; }
        [JsonProperty("main")]
        public string Main { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("icon")]
        public string Icon { get; set; }
    }
    /// <summary>
    /// Information of a specific city.
    /// </summary>
    public class City
    {
        [JsonProperty("weather")]
        public List<WeatherData> Weather { get; set; }
    }

    /// <summary>
    /// Tools for accessing the OpenWeather API.
    /// </summary>
    public static class OpenWeather
    {
        /// <summary>
        /// The URL to fetch the up to date Weather from OpenWeather.
        /// </summary>
        private const string openWeatherURL = "http://api.openweathermap.org/data/2.5/weather?q={0}&appid={1}";

        public static async Task<WeatherData> GetWeather()
        {
            // Otherwise, format the correct URL
            string url = string.Format(openWeatherURL, Convars.OpenWeatherCity, Convars.OpenWeatherKey);
            // Make the request
            HttpResponseMessage response = await url.WithHeader("User-Agent", "SimpleSync (+https://github.com/justalemon/SimpleSync)").GetAsync();
            // And get the text of the response
            string text = await response.Content.ReadAsStringAsync();

            // If the status code is not 200
            if (response.StatusCode != HttpStatusCode.OK)
            {
                // Show the status code and content and return
                Debug.WriteLine($"OpenWeather returned code {response.StatusCode}! ({text})");
                return null;
            }

            // Parse the data as JSON
            City data = JsonConvert.DeserializeObject<City>(text);

            // If there is no weather data for the city
            // (unlikely, but just as a safety measure)
            if (data.Weather.Count == 0)
            {
                // Notify it and return
                Debug.WriteLine($"Open weather didn't returned Weather data for that city!");
                return null;
            }

            // Otherwise, return the first weather
            return data.Weather[0];
        }
    }
}
