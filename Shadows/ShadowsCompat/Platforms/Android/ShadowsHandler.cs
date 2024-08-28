using Android.Views;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Sharpnado.Shades.Droid;

/// <summary>
/// TODO: follow the pattern used in the link below, which correctly sets up a ContentView
/// https://github.com/dotnet/maui/blob/0ec9b63ccf21038f6558243c781a6ce5f5a22af1/src/Core/src/Handlers/Border/BorderHandler.Android.cs
/// TODO: for the shows items, follow the pattern used in the link below to get more common code between platforms:
/// https://github.com/vapolia/SegmentedViews
/// </summary>
public class ShadowsHandler : ViewHandler<IShadows, ContentViewGroup>
{
    public static IPropertyMapper<IShadows, ShadowsHandler> Mapper = new PropertyMapper<IShadows, ShadowsHandler>(ViewMapper)
    {
        [nameof(IShadows.CornerRadius)] = MapCornerRadius,
        [nameof(IShadows.Shades)] = MapShades,
    };

    public static CommandMapper<Shadows, ShadowsHandler> CommandMapper = new();

    private ShadowView? shadowView;
    private string tag = nameof(ShadowsHandler);

    public ShadowsHandler() : this(Mapper)
    {
    }

    public ShadowsHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper)
    {
    }
    
    public override bool NeedsContainer => false;
    protected override ContentViewGroup CreatePlatformView()
    {
        var viewGroup = new ContentViewGroup(Context) { CrossPlatformLayout = VirtualView };
        viewGroup.SetLayerType(Android.Views.LayerType.Hardware, null);

        return viewGroup;
    }

    public override void SetVirtualView(IView view)
    {
        base.SetVirtualView(view);
        PlatformView.CrossPlatformLayout = VirtualView;
    }

    protected override void ConnectHandler(ContentViewGroup platformView)
    {
        base.ConnectHandler(platformView);

        if (!string.IsNullOrWhiteSpace(VirtualView.StyleId))
            tag += $" | {VirtualView.StyleId}@{VirtualView.InstanceNumber}";

        PlatformView.LayoutChange += OnPlatformViewOnLayoutChange;
        PlatformView.ChildViewAdded += PlatformViewOnChildViewAdded;
        PlatformView.ChildViewRemoved += PlatformViewOnChildViewRemoved;
    }

    private void PlatformViewOnChildViewAdded(object? sender, ViewGroup.ChildViewAddedEventArgs e)
    {
        if (e.Child != shadowView)
        {
            var contentView = e.Child;
        
            if (shadowView != null)
                ClearShadowView();

            if (contentView == null)
                return;

            shadowView = new ShadowView(Context, contentView, Context.ToPixels(VirtualView.CornerRadius));
            shadowView.UpdateShades(VirtualView.Shades);
            PlatformView.AddView(shadowView, 0);
        
            VirtualView.WeakCollectionChanged += shadowView.ShadesSourceCollectionChanged;
        }
    }

    private void PlatformViewOnChildViewRemoved(object? sender, ViewGroup.ChildViewRemovedEventArgs e)
    {
        if (e.Child != shadowView)
            ClearShadowView();
    }

    private void OnPlatformViewOnLayoutChange(object? sender, Android.Views.View.LayoutChangeEventArgs args)
    {
        InternalLogger.Debug(tag, () => $"OnLayout( {args.Left}l, {args.Top}t, {args.Right}r, {args.Bottom}b )");

        var children = ((ContentViewGroup)sender!).GetChildAt(1);
        if (children == null)
            return;

        shadowView?.Layout(children.MeasuredWidth, children.MeasuredHeight);
    }

    protected override void DisconnectHandler(ContentViewGroup platformView)
    {
        base.DisconnectHandler(platformView);

        platformView.LayoutChange -= OnPlatformViewOnLayoutChange;
        ClearShadowView();
    }

    void ClearShadowView()
    {
        if (shadowView != null)
        {
            shadowView.RemoveFromParent();
            shadowView.Dispose();
            shadowView = null;
        }
    }
    
    static void MapCornerRadius(ShadowsHandler handler, IShadows virtualView)
    {
        handler.shadowView?.UpdateCornerRadius(handler.Context.ToPixels(virtualView.CornerRadius));
    }
    
    static void MapShades(ShadowsHandler handler, IShadows virtualView)
    {
        handler.shadowView?.UpdateShades(virtualView.Shades);
    }
}