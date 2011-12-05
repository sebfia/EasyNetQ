using System;
using System.Configuration;
using System.Xml;
using Common.Logging;
using Common.Logging.Simple;

namespace EasyNetQ.Host
{
    //public class CommonLoggingConfigurer : ILoggingConfigurer
    //{
    //    public void Configure(Type endpointType)
    //    {
    //        if(endpointType == null)
    //            throw new ArgumentNullException("endpointType");

    //        var config = endpointType.OpenMappedEnpointConfiguration();

    //        if (config != null)
    //        {
    //            var rawXmlSection = config.GetSection("common/logging").SectionInformation.GetRawXml();
    //            if (rawXmlSection != null)
    //            {
    //                XmlNode node = new ConfigXmlDocument();
    //                node.InnerXml = rawXmlSection;
    //                node = node.FirstChild;
    //                var handler = new ConfigurationSectionHandler();
    //                var di = handler.Create(null, null, node);
    //                var adapter = Activator.CreateInstance(di.FactoryAdapterType, new[] { di.Properties }) as ILoggerFactoryAdapter;
    //                LogManager.Adapter = adapter; 
    //            }
    //        }
    //        else
    //        {
    //            LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter();
    //        }
    //    }
    //}
}