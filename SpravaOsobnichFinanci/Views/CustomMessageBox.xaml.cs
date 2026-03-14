using System;
using System.Windows;
using System.Windows.Controls;

namespace SpravaOsobnichFinanci.Views
{
    /// <summary>
    /// Interakční logika pro CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        public bool Result { get; private set; } = false;

        public CustomMessageBox(string message, string title, bool isWarning = false)
        {
            InitializeComponent();
            MessageText.Text = message;
            TitleText.Text = title;

            if (isWarning)
            {
                // Zobrazit jen OK, skrýt Ano/Ne
                BtnOk.Visibility = Visibility.Visible;
                BtnYes.Visibility = Visibility.Collapsed;
                BtnNo.Visibility = Visibility.Collapsed;
                
                // Změna barvy hlavičky pro varování
                TitleText.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F57C00"));
            }
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            DialogResult = true;
            Close();
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            DialogResult = false;
            Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            DialogResult = true;
            Close();
        }

        // Statická metoda pro otázku (Ano/Ne)
        public static bool Show(string message, string title, Window? owner)
        {
            var msgBox = new CustomMessageBox(message, title, false);
            
            // Bezpečné přiřazení vlastníka
            if (owner != null)
            {
                msgBox.Owner = owner;
                msgBox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else if (Application.Current != null && Application.Current.MainWindow != null)
            {
                // Když jsme z ViewModelu poslali null, zkusí si najít hlavní okno sám přímo zde (mimo pohled ViewModelu)
                msgBox.Owner = Application.Current.MainWindow!;
                msgBox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                // Pokud vůbec běžíme např v Unit Testu, vycentrujeme okno na střed nezávisle na existenci oken aplikací.
                msgBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            msgBox.ShowDialog();
            return msgBox.Result;
        }

        // Statická metoda pro varování (pouze OK)
        public static void ShowWarning(string message, string title, Window? owner)
        {
            var msgBox = new CustomMessageBox(message, title, true);
            
            // Bezpečné přiřazení vlastníka
            if (owner != null)
            {
                msgBox.Owner = owner;
                msgBox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else if (Application.Current != null && Application.Current.MainWindow != null)
            {
                msgBox.Owner = Application.Current.MainWindow!;
                msgBox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                msgBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            msgBox.ShowDialog();
        }
    }
}