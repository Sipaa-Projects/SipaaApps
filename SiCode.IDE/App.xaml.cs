using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SiCode.IDE
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (IsProcessRunning("MicaForEveryone"))
                MessageBox.Show(
                    "SiCode IDE detected MicaForEveryone is running on your PC. " +
                    "Due to a bug of WPF showing a big rectangle when these software is running, " +
                    "please make an exception for process 'SiCode.IDE' in MicaForEveryone's settings.");
            base.OnStartup(e);
        }

        static bool IsProcessRunning(string processName)
        {
            // Get all running processes with the specified name
            Process[] processes = Process.GetProcessesByName(processName);

            // Check if any processes were found
            return processes.Length > 0;
        }
    }
}
