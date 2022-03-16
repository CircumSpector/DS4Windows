using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Configuration.Profiles.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DS4Windows.Client.Modules.Profiles
{
    public interface IProfileListItemViewModel : IViewModel<IProfileListItemViewModel>
    {
        Guid Id { get; }
        string Name { get; }
        string OutputControllerType { get; }
        SolidColorBrush LightbarColor { get; }
        string TouchpadMode { get; }
        string GyroMode { get; }

        void SetProfile(IProfile profile);
    }
}
