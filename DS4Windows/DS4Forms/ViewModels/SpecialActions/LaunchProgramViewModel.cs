using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DS4Windows;
using DS4WinWPF.DS4Control;
using DS4WinWPF.DS4Forms.ViewModels.Util;
using JetBrains.Annotations;

namespace DS4WinWPF.DS4Forms.ViewModels.SpecialActions
{
    public class LaunchProgramViewModel : NotifyDataErrorBase, INotifyPropertyChanged
    {
        public LaunchProgramViewModel()
        {
            SpecialAction.PropertyChanged += SpecialActionOnPropertyChanged;
        }

        public SpecialActionProgram SpecialAction { get; set; } = new();

        public ImageSource ProgramIcon
        {
            get
            {
                ImageSource exeIcon = null;
                var path = SpecialAction.FilePath;
                if (!File.Exists(path) || Path.GetExtension(path).ToLower() != ".exe") return exeIcon;
                using var ico = Icon.ExtractAssociatedIcon(path);
                exeIcon = Imaging.CreateBitmapSourceFromHIcon(
                    ico.Handle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                );
                exeIcon.Freeze();

                return exeIcon;
            }
        }

        public string ProgramName =>
            string.IsNullOrEmpty(SpecialAction.FilePath) ? string.Empty : Path.GetFileNameWithoutExtension(SpecialAction.FilePath);

        public event PropertyChangedEventHandler PropertyChanged;

        public void LoadAction(SpecialActionV3 action)
        {
            //FilePath = action.Details;
            //Delay = action.DelayTime;
            //Arguments = action.Extras;
        }

        public void SaveAction(SpecialActionV3 action, bool edit = false)
        {
            //Global.Instance.SaveAction(action.Name, action.Controls, 2,
            //    $"{FilePath}?{Delay.ToString("#.##", Global.ConfigFileDecimalCulture)}", edit, Arguments);
        }

        public override bool IsValid(SpecialActionV3 action)
        {
            ClearOldErrors();

            var valid = true;
            var filepathErrors = new List<string>();

            if (SpecialAction.FilePath.Length == 0)
                filepathErrors.Add("FilePath empty");
            else if (!File.Exists(SpecialAction.FilePath)) filepathErrors.Add("File at path does not exist");

            if (filepathErrors.Count > 0)
            {
                errors["FilePath"] = filepathErrors;
                RaiseErrorsChanged("FilePath");
            }

            return valid;
        }

        public override void ClearOldErrors()
        {
            if (errors.Count > 0)
            {
                errors.Clear();
                RaiseErrorsChanged("FilePath");
            }
        }

        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            switch (propertyName)
            {
                case nameof(SpecialAction):
                    SpecialAction.PropertyChanged += SpecialActionOnPropertyChanged;
                    OnPropertyChanged(nameof(ProgramName));
                    OnPropertyChanged(nameof(ProgramIcon));
                    break;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SpecialActionOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SpecialAction.FilePath):
                    OnPropertyChanged(nameof(ProgramName));
                    OnPropertyChanged(nameof(ProgramIcon));
                    break;
            }
        }
    }
}