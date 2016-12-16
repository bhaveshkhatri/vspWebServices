using System.Configuration;

namespace VspWS.Plugins
{
    public static class ConnectionStringProvider
    {
        public static string GetConnectionStringFromConfig(string connectionStringName)
        {
            var configFileName = string.Format("{0}.dll.config", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
            var connectionStringSettings = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap { ExeConfigFilename = configFileName }, ConfigurationUserLevel.None).ConnectionStrings.ConnectionStrings[connectionStringName];

            return connectionStringSettings.ConnectionString;
        }
    }
}
