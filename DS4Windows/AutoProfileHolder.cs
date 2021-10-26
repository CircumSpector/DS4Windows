using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Data;
using DS4Windows;
using DS4WinWPF.DS4Control.Attributes;
using DS4WinWPF.DS4Control.Profiles.Schema;

namespace DS4WinWPF
{
    public class AutoProfileHolder
    {
        private readonly object _collectionLock = new();

        //public Dictionary<string, AutoProfileEntity> AutoProfileDict { get => autoProfileDict; }

        //private Dictionary<string, AutoProfileEntity> autoProfileDict;

        public AutoProfileHolder()
        {
            AutoProfileCollection = new ObservableCollection<AutoProfileEntity>();
            //autoProfileDict = new Dictionary<string, AutoProfileEntity>();
            Load();

            BindingOperations.EnableCollectionSynchronization(AutoProfileCollection, _collectionLock);
        }

        public ObservableCollection<AutoProfileEntity> AutoProfileCollection { get; }

        [ConfigurationSystemComponent]
        private void Load()
        {
            var settingsPath = Path.Combine(Global.RuntimeAppDataPath, Constants.AutoProfilesFileName);

            if (!File.Exists(settingsPath))
                return;

            var settings = new AutoProfileProgramsV3();

            try
            {
                using (var stream = File.OpenRead(settingsPath))
                {
                    settings = AutoProfileProgramsV3.Deserialize(stream);
                }
            }
            catch
            {
                //
                // TODO: migrate
                // 
            }

            foreach (var programEntry in settings.ProgramEntries)
            {
                var autoProfile = new AutoProfileEntity(programEntry.Path, programEntry.Title);

                for (var i = 0; i < programEntry.Controllers.Count; i++)
                    autoProfile.ProfileNames[i] = programEntry.Controllers[i].Profile;

                autoProfile.Turnoff = programEntry.TurnOff;

                AutoProfileCollection.Add(autoProfile);
            }
        }

        [ConfigurationSystemComponent]
        public bool Save(string profile)
        {
            var settings = new AutoProfileProgramsV3();

            foreach (var profileEntity in AutoProfileCollection)
            {
                var entry = new AutoProfileProgramV3
                {
                    Path = profileEntity.Path,
                    Title = profileEntity.Title,
                    TurnOff = profileEntity.Turnoff
                };

                foreach (var profileName in profileEntity.ProfileNames)
                    entry.Controllers.Add(new AutoProfileControllerV3
                    {
                        Profile = profileName
                    });

                settings.ProgramEntries.Add(entry);
            }

            try
            {
                using var stream = File.Open(profile, FileMode.Create);

                settings.Serialize(stream);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Remove(AutoProfileEntity item)
        {
            //autoProfileDict.Remove(item.Path);
            AutoProfileCollection.Remove(item);
        }
    }

    public class AutoProfileEntity
    {
        public const string NONE_STRING = "(none)";
        public string path = string.Empty;
        private string path_lowercase;

        public string title = string.Empty;
        private string title_lowercase;

        public AutoProfileEntity(string pathStr, string titleStr)
        {
            // Initialize autoprofile search keywords(xxx_tolower).To improve performance the search keyword is pre - calculated in xxx_tolower variables,
            // so autoprofile timer thread doesn't have to create substrings/replace/tolower string instances every second over and over again.
            SetSearchPath(pathStr);
            SetSearchTitle(titleStr);
        }

        public string Path
        {
            get => path;
            set => SetSearchPath(value);
        }

        public string Title
        {
            get => title;
            set => SetSearchTitle(value);
        }

        public bool Turnoff { get; set; }

        public List<string> ProfileNames { get; set; } = new(Global.MAX_DS4_CONTROLLER_COUNT)
        {
            string.Empty, string.Empty,
            string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty
        };

        public bool IsMatch(string searchPath, string searchTitle)
        {
            var bPathMatched = true;
            var bTitleMwatched = true;

            if (!string.IsNullOrEmpty(path_lowercase))
                bPathMatched = path_lowercase == searchPath
                               || path[0] == '^' && searchPath.StartsWith(path_lowercase)
                               || path[path.Length - 1] == '$' && searchPath.EndsWith(path_lowercase)
                               || path[0] == '*' && searchPath.Contains(path_lowercase);

            if (bPathMatched && !string.IsNullOrEmpty(title_lowercase))
                bTitleMwatched = title_lowercase == searchTitle
                                 || title[0] == '^' && searchTitle.StartsWith(title_lowercase)
                                 || title[title.Length - 1] == '$' && searchTitle.EndsWith(title_lowercase)
                                 || title[0] == '*' && searchTitle.Contains(title_lowercase);

            // If both path and title defined in autoprofile entry then do AND condition (ie. both path and title should match)
            return bPathMatched && bTitleMwatched;
        }

        private void SetSearchPath(string pathStr)
        {
            if (!string.IsNullOrEmpty(pathStr))
            {
                path = pathStr;
                path_lowercase = path.ToLower().Replace('/', '\\');

                if (path.Length >= 2)
                {
                    if (path[0] == '^') path_lowercase = path_lowercase.Substring(1);
                    else if (path[path.Length - 1] == '$')
                        path_lowercase = path_lowercase.Substring(0, path_lowercase.Length - 1);
                    else if (path[0] == '*') path_lowercase = path_lowercase.Substring(1);
                }
            }
            else
            {
                path = path_lowercase = string.Empty;
            }
        }

        private void SetSearchTitle(string titleStr)
        {
            if (!string.IsNullOrEmpty(titleStr))
            {
                title = titleStr;
                title_lowercase = title.ToLower();

                if (title.Length >= 2)
                {
                    if (title[0] == '^') title_lowercase = title_lowercase.Substring(1);
                    else if (title[title.Length - 1] == '$')
                        title_lowercase = title_lowercase.Substring(0, title_lowercase.Length - 1);
                    else if (title[0] == '*') title_lowercase = title_lowercase.Substring(1);
                }
            }
            else
            {
                title = title_lowercase = string.Empty;
            }
        }
    }
}