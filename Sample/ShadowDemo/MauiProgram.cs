using Microsoft.Extensions.Logging;
using Sharpnado.Shades;

namespace ShadowDemo;

public static partial class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-SemiBold.ttf", "OpenSansSemiBold");
            })
            .UseSharpnadoShadowsCompat();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}