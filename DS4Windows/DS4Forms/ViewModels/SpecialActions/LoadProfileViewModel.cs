using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels.Util;

namespace DS4WinWPF.DS4Forms.ViewModels.SpecialActions
{
    public class LoadProfileViewModel : NotifyDataErrorBase
    {
        private bool autoUntrigger;
        private ProfileList profileList;
        private int profileIndex;
        private bool normalTrigger = true;

        public bool AutoUntrigger { get => autoUntrigger; set => autoUntrigger = value; }
        public int ProfileIndex
        {
            get => profileIndex;
            set
            {
                if (profileIndex == value) return;
                profileIndex = value;
                ProfileIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ProfileIndexChanged;

        public bool UnloadEnabled { get => profileIndex > 0; }
        public event EventHandler UnloadEnabledChanged;

        public ProfileList ProfileList { get => profileList; }
        public bool NormalTrigger { get => normalTrigger; set => normalTrigger = value; }

        public LoadProfileViewModel(ProfileList profileList)
        {
            this.profileList = profileList;

            ProfileIndexChanged += LoadProfileViewModel_ProfileIndexChanged;
        }

        public void LoadAction(SpecialAction action)
        {
            autoUntrigger = action.AutomaticUnTrigger;
            string profilename = action.Details;
            ProfileEntity item = profileList.ProfileListCollection.SingleOrDefault(x => x.Name == profilename);
            if (item != null)
            {
                profileIndex = profileList.ProfileListCollection.IndexOf(item) + 1;
            }
        }

        private void LoadProfileViewModel_ProfileIndexChanged(object sender, EventArgs e)
        {
            UnloadEnabledChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SaveAction(SpecialAction action, bool edit = false)
        {
            if (profileIndex > 0)
            {
                string profilename = profileList.ProfileListCollection[profileIndex - 1].Name;
                if (action.UControls == null)
                {
                    action.UControls = string.Empty;
                }

                Global.Instance.SaveAction(action.Name, action.Controls, 3, profilename, edit,
                    action.UControls +
                    (autoUntrigger ? (action.UControls.Length > 0 ? "/" : "") + "AutomaticUntrigger" : ""));
            }
        }

        public override bool IsValid(SpecialAction action)
        {
            ClearOldErrors();

            bool valid = true;
            List<string> profileIndexErrors = new List<string>();

            if (profileIndex == 0)
            {
                profileIndexErrors.Add("No profile given");
                errors["ProfileIndex"] = profileIndexErrors;
                RaiseErrorsChanged("ProfileIndex");
            }

            return valid;
        }

        public override void ClearOldErrors()
        {
            if (errors.Count > 0)
            {
                errors.Clear();
                RaiseErrorsChanged("ProfileIndex");
            }
        }
    }
}
