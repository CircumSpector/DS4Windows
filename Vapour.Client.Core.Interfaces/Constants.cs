namespace Vapour.Client.Core;

public static class Constants
{
    // TODO: use webserver to deliver this tool!
    [Obsolete]
    public static readonly string BezierCurveEditorPath = $"file:///{AppContext.BaseDirectory.Replace('\\', '/')}BezierCurveEditor/index.html";
}