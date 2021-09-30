using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ExtendedXmlSerializer;

namespace DS4WinWPF.DS4Control.Profiles.Legacy.Migrations
{
    internal sealed class AutoProfilesMigration : IEnumerable<Action<XElement>>
    {
        public IEnumerator<Action<XElement>> GetEnumerator()
        {
            yield return MigrateFromAppVersion3;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static void MigrateFromAppVersion3(XElement node)
        {
            XNamespace sysNs = "https://extendedxmlserializer.github.io/system";
            XNamespace exsNs = "https://extendedxmlserializer.github.io/v2";

            //
            // Store copies of data we need to keep and transform
            // 
            var appVersion = node.Attribute("app_version")?.Value ?? string.Empty;

            var entries = new List<XElement>();

            //
            // Enumerate and transform existing entries
            // 
            foreach (var programEntry in node.Nodes().OfType<XElement>())
            {
                var index = 0;

                //
                // Convert controllers to proper list entries
                // 
                var controllers = programEntry
                    .Nodes()
                    .OfType<XElement>()
                    .Where(e =>
                        e.Name.ToString()
                            .StartsWith("Controller"))
                    .Select(controllerEntry =>
                        new XElement("Controller",
                            new XAttribute("Index", index++),
                            new XElement("Profile", controllerEntry.Value))
                    )
                    .ToList();

                //
                // Merge new Program and Controller sections together
                // 
                entries.Add(new XElement("Program",
                    programEntry.Attribute("path"),
                    programEntry.Attribute("title"),
                    new XElement("TurnOff", programEntry.Member("TurnOff").Value),
                    new XElement("Controller",
                        new XAttribute(exsNs + "member", string.Empty),
                        new XElement("Capacity", 8),
                        controllers
                    )
                ));
            }

            //
            // Rebuild root node with necessary namespaces and new structure
            // 
            node.ReplaceAll(
                new XAttribute("app_version", appVersion),
                new XAttribute(XNamespace.Xmlns + "sys", sysNs),
                new XAttribute(XNamespace.Xmlns + "exs", exsNs),
                new XAttribute(exsNs + "version", 1),
                new XElement("Program",
                    new XAttribute(exsNs + "member", string.Empty),
                    new XElement("Capacity", 255),
                    entries
                ));
        }
    }
}