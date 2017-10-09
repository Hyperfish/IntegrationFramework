using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antrea.Windows.ProfileProvider.Identifiers;

namespace SampleProvider
{
    public class EmployeeIdIdentifier : IIdentifier
    {
        public EmployeeIdIdentifier(string employeeId)
        {
            EmployeeId = employeeId;
        }

        public EmployeeIdIdentifier()
        {
        }

        public string Name { get; set; } = "employeeid";
        
        public string EmployeeId
        {
            get { return (string)this.Value; }
            set { this.Value = value; }
        }

        public object Value { get; internal set; }

        public string StringValue => Value.ToString();

        public IdentifierType IdentifierType => new IdentifierType(CommonIdentifierTypes.EmployeeId);

        public override string ToString()
        {
            return this.EmployeeId;
        }

        public int CompareTo(object obj)
        {
            var identifer = obj as UpnIdentifier;
            if (identifer == null) throw new ArgumentException();
            return this.CompareTo(identifer);
        }

        public int CompareTo(IIdentifier obj)
        {
            var identifer = obj as EmployeeIdIdentifier;
            if (identifer == null) throw new ArgumentException();

            return this.EmployeeId.CompareTo(identifer.EmployeeId);
        }

        public void Parse(IIdentifier from)
        {
            if (from.IdentifierType.Matches(IdentifierType))
            {
                this.Name = from.Name;
                this.Value = from.Value;
            }

            throw new ArgumentException($"from must have a Type of {IdentifierType}");
        }
    }
}

