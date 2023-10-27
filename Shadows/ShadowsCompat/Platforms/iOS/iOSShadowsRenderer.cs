using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using System.ComponentModel;
using UIKit;

namespace Sharpnado.Shades.iOS
{
    public class iOSShadowsRenderer : ViewHandler<Shadows, UIView>
    {
        public static IPropertyMapper<Shadows, iOSShadowsRenderer> Mapper = new PropertyMapper<Shadows, iOSShadowsRenderer>() { };
        public static CommandMapper<Shadows, iOSShadowsRenderer> CommandMapper = new CommandMapper<Shadows, iOSShadowsRenderer>() { };

        private static int instanceCount;

        private string _tag = nameof(iOSShadowsRenderer);

        private iOSShadowsController _shadowsController;
        protected Shadows Element => VirtualView;
        private CALayer _shadowsLayer;

        public static void Initialize()
        {
            var preserveRenderer = typeof(iOSShadowsRenderer);
        }

        public iOSShadowsRenderer() : base(Mapper, CommandMapper)
        {
        }

        public void LayoutSublayersOfLayer(CALayer layer)
        {
            _shadowsController?.OnLayoutSubLayers();
        }

        protected override void DisconnectHandler(UIView platformView)
        {
            base.DisconnectHandler(platformView);

            _shadowsController?.Dispose();
            _shadowsController = null;

            _shadowsLayer?.Dispose();
            _shadowsLayer = null;

            instanceCount--;

            InternalLogger.Debug(_tag, () => $"Disposed( " +
            $" => {instanceCount} instances");
        }

        protected override void ConnectHandler(UIView platformView)
        {
            base.ConnectHandler(platformView);
            Element.PropertyChanged += OnElementPropertyChanged;

            if (_shadowsController != null)
            {
                _shadowsController?.Dispose();
                _shadowsController = null;

                _shadowsLayer?.Dispose();
                _shadowsLayer = null;
                return;
            }

            if (_shadowsController == null && platformView.Subviews.Length > 0)
            {
                CreateShadowController(platformView.Subviews[0], this.Element);
            }
        }

        protected void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Renderer":
                    break;

                case nameof(Element.CornerRadius):
                    _shadowsController?.UpdateCornerRadius(Element.CornerRadius);
                    break;

                case nameof(Element.Shades):
                    _shadowsController?.UpdateShades(Element.Shades);
                    break;
            }
        }

        private void CreateShadowController(UIView shadowSource, Shadows formsElement)
        {
            this.PlatformView.Layer.BackgroundColor = new CGColor(0, 0, 0, 0);
            this.PlatformView.Layer.MasksToBounds = false;

            _shadowsLayer = new CALayer { MasksToBounds = false };
            this.PlatformView.Layer.InsertSublayer(_shadowsLayer, 0);

            _shadowsController = new iOSShadowsController(shadowSource, _shadowsLayer, formsElement.CornerRadius);
            _shadowsController.UpdateShades(formsElement.Shades);

            instanceCount++;
            InternalLogger.Debug(_tag, () => $"Create ShadowView => {instanceCount} instances");
        }

        protected override UIView CreatePlatformView()
        {
            return new UIViewCustom(LayoutSublayersOfLayer);
        }
    }

    public class UIViewCustom : UIView
    {
        private readonly Action<CALayer> _onLayoutSubLayers;

        public UIViewCustom(Action<CALayer> onSublayers)
        {
            _onLayoutSubLayers = onSublayers;
        }

        public override void LayoutSublayersOfLayer(CALayer layer)
        {
            base.LayoutSublayersOfLayer(layer);
            _onLayoutSubLayers(layer);
        }
    }
}
