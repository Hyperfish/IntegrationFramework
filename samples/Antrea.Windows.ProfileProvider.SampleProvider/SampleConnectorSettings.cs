using System.Collections.Generic;
using Antrea.Windows.ProfileProvider;

namespace SampleConnector
{
    public class SampleConnectorSettings: ProfileProviderSettings
    {
        public const string ProviderTypeName = "sample";

        private static string _fileName = "sample.json";

        public SampleConnectorSettings(Dictionary<string, object> providerSettings)
            :base(_fileName)
        {
            this.Settings = providerSettings;
        }

        public SampleConnectorSettings()
            :base( _fileName)
        {
        }
    }
}
