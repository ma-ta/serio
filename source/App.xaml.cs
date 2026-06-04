using System.Windows;

using System.Runtime.InteropServices;  // pro RuntimeInformation (info o architektuře)


namespace Serio;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // programové konstanty
    public static readonly string NAZEV = "Serio";
    public static readonly string PODTITUL = "Sériový terminál";
    public static readonly string AUTOR = "Martin TÁBOR";
    public static readonly string LICENCE = "GNU GPLv3";
    public static readonly string VERZE = "1.1.0  |  2026-06-04";
    public static readonly string GITHUB = "github.com/ma-ta/serio";
    public static readonly string DOTNET_INFO = RuntimeInformation.FrameworkDescription;
    public static readonly string ARCH_INFO = RuntimeInformation.ProcessArchitecture.ToString().ToLower();
}
