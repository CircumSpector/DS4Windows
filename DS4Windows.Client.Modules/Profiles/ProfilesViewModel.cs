﻿using DS4Windows.Client.Core.ViewModel;

namespace DS4Windows.Client.Modules.Profiles
{
    public class ProfilesViewModel : NavigationTabViewModel<IProfilesViewModel, IProfilesView>, IProfilesViewModel
    {
        //TODO: Change to pull localization values
        public override string? Header => "Profiles";

        public override int TabIndex => 2;
    }
}