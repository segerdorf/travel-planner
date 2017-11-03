using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiServices.Models
{
    public class Trip
    {
        public Destination Origin { get; set; }

        public Destination Destination { get; set; }

        public DateTime? DepartureTime { get; set; }

        public DateTime ArrivalTime { get; set; }

        public List<Transit> Transits { get; set; }

        public Forecast Forecast { get; set; }
        
    }
}