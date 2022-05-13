using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DS4Windows.Shared.Common.Core;
using DS4Windows.Shared.Configuration.Profiles.Schema;
using Newtonsoft.Json;

namespace DS4Windows.Client.Modules.Profiles.Utils
{
    public class ProfileServiceClient : IProfileServiceClient
    {
        public ObservableCollection<IProfile> ProfileList { get; private set; }

        public async Task Initialize()
        {
            var client = new HttpClient();
            var result = await client.GetAsync(new Uri($"{Constants.HttpUrl}/profile/list"));
            if (result.IsSuccessStatusCode)
            {
                var list = JsonConvert.DeserializeObject<List<ProfileItem>>(
                    await result.Content.ReadAsStringAsync())?.ToList();

                ProfileList = new ObservableCollection<IProfile>(list);
            }
            else
            {
                throw new Exception($"Could not get the profile list {result.ReasonPhrase}");
            }
        }

        public async Task<IProfile> CreateNewProfile()
        {
            var client = new HttpClient();
            var result = await client.GetAsync(new Uri($"{Constants.HttpUrl}/profile/new", UriKind.Absolute));
            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync();
                var profile = JsonConvert.DeserializeObject<ProfileItem>(content);

                return profile;
            }
            else
            {
                throw new Exception($"Could not get new {result.ReasonPhrase}");
            }
        }

        public async Task DeleteProfile(Guid id)
        {
            var client = new HttpClient();
            var result = await client.PostAsync(new Uri($"{Constants.HttpUrl}/profile/delete", UriKind.Absolute), new StringContent(id.ToString()));
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception($"Could not delete profile {result.ReasonPhrase}");
            }

            ProfileList.Remove(ProfileList.Single(i => i.Id == id));
        }

        public async Task<IProfile> SaveProfile(IProfile profile)
        {
            var content = JsonConvert.SerializeObject(profile);
            var client = new HttpClient();
            var result = await client.PostAsync(new Uri($"{Constants.HttpUrl}/profile/save", UriKind.Absolute), new StringContent(content));
            if (result.IsSuccessStatusCode)
            {
                var savedProfile = JsonConvert.DeserializeObject<ProfileItem>(
                    await result.Content.ReadAsStringAsync());

                var existing = ProfileList.SingleOrDefault(i => i.Id == savedProfile.Id);
                if (existing != null)
                {
                    var existingIndex = ProfileList.IndexOf(existing);
                    ProfileList[existingIndex] = savedProfile;
                }
                else
                {
                    ProfileList.Add(savedProfile);
                }

                return savedProfile;
            }

            throw new Exception($"Could not save profile {result.ReasonPhrase}");
        }
    }
}
