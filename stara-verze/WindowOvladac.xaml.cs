using System;
using System.Windows;
using System.Windows.Controls;

namespace SerialTerminal
{
    /// <summary>
    /// Interakční logika pro WindowOvladac.xaml
    /// </summary>
    public partial class WindowOvladac : Window
    {
        public WindowOvladac()
        {
            InitializeComponent();
        }

        private void Tlacitka_Click(object sender, RoutedEventArgs e)
        {
            String odesilatel = (sender as Button).Content.ToString();
            String prikaz = odesilatel;

            
        }
    }

}