using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using EasyNetQ.Config;

namespace EasyNetQ.Host
{
    internal static class ExtensionMethods
    {
        public static string GetOption(this string[] value, string option)
        {
            if(value == null || value.Length == 0)
                throw new ArgumentException("Invalid argument!", "value");

            var replaced = value.Select(x => Regex.Replace(x, "^[-/]", String.Empty));

            var pairs = replaced.Select(x => Regex.Split(x, ":"));

            var requestedPair = pairs.FirstOrDefault(x => x[0] == option);

            if (requestedPair != null)
            {
                return requestedPair[1];
            }

            return String.Empty;
        }

        public static bool RequestInstall(this string[] value)
        {
            if (value != null && value.Length > 0)
            {
                return value[0] == "install";
            }

            return false;
        }

        public static bool RequestUninstall(this string[] value)
        {
            if (value != null && value.Length > 0)
            {
                return value[0] == "uninstall";
            }

            return false;
        }

        public static bool RequestInstallOrUninstall(this string[] args)
        {
            return (args.RequestInstall() || args.RequestUninstall());
        }

        public static bool NotNullOrWhitespace(this string value)
        {
            return !String.IsNullOrWhiteSpace(value);
        }

        public static void ThrowIfInvalidInContext(this ServiceData value, string[] args)
        {
            if(value == null || args == null || args.Length < 1)
                throw new Exception();

            if(args.RequestInstall())
                if (!value.IsValidForInstall)
                    throw new Exception();

            if(args.RequestUninstall())
                if(!value.IsValidForUninstall)
                    throw new Exception();
        }

        public static void InjectBusIfRequested(this object value, IBus bus)
        {
            var properties = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var busProperty = properties.FirstOrDefault(x => x.PropertyType == typeof (IBus));

            if (busProperty != null &&  busProperty.CanWrite)
            {
                busProperty.SetValue(value, bus, BindingFlags.Instance|BindingFlags.SetProperty, null, null, CultureInfo.InvariantCulture);
            }
        }

        public static string GetEndpointConfigurationFilePath(this Type endpointType)
        {
            if(endpointType == null)
                throw new ArgumentNullException("endpointType");

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, endpointType.Assembly.ManifestModule.Name + ".config");
        }

        public static Configuration OpenMappedEnpointConfiguration(this Type endpointType)
        {
            if(endpointType == null)
                throw new ArgumentNullException("endpointType");

            var path = endpointType.GetEndpointConfigurationFilePath();

            if (!File.Exists(path))
                return null;

            var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = path };
            return ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
        }

        public static EasyNetQConfigurationSection GetEasyNetQSection(this Configuration value)
        {
            if(value == null)
                throw new ArgumentNullException("value");

            var section = value.GetSection("easyNetQ") as EasyNetQConfigurationSection;

            return section ?? new EasyNetQConfigurationSection();
        }

        public static PropertyInfo GetPropertyInfoFromExpression<T>(this Expression<Func<T, object>> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            var lambda = value as LambdaExpression;
            if (lambda == null)
            {
                throw new ArgumentException("Not a lambda expression", "value");
            }
            MemberExpression memberExpr = null;
            if (lambda.Body.NodeType == ExpressionType.Convert)
            {
                memberExpr = ((UnaryExpression)lambda.Body).Operand as MemberExpression;
            }
            else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpr = lambda.Body as MemberExpression;
            }
            if (memberExpr == null)
            {
                throw new ArgumentException("Not a member access", "value");
            }
            
            return memberExpr.Member as PropertyInfo;
        }

        public static bool IsLaterThan(this DateTime value, DateTime compare)
        {
            return value > compare;
        }
    }
}