using System.Collections.Generic;
using DS4Windows;
using DS4WinWPF.DS4Forms.ViewModels.Util;

namespace DS4WinWPF.DS4Forms.ViewModels.SpecialActions
{
    public class SASteeringWheelViewModel : NotifyDataErrorBase
    {
        public double Delay { get; set; }

        public void LoadAction(SpecialActionV3 action)
        {
            Delay = action.DelayTime;
        }

        public void SaveAction(SpecialActionV3 action, bool edit = false)
        {
            Global.Instance.SaveAction(action.Name, action.Controls, 8,
                Delay.ToString("#.##", Global.ConfigFileDecimalCulture), edit);
        }

        public override bool IsValid(SpecialActionV3 action)
        {
            ClearOldErrors();

            var valid = true;
            var delayErrors = new List<string>();

            if (Delay < 0 || Delay > 60)
            {
                delayErrors.Add("Delay out of range");
                errors["Delay"] = delayErrors;
                RaiseErrorsChanged("Delay");
            }

            return valid;
        }

        public override void ClearOldErrors()
        {
            if (errors.Count > 0)
            {
                errors.Clear();
                RaiseErrorsChanged("Delay");
            }
        }
    }
}