using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PocAadClient.Models
{
    public class Weather
    {
        public string DateFormatted { get; set; }
        public int TemperatureC { get; set; }
        public string Summary { get; set; }

        public int TemperatureF
        {
            get
            {
                return 32 + (int)(TemperatureC / 0.5556);
            }
        }
    }
}