using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;

namespace SiCode.IDE.OptionsPages
{
    /// <summary>
    /// Logique d'interaction pour AboutPage.xaml
    /// </summary>
    public partial class AboutPage : Page
    {
        public AboutPage()
        {
            InitializeComponent();
            this.label2.Content = $"Version {Assembly.GetExecutingAssembly().GetName().Version}";
        }
        private void label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Wpf.Ui.Controls.MessageBox m = new();
            m.Title = "Hello 👋";
            m.Content = "You just found one of SiCode IDE's secrets! In the old versions of WPF SiCode IDE, it was used to do a exception.";
            m.ShowDialogAsync();

        }
    }
}
