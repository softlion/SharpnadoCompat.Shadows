using System.ComponentModel;
using Android.Content;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Sharpnado.Shades.Droid;

namespace Sharpnado.Shades.Droid
{
    public class AndroidShadowsRenderer : ViewHandler<Shadows, FrameLayout>
    {
        public static IPropertyMapper<Shadows, AndroidShadowsRenderer> Mapper = new PropertyMapper<Shadows, AndroidShadowsRenderer>() { };
        public static CommandMapper<Shadows, AndroidShadowsRenderer> CommandMapper = new CommandMapper<Shadows, AndroidShadowsRenderer>() { };

        private static int instanceCount;

        private ShadowView _shadowView;

        private string _tag = nameof(AndroidShadowsRenderer);

        protected Shadows Element => VirtualView;

        private LayoutChangeListener _listner;

        public AndroidShadowsRenderer() : base(Mapper, CommandMapper)
        {
            _listner = new LayoutChangeListener(OnLayout);
        }

        protected override void ConnectHandler(FrameLayout platformView)
        {

            base.ConnectHandler(platformView);
            platformView.AddOnLayoutChangeListener(_listner);
            VirtualView.PropertyChanged += OnElementPropertyChanged;

            if (!_shadowView.IsNullOrDisposed())
            {
                _shadowView.RemoveFromParent();
                _shadowView.Dispose();
            }
        }

        protected override void DisconnectHandler(FrameLayout platformView)
        {
            base.DisconnectHandler(platformView);

            if (!_shadowView.IsNullOrDisposed())
            {
                _shadowView.RemoveFromParent();
                _shadowView.Dispose();
            }

            instanceCount--;

            InternalLogger.Debug(_tag, () => $"Disposed( => {instanceCount} instances");
        }

        protected void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Renderer":
                    var content = PlatformView.GetChildAt(0);
                    if (content == null)
                    {
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(Element.StyleId))
                    {
                        _tag += $" | {Element.StyleId}@{Element.InstanceNumber}";
                    }

                    if (_shadowView == null)
                    {
                        _shadowView = new ShadowView(Context, content, Context.ToPixels(Element.CornerRadius));
                        _shadowView.UpdateShades(Element.Shades);

                        Element.WeakCollectionChanged += _shadowView.ShadesSourceCollectionChanged;

                        PlatformView.AddView(_shadowView, 0);

                        instanceCount++;
                        InternalLogger.Debug(_tag, () => $"Create ShadowView => {instanceCount} instances");
                    }

                    break;

                case nameof(Element.CornerRadius):
                    _shadowView.UpdateCornerRadius(Context.ToPixels(Element.CornerRadius));
                    break;

                case nameof(Element.Shades):
                    _shadowView.UpdateShades(Element.Shades);
                    break;
            }
        }


        protected void OnLayout(bool changed, int l, int t, int r, int b)
        {

            InternalLogger.Debug(_tag, () => $"OnLayout( {l}l, {t}t, {r}r, {b}b )");

            var children = PlatformView.GetChildAt(1);
            if (children == null)
            {
                return;
            }

            _shadowView?.Layout(children.MeasuredWidth, children.MeasuredHeight);
        }

        protected override FrameLayout CreatePlatformView()
        {
            return new FrameLayout(Platform.CurrentActivity);
        }
    }

    public class LayoutChangeListener : Java.Lang.Object, Android.Views.View.IOnLayoutChangeListener
    {
        private readonly Action<bool, int, int, int, int> onChanged;

        public LayoutChangeListener(Action<bool, int, int, int, int> onChanged)
        {
            this.onChanged = onChanged;
        }

        public void OnLayoutChange(Android.Views.View v, int left, int top, int right, int bottom, int oldLeft, int oldTop, int oldRight, int oldBottom)
        {
            var changed = left != oldLeft || top != oldTop || right != oldRight || bottom != oldBottom;

            onChanged(changed, left, top, right, bottom);
        }
    }
}
