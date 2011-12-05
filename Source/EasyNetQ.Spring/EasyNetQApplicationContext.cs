using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using Spring.Context;
using Spring.Context.Support;
using Spring.Core.IO;
using Spring.Util;

namespace EasyNetQ.Spring
{
    public class EasyNetQApplicationContext : AbstractXmlApplicationContext
    {
        private readonly string[] _configurationLocations;

        public EasyNetQApplicationContext(IApplicationContext parentContext) :
            this(false, null, true, parentContext,
                 ParseConfigurationLocationsFromAppConfig())
        { }

        public EasyNetQApplicationContext(params string[] configurationLocations)
            : this(false, null, true, null, configurationLocations)
        { }

        public EasyNetQApplicationContext(IApplicationContext parentContext, params string[] configurationLocations)
            : this(false, null, true, parentContext, configurationLocations)
        { }

        public EasyNetQApplicationContext(bool caseSensitive, params string[] configurationLocations)
            : this(false, null, caseSensitive, null, configurationLocations)
        { }

        public EasyNetQApplicationContext(bool caseSensitive, IApplicationContext parentContext, params string[] configurationLocations)
            : this(false, null, caseSensitive, parentContext, configurationLocations)
        { }

        public EasyNetQApplicationContext(string name, bool caseSensitive, params string[] configurationLocations)
            : this(false, name, caseSensitive, null, configurationLocations)
        { }

        public EasyNetQApplicationContext(string name, bool caseSensitive, IApplicationContext parentContext, params string[] configurationLocations)
            : this(false, name, caseSensitive, parentContext, configurationLocations)
        { }

        public EasyNetQApplicationContext(string name, bool caseSensitive, IApplicationContext parentContext, string[] includeFolders, params string[] configurationLocations)
            : this(true, name, caseSensitive, parentContext, configurationLocations)
        { }

        public EasyNetQApplicationContext(bool refresh, string name, bool caseSensitive, IApplicationContext parentContext, params string[] configurationLocations)
            : base(name, caseSensitive, parentContext)
        {
            _configurationLocations = configurationLocations;

            AddDefaultObjectPostProcessor(new MessageHandlerObjectPostProcessor(this));

            if (refresh)
            {
                AssertUtils.ArgumentHasElements(configurationLocations, "configurationLocations");
                base.Refresh();
            }
        }

        private static string[] ParseConfigurationLocationsFromAppConfig()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            XDocument doc = XDocument.Parse(config.GetSection("spring/context").SectionInformation.GetRawXml(), LoadOptions.None);

            return (from x in doc.Descendants("resource") select x.Attribute("uri").Value).ToArray<string>();
        }

        protected override string[] ConfigurationLocations
        {
            get { return _configurationLocations; }
        }

        protected override IResource[] ConfigurationResources
        {
            get { return null; }
        }
    }
}