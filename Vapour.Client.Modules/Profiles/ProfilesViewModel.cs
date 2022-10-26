﻿using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;

using MaterialDesignThemes.Wpf;

using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.Modules.Profiles.Edit;
using Vapour.Client.ServiceClients;
using Vapour.Shared.Configuration.Profiles.Schema;

using Constants = Vapour.Client.Modules.Main.Constants;

namespace Vapour.Client.Modules.Profiles;

public class ProfilesViewModel : NavigationTabViewModel<IProfilesViewModel, IProfilesView>, IProfilesViewModel
{
    private readonly IProfileServiceClient profilesService;
    private readonly IViewModelFactory viewModelFactory;

    public ProfilesViewModel(
        IProfileServiceClient profilesService,
        IViewModelFactory viewModelFactory)
    {
        this.viewModelFactory = viewModelFactory;
        this.profilesService = profilesService;

        AddCommand = new RelayCommand(AddProfile);
        ShareCommand = new RelayCommand<IProfileListItemViewModel>(ShareProfile);
        EditCommand = new RelayCommand<IProfileListItemViewModel>(EditProfile);
        DeleteCommand = new RelayCommand<IProfileListItemViewModel>(DeleteProfile);
    }

    public ObservableCollection<IProfileListItemViewModel> ProfileItems { get; } = new();

    public RelayCommand AddCommand { get; }
    public RelayCommand<IProfileListItemViewModel> ShareCommand { get; }
    public RelayCommand<IProfileListItemViewModel> EditCommand { get; }
    public RelayCommand<IProfileListItemViewModel> DeleteCommand { get; }

    public override async Task Initialize()
    {
        await CreateProfileItems();
    }

    private async void AddProfile()
    {
        var newProfile = await profilesService.CreateNewProfile();
        ShowProfile(newProfile, true);
    }

    private async void ShareProfile(IProfileListItemViewModel profile)
    {
        var sourceProfile = profilesService.ProfileList.SingleOrDefault(p => p.Id == profile.Id);

        var saveFileDialog = new SaveFileDialog
        {
            Filter = "JSON|*.json",
            FileName = sourceProfile.FileName,
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        var result = saveFileDialog.ShowDialog();
        if (result != null && result.Value)
            using (var stream = saveFileDialog.OpenFile())
            {
                var text = JsonSerializer.Serialize(sourceProfile);
                await stream.WriteAsync(Encoding.UTF8.GetBytes(text));
                stream.Close();
            }

        //TODO:  this is the windows 10 native file save picker.  cant get it to work though.  the initializewithwindow is supposed to fix it but doesnt
        //       see https://docs.microsoft.com/en-us/windows/apps/develop/ui-input/display-ui-objects
        //       in section WinUI 3 with C# (also WPF/WinForms with .NET 5 or later)
        //       also see https://docs.microsoft.com/en-us/windows/apps/develop/ui-input/retrieve-hwnd#wpf-with-c

        //var picker = new FileSavePicker();
        //picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        //picker.DefaultFileExtension = ".json";
        //picker.SuggestedFileName = sourceProfile.FileName;

        //var handle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
        //InitializeWithWindow.Initialize(picker, handle);

        //StorageFile file = await picker.PickSaveFileAsync();
        //if (file != null)
        //{
        //    CachedFileManager.DeferUpdates(file);
        //    await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(sourceProfile));
        //    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
        //    if (status == FileUpdateStatus.Complete)
        //    {
        //    }
        //}
    }

    private void EditProfile(IProfileListItemViewModel profile)
    {
        ShowProfile(profilesService.ProfileList.SingleOrDefault(p => p.Id == profile.Id));
    }

    private async void DeleteProfile(IProfileListItemViewModel profile)
    {
        await profilesService.DeleteProfile(profile.Id);
        RemoveProfileItem(profile.Id);
    }

    private async Task CreateProfileItems()
    {
        foreach (var profile in profilesService.ProfileList) await CreateProfileItem(profile);
    }

    private async Task CreateProfileItem(IProfile profile)
    {
        if (!ProfileItems.Any(p => p.Id == profile.Id))
        {
            var profileItemViewModel = await viewModelFactory.CreateViewModel<IProfileListItemViewModel>();
            profileItemViewModel.SetProfile(profile);
            ProfileItems.Add(profileItemViewModel);
        }
    }

    private void RemoveProfileItem(Guid id)
    {
        var existingProfile = ProfileItems.SingleOrDefault(p => p.Id == id);
        if (existingProfile != null)
        {
            existingProfile.Dispose();
            ProfileItems.Remove(existingProfile);
        }
    }

    private async void ShowProfile(IProfile profile, bool isNew = false)
    {
        var editViewModel = await viewModelFactory.Create<IProfileEditViewModel, IProfileEditView>();
        editViewModel.SetProfile(profile, isNew);
        await DialogHost.Show(editViewModel.MainView, Constants.DialogHostName, (o, e) =>
        {
            if (e.Parameter != null) SaveProfile((IProfileEditViewModel)e.Parameter);

            editViewModel.Dispose();
        });
    }

    private async void SaveProfile(IProfileEditViewModel profile)
    {
        var profileToSave = profile.GetUpdatedProfile();
        var savedProfile = await profilesService.SaveProfile(profileToSave);
        RemoveProfileItem(savedProfile.Id);
        await CreateProfileItem(savedProfile);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            foreach (var profile in ProfileItems.ToList())
            {
                profile.Dispose();
                ProfileItems.Remove(profile);
            }

        base.Dispose(disposing);
    }

    #region configuration properties

    //TODO: Change to pull localization values
    public override string Header => "Profiles";

    public override int TabIndex => 2;

    #endregion
}