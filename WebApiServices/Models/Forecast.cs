using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace WebApiServices.Models
{
    public class Forecast
    {
        public string Weather { get; set; }

        public double Temperature { get; set; }

        public string Background { get; set; }
    

        public static Tuple<string, string> ForecastWeather(JArray parameters)
        {
            var wSymb = parameters.SingleOrDefault(x => x.Value<string>("name") == "Wsymb2");
            if (wSymb == null)
                return null;
            var weatherInt = wSymb
                .Value<JArray>("values").First().Value<int>();

            switch (weatherInt)
            {
                case 1:
                    return new Tuple<string, string>("sunny", "Klar himmel");
                case 2:
                    return new Tuple<string, string>("sunny", "Nästan klar himmel");
                case 3:
                    return new Tuple<string, string>("sunny", "Växlande molnighet");
                case 4:
                    return new Tuple<string, string>("sunny", "Halvklar himmel");
                case 5:
                    return new Tuple<string, string>("cloudy", "Molnigt");
                case 6:
                    return new Tuple<string, string>("cloudy", "Mulet");
                case 7:
                    return new Tuple<string, string>("cloudy", "Dimma");
                case 8:
                    return new Tuple<string, string>("rain", "Lätta regnskurar");
                case 9:
                    return new Tuple<string, string>("rain", "Måttligta regnskurar");
                case 10:
                    return new Tuple<string, string>("rain", "Tunga regnskurar");
                case 11:
                    return new Tuple<string, string>("rain", "Åska");
                case 12:
                    return new Tuple<string, string>("rain", "Lätt snöblandat regn");
                case 13:
                    return new Tuple<string, string>("rain", "Måttligt snöblandat regn");
                case 14:
                    return new Tuple<string, string>("rain", "Kraftigt snöblandat regn");
                case 15:
                    return new Tuple<string, string>("snow", "Lätta snöbyar");
                case 16:
                    return new Tuple<string, string>("snow", "Måttliga snöbyar");
                case 17:
                    return new Tuple<string, string>("snow", "Tunga snöbyar");
                case 18:
                    return new Tuple<string, string>("rain", "Duggregn");
                case 19:
                    return new Tuple<string, string>("rain", "Måttligt regnfall");
                case 20:
                    return new Tuple<string, string>("rain", "Kraftigt regnfall");
                case 21:
                    return new Tuple<string, string>("rain", "Åska");
                case 22:
                    return new Tuple<string, string>("snow", "Lätt halka");
                case 23:
                    return new Tuple<string, string>("snow", "Måttlig halka");
                case 24:
                    return new Tuple<string, string>("snow", "Kraftig halka");
                case 25:
                    return new Tuple<string, string>("snow", "Lätt snöfall");
                case 26:
                    return new Tuple<string, string>("snow", "Måttligt snöfall");
                case 27:
                    return new Tuple<string, string>("snow", "Kraftigt snöfall");
                default:
                    return new Tuple<string, string>("default", "Väderprognos saknas");
            }
        }
    }
}