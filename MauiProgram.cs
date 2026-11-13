using Microsoft.Extensions.Logging;

namespace MauiRealParentRepro;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        // Element zapiše opozorilo prek ILogger -> Debug output
        _ = builder.Logging.AddDebug();
        return builder.Build();
    }
}
