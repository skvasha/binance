using Microsoft.Win32;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    public DateTime EpochToDateTime(long epoch)
    {
        DateTime dateTime = (new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds(epoch);
        return dateTime;
    }

    public long DateTimeToEpoch(DateTime dateTime)
    {
        DateTimeOffset dateTimeToEpochOffcet = dateTime.ToUniversalTime();
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
    public MainWindow()
    {
        InitializeComponent();
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {

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
}