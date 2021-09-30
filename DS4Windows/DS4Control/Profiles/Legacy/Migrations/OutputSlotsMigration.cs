using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DS4WinWPF.DS4Control.Profiles.Legacy.Migrations
{
    /// <summary>
    ///     Transparently transforms outdated XML formats into the currently required one.
    /// </summary>
    internal sealed class OutputSlotsMigration : IEnumerable<Action<XElement>>
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
            var slots = node.Nodes().ToList();

            //
            // Rebuild root node with necessary namespaces and new structure
            // 
            node.ReplaceAll(
                new XAttribute("app_version", appVersion),
                new XAttribute(XNamespace.Xmlns + "sys", sysNs),
                new XAttribute(XNamespace.Xmlns + "exs", exsNs),
                new XAttribute(exsNs + "version", 1),
                new XElement("Slot",
                    new XAttribute(exsNs + "member", string.Empty),
                    new XElement("Capacity", 8),
                    slots
                ));
        }
    }
}