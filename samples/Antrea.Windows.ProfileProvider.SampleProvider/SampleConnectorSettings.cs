using System.Collections.Generic;
using Antrea.Windows.ProfileProvider;

namespace SampleConnector
{
    public class SampleConnectorSettings : IProfileProviderSettings
    {
        public const string ProviderTypeName = "sample";
        private static string _fileName = "sample.json";

        private string _settingsDirectory;

        public SampleConnectorSettings()
        {
        }

        public bool PersistStore { get; set; }

        public void SaveSettings()
        {
            // save settings here
        }

        public void LoadSettings(string settingsDirectory = null)
        {
            _settingsDirectory = settingsDirectory;

            // load settings here
        }
    }
}
