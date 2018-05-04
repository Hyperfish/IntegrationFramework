using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antrea.Windows.ProfileProvider;
using Newtonsoft.Json.Schema;

namespace SampleConnector
{
    class AudienceCollection : IAudienceCollection
    {
        public List<IAudience> Audiences { get; set; }

        public string[] Identifiers { get; set; }

        public string ResourceType { get; set; }
    }

    class Audience : IAudience
    {
        public Guid AudienceId { get; set; }

        public bool Master { get; set; }

        public string[] RawScopes { get; set; }

        public List<IAudienceScope> Scopes { get; set; }

        public JSchema Schema { get; set; }
    }

    class Scope : IAudienceScope
    {
        public string Name { get; set; }
    }

}
