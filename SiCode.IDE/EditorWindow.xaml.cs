using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
