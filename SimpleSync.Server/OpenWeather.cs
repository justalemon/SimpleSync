using CitizenFX.Core;
using Flurl.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
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
            // Format the OpenWeatherMap API URL
            string url = string.Format(openWeatherURL, Convars.OpenWeatherCity, Convars.OpenWeatherKey);

            // And try to make the request
            HttpResponseMessage response = null;
            try
            {
                response = await url.WithHeader("User-Agent", "SimpleSync (+https://github.com/justalemon/SimpleSync)").GetAsync();
            }
            // If there was a problem with the request
            catch (FlurlHttpException e)
            {
                // If the request was completed
                if (e.Call.Completed)
                {
                    Debug.WriteLine($"OpenWeatherMap returned code {(int)e.Call.HttpStatus} {e.Call.HttpStatus}!");
                    return null;
                }
                // If the request was not completed
                Debug.WriteLine($"Error when trying to request information from OpenWeatherMap: {e.Message}");
                return null;
            }

            // And get the text of the response
            string text = await response.Content.ReadAsStringAsync();

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
