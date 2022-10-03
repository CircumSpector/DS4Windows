using Vapour.Client.Core.View;

namespace Vapour.Client.Core.ViewModel;

public abstract class NavigationTabViewModel<TViewModel, TView> : ViewModel<TViewModel>,
    INavigationTabViewModel<TViewModel, TView>
    where TViewModel : INavigationTabViewModel<TViewModel, TView>
    where TView : IView<TView>
{
    public abstract int TabIndex { get; }
    public abstract string? Header { get; }

    public Type GetViewType()
    {
        return typeof(TView);
    }
}