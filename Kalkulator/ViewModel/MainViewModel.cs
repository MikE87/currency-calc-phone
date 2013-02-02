using System;
using System.Net;
using System.ComponentModel;
using Kalkulator.Model;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Xml.Linq;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Text;

namespace Kalkulator.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public nbpDataContext nbpDB;

        public MainViewModel(string connectionString)
        {
            this.nbpDB = new nbpDataContext(connectionString);
            this.LoadDataFromDB();
        }

        /// <summary>
        /// Lista aktualnych walut
        /// </summary>
        private ObservableCollection<Currency> _currencies;
        public ObservableCollection<Currency> Currencies
        {
            get { return _currencies; }
            set
            {
                _currencies = value;
                NotifyPropertyChanged("Currencies");
            }
        }

        /// <summary>
        /// Kod waluty dla pierwszego pola wyboru
        /// </summary>
        public string FirstSelectedCurrency
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("FirstSelectedCurrency"))
                {
                    return (string)IsolatedStorageSettings.ApplicationSettings["FirstSelectedCurrency"];
                }
                else
                {
                    return "PLN";
                }
            }
            set
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("FirstSelectedCurrency"))
                {
                    IsolatedStorageSettings.ApplicationSettings["FirstSelectedCurrency"] = value;
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings.Add("FirstSelectedCurrency", value);
                }

                IsolatedStorageSettings.ApplicationSettings.Save();

                NotifyPropertyChanged("FirstSelectedCurrency");
            }
        }

        /// <summary>
        /// Kod waluty dla drugiego pola wyboru
        /// </summary>
        public string SecondSelectedCurrency
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("SecondSelectedCurrency"))
                {
                    return (string)IsolatedStorageSettings.ApplicationSettings["SecondSelectedCurrency"];
                }
                else
                {
                    return "EUR";
                }
            }
            set
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("SecondSelectedCurrency"))
                {
                    IsolatedStorageSettings.ApplicationSettings["SecondSelectedCurrency"] = value;
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings.Add("SecondSelectedCurrency", value);
                }

                IsolatedStorageSettings.ApplicationSettings.Save();

                NotifyPropertyChanged("SecondSelectedCurrency");
            }
        }

        /// <summary>
        /// Stan pobieranej aktualizacji
        /// </summary>
        private string _dlStatus;
        public string DownloadStatus
        {
            get { return _dlStatus; }
            set 
            {
                _dlStatus = value;
                NotifyPropertyChanged("DownloadStatus");
            }
        }

        /// <summary>
        /// Aktualizacja bazy walut
        /// </summary>
        public void Update()
        {
            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webClient_DownloadStringCompleted);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);

            this.DownloadStatus = null;

            // Pobranie najnowszej listy walut z nbp
            webClient.DownloadStringAsync(new Uri("http://www.nbp.pl/kursy/xml/LastA.xml", UriKind.Absolute));
        }

        void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (this.DownloadStatus == null)
                this.DownloadStatus = "Pobieranie ";
            else
                this.DownloadStatus += "#";
        }

        void webClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null && e.Result != null)
            {
                XDocument xmlData;

                try
                {
                    // Próba parsowania pobranych danych
                    xmlData = XDocument.Parse(e.Result);
                }
                catch
                {
                    this.DownloadStatus = "Błąd przetwarzania danych!";
                    return;
                }

                // Sprawdzenie czy pobrane dane były zaktualizowane na serwerze
                var code = (string)xmlData.Descendants("numer_tabeli").First();
                if (code == this.DataCheckCode)
                {
                    this.DownloadStatus = "Baza walut jest aktualna.";
                    return;
                }

                // Parsowanie danych z pobranego xml'a
                var data = from xml in xmlData.Descendants("pozycja")
                           select new Currency()
                           {
                               Code = (string)xml.Element("kod_waluty"),
                               Name = (string)xml.Element("nazwa_waluty"),
                               ExchangeFactor = (int)xml.Element("przelicznik"),
                               AvgExchangeRate = Double.Parse(((string)xml.Element("kurs_sredni")).Replace(",","."))
                           };

                // Aktualizacja danych w bazie
                nbpDB.Currencies.DeleteAllOnSubmit(nbpDB.Currencies.Where(x => x.Code != "PLN"));
                nbpDB.Currencies.InsertAllOnSubmit(data);
                nbpDB.SubmitChanges();

                // Wczytanie aktualnych danych
                this.LoadDataFromDB();

                // Oznaczenie daty ostatniej aktualizacji
                this.LastUpdate = DateTime.Now;
                this.DataCheckCode = code;
                this.DownloadStatus = "Baza walut zaktualizowana.";
            }
        }

        /// <summary>
        /// Data ostatniego uaktualnienia bazy walut przechowywana w ustawieniach aplikacji
        /// </summary>
        public DateTime LastUpdate
        {
            get 
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("LastUpdate"))
                {
                    return (DateTime)IsolatedStorageSettings.ApplicationSettings["LastUpdate"];
                }
                else
                {
                    return DateTime.MinValue; 
                }
            }

            set 
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("LastUpdate"))
                {
                    IsolatedStorageSettings.ApplicationSettings["LastUpdate"] = value;
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings.Add("LastUpdate", value);
                }

                IsolatedStorageSettings.ApplicationSettings.Save();

                NotifyPropertyChanged("LastUpdate");
            }
        }

        // Kod do sprawdzenia aktualności danych
        public string DataCheckCode
        {
            get 
            { 
                if (IsolatedStorageSettings.ApplicationSettings.Contains("DataCheckCode"))
                {
                    return (string)IsolatedStorageSettings.ApplicationSettings["DataCheckCode"];
                }
                else
                {
                    return ""; 
                }
            }
            set 
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("DataCheckCode"))
                {
                    IsolatedStorageSettings.ApplicationSettings["DataCheckCode"] = value;
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings.Add("DataCheckCode", value);
                }

                IsolatedStorageSettings.ApplicationSettings.Save();

                NotifyPropertyChanged("DataCheckCode");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Pobranie danych z bazy lokalnej
        /// </summary>
        public void LoadDataFromDB()
        {
            var data = nbpDB.Currencies.ToList();
            
            Currencies = new ObservableCollection<Currency>(data);
        }

        /// <summary>
        /// Konwertuje waluty
        /// </summary>
        /// <param name="inCurrency">Waluta z której konwertujemy</param>
        /// <param name="value">Wartość jaką chcemy przeliczyć</param>
        /// <param name="outCurrency">Waluta na którą konwertujemy</param>
        /// <returns>Przeliczona wartość value na outCurrency</returns>
        public string Calculate(string inCurrency, string value, string outCurrency)
        {
            double val;
            if (Double.TryParse(value.Replace(",", "."), System.Globalization.NumberStyles.Currency, CultureInfo.CurrentCulture, out val))
            {

                Currency from = Currencies.Single(x => x.Code == inCurrency);
                Currency to = Currencies.Single(x => x.Code == outCurrency);

                // Konwersja na złotówki
                double result = (from.AvgExchangeRate / from.ExchangeFactor) * val;

                // Konwersja na walutę wyjściową
                result = result / (to.AvgExchangeRate / to.ExchangeFactor);

                return Math.Round(result, 4).ToString();
            }

            return "";
        }
    }
}
