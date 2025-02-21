using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Serio
{
    /// <summary>
    /// Interaction logic for WindowOAplikaci.xaml
    /// </summary>
    public partial class WindowOAplikaci : Window
    {
        public WindowOAplikaci()
        {
            InitializeComponent();

            // nastavení popisků okna OAplikaci
            titleNazev.Text = "SerIO";
            titleVerze.Text += App.VERZE;
            titleArch.Text += App.DOTNET_INFO + " (" + App.ARCH_INFO + ")";
            imgGithub.ToolTip += " – " + App.GITHUB;
            titleLicence.Text += App.LICENCE;
            titleAutor.Text += String.Format("–{0}  {1}", DateTime.Now.Year, App.AUTOR);

            // nastavení časovače pro EasterEgg
            casovacEgg.Interval = new TimeSpan(0, 0, 0, 0, 350); // interval blikání [ms]
            casovacEgg.Tick += new EventHandler(casovacEgg_Tick);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // zavře okno po stisknutí Esc
            if (e.Key.ToString() == "Escape")
                this.Close();
        }


        // EasterEgg pro ikonu loga

        System.Windows.Threading.DispatcherTimer casovacEgg = new();
        Boolean eggActive = false;

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!eggActive)
            {
                eggActive = true;
                casovacEgg.Start();
            }
            else
            {
                eggActive = false;
                casovacEgg.Stop();
                logoImage.Source = ikonaPuvodni;
                eggZmena = false;
            }

        }


        ImageSource ikonaCb = new BitmapImage(new Uri("ikona_cb_128px.png", UriKind.Relative));
        ImageSource ikonaPuvodni = new BitmapImage(new Uri("ikona_128px.png", UriKind.Relative));

        Boolean eggZmena = false;

        private void casovacEgg_Tick(object sender, EventArgs e)
        {
            if (!eggZmena)
            {
                logoImage.Source = ikonaCb;
                eggZmena = true;
            }
            else
            {
                logoImage.Source = ikonaPuvodni;
                eggZmena = false;
            }

        }

        // </EasterEgg>
    }
}
