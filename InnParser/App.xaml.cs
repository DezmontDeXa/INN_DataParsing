using OfficeOpenXml;
using System;
using System.Windows;

namespace InnParser
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            //FreelancerProtectionLib.FreelancerProtection.ActivationDate = new DateTime(2022, 08, 25);
            //FreelancerProtectionLib.FreelancerProtection.Duration = new TimeSpan(3, 0, 0, 0);
            //if (!FreelancerProtectionLib.FreelancerProtection.CheckActivation())
            //{
            //    MessageBox.Show("End of DEMO.");
            //    Application.Current.Shutdown();
            //}
        }
    }
}
