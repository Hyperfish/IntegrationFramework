using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antrea.Windows.ProfileProvider;
using Antrea.Windows.ProfileProvider.Identifiers;
using Newtonsoft.Json.Linq;
using SampleProvider;

namespace SampleConnector
{
    public class SampleEmployee : IPerson
    {
        private UpnIdentifier _upn;
        public EmployeeIdIdentifier _employeeId;

        public SampleEmployee()
        {
            this.Properties = new Dictionary<string, object>();
        }

        public SampleEmployee(EmployeeIdIdentifier employeeId)
        {
            this.Properties = new Dictionary<string, object>();

            this.EmployeeId = employeeId;
        }

        public EmployeeIdIdentifier EmployeeId
        {
            get { return _employeeId; }

            set
            {
                this.Properties["employeeid"] = value.StringValue;
                _employeeId = value;
            }
        }


        public UpnIdentifier Upn
        {
            get { return _upn; }
            set { _upn = value; }
        }

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

        public IdentifierCollection Identifiers
        {
            get
            {
                var identifiers = new IdentifierCollection();

                var id = this.EmployeeId;
                identifiers.Add(id);

                if (this._upn != null) identifiers.Add(this._upn);

                return identifiers;
            }
        }
    }
}
