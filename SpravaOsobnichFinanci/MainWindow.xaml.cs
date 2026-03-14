using SpravaOsobnichFinanci.ViewModels;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpravaOsobnichFinanci;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Ošetřené načtení ikony (pokud by se soubor smazal nebo nenašel, aplikace nespadne)
        try
        {
            // Pomocí Uri s pack:// scheme zajistíme přesné vyhledávání podle cesty v sestavení
            this.Icon = BitmapFrame.Create(new Uri("pack://application:,,,/SpravaOsobnichFinanci;component/Assets/Icon/apk_icon.ico", UriKind.Absolute));
        }
        catch (Exception)
        {
            // Ikona nenalezena nebo je poškozena - WPF automaticky poskytne výchozí systémovou ikonku okna,
            // aplikace dále normálně funguje, výjimka je potlačena.
        }

        // Nastavení DataContextu na hlavní ViewModel aplikace
        DataContext = new MainViewModel();
    }
}