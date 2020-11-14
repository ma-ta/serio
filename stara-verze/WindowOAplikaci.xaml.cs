using System;
using System.Windows;
using System.Windows.Input;

namespace SerialTerminal
{
    /// <summary>
    /// Interakční logika pro WindowOAplikaci.xaml
    /// </summary>
    public partial class WindowOAplikaci : Window
    {
        // zajistí přístup k proměnným v MainWindow.xaml.cs
        MainWindow hlavniOkno = new MainWindow();
        
        // informuje, zda byl již spuštěn easterEgg
        bool easterEgg1 = false;

        public WindowOAplikaci()
        {
            InitializeComponent();
            textBlockNazev.Text = hlavniOkno.nazevAplikace + "  " + hlavniOkno.verzeAplikace;
            textBlockNazev.ToolTip = hlavniOkno.podTitul;
            textBlockAutor.Text = "©  " + hlavniOkno.aktualniRok + "  " + hlavniOkno.autor;
        }

        private void zavrit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!easterEgg1)
            {
                easterEgg1 = true;
                MessageBox.Show("E-mail:\n" + hlavniOkno.email + "\n\nWeb:\n" + hlavniOkno.webURL, "Kontakt", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.ToString() == "Escape")
            {
                this.Close();
            }
        }
    }

}
