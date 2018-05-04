using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antrea.Windows.ProfileProvider;
using Antrea.Windows.ProfileProvider.Identifiers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SampleConnector
{
    public class SampleEmployee : IPerson
    {

        public SampleEmployee()
        {
            this.Properties = new Dictionary<string, object>();
            this.Identifiers = new IdentifierCollection();
        }

        [JsonConstructor]
        public SampleEmployee(Dictionary<string, object> properties)
        {
            this.Properties = properties;

            this.Identifiers = new IdentifierCollection();

            this.Identifiers.Add(this.EmployeeId);
            this.Identifiers.Add(this.Upn);
            
        }

        public SampleEmployee(EmployeeIdIdentifier employeeId, UpnIdentifier upnIdentifier)
            : this()
        {
            this.EmployeeId = employeeId;
            this.Upn = upnIdentifier;

            this.Identifiers.Add(this.EmployeeId);
            this.Identifiers.Add(this.Upn);
        }

        [JsonIgnore]
        public EmployeeIdIdentifier EmployeeId
        {
            get
            {
                return new EmployeeIdIdentifier(this.Properties["employeeid"] as string);
            }

            set
            {
                this.Properties["employeeid"] = value.StringValue;
            }
        }

        [JsonIgnore]
        public UpnIdentifier Upn
        {
            get
            {
                return new UpnIdentifier(this.Properties["upn"] as string);
            }

            set
            {
                this.Properties["upn"] = value.StringValue;
            }
        }

        [JsonProperty("properties")]
        public IDictionary<string, object> Properties { get; set; }

        public JObject ToJson()
        {
            List<JProperty> properties = new List<JProperty>();
            foreach (var p in this.Properties)
            {
                var value = p.Value;

                if (value is string || value is int || value is Array || value is DateTime)
                {
                    properties.Add(new JProperty(p.Key, value));
                }
            }

            JObject user = new JObject(properties);
            return user;
        }

        [JsonIgnore]
        public IdentifierCollection Identifiers { get; }
        
    }
}
