﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Xml.Linq;

namespace DS4WinWPF.DS4Control.Profiles.Schema.Migrations
{
    /// <summary>
    ///     Transparently transforms outdated XML formats into the currently required one.
    /// </summary>
    internal sealed class LinkedControllersMigration : IEnumerable<Action<XElement>>
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
            //
            // Store copies of data we need to keep and transform
            // 
            var appVersion = node.Attribute("app_version")?.Value ?? string.Empty;
            var entries = node.Nodes().Select(n => new
            {
                Address = PhysicalAddress.Parse(string.Concat(((XElement)n).Name.ToString().Skip(3))),
                Profile = ((XElement)n).Value
            });

            var dictionary = entries.Select(entry =>
                new XElement("Item",
                    new XElement("Key", entry.Address.ToString()),
                    new XElement("Value", entry.Profile)
                )
            ).ToList();

            //
            // Rebuild root node with necessary namespaces and new structure
            // 
            node.ReplaceAll(
                new XAttribute("app_version", appVersion),
                new XElement("Assignments",
                    dictionary
                ));
        }
    }
}