using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.Shared.Configuration.Profiles
{
    public static class ProfileConstants
    {
        public static readonly string LocalProfilesLocation = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\DS4Windows\\Profiles";
        public static readonly string GlobalProfilesLocation = $"{AppContext.BaseDirectory}\\Profiles";
        public static readonly string GlobalDefaultProfileLocation = $"{GlobalProfilesLocation}\\default.json";
    }
}
