using System.Windows;
using System.Windows.Media;

using System.IO.Ports;
using System.IO;
using Microsoft.Win32;
using System.Windows.Input;

namespace Serio;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    private SolidColorBrush barvaBila = new(Color.FromArgb(0xFF, 0xF2, 0xFF, 0xFF)); // #FFF2FFFF
    private SolidColorBrush barvaModra1 = new(Color.FromArgb(0xFF, 0x92, 0xCA, 0xF4)); // #FF92CAF4
    private SolidColorBrush barvaModra2 = new(Color.FromArgb(0xFF, 0x56, 0x9C, 0xD6)); // #FF569CD6
    private SolidColorBrush barvaZelena = new(Color.FromArgb(0xFF, 0x00, 0xCC, 0x6A)); // #FF00CC6A
    private SolidColorBrush barvaCervena = new(Color.FromArgb(0xFF, 0xE8, 0x11, 0x23)); // #FFE81123

    // inicializace konstant a proměnných
    private readonly string[] baudy = { "300", "1200", "2400", "4800", "9600", "19200", "23040", "28800", "38400", "57600", "74880", "115200", "230400", "250000" };
    private const int vychoziPolozka_comboBox_BaudRate = 11;  // 115200
    
    private readonly String nazevAplikace = App.NAZEV;
    private readonly String podTitul = App.PODTITUL;
    private readonly String autor = App.AUTOR;
    private readonly String email = App.GITHUB;
    private readonly String verzeAplikace = App.VERZE;
    private readonly String aktualniRok = DateTime.Now.Year.ToString();

    private delegate void NoArgDelegate();
    private bool status = false;
    private string port;
    private string[] dostupnePorty1;

    private SerialPort prvniSP = new SerialPort();
    private System.Windows.Threading.DispatcherTimer casovac1 = new System.Windows.Threading.DispatcherTimer();


    public MainWindow()
    {
        barvaBila.Freeze(); barvaModra1.Freeze(); barvaModra2.Freeze(); barvaZelena.Freeze(); barvaCervena.Freeze();
        
        InitializeComponent();
        
        // Registrace pro případ neočekávaného pádu aplikace
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        // Registrace pro případ náhlého ukončení procesu
        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

        this.Title = nazevAplikace;

        casovac1.Interval = new TimeSpan(0, 0, 0, 5, 000);
        casovac1.Tick += new EventHandler(casovac1_Tick);
        casovac1.Start();

        foreach (string rychlost in baudy)
        {
            comboBox_BaudRate.Items.Add(rychlost);
        }
        comboBox_BaudRate.SelectedIndex = vychoziPolozka_comboBox_BaudRate;
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

                    textBox_In.AppendText(/*detail + */zprava.TrimEnd('\r', '\n') + Environment.NewLine);
                }
                //textBox_In.Focus();
                textBox_In.CaretIndex = textBox_In.Text.Length;
                textBox_In.ScrollToEnd();
                //textBox_Vstup.Focus();
            }
        );
    }


    // OVLÁDACÍ PRVKY HLAVNÍHO OKNA
    private void comboBox_BaudRate_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        prvniSP.BaudRate = Int32.Parse(comboBox_BaudRate.SelectedItem.ToString());
    }


    private void button_StartStop_Click(object sender, RoutedEventArgs e)
    {
        if (status == false)
        {
            if (comboBox_Port.SelectedItem != null)
            {
                port = comboBox_Port.SelectedItem.ToString();

                prvniSP.BaudRate = Int32.Parse(comboBox_BaudRate.SelectedItem.ToString());
                prvniSP.PortName = port;
                // 8-N-1
                prvniSP.DataBits = 8;
                prvniSP.Parity = Parity.None;
                prvniSP.StopBits = StopBits.One;
                prvniSP.Handshake = Handshake.None;
                prvniSP.ReadTimeout = 500;
                prvniSP.WriteTimeout = 500;
                prvniSP.Encoding = System.Text.Encoding.UTF8;

                prvniSP.DataReceived += new SerialDataReceivedEventHandler(PrichoziData);

                try
                {
                    prvniSP.Open();

                    button_StartStop.Content = "S_top";
                    rectangle_Status.Fill = barvaZelena;
                    comboBox_Port.IsEnabled = false;
                    casovac1.Stop();

                    textBox_In.IsEnabled = true;
                    textBox_Out.IsEnabled = true;
                    textBox_Vstup.IsEnabled = true;
                    button_Odeslat.IsEnabled = true;
                    rectangle_Status.ToolTip = "Port OTEVŘEN\n• 8 data bits\n• No parity\n• 1 stop bit\n• No Handshake";
                    button_StartStop.ToolTip = "Zavře port (F5)";
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
            zavriPort();

            button_StartStop.Content = "S_tart";
            rectangle_Status.Fill = barvaCervena;
            comboBox_Port.IsEnabled = true;
            casovac1.Start();

            textBox_Vstup.IsEnabled = false;
            button_Odeslat.IsEnabled = false;
            rectangle_Status.ToolTip = "Port není otevřen";
            button_StartStop.ToolTip = "Otevře port (F5)";

            status = false;
        }
    }

    private void button_Odeslat_Click(object sender, RoutedEventArgs e)
    {
        if (status == true)
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

            if (vstup != "")
            {
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
                const String ohraniceni = "####################################";
                String cas = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                String exportString = "## Log vytvořen: " + cas + Environment.NewLine;
                exportString += ohraniceni + Environment.NewLine + "## PŘIJATO (Rx):" + Environment.NewLine + ohraniceni + Environment.NewLine;
                exportString += textBox_In.Text;
                exportString += Environment.NewLine + ohraniceni + Environment.NewLine + "## ODESLÁNO (Tx):" + Environment.NewLine + ohraniceni + Environment.NewLine;
                exportString += textBox_Out.Text;

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Textový soubor (*.txt)|*.txt|Bez přípony|*.*";
                saveFileDialog.Title = "Export";
                saveFileDialog.FileName = nazevAplikace.ToLower() + "-log";
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
                zavriPort();
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

    // MENU - Port
    private void dtrCheckBox_Click(object sender, RoutedEventArgs e)
    {
        if (MenuCheckboxDTR.IsChecked)
        {
            prvniSP.DtrEnable = true;
        }
        else {
            prvniSP.DtrEnable = false;
        }
    }
    
    private void rtsCheckBox_Click(object sender, RoutedEventArgs e)
    {
        if (MenuCheckboxRTS.IsChecked)
        {
            prvniSP.RtsEnable = true;
        }
        else
        {
            prvniSP.RtsEnable = false;
        }
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
        if (e.Key == Key.F5)
        {
            button_StartStop_Click(sender, e);
        }
    }

    private void zavriPort()
    {
        if (prvniSP != null)  {
            try
            {
                prvniSP.DataReceived -= PrichoziData;
                if (prvniSP.IsOpen)
                {
                    prvniSP.DiscardInBuffer();
                    prvniSP.DiscardOutBuffer();
                    prvniSP.Close();
                }
            }
            finally {
                prvniSP.Dispose();
            }
        }
    }

    // KONEC

    // Uživatel kliknul na křížek nebo Alt+F4
    protected override void OnClosed(EventArgs e)
    {
        zavriPort();
        base.OnClosed(e);
    }

    // Aplikace končí standardně jako proces
    private void CurrentDomain_ProcessExit(object sender, EventArgs e)
    {
        zavriPort();
    }

    // V aplikaci nastala kritická chyba (pád)
    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        zavriPort();
    }
}
