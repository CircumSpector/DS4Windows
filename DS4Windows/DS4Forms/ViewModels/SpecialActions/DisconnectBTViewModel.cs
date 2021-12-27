using System.Collections.Generic;
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels.Util;

namespace DS4WinWPF.DS4Forms.ViewModels.SpecialActions
{
    public class DisconnectBTViewModel : NotifyDataErrorBase
    {
        public double HoldInterval { get; set; }

        public void LoadAction(SpecialActionV3 action)
        {
            HoldInterval = action.DelayTime;
        }

        public void SaveAction(SpecialActionV3 action, bool edit = false)
        {
            Global.Instance.SaveAction(action.Name, action.Controls, 5,
                $"{HoldInterval.ToString("#.##", Global.ConfigFileDecimalCulture)}", edit);
        }

        public override bool IsValid(SpecialActionV3 action)
        {
            ClearOldErrors();

            var valid = true;
            var holdIntervalErrors = new List<string>();

            if (HoldInterval < 0 || HoldInterval > 60)
            {
                holdIntervalErrors.Add("Interval not valid");
                errors["HoldInterval"] = holdIntervalErrors;
                RaiseErrorsChanged("HoldInterval");
            }

            return valid;
        }

        public override void ClearOldErrors()
        {
            if (errors.Count > 0)
            {
                errors.Clear();
                RaiseErrorsChanged("HoldInterval");
            }
        }
    }
}