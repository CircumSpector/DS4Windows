
namespace DS4Windows.Client.Core.View
{
    public interface IView<TView> : IView where TView : IView<TView>
    {

    }

    public interface IView
    {
        object DataContext { get; set; }
    }
}
