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
        private readonly object _proLockobj = new();

        public ProfileList()
        {
            BindingOperations.EnableCollectionSynchronization(ProfileListCollection, _proLockobj);
        }

        public ObservableCollection<ProfileEntity> ProfileListCollection { get; set; } = new();

        [ConfigurationSystemComponent]
        public void Refresh()
        {
            ProfileListCollection.Clear();
            var profiles = Directory.GetFiles(Global.RuntimeAppDataPath + @"\Profiles\");
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

        public void AddProfileSort(string profilename)
        {
            var idx = 0;
            var inserted = false;
            foreach (var entry in ProfileListCollection)
            {
                if (entry.Name.CompareTo(profilename) > 0)
                {
                    ProfileListCollection.Insert(idx, new ProfileEntity { Name = profilename });
                    inserted = true;
                    break;
                }

                idx++;
            }

            if (!inserted) ProfileListCollection.Add(new ProfileEntity { Name = profilename });
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