using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Serio
{
    /// <summary>
    /// Interaction logic for WindowAbout.xaml
    /// </summary>
    public partial class WindowAbout : Window
    {
        public WindowAbout()
        {
            InitializeComponent();

            // WindowAbout titles setup
            labelTitle.Text = "SerIO";
            labelVersion.Text += App.VERSION;
            labelArch.Text += App.DOTNET_INFO + " (" + App.ARCH_INFO + ")";
            labelLicense.Text += App.LICENSE;
            labelLicense.ToolTip = App.LICENSE_LONG;
            labelRepo.Text += App.GITHUB;
            windowAboutRepoLink.NavigateUri = new System.Uri("https://" + App.GITHUB);
            labelAuthor.Text += String.Format("–{0}  {1}", DateTime.Now.Year, App.AUTHOR);

            // easteregg timer setup
            timerEgg.Interval = new TimeSpan(0, 0, 0, 0, 350); // blinking interval [ms]
            timerEgg.Tick += new EventHandler(timerEgg_Tick);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // closing the WindowsAbout on Esc key
            if (e.Key == Key.Escape)
                this.Close();
        }


        // EasterEgg for the logo (app icon)

        System.Windows.Threading.DispatcherTimer timerEgg = new();
        Boolean eggActive = false;

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!eggActive)
            {
                eggActive = true;
                timerEgg.Start();
            }
            else
            {
                eggActive = false;
                timerEgg.Stop();
                logoImage.Source = icon;
                eggChange = false;
            }

        }


        ImageSource iconBW = new BitmapImage(new Uri("icon_bw_128px.png", UriKind.Relative));
        ImageSource icon = new BitmapImage(new Uri("icon_128px.png", UriKind.Relative));

        Boolean eggChange = false;

        private void timerEgg_Tick(object sender, EventArgs e)
        {
            if (!eggChange)
            {
                logoImage.Source = iconBW;
                eggChange = true;
            }
            else
            {
                logoImage.Source = icon;
                eggChange = false;
            }

        }

        private void windowAboutRepoLink_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(App.GITHUB);
        }

        // </EasterEgg>
    }
}
