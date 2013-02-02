using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Kalkulator.Model;

namespace Kalkulator
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            this.DataContext = App.ViewModel;
            this.maintb.Text = "1";

            this.mainclist.SelectionChanged += new SelectionChangedEventHandler(mainclist_SelectionChanged);
            this.mainclist2.SelectionChanged += new SelectionChangedEventHandler(mainclist2_SelectionChanged);

            this.mainclist.SelectedItem = App.ViewModel.Currencies.Single(x => x.Code == App.ViewModel.FirstSelectedCurrency);
            //this.mainclist2.SelectedItem = App.ViewModel.Currencies.Single(x => x.Code == App.ViewModel.SecondSelectedCurrency);
        }

        private void maintb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (maintb.Text != "")
            {
                Currency c_in = mainclist.SelectedItem as Currency;
                Currency c_out = mainclist2.SelectedItem as Currency;

                maintb2.Text = App.ViewModel.Calculate(c_in.Code, maintb.Text, c_out.Code);
            }
        }

        private void maintb2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (maintb2.Text != "")
            {
                Currency c_in = mainclist2.SelectedItem as Currency;
                Currency c_out = mainclist.SelectedItem as Currency;

                maintb.Text = App.ViewModel.Calculate(c_in.Code, maintb2.Text, c_out.Code);
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.Update();
        }

        private void mainclist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Currency c_in = mainclist.SelectedItem as Currency;

            if (maintb.Text != "" && mainclist2.SelectedItem != null)
            {
                Currency c_out = mainclist2.SelectedItem as Currency;

                maintb2.Text = App.ViewModel.Calculate(c_in.Code, maintb.Text, c_out.Code);
            }

            App.ViewModel.FirstSelectedCurrency = c_in.Code;
        }

        private void mainclist2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Currency c_in = mainclist.SelectedItem as Currency;
            Currency c_out = mainclist2.SelectedItem as Currency;

            if (maintb2.Text != "")
            {
                maintb2.Text = App.ViewModel.Calculate(c_in.Code, maintb.Text, c_out.Code);
            }

            App.ViewModel.SecondSelectedCurrency = c_out.Code;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            string first = App.ViewModel.FirstSelectedCurrency;
            string second = App.ViewModel.SecondSelectedCurrency;
            this.mainclist.SelectedItem = App.ViewModel.Currencies.Single(x => x.Code == second);
            this.mainclist2.SelectedItem = App.ViewModel.Currencies.Single(x => x.Code == first);
        }
    }
}