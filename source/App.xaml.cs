using System.Windows;

using System.Runtime.InteropServices;  // pro RuntimeInformation (info o architektuře)


namespace Serio;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // programové konstanty
    public static string NAZEV = "Serio";
    public static string PODTITUL = "Sériový terminál";
    public static string AUTOR = "Martin TÁBOR";
    public static string LICENCE = "GNU GPLv3+";
    public static string VERZE = "1.0.0  |  2025-04-08";
    public static string GITHUB = "github.com/ma-ta/serio";
    public static string DOTNET_INFO = RuntimeInformation.FrameworkDescription;
    public static string ARCH_INFO = RuntimeInformation.ProcessArchitecture.ToString().ToLower();
}
