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

    private SolidColorBrush colorWhite = new(Color.FromArgb(0xFF, 0xF2, 0xFF, 0xFF)); // #FFF2FFFF
    private SolidColorBrush colorBlue1 = new(Color.FromArgb(0xFF, 0x92, 0xCA, 0xF4)); // #FF92CAF4
    private SolidColorBrush colorBlue2 = new(Color.FromArgb(0xFF, 0x56, 0x9C, 0xD6)); // #FF569CD6
    private SolidColorBrush colorGreen = new(Color.FromArgb(0xFF, 0x00, 0xCC, 0x6A)); // #FF00CC6A
    private SolidColorBrush colorRed = new(Color.FromArgb(0xFF, 0xE8, 0x11, 0x23)); // #FFE81123
    private double textBoxes_defaultFontSize;

    // initialization of constants and variables
    private readonly string[] baudRates = { "300", "1200", "2400", "4800", "9600", "19200", "23040", "28800", "38400", "57600", "74880", "115200", "230400", "250000" };
    private const int defaultBaudRate = 11;  // 115200
    
    private delegate void NoArgDelegate();
    private bool status = false;
    private string port;
    private string[] availablePorts;

    private SerialPort sPort = new SerialPort();
    private int sPortDataBits = 8;
    private StopBits sPortStopBits = StopBits.One;
    private Parity sPortParity = Parity.None;
    private Handshake sPortHandshake = Handshake.None;
    private System.Windows.Threading.DispatcherTimer timerCheckPorts = new System.Windows.Threading.DispatcherTimer();


    public MainWindow()
    {
        colorWhite.Freeze(); colorBlue1.Freeze(); colorBlue2.Freeze(); colorGreen.Freeze(); colorRed.Freeze();
        
        InitializeComponent();

        textBoxes_defaultFontSize = textBox_Rx.FontSize;

        // registration of functions for unexpected exit
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

        this.Title = App.TITLE;

        timerCheckPorts.Interval = new TimeSpan(0, 0, 0, 5, 000);
        timerCheckPorts.Tick += new EventHandler(timerCheckPorts_Tick);
        timerCheckPorts.Start();

        foreach (string rychlost in baudRates)
        {
            comboBox_BaudRate.Items.Add(rychlost);
        }
        comboBox_BaudRate.SelectedIndex = defaultBaudRate;
        availablePorts = SerialPort.GetPortNames();
        foreach (string port in availablePorts)
        {
            comboBox_Port.Items.Add(port);
        }
        comboBox_Port.SelectedIndex = -1;

    }

    // the list of available ports update
    private void timerCheckPorts_Tick(object sender, EventArgs e)
    {
        string[] actualPorts = SerialPort.GetPortNames();
        if (!actualPorts.SequenceEqual(availablePorts))
        {
            comboBox_Port.Items.Clear();
            foreach (string port in actualPorts)
            {
                comboBox_Port.Items.Add(port);
            }
            availablePorts = actualPorts;

            // DEBUG
            //textBox_Tx.AppendText("> Ports changed\n");
        }

    }

    public void incomingData(object sender, SerialDataReceivedEventArgs e)
    {
        base.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.
            Send, (NoArgDelegate)delegate
            {
                SerialPort sp = (SerialPort)sender;
                string msg = sp.ReadExisting();

                //string detail = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n";

                if (!MenuCheckboxWrapRx.IsChecked)
                {
                    textBox_Rx.AppendText(/*detail + */msg);
                }
                else if (MenuCheckboxWrapRx.IsChecked)
                {

                    textBox_Rx.AppendText(/*detail + */msg.TrimEnd('\r', '\n') + Environment.NewLine);
                }
                //textBox_Rx.Focus();
                textBox_Rx.CaretIndex = textBox_Rx.Text.Length;
                textBox_Rx.ScrollToEnd();
                //textBox_Input.Focus();
            }
        );
    }


    // MainWindow GUI widgets
    private void comboBox_BaudRate_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        sPort.BaudRate = Int32.Parse(comboBox_BaudRate.SelectedItem.ToString());
    }


    private void button_StartStop_Click(object sender, RoutedEventArgs e)
    {
        if (status == false)
        {
            if (comboBox_Port.SelectedItem != null)
            {
                port = comboBox_Port.SelectedItem.ToString();

                sPort.BaudRate = Int32.Parse(comboBox_BaudRate.SelectedItem.ToString());
                sPort.PortName = port;
                // 8-N-1
                sPort.DataBits = sPortDataBits;
                sPort.StopBits = sPortStopBits;
                sPort.Parity = sPortParity;
                sPort.Handshake = sPortHandshake;
                sPort.ReadTimeout = 500;
                sPort.WriteTimeout = 500;
                sPort.Encoding = System.Text.Encoding.UTF8;

                sPort.DataReceived += new SerialDataReceivedEventHandler(incomingData);

                try
                {
                    sPort.Open();

                    button_StartStop.Content = Strings.ButtonStartStopStop;
                    rectangle_Status.Fill = colorGreen;
                    comboBox_Port.IsEnabled = false;
                    timerCheckPorts.Stop();

                    textBox_Rx.IsEnabled = true;
                    textBox_Tx.IsEnabled = true;
                    textBox_Input.IsEnabled = true;
                    button_Send.IsEnabled = true;
                    rectangle_Status.ToolTip = Strings.StatusOpen;
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
            closePort();

            button_StartStop.Content = Strings.ButtonStartStopStart;
            rectangle_Status.Fill = colorRed;
            comboBox_Port.IsEnabled = true;
            timerCheckPorts.Start();

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
            string input = textBox_Input.Text;

            if (MenuCheckboxWrapCRTx.IsChecked)
            {
                input += "\r";
            }
            if (MenuCheckboxWrapLFTx.IsChecked)
            {
                input += "\n";
            }

            if (input != "")
            {
                try
                {
                    sPort.Write(input);
                    //string detail = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\n";
                    textBox_Input.Clear();
                    textBox_Tx.Foreground = colorBlue2;

                    textBox_Tx.AppendText(/*detail + */input/* + "\n"*/);

                    textBox_Tx.Focus();
                    textBox_Tx.CaretIndex = textBox_Tx.Text.Length;
                    textBox_Tx.ScrollToEnd();
                }
                catch (Exception ex)
                {
                    string detail = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + Strings.Exception.ToUpper() + ":\n";
                    textBox_Tx.Foreground = colorRed;
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

    // MENU - File - Export to *.txt
    private void export_Click(object sender, RoutedEventArgs e)
    {
        if ((textBox_Rx.Text != "") || (textBox_Tx.Text != ""))
        {
            try
            {
                const String border = "####################################";
                String actualTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                String exportString = "## " + Strings.LogCreated + ": " + actualTime + Environment.NewLine;
                exportString += border + Environment.NewLine + "## " + Strings.Received.ToUpper() + " (Rx):" + Environment.NewLine + border + Environment.NewLine;
                exportString += textBox_Rx.Text;
                exportString += Environment.NewLine + border + Environment.NewLine + "## " + Strings.Sent.ToUpper() + " (Tx):" + Environment.NewLine + border + Environment.NewLine;
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

    // MENU - File - Quit
    private void exit_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(Strings.DialogQuit, Strings.Quit, MessageBoxButton.OKCancel, MessageBoxImage.Warning);
        switch (result)
        {
            case MessageBoxResult.OK:
                closePort();
                Application.Current.Shutdown();
                break;
        }
    }

    // MENU - Tools

    private void copyRx_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(textBox_Rx.Text);
    }

    private void copyTx_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(textBox_Tx.Text);
    }

    private void eraseReceived_Click(object sender, RoutedEventArgs e)
    {
        textBox_Rx.Clear();
    }
    private void eraseSent_Click(object sender, RoutedEventArgs e)
    {
        textBox_Tx.Clear();
    }

    // MENU - Port
    private void dtrCheckBox_Click(object sender, RoutedEventArgs e)
    {
        if (MenuCheckboxDTR.IsChecked)
        {
            sPort.DtrEnable = true;
        }
        else {
            sPort.DtrEnable = false;
        }
    }
    
    private void rtsCheckBox_Click(object sender, RoutedEventArgs e)
    {
        if (MenuCheckboxRTS.IsChecked)
        {
            sPort.RtsEnable = true;
        }
        else
        {
            sPort.RtsEnable = false;
        }
    }

    private void buttonDataBitsUp_Click(object sender, RoutedEventArgs e)
    {
        int dataBits = int.Parse(MenuLabelDataBits.Content.ToString());

        if (dataBits < 8)
        {
            dataBits++;
            sPortDataBits = dataBits;
            sPort.DataBits = sPortDataBits;
            MenuLabelDataBits.Content = sPortDataBits;
        }
    }

    private void buttonDataBitsDown_Click(object sender, RoutedEventArgs e)
    {
        int dataBits = int.Parse(MenuLabelDataBits.Content.ToString());

        if (dataBits > 5)
        {
            dataBits--;
            sPortDataBits = dataBits;
            sPort.DataBits = sPortDataBits;
            MenuLabelDataBits.Content = sPortDataBits;
        }
    }

    private void buttonStopBitUp_Click(object sender, RoutedEventArgs e)
    {
        string stopBit = MenuLabelStopBit.Content.ToString();

        switch (stopBit)
        {
            case "1":
                sPortStopBits = StopBits.OnePointFive;
                sPort.StopBits = sPortStopBits;
                MenuLabelStopBit.Content = "1.5";
                break;
            case "1.5":
                sPortStopBits = StopBits.Two;
                sPort.StopBits = sPortStopBits;
                MenuLabelStopBit.Content = "2";
                break;
            default:
                break;
        }
    }

    private void buttonStopBitDown_Click(object sender, RoutedEventArgs e)
    {
        string stopBit = MenuLabelStopBit.Content.ToString();

        switch (stopBit)
        {
            case "2":
                sPortStopBits = StopBits.OnePointFive;
                sPort.StopBits = sPortStopBits;
                MenuLabelStopBit.Content = "1.5";
                break;
            case "1.5":
                sPortStopBits = StopBits.One;
                sPort.StopBits = sPortStopBits;
                MenuLabelStopBit.Content = "1";
                break;
            default:
                break;
        }
    }

    private void buttonParityUp_Click(object sender, RoutedEventArgs e)
    {
        string parity = MenuLabelParity.Content.ToString();
        string[] parityValues = { "None", Strings.ParityOdd, Strings.ParityEven, "Mark", "Space" };

        int index = -1;
        if ((index = parityValues.IndexOf(parity)) > -1) {
            index = (index + 1 >= parityValues.Length) ? index : index + 1;
            sPortParity = (Parity)index;
            sPort.Parity = sPortParity;
            MenuLabelParity.Content = parityValues[index];
        }
    }

    private void buttonParityDown_Click(object sender, RoutedEventArgs e)
    {
        string parity = MenuLabelParity.Content.ToString();
        string[] parityValues = { "None", Strings.ParityOdd, Strings.ParityEven, "Mark", "Space" };

        int index = -1;
        if ((index = parityValues.IndexOf(parity)) > -1)
        {
            index = (index - 1 < 0) ? index : index - 1;
            sPortParity = (Parity)index;
            sPort.Parity = sPortParity;
            MenuLabelParity.Content = parityValues[index];
        }
    }

    // ButtonHandshake
    private void buttonHandshakeUp_Click(object sender, RoutedEventArgs e)
    {
        string handshake = MenuLabelHandshake.Content.ToString();
        string[] handshakeValues = { "None", "XON/XOFF", "RTS/CTS", "RTS/CTS & XON/XOFF" };

        int index = -1;
        if ((index = handshakeValues.IndexOf(handshake)) > -1)
        {
            index = (index + 1 >= handshakeValues.Length) ? index : index + 1;
            sPortHandshake = (Handshake)index;
            sPort.Handshake = sPortHandshake;
            MenuLabelHandshake.Content = handshakeValues[index];
        }
    }

    private void buttonHandshakeDown_Click(object sender, RoutedEventArgs e)
    {
        string handshake = MenuLabelHandshake.Content.ToString();
        string[] handshakeValues = { "None", "XON/XOFF", "RTS/CTS", "RTS/CTS & XON/XOFF" };

        int index = -1;
        if ((index = handshakeValues.IndexOf(handshake)) > -1)
        {
            index = (index - 1 < 0) ? index : index - 1;
            sPortHandshake = (Handshake)index;
            sPort.Handshake = sPortHandshake;
            MenuLabelHandshake.Content = handshakeValues[index];
        }
    }

    // MENU - Window

    private void MenuCheckboxOnTop_Click(object sender, RoutedEventArgs e)
    {
        if (MenuCheckboxOnTop.IsChecked)
        {
            this.Topmost = true;
        }
        else
        {
            this.Topmost = false;
        }
    }

    // zoom in text in textBoxes
    private void MenuZoomIn_Click(object sender, RoutedEventArgs e)
    {
        const double shift = 1.0;
        textBox_Rx.FontSize += shift;
        textBox_Tx.FontSize += shift;
        textBox_Input.FontSize += shift;
    }

    // zoom out text in textBoxes
    private void MenuZoomOut_Click(object sender, RoutedEventArgs e)
    {
        const double shift = 1.0;
        textBox_Rx.FontSize -= (textBox_Rx.FontSize - shift > 0) ? shift : 0;
        textBox_Tx.FontSize -= (textBox_Tx.FontSize - shift > 0) ? shift : 0;
        textBox_Input.FontSize -= (textBox_Input.FontSize - shift > 0) ? shift : 0;
    }

    // reset text zoom in textBoxes
    private void MenuZoomReset_Click(object sender, RoutedEventArgs e)
    {
        textBox_Rx.FontSize = textBoxes_defaultFontSize;
        textBox_Tx.FontSize = textBoxes_defaultFontSize;
        textBox_Input.FontSize = textBoxes_defaultFontSize;
    }

    // MENU - Help
    private void about_Click(object sender, RoutedEventArgs e)
    {
        WindowAbout winAbout = new WindowAbout();
        winAbout.Owner = Application.Current.MainWindow;
        winAbout.ShowDialog();
    }




    private void window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            button_StartStop_Click(sender, e);
        }
    }

    private void closePort()
    {
        if (sPort != null)  {
            try
            {
                sPort.DataReceived -= incomingData;
                if (sPort.IsOpen)
                {
                    sPort.DiscardInBuffer();
                    sPort.DiscardOutBuffer();
                    sPort.Close();
                }
            }
            finally {
                sPort.Dispose();
            }
        }
    }

    // EXIT

    // user pushed standard quit or Alt+F4
    protected override void OnClosed(EventArgs e)
    {
        closePort();
        base.OnClosed(e);
    }

    // standard app exit
    private void CurrentDomain_ProcessExit(object sender, EventArgs e)
    {
        closePort();
    }

    // a critical error (forced exit)
    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        closePort();
    }
}
