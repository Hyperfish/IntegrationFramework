using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Antrea.Windows.ProfileProvider;
using Antrea.Windows.ProfileProvider.Exceptions;
using Antrea.Windows.ProfileProvider.Identifiers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

namespace SampleConnector.Tests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void TestGetUser()
        {
            var baseLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            SampleConnector connector = new SampleConnector();
            connector.Initialize(baseLocation, null);

            var identifiers = new IdentifierCollection();
            identifiers.Add(new UpnIdentifier("danj@contoso.com"));

            var attributes = new List<IAttribute>();

            var profile = connector.GetUser(identifiers, attributes);

            Assert.IsTrue(profile.Properties.Any(), "Failed to get attributes");
            Assert.IsTrue(profile.Identifiers.Identifiers.Any(), "Failed to get identifiers");
        }

        [TestMethod]
        public void TestAttributes()
        {
            var baseLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            SampleConnector connector = new SampleConnector();
            connector.Initialize(baseLocation, null);

            var attributes = connector.GetAttributes();

            Assert.IsTrue(attributes.Any(), "Failed to get attributes list from SPO connector");
        }

        [TestMethod]
        public void TestUpdate()
        {
            var baseLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            SampleConnector connector = new SampleConnector();
            connector.Initialize(baseLocation, null);

            var identifiers = new IdentifierCollection();
            identifiers.Add(new UpnIdentifier("danj@contoso.com"));

            var emergencyContactValue = "You need a " + Guid.NewGuid().ToString("D") + " emergency contact";

            // set the value
            connector.UpdateAttribute(identifiers, new SampleAttribute("emergencycontact"), emergencyContactValue);

            var attributes = new List<IAttribute>();
            var profile = connector.GetUser(identifiers, attributes);

            Assert.IsTrue(profile.Properties["emergencycontact"].ToString() == emergencyContactValue, "Expected attibute value to be the new one. It wasn't.");
        }

        [TestMethod]
        public void TestAudit()
        {
            var baseLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            SampleConnector connector = new SampleConnector();
            connector.Initialize(baseLocation, null);
            
            var store = new MemoryAuditStore();

            // do the audit
            connector.Audit(false, GetAudienceCollection(), store);

            Assert.IsTrue(store.Results.Any(), "Didn't get audit results");
            Assert.IsTrue(store.Results.SelectMany(r => r.Errors).Any(), "Didn't get audit errors");
        }

        [TestMethod]
        public void TestGetAttribute()
        {
            var baseLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            SampleConnector connector = new SampleConnector();
            connector.Initialize(baseLocation, null);

            var attribute = connector.GetAttribute("emergencycontact");

            Assert.IsNotNull(attribute is SampleAttribute, "Attribute type didnt match expected.");
            Assert.IsNotNull(attribute.Name == "emergencycontact", "Attribute name didnt match expected.");

        }

        [TestMethod]
        public void TestIdentifiersRequired()
        {
            var baseLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            SampleConnector connector = new SampleConnector();
            connector.Initialize(baseLocation, null);

            var identifiersRequired = connector.IdentifierTypesRequired;

            var upn = identifiersRequired.FirstOrDefault(i => i.Type == CommonIdentifierTypes.Upn.ToString());
            var employeeId = identifiersRequired.FirstOrDefault(i => i.Type == CommonIdentifierTypes.EmployeeId.ToString());
            
            Assert.IsNotNull(upn, "Didn't get a upn identifier required, should have");
            Assert.IsNotNull(employeeId, "Didn't get an employeeid identifier required, should have");
        }

        [TestMethod]
        public void TestIdentifiersProvided()
        {
            var baseLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            SampleConnector connector = new SampleConnector();
            connector.Initialize(baseLocation, null);

            var identifierTypes = connector.IdentifierTypesProvided;

            Assert.IsTrue(identifierTypes.Any(), "Expected identifiers provided.");

        }
        
        public IAudienceCollection GetAudienceCollection()
        {
            var collection = new AudienceCollection();

            var audience = new Audience()
            {
                AudienceId = Guid.NewGuid(),
                Master = true,
                RawScopes = new string[0],
                Schema = JsonConvert.DeserializeObject<JSchema>(File.ReadAllText("Schema.json")),
                Scopes = new List<IAudienceScope>()
            };

            collection.Audiences = new List<IAudience>() { audience };
            collection.Identifiers = new string[0];
            collection.ResourceType = "sampleuser";

            return collection;
        }
    }
}
