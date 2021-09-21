using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using DS4Windows;

namespace DS4WinWPF
{
    public class ProfileList
    {
        private readonly object _proLockobj = new();

        public ProfileList()
        {
            BindingOperations.EnableCollectionSynchronization(ProfileListCol, _proLockobj);
        }

        public ObservableCollection<ProfileEntity> ProfileListCol { get; set; } = new();

        [ConfigurationSystemComponent]
        public void Refresh()
        {
            ProfileListCol.Clear();
            var profiles = Directory.GetFiles(Global.RuntimeAppDataPath + @"\Profiles\");
            foreach (var s in profiles)
                if (s.EndsWith(".xml"))
                {
                    var item = new ProfileEntity
                    {
                        Name = Path.GetFileNameWithoutExtension(s)
                    };

                    ProfileListCol.Add(item);
                }
        }

        public void AddProfileSort(string profilename)
        {
            var idx = 0;
            var inserted = false;
            foreach (var entry in ProfileListCol)
            {
                if (entry.Name.CompareTo(profilename) > 0)
                {
                    ProfileListCol.Insert(idx, new ProfileEntity { Name = profilename });
                    inserted = true;
                    break;
                }

                idx++;
            }

            if (!inserted) ProfileListCol.Add(new ProfileEntity { Name = profilename });
        }

        public void RemoveProfile(string profile)
        {
            var selectedEntity = ProfileListCol.SingleOrDefault(x => x.Name == profile);
            if (selectedEntity != null)
            {
                var selectedIndex = ProfileListCol.IndexOf(selectedEntity);
                ProfileListCol.RemoveAt(selectedIndex);
            }
        }
    }
}