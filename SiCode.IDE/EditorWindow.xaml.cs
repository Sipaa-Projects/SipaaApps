using System.Windows;

namespace SiCode.IDE
{
    /// <summary>
    /// Logique d'interaction pour EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : Wpf.Ui.Controls.FluentWindow
    {
        public EditorWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show(); this.Close();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            new SettingsWindow().Show();
        }
    }
}
