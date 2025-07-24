using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebTools;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>

public class BinanceKLinesInterval
{
    public string IntervalName { get; set; }
    public string IntervalValue { get; set; } 
};


public class BinanceApiParams {
    private string _symbol;
    private BinanceKLinesInterval _interval = new BinanceKLinesInterval { IntervalName = "Days: 1d", IntervalValue = "1d" };
    private long _startTime;
    private long _endTime;
    private string _timeZone = "0"; //Default: 0 - UTC
    private int _limit; // Default: 500; Maximum: 1000

}
public class B_KlineData
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
        DateTimeOffset dateTimeToEpochOffcet = new DateTimeOffset(dateTime, new TimeSpan(0,0,0,0,0));//.ToUniversalTime();
        long timeEpochUnix = dateTimeToEpochOffcet.ToUnixTimeMilliseconds();
        return timeEpochUnix;
    }

    


    public B_KlineData(List<object> values)
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
            $" Diff: {string.Format("{0,10:0.00}", closePrice - openPrice)} Dir: {B_KlineData.DirectionStr(openPrice, closePrice)}";
    }

}


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

public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    private readonly string Format;
    public CustomDateTimeConverter(string format)
    {
        Format = format;
    }
    public override void Write(Utf8JsonWriter writer, DateTime date, JsonSerializerOptions options)
    {
        writer.WriteStringValue(date.ToString(Format));
    }
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.ParseExact(reader.GetString(), Format, null);
    }
}

public partial class MainWindow : Window
{
    public ViewModel BinanceViewModel { get; private set; }

    public string regexTimePattern = "^([0-1][0-9]|[2][0-3]):[0-5][0-9]:[0-5][0-9].[0-9]{3,3}$";

    public MainWindow()
    {
        InitializeComponent();
        BinanceViewModel = new ViewModel();
        DataContext = BinanceViewModel;
        dateFrom.SelectedDate = DateTime.UtcNow;
        dateTo.SelectedDate = DateTime.UtcNow;
        //BinanceViewModel.BinanceKLinesIntervals.
        //foreach (var item in BinanceApiInitParams.intervals)
        //{
        //    cbInterval.Items.Add(item._name);
        //}
    }

    private void btnStart_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog fileDialog = new OpenFileDialog();
        fileDialog.Title = "Open JSON file";
        fileDialog.Filter = "DBF Files|*.json";
        fileDialog.Multiselect = false;
        fileDialog.ValidateNames = false;
        if (fileDialog.ShowDialog() == true)
        {
            try
            {
                string jsonFileName = fileDialog.FileName;
                //parseJsonFile(jsonFileName);
                parseJsonBinanceFile(jsonFileName);
            }
            catch (Exception exception)
            {
                MessageBox.Show(string.Format("Exception {0}", exception.Message.ToString()), "Error");
            }
        }
        else
        {
            getJsonHTTP();
        }
    }

    private void WriteListToConsole<T>(List<T> list)
    {
        foreach (T listItem in list)
        {
            Console.WriteLine(listItem.ToString());
        }
    }
    private void parseJsonFile(string fileName)
    {
        using (StreamReader reader = new StreamReader(fileName))
        {
            var jsonText = reader.ReadToEnd();
            List<CurrencyRate> currencyRates = DeserializeObject<CurrencyRate>(jsonText);
            Console.WriteLine("File : TEXT:");
            Console.WriteLine(jsonText);
            Console.WriteLine("Deserialized to Object List from File:");
            WriteListToConsole(currencyRates);

        }
    }

    private void parseJsonBinanceFile(string fileName)
    {
        using (StreamReader reader = new StreamReader(fileName))
        {
            var jsonText = reader.ReadToEnd();
            List<B_KlineData> currencyRates = DeserializeBinanceObject(jsonText);
            Console.WriteLine("File : TEXT:");
            Console.WriteLine(jsonText);
            Console.WriteLine("Deserialized to Object List from File:");
            WriteListToConsole(currencyRates);

        }
    }

    private List<B_KlineData> DeserializeBinanceObject(string textToDeserialize)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new CustomDateTimeConverter("dd.MM.yyyy"));
        var obj = JsonSerializer.Deserialize<List<List<object>>>(textToDeserialize);


        //List<B_KlineData> list = new List<B_KlineData>(); //JsonSerializer.Deserialize<List<T>>(textToDeserialize, options);
        List<B_KlineData> list = obj.Select(values => new B_KlineData(values)).ToList();
        WriteListToConsole(list);

        return list;
    }

    private List<T> DeserializeObject<T>(string textToDeserialize)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new CustomDateTimeConverter("dd.MM.yyyy"));
        List<T> list = JsonSerializer.Deserialize<List<T>>(textToDeserialize, options);
        return list;
    }

    private async void getJsonHTTP()
    {
        string url = @"https://bank.gov.ua/NBU_Exchange/exchange_site?start=20250701&end=20250716&valcode=usd&sort=exchangedate&order=desc&json";

        using (var httpClient = new HttpClient())
        {
            var json = await httpClient.GetStringAsync(url);
            // List<CurrencyRate> exchange = JsonConvert.DeserializeObject<List<CurrencyRate>>(json, new JsonSerializerSettings { DateFormatString = "dd.mm.yyyy" });
            //List<CurrencyRate> exchange = 
            List<CurrencyRate> currencyRates = DeserializeObject<CurrencyRate>(json);
            //JsonCo.DeserializeObject<List<CurrencyRate>>(testJson /*, new JsonSerializerSettings { DateFormatString = "dd.mm.yyyy" }*/ );
            //json.I
            Console.WriteLine("WEB fetch: TEXT:");
            Console.WriteLine(json.ToString());
            Console.WriteLine("Deserialized to Object List:");
            WriteListToConsole(currencyRates);
        }
    }

    private void cbInterval_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cbInterval.SelectedIndex > -1)
        {
            BinanceKLinesInterval selectedInterval = (BinanceKLinesInterval) cbInterval.SelectedItem;
           // MessageBox.Show(string.Format(@" {0} ( {1} )", selectedInterval.IntervalName, selectedInterval.IntervalValue));
        }
    }

    private void btnCalcDateTimeToEpoch_Click(object sender, RoutedEventArgs e)
    {
       /* 
        DateTime dateFromParse = new DateTime();
        DateTime dateToParse = new DateTime();
        bool isDateFrom = DateTime.TryParse(dateFrom.Text, out dateFromParse);
        bool isDateTo = DateTime.TryParse(dateTo.Text, out dateToParse);
        */
        if(dateTo.SelectedDate < dateFrom.SelectedDate)
        {
            MessageBox.Show("Date To must be greater than Date From");
            return;
        }
        //        string regexTimePattern = "^[0-9]{2,2}:[0-9]{2,2}:[0-9]{2,2}.[0-9]{3,3}$";
        bool isTimeFrom = Regex.IsMatch(timeFrom.Text, regexTimePattern);
        bool isTimeTo = Regex.IsMatch(timeTo.Text, regexTimePattern);
        // bool isTimeFrom = Time.TryParse(dateFrom.Text, out dateFromParse);
        // bool isTimeTo = DateTime.TryParse(dateTo.Text, out dateToParse);
        if (isTimeFrom && isTimeTo)
        {
            //MessageBox.Show("Date From: " + dateFromParse.ToString("dd.MM.yyyy HH:mm:ss.fff z") + " - DateTo: " + dateToParse.ToString("dd.MM.yyyy HH:mm:ss.fff z"));
            //MessageBox.Show("Time From: " + timeFrom.Text + " - TimeTo: " + timeTo.Text);

            string textDateTimeFrom = dateFrom.SelectedDate.GetValueOrDefault(DateTime.UtcNow).ToString("dd.MM.yyyy ") + timeFrom.Text;
            string textDateTimeTo = dateTo.SelectedDate.GetValueOrDefault(DateTime.UtcNow).ToString("dd.MM.yyyy ") + timeTo.Text;
            DateTime dateTimeFrom = DateTime.ParseExact(textDateTimeFrom, "dd.MM.yyyy HH:mm:ss.fff", null);
            DateTime dateTimeTo = DateTime.ParseExact(textDateTimeTo, "dd.MM.yyyy HH:mm:ss.fff", null);

            //MessageBox.Show("DateTime From: " + dateTimeFrom.ToString("dd.MM.yyyy HH:mm:ss.fff") + " - DateTime To: " 
              //  + dateTimeTo.ToString("dd.MM.yyyy HH:mm:ss.fff"));

            epochTimeFrom.Text = B_KlineData.DateTimeToEpoch(dateTimeFrom).ToString();
            epochTimeTo.Text = B_KlineData.DateTimeToEpoch(dateTimeTo).ToString();
        }
        else
        {
            MessageBox.Show("Date or Time not correct!");
            return;
        }
    }

    private void timeTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender != null)
        {
            (sender as TextBox).SelectAll();
        }
    }

    private void timeFrom_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!Regex.IsMatch((sender as TextBox).Text, regexTimePattern))
        {
            (sender as TextBox).Background = Brushes.Pink;
        } else
        {
            (sender as TextBox).Background = Brushes.White;
        }
    }

    private void btnCalcEpochToDateTime_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            long inputTimeEpochFrom = long.Parse(epochTimeFrom.Text);
            long inputTimeEpochTo = long.Parse(epochTimeTo.Text);

            DateTime dateTimeFromEpochFrom = B_KlineData.EpochToDateTime(inputTimeEpochFrom);
            DateTime dateTimeFromEpochTo = B_KlineData.EpochToDateTime(inputTimeEpochTo);
            dateFrom.SelectedDate = dateTimeFromEpochFrom.Date;
            dateTo.SelectedDate = dateTimeFromEpochTo.Date;
            timeFrom.Text = dateTimeFromEpochFrom.ToString("HH:mm:ss.fff");
            timeTo.Text = dateTimeFromEpochTo.ToString("HH:mm:ss.fff");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Epoch DateTime is not correct");
        }

    }
}