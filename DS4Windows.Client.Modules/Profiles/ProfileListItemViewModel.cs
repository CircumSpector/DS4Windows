using DS4Windows.Client.Core.ViewModel;
using System.Windows.Media;

namespace DS4Windows.Client.Modules.Profiles
{
    public class ProfileListItemViewModel : ViewModel<IProfileListItemViewModel>, IProfileListItemViewModel
    {
        #region Properties

        private string name;
        public string Name
        {
            get => name;
            private set => SetProperty(ref name, value);
        }


        private string outputControllerType;
        public string OutputControllerType
        {
            get => outputControllerType;
            private set => SetProperty(ref outputControllerType, value);
        }


        private SolidColorBrush lightbarColor;
        public SolidColorBrush LightbarColor
        {
            get => lightbarColor;
            private set => SetProperty(ref lightbarColor, value);
        }


        private string touchpadMode;
        public string TouchpadMode
        {
            get => touchpadMode;
            private set => SetProperty(ref touchpadMode, value);
        }


        private string gyroMode;
        public string GyroMode
        {
            get => gyroMode;
            private set => SetProperty(ref gyroMode, value);
        }

        #endregion

        public void SetProfile()
        {

        }
    }
}
