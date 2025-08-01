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
using WebTools.Class;
using WebTools.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebTools;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>


public class BinanceApiParams {
    private string _symbol;
    private BinanceKLinesInterval _interval = new BinanceKLinesInterval { IntervalName = "Days: 1d", IntervalValue = "1d" };
    private long _startTime;
    private long _endTime;
    private string _timeZone = "0"; //Default: 0 - UTC
    private int _limit; // Default: 500; Maximum: 1000

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
            List<BinanceKlineData> currencyRates = DeserializeBinanceObject(jsonText);
            Console.WriteLine("File : TEXT:");
            Console.WriteLine(jsonText);
            Console.WriteLine("Deserialized to Object List from File:");
            WriteListToConsole(currencyRates);
        }
    }

    private List<BinanceKlineData> DeserializeBinanceObject(string textToDeserialize)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new CustomDateTimeConverter("dd.MM.yyyy"));
        var obj = JsonSerializer.Deserialize<List<List<object>>>(textToDeserialize);


        //List<B_KlineData> list = new List<B_KlineData>(); //JsonSerializer.Deserialize<List<T>>(textToDeserialize, options);
        List<BinanceKlineData> list = obj.Select(values => new BinanceKlineData(values)).ToList();
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

            epochTimeFrom.Text = BinanceKlineData.DateTimeToEpoch(dateTimeFrom).ToString();
            epochTimeTo.Text = BinanceKlineData.DateTimeToEpoch(dateTimeTo).ToString();
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

            DateTime dateTimeFromEpochFrom = BinanceKlineData.EpochToDateTime(inputTimeEpochFrom);
            DateTime dateTimeFromEpochTo = BinanceKlineData.EpochToDateTime(inputTimeEpochTo);
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