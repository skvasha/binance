using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WebTools.Class;

namespace WebTools.Models
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<BinanceKLinesInterval> AllIntervals { get; private set; }
        public BinanceKLinesInterval _selectedInterval;
        public BinanceKLinesInterval SelectedInterval
        {
            get { return _selectedInterval; }
            set
            {
                _selectedInterval = value;
                OnPropertyChanged("IntervalChanded");
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ViewModel()
        {
            AllIntervals = new ObservableCollection<BinanceKLinesInterval>();
            SetIntervals();
            SelectedInterval = AllIntervals.FirstOrDefault(static item => item.IntervalValue == "1d");
        }

        private void SetIntervals()
        {

            AllIntervals.Add(new BinanceKLinesInterval { IntervalName = "Seconds: 1s", IntervalValue = "1s" });

            AllIntervals.Add(new BinanceKLinesInterval { IntervalName = "Minutes: 1m", IntervalValue = "1m" });
            AllIntervals.Add(new BinanceKLinesInterval { IntervalName = "Minutes: 3m", IntervalValue = "3m" });
            AllIntervals.Add(new BinanceKLinesInterval { IntervalName = "Minutes: 5m", IntervalValue = "5m" });
            AllIntervals.Add(new BinanceKLinesInterval { IntervalName = "Minutes: 15m", IntervalValue = "15m" });
            AllIntervals.Add(new BinanceKLinesInterval { IntervalName = "Minutes: 30m", IntervalValue = "30m" });

            AllIntervals.Add(new BinanceKLinesInterval { IntervalName = "Hours: 1h", IntervalValue = "1h" });
            AllIntervals.Add(new BinanceKLinesInterval { IntervalName = "Hours: 2h", IntervalValue = "2h" });
            AllIntervals.Add(new BinanceKLinesInterval { IntervalName = "Hours: 4h", IntervalValue = "4h" });
            AllIntervals.Add(new BinanceKLinesInterval { IntervalName = "Hours: 6h", IntervalValue = "6h" });
            AllIntervals.Add(new BinanceKLinesInterval { IntervalName = "Hours: 8h", IntervalValue = "8h" });
            AllIntervals.Add(new BinanceKLinesInterval { IntervalName = "Hours: 12h", IntervalValue = "12h" });

            AllIntervals.Add(new BinanceKLinesInterval { IntervalName = "Days: 1d", IntervalValue = "1d" });
            AllIntervals.Add(new BinanceKLinesInterval { IntervalName = "Days: 3d", IntervalValue = "3d" });

            AllIntervals.Add(new BinanceKLinesInterval { IntervalName = "Weeks: 1w", IntervalValue = "1w" });

            AllIntervals.Add(new BinanceKLinesInterval { IntervalName = "Months: 1M", IntervalValue = "1M" });
        }


    }
}
