using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Antrea.Windows.ProfileProvider;
using Antrea.Windows.ProfileProvider.Identifiers;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace SampleConnector
{
    /// <summary>
    /// This sample connector simply provides a number of custom attibutes to Hyperfish 
    /// and stores them in a simple JSON file. 
    /// </summary>

    public partial class SampleConnector : IProfileProvider
    {
        private SampleStore _store;
        private List<SampleAttribute> attributes;
        private string _settingsLocation;
        private const string InternalName = "sample";
        private const string InternalDisplayName = "Sample";

        public SampleConnector()
        {

            // keep a basic hardcoded list of attributes this connector knows about
            attributes = new List<SampleAttribute>();

            attributes.Add(new SampleAttribute("startdate"));
            attributes.Add(new SampleAttribute("employeeid"));
            attributes.Add(new SampleAttribute("upn"));
            attributes.Add(new SampleAttribute("emergencycontact"));
            attributes.Add(new SampleAttribute("emergencycontactnumber"));
            
            var location = Assembly.GetExecutingAssembly().Location;

            Debug.WriteLine($"Hyperfish: Sample provider loading from {location}");
            Logger?.Debug($"Sample provider loading from {location}");
        }

#region IProfileProvider implementation

        /// <summary>
        /// Initialize is called by the Hyperfish agent process to set up the connector. 
        /// </summary>
        public void Initialize(string settingsLocation, ILog logger = null)
        {
            try
            {
                this.Logger = logger ?? this.Logger;

                _settingsLocation = settingsLocation;

                if (this.Logger == null)
                {
                    Debug.WriteLine("Hyperfish: Logger not set for Sample worker!");
                }

                // load and initialize settings
                var settings = new SampleConnectorSettings();
                settings.LoadSettings(settingsLocation);

                this.Settings = settings;
            
                InitializeStore();

            }
            catch (Exception e)
            {
                Debug.WriteLine($"Hyperfish: Exception initializing sample provider. {e}");
                Logger?.Debug($"Exception initializing sample provider: {e}");
                throw;
            }
        }

        public string Name => InternalName;

        public void UpdateAttribute(IdentifierCollection identifiers, IAttribute attribute, object propertyValue)
        {
            Logger?.Debug($"Sample provider update for user: {identifiers}");

            var identifier = GetRequiredIdentifier(identifiers);

            Logger?.Debug($"Found needed identifier: {identifier}");

            if (_store.People.Any(e => e.Identifiers.Has(identifiers)))
            {
                var person = _store.People.First(e => e.Identifiers.Has(identifiers));
                person.Properties[attribute.Name] = propertyValue;

                Logger?.Debug($"Found person: {person.Identifiers}");
            }
            else
            {
                Logger?.Debug($"Didnt find existing person. Creating one.");

                var emp = CreateNewPerson(identifier);
                _store.People.Add(emp);
                SaveStore();
            }

            SaveStore();
        }

        public IPerson GetUser(IdentifierCollection identifiers, IEnumerable<IAttribute> properties)
        {
            Logger?.Debug($"Sample provider lookup for user: {identifiers}");

            var identifier = GetRequiredIdentifier(identifiers);

            Logger?.Debug($"Found needed identifier: {identifier}");

            if (_store.People.Any(e => e.Identifiers.Has(identifiers)))
            {
                Logger?.Debug($"Found person.");
                return _store.People.First(e => e.Identifiers.Has(identifiers));
            }
            else
            {
                Logger?.Debug($"Didnt find existing person. Creating one.");

                var emp = CreateNewPerson(identifier);
                _store.People.Add(emp);
                SaveStore();

                return emp;

            }
        }

        public IEnumerable<IAttribute> GetAttributes()
        {
            return attributes.ToList<IAttribute>();
        }

        public IAttribute GetAttribute(string attributeName)
        {
            var foundAttribute = GetAttributes().FirstOrDefault(a => a.Name == attributeName);

            if (foundAttribute != null)
            {
                return foundAttribute;
            }
            else
            {
                return new SampleAttribute(attributeName);
            }
        }

        public IProfileProviderSettings Settings { get; set; }

        public ILog Logger { get; set; }

        public IEnumerable<IAttribute> GetBasicAttributesList
        {
            get { return attributes.ToList<IAttribute>(); }
        }

        public IEnumerable<IdentifierType> IdentifierTypesRequired
        {
            get
            {
                var types = new List<IdentifierType>
                {
                    _primaryIdentifierType,
                    _secondaryIdentifierType
                };

                return types;
            }
        }

        public IEnumerable<IdentifierType> IdentifierTypesProvided
        {
            get
            {
                var types = new List<IdentifierType>();

                types.Add(new IdentifierType(CommonIdentifierTypes.EmployeeId));

                return types;
            }
        }

        public void Audit(bool incremental, IAudienceCollection orgAudiences, IAuditResultsStore auditResultsStore)
        {
            foreach (var audience in orgAudiences.Audiences)
            {
                var providerAudienceSchema = audience.Schema.GetProviderSpecificSchema(this.Name);
            
                foreach (var employee in _store.People)
                {
                    var result = this.CheckUser(employee, audience, providerAudienceSchema);
                    auditResultsStore.AddResult(result);
                }
            }
        }
        
#endregion

        private void InitializeStore()
        {
            Logger?.Debug($"Loading sample store: {StoreFilePath}");
            
            if (File.Exists(StoreFilePath))
            {
                var json = File.ReadAllText(StoreFilePath);

                var store = JsonConvert.DeserializeObject<SampleStore>(json, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects
                });

                _store = store;
            }
            else
            {
                _store = new SampleStore();
                SaveStore();
            }
        }

        private string StoreFilePath
        {
            get
            {
                var StorePath = "SampleStore.json";

                return Path.Combine(_settingsLocation, StorePath);
            }
        }

        private void SaveStore()
        {
            var json = JsonConvert.SerializeObject(_store, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            });

            File.WriteAllText(StoreFilePath, json);

            Logger?.Debug($"Saved sample store: {StoreFilePath}");

        }
        
        private IIdentifier GetRequiredIdentifier(IdentifierCollection identifiers)
        {
            // make sure it has all the identifiers needed
            if (!this.IdentifierTypesRequired.Any(i => identifiers.HasIdentiferOfType(i)))
            {
                throw new ArgumentException("Need to pass one of the required identifiers", nameof(identifiers));
            }

            // try for objectguid first ... thats immutable
            IIdentifier identifier = identifiers.GetIdentifierOfTypeAs<EmployeeIdIdentifier>(_primaryIdentifierType)?.FirstOrDefault() ??
                                     (IIdentifier)identifiers.GetIdentifierOfTypeAs<UpnIdentifier>(_secondaryIdentifierType)?.FirstOrDefault();

            if (identifier == null)
                throw new ArgumentOutOfRangeException("Needs an employee id or upn identifier and didnt get one");

            return identifier;
        }

        private IdentifierType _primaryIdentifierType = new IdentifierType(CommonIdentifierTypes.EmployeeId);
        private IdentifierType _secondaryIdentifierType = new IdentifierType(CommonIdentifierTypes.Upn);

        private SampleEmployee CreateNewPerson(IIdentifier identifier)
        {
            EmployeeIdIdentifier id = new EmployeeIdIdentifier(DateTime.UtcNow.Ticks.ToString());

            var emp = new SampleEmployee(id);
            emp.Properties["compensation"] = "$100,000";
            emp.Properties["startdate"] = "2-May-2008";
            
            if (identifier.IdentifierType.Matches(_secondaryIdentifierType))
            {
                Logger?.Debug($"Setting upn: {identifier}");

                emp.Upn = new UpnIdentifier(identifier.StringValue);
            }

            return emp;
        }

        public string DisplayName => InternalDisplayName;

        private AuditResult CheckUser(SampleEmployee person, IAudience audience, JSchema schema)
        {
            IList<ValidationError> errors = new List<ValidationError>();

            var userToCheck = person.ToJson();

            //var schema = audience.Schema.GetProviderSpecificSchema(this.Name);

            bool valid = userToCheck.IsValid(schema, out errors);

            // map these to our own error object
            var auditErrors = new List<AuditError>();

            foreach (var error in errors)
            {
                auditErrors.Add(new AuditError(error, audience, this, schema));
            }

            var result = new AuditResult(person, auditErrors);

            return result;
        }
        
    }
}
