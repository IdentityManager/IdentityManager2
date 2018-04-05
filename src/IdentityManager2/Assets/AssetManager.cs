using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("IdentityManager2.Tests")]

namespace IdentityManager2.Assets
{
    internal class AssetManager
    {
        private static readonly ConcurrentDictionary<string, string> ResourceStrings = new ConcurrentDictionary<string, string>();

        internal static string LoadResourceString(string name)
        {
            if (!ResourceStrings.TryGetValue(name, out var value))
            {
                var assembly = typeof(AssetManager).Assembly;

                using (var sr = new StreamReader(assembly.GetManifestResourceStream(name)))
                {
                    ResourceStrings[name] = value = sr.ReadToEnd();
                }
            }
            return value;
        }

        internal static string LoadResourceString(string name, IDictionary<string, object> values)
        {
            var value = LoadResourceString(name);
            foreach(var key in values.Keys)
            {
                var val = values[key];
                value = value.Replace("{" + key + "}",  val != null ? val.ToString() : "");
            }
            return value;
        }
        
        internal static string LoadResourceString(string name, object values)
        {
            return LoadResourceString(name, Map(values));
        }

        private static IDictionary<string, object> Map(object values)
        {
            var dictionary = values as IDictionary<string, object>;
            
            if (dictionary == null) 
            {
                dictionary = new Dictionary<string, object>();
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(values))
                {
                    dictionary.Add(descriptor.Name, descriptor.GetValue(values));
                }
            }

            return dictionary;
        }
    }
}

