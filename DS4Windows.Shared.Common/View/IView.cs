using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.Shared.Common.View
{
    public interface IView<TView> : IView where TView : IView<TView>
    {

    }

    public interface IView
    {
        object DataContext { get; set; }
    }
}
