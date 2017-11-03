using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using WebApiServices.Models;

namespace WebApiServices.Controllers
{
    
    [RoutePrefix("api/search")]
    public class SearchController : ApiController
    {
        [HttpGet]
        [Route("stations/{query}")]
        public async Task<IHttpActionResult> GetStations(string query)
        {
            var wildQuery = $"{query}*";
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"https://api.resrobot.se/v2/location.name?key=b9c069be-46e8-4d23-a729-573e05222804&input={wildQuery}&format=json" );
                if (!response.IsSuccessStatusCode)
                    return BadRequest($"{response.StatusCode} :: {response.ReasonPhrase}");

                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content).Value<JArray>("StopLocation");

                if (!json.Any()) return NotFound();

                var stations = json.Select(x => new
                {
                    name = x.Value<string>("name"),
                    id = x.Value<string>("id")
                });

                return Ok(stations);
            }
        }

        [HttpGet]
        [Route("trips")]
        public async Task<IHttpActionResult> Get(string originId, string destId, DateTime departureTime)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"https://api.resrobot.se/v2/trip?key=b9c069be-46e8-4d23-a729-573e05222804&originId={originId}&destId={destId}&date={departureTime:yyyy-MM-dd}&time={departureTime:HH:mm}&format=json");
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result).Value<string>("errorCode");
                    return BadRequest(errorResponse == "SVC_DATATIME_PERIOD" 
                        ? "Din sökning är för långt fram i tiden, var god ändra datum" 
                        : "Något gick fel på servern");
                }

                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content).Value<JArray>("Trip");
                if (!json.Any()) return NotFound();
                var trips = new List<Trip>();
                    
                foreach (var item in json)
                { 
                    var legs = item.Value<JObject>("LegList").Value<JArray>("Leg");
                    var jsonOrigin = legs.First().Value<JObject>("Origin"); 
                    var jsonDestination = legs.Last().Value<JObject>("Destination");

                    var trip = new Trip
                    {
                        Origin = new Destination
                        {
                            Id = originId,
                            StationName = jsonOrigin.Value<string>("name"),
                            Latitude = jsonOrigin.Value<string>("lat"),
                            Longitude = jsonOrigin.Value<string>("lon")
                        },
                        Destination = new Destination
                        {
                            Id = destId,
                            StationName = jsonDestination.Value<string>("name"),
                            Latitude = jsonDestination.Value<string>("lat"),
                            Longitude = jsonDestination.Value<string>("lon")
                        },
                        Transits = new List<Transit>(),
                        DepartureTime = DateTime.Parse($"{jsonOrigin.Value<string>("date")} {jsonOrigin.Value<string>("time")}"),
                        ArrivalTime = DateTime.Parse($"{jsonDestination.Value<string>("date")} {jsonDestination.Value<string>("time")}")
                    };
                    foreach (var leg in legs)
                    {
                        var transit = new Transit();
                        if (leg["Product"] != null) {
                            transit.Transportation = leg["Product"].Value<string>("catOutL");
                            transit.Operator = leg["Product"].Value<string>("operator") ?? "";
                            transit.Url = leg["Product"].Value<string>("operatorUrl") ?? "";
                        }
                        else
                            transit.Transportation = "Walk";

                        trip.Transits.Add(transit);
                    }
                    try
                    {
                        trip.Forecast = await Forecast(trip.ArrivalTime, trip.Destination);
                    }
                    catch (Exception)
                    {
                        trip.Forecast = null;
                    }
                    trips.Add(trip);
                }
                return Ok(trips);
            }
        }

        public async Task<Forecast> Forecast(DateTime arrivalTime, Destination destination)
        {
            var tenDaysFromNow = DateTime.Today.Date.AddDays(10);
            if (arrivalTime > tenDaysFromNow || arrivalTime < DateTime.Now)
                return null;
            var lon = destination.Longitude;
            var lat = destination.Latitude;
            
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"https://opendata-download-metfcst.smhi.se/api/category/pmp3g/version/2/geotype/point/lon/{lon}/lat/{lat}/data.json");
                if (!response.IsSuccessStatusCode) return null; 

                var content = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(content);
                    var timeseries = json.Value<JArray>("timeSeries");
                    JObject selectedTimeserie = new JObject(); 
                    var prevTime = new DateTime();
                    var forecast = new Forecast();

                    foreach (var item in timeseries)
                    {
                        if (!selectedTimeserie.Children().Any())
                        {
                            selectedTimeserie = item.Value<JObject>();
                            prevTime = selectedTimeserie.Value<DateTime>("validTime");
                            continue;
                        };

                        var nextTime = item.Value<JObject>().Value<DateTime>("validTime");
                        if (prevTime < arrivalTime && arrivalTime < nextTime)
                        {
                            if (arrivalTime - prevTime < nextTime - arrivalTime)
                                break;
                            else
                            {
                                selectedTimeserie = item.Value<JObject>();
                                break;
                            }
                        }
                        else
                        {
                            selectedTimeserie = item.Value<JObject>();
                            prevTime = nextTime;
                        }
                    }
                    
                    var parameters = selectedTimeserie.Value<JArray>("parameters");
                    try
                    {
                        forecast.Temperature = parameters.SingleOrDefault(x => x.Value<string>("name") == "t")
                            .Value<JArray>("values").First().Value<double>();

                    var tuple = Models.Forecast.ForecastWeather(parameters);
                    forecast.Weather = tuple.Item2;
                    forecast.Background = tuple.Item1;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                    
                    return forecast;
                }
        }
    }
}
