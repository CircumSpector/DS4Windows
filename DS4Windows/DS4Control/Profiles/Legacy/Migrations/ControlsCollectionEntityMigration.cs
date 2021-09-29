using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using ExtendedXmlSerializer;

namespace DS4WinWPF.DS4Control.Profiles.Legacy.Migrations
{
    internal sealed class ControlsCollectionEntityMigration : IEnumerable<Action<XElement>>
    {
        public IEnumerator<Action<XElement>> GetEnumerator()
        {
            yield return Migration;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static void Migration(XElement node)
        {
            //
            // Missing critical value, no use to continue
            // 
            if (node.LastNode is null)
                return;

            //
            // Already converted, nothing to do
            // 
            if (node.Member("Value") != null)
                return;

            var value = node.LastNode?.ToString();
            var shiftTrigger = node.Attribute("Trigger")?.Value;

            node.RemoveAll();

            //
            // Convert Trigger attribute to element, if found
            // 
            if (!string.IsNullOrEmpty(shiftTrigger)) node.Add(new XElement("Trigger", shiftTrigger));

            node.Add(new XElement("Value", value));
        }
    }
}