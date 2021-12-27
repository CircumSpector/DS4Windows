using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels.Util;

namespace DS4WinWPF.DS4Forms.ViewModels.SpecialActions
{
    public class LaunchProgramViewModel : NotifyDataErrorBase
    {
        private string filepath;

        public LaunchProgramViewModel()
        {
            FilepathChanged += LaunchProgramViewModel_FilepathChanged;
        }

        public string Filepath
        {
            get => filepath;
            set
            {
                if (filepath == value) return;
                filepath = value;
                FilepathChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double Delay { get; set; }

        public string Arguments { get; set; }

        public ImageSource ProgramIcon
        {
            get
            {
                ImageSource exeicon = null;
                var path = filepath;
                if (File.Exists(path) && Path.GetExtension(path).ToLower() == ".exe")
                    using (var ico = Icon.ExtractAssociatedIcon(path))
                    {
                        exeicon = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                        exeicon.Freeze();
                    }

                return exeicon;
            }
        }

        public string ProgramName
        {
            get
            {
                var temp = "";
                if (!string.IsNullOrEmpty(filepath)) temp = Path.GetFileNameWithoutExtension(filepath);

                return temp;
            }
        }

        public event EventHandler FilepathChanged;
        public event EventHandler ProgramIconChanged;
        public event EventHandler ProgramNameChanged;

        private void LaunchProgramViewModel_FilepathChanged(object sender, EventArgs e)
        {
            ProgramIconChanged?.Invoke(this, EventArgs.Empty);
            ProgramNameChanged?.Invoke(this, EventArgs.Empty);
        }

        public void LoadAction(SpecialActionV3 action)
        {
            filepath = action.Details;
            Delay = action.DelayTime;
            Arguments = action.Extras;
        }

        public void SaveAction(SpecialActionV3 action, bool edit = false)
        {
            Global.Instance.SaveAction(action.Name, action.Controls, 2,
                $"{filepath}?{Delay.ToString("#.##", Global.ConfigFileDecimalCulture)}", edit, Arguments);
        }

        public override bool IsValid(SpecialActionV3 action)
        {
            ClearOldErrors();

            var valid = true;
            var filepathErrors = new List<string>();

            if (filepath.Length == 0)
                filepathErrors.Add("Filepath empty");
            else if (!File.Exists(filepath)) filepathErrors.Add("File at path does not exist");

            if (filepathErrors.Count > 0)
            {
                errors["Filepath"] = filepathErrors;
                RaiseErrorsChanged("Filepath");
            }

            return valid;
        }

        public override void ClearOldErrors()
        {
            if (errors.Count > 0)
            {
                errors.Clear();
                RaiseErrorsChanged("Filepath");
            }
        }
    }
}