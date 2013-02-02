using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Data.Linq;

namespace Kalkulator.Model
{
    [Table]
    public class Currency : INotifyPropertyChanged
    {
        private string _code;

        [Column(IsPrimaryKey = true, CanBeNull = false)]
        public string Code
        {
            get { return _code; }
            set
            {
                _code = value;
                NotifyPropertyChanged("Code");
            }
        }

        private string _name;

        [Column]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyPropertyChanged("Name");
            }
        }

        private int _exFactor;

        [Column(CanBeNull = false)]
        public int ExchangeFactor
        {
            get { return _exFactor; }
            set
            {
                _exFactor = value;
                NotifyPropertyChanged("ExchangeFactor");
            }
        }

        private double _avgExRate;

        [Column(CanBeNull = false)]
        public double AvgExchangeRate
        {
            get { return _avgExRate; }
            set
            {
                _avgExRate = value;
                NotifyPropertyChanged("AvgExchangeRate");
            }
        }


        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion //Events
    }

    public class nbpDataContext : DataContext
    {
        public nbpDataContext(string connectionString) : base(connectionString) 
        { 
        }

        public Table<Currency> Currencies;
    }
    
}
