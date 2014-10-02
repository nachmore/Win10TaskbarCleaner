using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Win10TaskbarCleaner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string REG_APPNAME = "Win10TasbarCleaner";

        private bool _installed = false;
        RegistryKey _regRun = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        public MainWindow()
        {
            InitializeComponent();

            var args = Environment.GetCommandLineArgs();

            if (args.Count() > 1 && args[1].Contains("justrun"))
            {
                Hide();
            }
        }
        
        private void StartHiding()
        {
            WorkScheduler sched = new WorkScheduler();
            sched.Schedule(5000, TaskbarCleaner.HideButtons);

            Hide();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _installed = IsInstalled();

            UpdateButton();
        }

        private void UpdateButton()
        {
            btnInstall.Content = (_installed ? "Uninstall" : "Install & Close");
        }

        private bool IsInstalled()
        {
            return (_regRun.GetValue(REG_APPNAME) != null);
        }

        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            if (_installed)
                Uninstall();
            else
                InstallAndClose();
        }

        private void Uninstall()
        {
            _regRun.DeleteValue(REG_APPNAME);

            _installed = false;
            UpdateButton();
        }

        private void InstallAndClose()
        {
            _regRun.SetValue(REG_APPNAME, System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName + " /justrun");

            _installed = true;

            StartHiding();
        }

        private void btnCleanNow_Click(object sender, RoutedEventArgs e)
        {
            TaskbarCleaner.HideButtons();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
