using DS4Windows.Shared.Common.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4WinWPF.DS4Forms.Views
{
    public interface IControllersView : IView<IControllersView>
    {
        void SetSort();
        void ChangeControllerPanel(int count);
        void SetIndex(int index);

    }
}
