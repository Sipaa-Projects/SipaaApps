using Browse.SettingsPages;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Browse
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public TabViewer ParentTabViewer { get; set; } = null;

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            NavigationViewItem selectedItem = (NavigationViewItem)args.SelectedItem;
            if (!String.IsNullOrEmpty((string)selectedItem.Tag))
                if (((string)selectedItem.Tag) == "AboutPage")
                {
                    ContentFrame.Navigate(typeof(AboutPage), null);
                }

            sender.Header = selectedItem.Content;
            Debug.WriteLine("settings: Selection changed!");

        }
    }
}
