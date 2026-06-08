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

        this.Title = App.TITLE;

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
            //textBox_Tx.AppendText("Změna portů\n");
        }

    }

    public void PrichoziData(object sender, SerialDataReceivedEventArgs e)
    {
        base.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.
            Send, (NoArgDelegate)delegate
            {
                SerialPort sp = (SerialPort)sender;
                string zprava = sp.ReadExisting();

                //string detail = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n";

                if (!MenuCheckboxWrapRx.IsChecked)
                {
                    textBox_Rx.AppendText(/*detail + */zprava);
                }
                else if (MenuCheckboxWrapRx.IsChecked)
                {

                    textBox_Rx.AppendText(/*detail + */zprava.TrimEnd('\r', '\n') + Environment.NewLine);
                }
                //textBox_Rx.Focus();
                textBox_Rx.CaretIndex = textBox_Rx.Text.Length;
                textBox_Rx.ScrollToEnd();
                //textBox_Input.Focus();
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

                    button_StartStop.Content = Strings.ButtonStartStopStop;
                    rectangle_Status.Fill = barvaZelena;
                    comboBox_Port.IsEnabled = false;
                    casovac1.Stop();

                    textBox_Rx.IsEnabled = true;
                    textBox_Tx.IsEnabled = true;
                    textBox_Input.IsEnabled = true;
                    button_Send.IsEnabled = true;
                    rectangle_Status.ToolTip = Strings.StatusOpen + "\n• 8 data bits\n• No parity\n• 1 stop bit\n• No Handshake";
                    button_StartStop.ToolTip = Strings.ButtonStartStopToolTipStop;
                    textBox_Input.Focus();

                    status = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            else
            {
                MessageBox.Show(Strings.ChoosePort, Strings.Warning, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        else if (status == true)
        {
            zavriPort();

            button_StartStop.Content = Strings.ButtonStartStopStart;
            rectangle_Status.Fill = barvaCervena;
            comboBox_Port.IsEnabled = true;
            casovac1.Start();

            textBox_Input.IsEnabled = false;
            button_Send.IsEnabled = false;
            rectangle_Status.ToolTip = Strings.StatusClosed;
            button_StartStop.ToolTip = Strings.ButtonStartStopToolTipStart;

            status = false;
        }
    }

    private void button_Send_Click(object sender, RoutedEventArgs e)
    {
        if (status == true)
        {
            string vstup = textBox_Input.Text;

            if (MenuCheckboxWrapCRTx.IsChecked)
            {
                vstup += "\r";
            }
            if (MenuCheckboxWrapLFTx.IsChecked)
            {
                vstup += "\n";
            }

            if (vstup != "")
            {
                try
                {
                    prvniSP.Write(vstup);
                    //string detail = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\n";
                    textBox_Input.Clear();
                    textBox_Tx.Foreground = barvaModra2;

                    textBox_Tx.AppendText(/*detail + */vstup/* + "\n"*/);

                    textBox_Tx.Focus();
                    textBox_Tx.CaretIndex = textBox_Tx.Text.Length;
                    textBox_Tx.ScrollToEnd();
                }
                catch (Exception ex)
                {
                    string detail = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + Strings.Exception.ToUpper() + ":\n";
                    textBox_Tx.Foreground = barvaCervena;
                    textBox_Tx.AppendText(detail + ex.Message + "\n\n");

                    if (ex is InvalidOperationException)
                    {
                        button_StartStop_Click(ex, e);
                    }
                }
            }

            textBox_Input.Focus();
        }
    }

    // MAIN MENU

    // MENU - Soubor - Export do *.txt
    private void export_Click(object sender, RoutedEventArgs e)
    {
        if ((textBox_Rx.Text != "") || (textBox_Tx.Text != ""))
        {
            try
            {
                const String ohraniceni = "####################################";
                String cas = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                String exportString = "## " + Strings.LogCreated + ": " + cas + Environment.NewLine;
                exportString += ohraniceni + Environment.NewLine + "## " + Strings.Received.ToUpper() + " (Rx):" + Environment.NewLine + ohraniceni + Environment.NewLine;
                exportString += textBox_Rx.Text;
                exportString += Environment.NewLine + ohraniceni + Environment.NewLine + "## " + Strings.Sent.ToUpper() + " (Tx):" + Environment.NewLine + ohraniceni + Environment.NewLine;
                exportString += textBox_Tx.Text;

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = Strings.TextFile + " (*.txt)|*.txt|" + Strings.AllFiles + "|*.*";
                saveFileDialog.Title = Strings.Export;
                saveFileDialog.FileName = App.TITLE.ToLower() + "-" + Strings.Log;
                if (saveFileDialog.ShowDialog() == true)
                    File.WriteAllText(saveFileDialog.FileName, exportString);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                                ex.Message,
                                Strings.Error,
                                MessageBoxButton.OK,
                                MessageBoxImage.Exclamation);
            }
        }
        else
        {
            MessageBox.Show(Strings.DialogNothingToExport1 + "\n" + Strings.DialogNothingToExport2, Strings.Warning, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // MENU - Soubor - Ukončit
    private void exit_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(Strings.DialogQuit, Strings.Quit, MessageBoxButton.OKCancel, MessageBoxImage.Warning);
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
        textBox_Rx.Clear();
    }
    private void smazatOdeslane_Click(object sender, RoutedEventArgs e)
    {
        textBox_Tx.Clear();
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
        WindowAbout winAbout = new WindowAbout();
        winAbout.Owner = Application.Current.MainWindow;
        winAbout.ShowDialog();
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

    private void MenuCheckboxOnTop_Click(object sender, RoutedEventArgs e)
    {
        if (MenuCheckboxOnTop.IsChecked) {
            this.Topmost = true;
        }
        else {
            this.Topmost = false;
        }
    }
}
