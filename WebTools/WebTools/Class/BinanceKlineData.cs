using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTools.Class
{
    public class BinanceKlineData
    {
        public long openTime { get; set; }
        public double openPrice { get; set; }
        public double highPrice { get; set; }
        public double lowPrice { get; set; }
        public double closePrice { get; set; }
        public double volume { get; set; }
        public long closeTime { get; set; }
        public double quoteAssetVolume { get; set; }
        public long numberOfTrades { get; set; }
        public double baseVolume { get; set; }
        public double quoteVolume { get; set; }
        public string ignore { get; set; }

        public static DateTime EpochToDateTime(long epoch)
        {
            DateTime dateTime = (new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local)).AddMilliseconds(epoch);
            return dateTime;
        }

        public static long DateTimeToEpoch(DateTime dateTime)
        {
            DateTimeOffset dateTimeToEpochOffcet = new DateTimeOffset(dateTime, new TimeSpan(0, 0, 0, 0, 0));//.ToUniversalTime();
            long timeEpochUnix = dateTimeToEpochOffcet.ToUnixTimeMilliseconds();
            return timeEpochUnix;
        }

        public BinanceKlineData(List<object> values)
        {
            if (values != null)
            {
                NumberFormatInfo format = new NumberFormatInfo();
                format.NumberDecimalSeparator = ".";

                //public long
                this.openTime = Int64.Parse(values.ElementAt(0).ToString());
                //string
                this.openPrice = Double.Parse(values.ElementAt(1).ToString(), format);
                //string
                this.highPrice = Double.Parse(values.ElementAt(2).ToString(), format);
                //string
                this.lowPrice = Double.Parse(values.ElementAt(3).ToString(), format);
                //string
                this.closePrice = Double.Parse(values.ElementAt(4).ToString(), format);
                //string
                this.volume = Double.Parse(values.ElementAt(5).ToString(), format);
                //long
                this.closeTime = Int64.Parse(values.ElementAt(6).ToString());
                //string
                this.quoteAssetVolume = Double.Parse(values.ElementAt(7).ToString(), format);
                //long
                this.numberOfTrades = Int64.Parse(values.ElementAt(8).ToString());
                //string
                this.baseVolume = Double.Parse(values.ElementAt(9).ToString(), format);
                //string
                this.quoteVolume = Double.Parse(values.ElementAt(10).ToString(), format);
                //string
                this.ignore = values.ElementAt(11).ToString();
            }
        }

        private static string DirectionStr(Double firstNumber, Double secondNumber)
        {
            return secondNumber - firstNumber > 0 ? "↑" : "↓";
        }

        public override string ToString()
        {
            return $"Time: {EpochToDateTime(openTime)} - {EpochToDateTime(closeTime)}: O:{string.Format("{0:0.00}", openPrice)} H:{string.Format("{0:0.00}", highPrice)} L:{string.Format("{0:0.00}", lowPrice)} C:{string.Format("{0:0.00}", closePrice)}" +
                $" Diff: {string.Format("{0,10:0.00}", closePrice - openPrice)} Dir: {BinanceKlineData.DirectionStr(openPrice, closePrice)}";
        }

    }
}
