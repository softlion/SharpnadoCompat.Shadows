using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Platform;
using System.Runtime.InteropServices;
using UIKit;

namespace Sharpnado.Shades.iOS
{
    public static class ShadeExtensions
    {
        public static CALayer ToCALayer(this Shade shade)
        {
            return new CALayer
                {
                    ShadowColor = shade.Color.ToCGColor(),
                    ShadowRadius = (NFloat)shade.BlurRadius / UIScreen.MainScreen.Scale,
                    ShadowOffset = new CGSize(shade.Offset.X, shade.Offset.Y),
                    ShadowOpacity = (float)shade.Opacity,
                    MasksToBounds = false,
                    RasterizationScale = UIScreen.MainScreen.Scale,
                    ShouldRasterize = true,
                };
        }
    }
}