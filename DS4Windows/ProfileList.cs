using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using DS4Windows;
using DS4WinWPF.DS4Control.Attributes;

namespace DS4WinWPF
{
    public class ProfileList
    {
        private readonly object _lockObject = new();

        public ProfileList()
        {
            BindingOperations.EnableCollectionSynchronization(ProfileListCollection, _lockObject);
        }

        public ObservableCollection<ProfileEntity> ProfileListCollection { get; set; } = new();

        [ConfigurationSystemComponent]
        public void Refresh()
        {
            ProfileListCollection.Clear();
            var profiles = Directory.GetFiles(Path.Combine(Global.RuntimeAppDataPath, Constants.ProfilesSubDirectory));
            foreach (var s in profiles)
                if (s.EndsWith(".xml"))
                {
                    var item = new ProfileEntity
                    {
                        Name = Path.GetFileNameWithoutExtension(s)
                    };

                    ProfileListCollection.Add(item);
                }
        }

        public void AddProfileSort(string profileName)
        {
            var idx = 0;
            var inserted = false;
            foreach (var entry in ProfileListCollection)
            {
                if (string.Compare(entry.Name, profileName, StringComparison.Ordinal) > 0)
                {
                    ProfileListCollection.Insert(idx, new ProfileEntity { Name = profileName });
                    inserted = true;
                    break;
                }

                idx++;
            }

            if (!inserted) ProfileListCollection.Add(new ProfileEntity { Name = profileName });
        }

        public void RemoveProfile(string profile)
        {
            var selectedEntity = ProfileListCollection.SingleOrDefault(x => x.Name == profile);
            if (selectedEntity != null)
            {
                var selectedIndex = ProfileListCollection.IndexOf(selectedEntity);
                ProfileListCollection.RemoveAt(selectedIndex);
            }
        }
    }
}