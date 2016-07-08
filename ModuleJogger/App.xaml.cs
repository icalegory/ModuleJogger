using System.Configuration;
using System.Diagnostics;
using System.Windows;

namespace ModuleJogger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // The following should make sure that the User settings stored in user.config get
            // migrated to the new version of the program when it is upgraded.
            Utility.DoUpgrade(ModuleJogger.Properties.Settings.Default);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            ModuleJogger.Properties.Settings.Default.Save();

            //ModuleJogger.Properties.Settings.Default.Upgrade();
            //ModuleJogger.Properties.Settings.Default.Reload();

            //Debug.WriteLine(System.Windows.Forms.Application.LocalUserAppDataPath);
            // For some reason, the user.config file was not written until I changed a value to be something
            // other than the default:
            //ModuleJogger.Properties.Settings.Default["RootFolder"] = @"C:\Users\ianc4\Documents";
            /*
            try
            {
                var UserConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                string path = UserConfig.FilePath;
                Debug.WriteLine(UserConfig.FilePath);
                //UserConfig.Save(ConfigurationSaveMode.Modified);
                UserConfig.Save(ConfigurationSaveMode.Full);
                // Force a reload of the changed section, 
                // if needed. This makes the new values available 
                // for reading.
                //ConfigurationManager.RefreshSection(sectionName);
                ConfigurationManager.RefreshSection(path);
            }
            catch (ConfigurationException ex)
            { }
            */

        }

    }
}
