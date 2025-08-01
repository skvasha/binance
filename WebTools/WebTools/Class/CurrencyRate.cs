using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebTools.Class
{
    public class CurrencyRate
    {
        /*
        "exchangedate":"05.05.2022"
        "r030":840
        "cc":"USD"
        "txt":"Долар США"
        "enname":"USDollar"
        "rate":29.2549
        "units":1
        "rate_per_unit":29.2549
        "group":"1"
        "calcdate":"04.05.2022"
        */

        [JsonPropertyName("exchangedate")]
        public DateTime ExchangeDate { get; set; } //":"16.07.2025",

        [JsonPropertyName("r030")]
        public int CurrencyCode { get; set; } //:840,

        [JsonPropertyName("cc")]
        public string CurrencyCodeStr { get; set; } //":"USD",

        [JsonPropertyName("txt")]
        public string CurrencyDescriptionUA { get; set; } //":"Долар США",

        [JsonPropertyName("enname")]
        public string CurrencyDescriptionEN { get; set; } //":"USDollar",

        [JsonPropertyName("rate")]
        public double Rate { get; set; } //":41.8211,

        [JsonPropertyName("units")]
        public int Units { get; set; } //":1,

        [JsonPropertyName("rate_per_unit")]
        public double RatePerUnit { get; set; } //":41.8211,

        [JsonPropertyName("group")]
        public string Group { get; set; } //":"1",

        [JsonPropertyName("calcdate")]
        public DateTime CalculateRateDate { get; set; } //":"15.07.2025"

        public override string ToString()
        {
            return $"{CurrencyCodeStr}: {ExchangeDate.ToString("dd.MM.yyyy")} - {Rate.ToString()}";
        }
    }
}
