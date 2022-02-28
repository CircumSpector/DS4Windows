using DS4Windows.Client.Modules.Profiles.Interfaces;
using System.Windows.Controls;

namespace DS4Windows.Client.Modules.Profiles
{
    /// <summary>
    /// Interaction logic for ProfilesView.xaml
    /// </summary>
    public partial class ProfilesView : Page, IProfilesView
    {
        public ProfilesView()
        {
            InitializeComponent();
        }
    }
}
