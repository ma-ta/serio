using System.Windows;

using System.Runtime.InteropServices;  // pro RuntimeInformation (info o architektuře)


namespace Serio;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // programové konstanty
    public static readonly string TITLE = "Serio";
    public static readonly string SUBTITLE = Strings.AppSubtitle;
    public static readonly string AUTHOR = "Ma-TA";
    public static readonly string LICENSE = "GNU GPLv3";
    public static readonly string LICENSE_LONG = "GNU General Public License";
    public static readonly string VERSION = "1.2.0  |  2026-06-14";
    public static readonly string GITHUB = "github.com/ma-ta/serio";
    public static readonly string DOTNET_INFO = RuntimeInformation.FrameworkDescription;
    public static readonly string ARCH_INFO = RuntimeInformation.ProcessArchitecture.ToString().ToLower();
}
