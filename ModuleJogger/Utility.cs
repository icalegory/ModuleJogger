using System.Configuration;

namespace ModuleJogger
{
    /// <summary>
    /// This class facilitates the migration of User settings stored in user.config
    /// after program updates.  It is taken directly from this page:
    /// http://blog.johnsworkshop.net/automatically-upgrading-user-settings-after-an-application-version-change/
    /// In order to make the entire auto-upgrade work, all you have to do is define a bool user-
    /// scoped setting called UpgradeRequired and set it’s default value to True.  Then, call the
    /// DoUpgrade upon application startup as follows:
    /// Utility.DoUpgrade(Properties.Settings.Default);
    /// </summary>
    public static class Utility
    {
        // the name of the setting that flags whether we
        // should perform an upgrade or not
        private const string UpgradeFlag = "UpgradeRequired";

        public static void DoUpgrade(ApplicationSettingsBase settings)
        {
            try
            {
                // attempt to read the upgrade flag
                if ((bool)settings[UpgradeFlag] == true)
                {
                    // upgrade the settings to the latest version
                    settings.Upgrade();

                    // clear the upgrade flag
                    settings[UpgradeFlag] = false;
                    settings.Save();
                }
                else
                {
                    // the settings are up to date
                }
            }
            catch (SettingsPropertyNotFoundException ex)
            {
                // notify the developer that the upgrade
                // flag should be added to the settings file
                throw ex;
            }
        }
    }
}
