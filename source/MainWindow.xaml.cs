using System.Windows;
using System.Windows.Media;

using System.IO.Ports;
using System.IO;
using Microsoft.Win32;

namespace Serio;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    SolidColorBrush barvaBila = new(Color.FromArgb(0xFF, 0xF2, 0xFF, 0xFF)); // #FFF2FFFF
    SolidColorBrush barvaModra1 = new(Color.FromArgb(0xFF, 0x92, 0xCA, 0xF4)); // #FF92CAF4
    SolidColorBrush barvaModra2 = new(Color.FromArgb(0xFF, 0x56, 0x9C, 0xD6)); // #FF569CD6
    SolidColorBrush barvaZelena = new(Color.FromArgb(0xFF, 0x00, 0xCC, 0x6A)); // #FF00CC6A
    SolidColorBrush barvaCervena = new(Color.FromArgb(0xFF, 0xE8, 0x11, 0x23)); // #FFE81123

    // inicializace konstant a proměnných
    public String nazevAplikace = App.NAZEV;
    public String podTitul = App.PODTITUL;
    public String autor = App.AUTOR;
    public String email = App.GITHUB;
    public String verzeAplikace = App.VERZE;
    public String aktualniRok = DateTime.Now.Year.ToString();

    public delegate void NoArgDelegate();
    public bool status = false;
    public string bd;
    public string port;
    string[] dostupnePorty1;

    public SerialPort prvniSP = new SerialPort();
    System.Windows.Threading.DispatcherTimer casovac1 = new System.Windows.Threading.DispatcherTimer();


    public MainWindow()
    {
        InitializeComponent();

        this.Title = nazevAplikace;

        casovac1.Interval = new TimeSpan(0, 0, 0, 2, 500);
        casovac1.Tick += new EventHandler(casovac1_Tick);
        casovac1.Start();

        string[] baudy = { "300", "1200", "2400", "4800", "9600", "19200", "23040", "28800", "38400", "57600", "74880", "115200", "230400", "250000" };
        foreach (string rychlost in baudy)
        {
            comboBox_BuadRate.Items.Add(rychlost);
        }
        dostupnePorty1 = SerialPort.GetPortNames();
        foreach (string port in dostupnePorty1)
        {
            comboBox_Port.Items.Add(port);
        }
        comboBox_Port.SelectedIndex = -1;

    }

    // aktualizace seznamu dostupných portů
    private void casovac1_Tick(object sender, EventArgs e)
    {
        string[] dostupnePorty = SerialPort.GetPortNames();
        if (!dostupnePorty.SequenceEqual(dostupnePorty1))
        {
            comboBox_Port.Items.Clear();
            foreach (string port in dostupnePorty)
            {
                comboBox_Port.Items.Add(port);
            }
            dostupnePorty1 = dostupnePorty;

            // DEBUG
            //textBox_Out.AppendText("Změna portů\n");
        }

    }

    public void PrichoziData(object sender, SerialDataReceivedEventArgs e)
    {
        base.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.
            Send, (NoArgDelegate)delegate
            {
                SerialPort sp = (SerialPort)sender;
                string zprava = sp.ReadExisting();

                //string detail = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\n";

                if (!MenuCheckboxCRLFPrijate.IsChecked)
                {
                    textBox_In.AppendText(/*detail + */zprava);
                }
                else if (MenuCheckboxCRLFPrijate.IsChecked)
                {

                    textBox_In.Text += (/*detail + */zprava + "\n");
                }
                //textBox_In.Focus();
                textBox_In.CaretIndex = textBox_In.Text.Length;
                textBox_In.ScrollToEnd();
                //textBox_Vstup.Focus();
            }
        );
    }


    private void button_StartStop_Click(object sender, RoutedEventArgs e)
    {
        if (status == false)
        {
            if (comboBox_Port.SelectedItem != null)
            {

                bd = comboBox_BuadRate.SelectedItem.ToString();
                port = comboBox_Port.SelectedItem.ToString();

                prvniSP.BaudRate = Int32.Parse(bd);
                prvniSP.PortName = port;
                prvniSP.Parity = Parity.None;
                prvniSP.StopBits = StopBits.One;
                prvniSP.DataBits = 8;
                prvniSP.Handshake = Handshake.None;
                prvniSP.RtsEnable = true;
                prvniSP.WriteTimeout = 3000;

                prvniSP.DataReceived += new SerialDataReceivedEventHandler(PrichoziData);

                try
                {
                    prvniSP.Open();

                    button_StartStop.Content = "Stop";
                    rectangle_Status.Fill = barvaZelena;
                    comboBox_BuadRate.IsEnabled = false;
                    comboBox_Port.IsEnabled = false;
                    casovac1.Stop();

                    textBox_In.IsEnabled = true;
                    textBox_Out.IsEnabled = true;
                    textBox_Vstup.IsEnabled = true;
                    button_Odeslat.IsEnabled = true;
                    rectangle_Status.ToolTip = "Port otevřen";
                    textBox_Vstup.Focus();

                    status = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            else
            {
                MessageBox.Show("Zvolte číslo portu.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        else if (status == true)
        {
            prvniSP.Close();

            button_StartStop.Content = "Start";
            rectangle_Status.Fill = barvaCervena;
            comboBox_BuadRate.IsEnabled = true;
            comboBox_Port.IsEnabled = true;
            casovac1.Start();

            //textBox_In.IsEnabled = false;
            //textBox_Out.IsEnabled = false;
            textBox_Vstup.IsEnabled = false;
            button_Odeslat.IsEnabled = false;
            rectangle_Status.ToolTip = "Port není otevřen";

            status = false;
        }
    }

    private void button_Odeslat_Click(object sender, RoutedEventArgs e)
    {
        if ((textBox_Vstup.Text != "") && (status == true))
        {
            string vstup = textBox_Vstup.Text;
            if (MenuCheckboxCR.IsChecked)
            {
                vstup += "\r";
            }
            if (MenuCheckboxLF.IsChecked)
            {
                vstup += "\n";
            }
            try
            {
                prvniSP.Write(vstup);
                //string detail = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\n";
                textBox_Vstup.Clear();
                textBox_Out.Foreground = barvaModra2;

                textBox_Out.AppendText(/*detail + */vstup/* + "\n"*/);

                textBox_Out.Focus();
                textBox_Out.CaretIndex = textBox_Out.Text.Length;
                textBox_Out.ScrollToEnd();
            }
            catch (Exception ex)
            {
                string detail = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " VÝJIMKA:\n";
                textBox_Out.Foreground = barvaCervena;
                textBox_Out.AppendText(detail + ex.Message + "\n\n");

                if (ex is InvalidOperationException)
                {
                    button_StartStop_Click(ex, e);
                }
            }

            textBox_Vstup.Focus();
        }
    }

    // MAIN MENU

    // MENU - Soubor - Export do *.txt
    private void export_Click(object sender, RoutedEventArgs e)
    {
        if ((textBox_In.Text != "") || (textBox_Out.Text != ""))
        {
            try
            {
                String podtrzeni = new String('=', 35);
                String cas = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                String exportString = podtrzeni + Environment.NewLine;
                exportString += "   " + nazevAplikace.Substring(0, 3) + nazevAplikace.Substring(3).ToUpper() + " v" + verzeAplikace.Substring(0, 5) + "   |   " + cas.Substring(0, 10) + Environment.NewLine;
                exportString += "    " + "Log export    |    " + cas.Substring(11) + Environment.NewLine;
                exportString += podtrzeni + Environment.NewLine;
                exportString += Environment.NewLine + Environment.NewLine + "PŘIJATÉ:" + Environment.NewLine + "--------" + Environment.NewLine;
                exportString += textBox_In.Text;
                exportString += Environment.NewLine + Environment.NewLine + "ODESLANÉ:" + Environment.NewLine + "---------" + Environment.NewLine;
                exportString += textBox_Out.Text;

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Textový soubor (*.txt)|*.txt|Bez přípony|*.*";
                saveFileDialog.Title = "Export";
                saveFileDialog.FileName = nazevAplikace + "_LOG001";
                if (saveFileDialog.ShowDialog() == true)
                    File.WriteAllText(saveFileDialog.FileName, exportString);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                                ex.Message,
                                "Chyba",
                                MessageBoxButton.OK,
                                MessageBoxImage.Exclamation);
            }
        }
        else
        {
            MessageBox.Show("Není co uložit.\nPřijměte či odešlete nějaká data.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // MENU - Soubor - Ukončit
    private void exit_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show("Přejete si aplikaci ukončit?", "Konec", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
        switch (result)
        {
            case MessageBoxResult.OK:
                if (status == true)
                {
                    prvniSP.Close();
                }
                Application.Current.Shutdown();
                break;
        }
    }

    // MENU - Nástroje
    private void smazatPrijate_Click(object sender, RoutedEventArgs e)
    {
        textBox_In.Clear();
    }
    private void smazatOdeslane_Click(object sender, RoutedEventArgs e)
    {
        textBox_Out.Clear();
    }

    // MENU - Nápověda
    private void about_Click(object sender, RoutedEventArgs e)
    {
        WindowOAplikaci oAplikaci = new WindowOAplikaci();
        oAplikaci.Owner = Application.Current.MainWindow;
        oAplikaci.ShowDialog();
    }

    private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key.ToString() == "F5")
        {
            button_StartStop_Click(sender, e);
        }
    }
}
