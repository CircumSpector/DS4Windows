using System;
using System.Collections.Generic;
using System.Linq;
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels.Util;

namespace DS4WinWPF.DS4Forms.ViewModels.SpecialActions
{
    public class LoadProfileViewModel : NotifyDataErrorBase
    {
        private int profileIndex;

        public LoadProfileViewModel(ProfileList profileList)
        {
            ProfileList = profileList;

            ProfileIndexChanged += LoadProfileViewModel_ProfileIndexChanged;
        }

        public bool AutoUntrigger { get; set; }

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

        public bool UnloadEnabled => profileIndex > 0;

        public ProfileList ProfileList { get; }

        public bool NormalTrigger { get; set; } = true;
        public event EventHandler ProfileIndexChanged;
        public event EventHandler UnloadEnabledChanged;

        public void LoadAction(SpecialAction action)
        {
            AutoUntrigger = action.AutomaticUnTrigger;
            var profilename = action.Details;
            var item = ProfileList.ProfileListCollection.SingleOrDefault(x => x.Name == profilename);
            if (item != null) profileIndex = ProfileList.ProfileListCollection.IndexOf(item) + 1;
        }

        private void LoadProfileViewModel_ProfileIndexChanged(object sender, EventArgs e)
        {
            UnloadEnabledChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SaveAction(SpecialAction action, bool edit = false)
        {
            if (profileIndex > 0)
            {
                var profilename = ProfileList.ProfileListCollection[profileIndex - 1].Name;
                if (action.UControls == null) action.UControls = string.Empty;

                Global.Instance.SaveAction(action.Name, action.Controls, 3, profilename, edit,
                    action.UControls +
                    (AutoUntrigger ? (action.UControls.Length > 0 ? "/" : "") + "AutomaticUntrigger" : ""));
            }
        }

        public override bool IsValid(SpecialAction action)
        {
            ClearOldErrors();

            var valid = true;
            var profileIndexErrors = new List<string>();

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