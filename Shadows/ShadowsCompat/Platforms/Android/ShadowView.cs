﻿namespace Sharpnado.Shades.Droid;

using Android.Content;
using Android.Graphics;
using Android.Renderscripts;
using Android.Runtime;
using Android.Util;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Platform;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Element = Android.Renderscripts.Element;
using Paint = Android.Graphics.Paint;
using RectF = Android.Graphics.RectF;
using View = Android.Views.View;

internal partial class ShadowView : View
{
    private const int MinimumSize = 5;
    private const int MaxRadius = 100;

    private static int instanceCount = 0;

    private readonly JniWeakReference<View> _weakSource;
    private readonly RenderScript _renderScript;
    private readonly Dictionary<Shade, ShadeInfo> _shadeInfos;

    private readonly BitmapCache _cache;

    private bool _isDisposed;
    
    public View? ShadowSource => _weakSource.TryGetTarget(out var view) ? view : null;

    public ShadowView(Context context, View shadowSource, float cornerRadius, string? tag = null)
        : base(context)
    {
        _renderScript = RenderScript.Create(context);
        _weakSource = new JniWeakReference<View>(shadowSource);

        _cache = BitmapCache.Instance;

        _shadeInfos = new Dictionary<Shade, ShadeInfo>();
        _cornerRadius = cornerRadius;

        LogTag = !string.IsNullOrEmpty(tag) ? $"{nameof(ShadowView)}@{tag}" : nameof(ShadowView);

        InternalLogger.Debug(LogTag, () => $"ShadowView(): {++instanceCount} instances");
    }

    public ShadowView(Context context, IAttributeSet attrs)
        : base(context, attrs)
    {
    }

    public ShadowView(Context context, IAttributeSet attrs, int defStyleAttr)
        : base(context, attrs, defStyleAttr)
    {
    }

    public ShadowView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes)
        : base(context, attrs, defStyleAttr, defStyleRes)
    {
    }

    protected ShadowView(IntPtr javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }

    public static Predicate<View> HasMinimumSize =>
        view => view.MeasuredWidth >= MinimumSize && view.MeasuredHeight >= 5;

    public string LogTag { get; }

    public void Layout(int width, int height)
    {
        if (width <= MinimumSize || height <= MinimumSize)
        {
            return;
        }

        InternalLogger.Debug(LogTag, () => $"Layout( width: {width}, height: {height} )");

        Measure(width, height);
        Layout(0, 0, width, height);
    }

    protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
    {
        base.OnSizeChanged(w, h, oldw, oldh);

        if (w <= MinimumSize || h <= MinimumSize || _isDisposed)
        {
            return;
        }

        if (_weakSource.TryGetTarget(out var source) && (w != oldw || h != oldh))
        {
            InternalLogger.Debug(LogTag, () => $"OnSizeChanged( {source.MeasuredWidth}w, {source.MeasuredHeight}h )");

            RefreshBitmaps();
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        InternalLogger.Debug(LogTag, $"Dispose( disposing: {disposing} )");

        if (_shadesSource is INotifyCollectionChanged shadeNotifyCollection)
        {
            shadeNotifyCollection.CollectionChanged -= ShadesSourceCollectionChanged;
        }

        if (_renderScript != null)
            _renderScript.Destroy();

        DisposeBitmaps();

        _isDisposed = true;
    }

    protected override void OnDraw(Canvas canvas)
    {
#if DEBUG
        var stopWatch = new Stopwatch();
        stopWatch.Start();
#endif
        if (!_weakSource.TryGetTarget(out var source) || _renderScript == null)
            return;

        foreach (var shadeInfo in _shadeInfos.Values)
        {
            var shadow = _cache.GetOrCreate(shadeInfo.Hash, () => CreateBitmap(shadeInfo));

            var x = source.GetX() + shadeInfo.OffsetX - MaxRadius;
            var y = source.GetY() + shadeInfo.OffsetY - MaxRadius;

            InternalLogger.Debug(LogTag, () => $"OnDraw( {x}x, {y}y )");

            canvas.DrawBitmap(shadow, x, y, null);
        }

        base.OnDraw(canvas);
#if DEBUG
        LogPerf(LogTag, stopWatch);
#endif
    }

    private static void LogPerf(string tag, Stopwatch stopwatch, [CallerMemberName] string methodName = "caller")
    {
        InternalLogger.Debug(tag, () => $"{methodName}: ran in {stopwatch.ElapsedMilliseconds:0000} ms");
    }

    private void RefreshBitmaps()
    {
#if DEBUG
        var stopWatch = new Stopwatch();
        stopWatch.Start();
#endif
        DisposeBitmaps();

        if (!_weakSource.TryGetTarget(out var source) || !HasMinimumSize(source))
        {
            return;
        }

        InternalLogger.Debug(LogTag, "RefreshBitmaps()");
        foreach (var shade in _shadesSource)
        {
            InsertBitmap(shade);
        }
#if DEBUG
        LogPerf(LogTag, stopWatch);
#endif
    }

    private void RefreshBitmap(Shade shade)
    {
#if DEBUG
        var stopWatch = new Stopwatch();
        stopWatch.Start();
#endif
        DisposeBitmaps();

        if (!_weakSource.TryGetTarget(out var source) || !HasMinimumSize(source))
        {
            return;
        }

        InternalLogger.Debug(LogTag, $"RefreshBitmap( shade: {shade} )");
        if (_shadeInfos.TryGetValue(shade, out var shadeInfo))
        {
            _shadeInfos.Remove(shade);
            _cache.Remove(shadeInfo.Hash);
        }

        InsertBitmap(shade);
#if DEBUG
        LogPerf(LogTag, stopWatch);
#endif
    }

    private void InsertBitmap(Shade shade)
    {
        if (!_weakSource.TryGetTarget(out var source)
            || !HasMinimumSize(source)
            || _renderScript == null)
        {
            return;
        }
#if DEBUG
        var stopWatch = new Stopwatch();
        stopWatch.Start();
#endif
        InternalLogger.Debug(LogTag, () => $"InsertBitmap( shade: {shade}, sourceWidth: {source.MeasuredWidth}, sourceHeight: {source.MeasuredHeight})");

        var shadeInfo = ShadeInfo.FromShade(Context, shade, _cornerRadius, source);
        _shadeInfos.Add(shade, shadeInfo);

        _cache.Add(shadeInfo.Hash, () => CreateBitmap(shadeInfo));
#if DEBUG
        LogPerf(LogTag, stopWatch);
#endif
    }

    private Bitmap CreateBitmap(ShadeInfo shadeInfo)
    {
#if DEBUG
        var stopWatch = new Stopwatch();
        stopWatch.Start();
#endif
        var shadow = Bitmap.CreateBitmap(
            shadeInfo.Width,
            shadeInfo.Height,
            Bitmap.Config.Argb8888);

        InternalLogger.Debug(LogTag, () => $"CreateBitmap( shadeInfo: {shadeInfo} )");
        var rect = new RectF(
            ShadeInfo.Padding,
            ShadeInfo.Padding,
            shadeInfo.Width - ShadeInfo.Padding,
            shadeInfo.Height - ShadeInfo.Padding);

        using var bitmapCanvas = new Canvas(shadow);
        using var paint = new Paint { Color = shadeInfo.Color };
        bitmapCanvas.DrawRoundRect(
            rect,
            _cornerRadius,
            _cornerRadius,
            paint);

        if (shadeInfo.BlurRadius < 1)
        {
            return shadow;
        }

        const int MaxBlur = 25;
        float blurAmount = shadeInfo.BlurRadius > MaxRadius ? MaxRadius : shadeInfo.BlurRadius;
        while (blurAmount > 0)
        {
            var input = Allocation.CreateFromBitmap(_renderScript, shadow, Allocation.MipmapControl.MipmapNone, AllocationUsage.Script);
            var output = Allocation.CreateTyped(_renderScript, input.Type);
            var script = ScriptIntrinsicBlur.Create(_renderScript, Element.U8_4(_renderScript));

            float blurRadius;
            if (blurAmount > MaxBlur)
            {
                blurRadius = MaxBlur;
                blurAmount -= MaxBlur;
            }
            else
            {
                blurRadius = blurAmount;
                blurAmount = 0;
            }

            script.SetRadius(blurRadius);
            script.SetInput(input);
            script.ForEach(output);
            output.CopyTo(shadow);
        }
#if DEBUG
        LogPerf(LogTag, stopWatch);
#endif
        return shadow;
    }

    private void DisposeBitmap(Shade shade)
    {
#if DEBUG
        var stopWatch = new Stopwatch();
        stopWatch.Start();
#endif
        InternalLogger.Debug(LogTag, () => $"DisposeBitmap( shade: {shade} )");
        var shadeInfo = _shadeInfos[shade];
        _shadeInfos.Remove(shade);

        _cache.Remove(shadeInfo.Hash);
#if DEBUG
        LogPerf(LogTag, stopWatch);
#endif
    }

    private void DisposeBitmaps()
    {
        if (_shadeInfos.Count == 0)
        {
            return;
        }
#if DEBUG
        var stopWatch = new Stopwatch();
        stopWatch.Start();
#endif
        InternalLogger.Debug(LogTag, () => $"DisposeBitmaps()");
        foreach (var shadeInfo in _shadeInfos.Values)
        {
            _cache.Remove(shadeInfo.Hash);
        }

        _shadeInfos.Clear();
#if DEBUG
        LogPerf(LogTag, stopWatch);
#endif
    }

    private void UpdateShadeInfo(Shade shade)
    {
        if (!_weakSource.TryGetTarget(out var source))
        {
            return;
        }

        InternalLogger.Debug(LogTag, () => $"UpdateShadeInfo( shade: {shade} )");

        _shadeInfos[shade] = ShadeInfo.FromShade(Context, shade, _cornerRadius, source);
    }

    private readonly struct ShadeInfo
    {
        public const int Padding = MaxRadius;

        private ShadeInfo(Android.Graphics.Color color, float blurRadius, float offsetX, float offsetY, float cornerRadius, int width, int height)
        {
            Color = color;
            BlurRadius = blurRadius;
            OffsetX = offsetX;
            OffsetY = offsetY;
            CornerRadius = cornerRadius;
            Width = width;
            Height = height;
            Hash = $"{Width}:{Height},{Color},{BlurRadius},{CornerRadius}";
        }

        public Android.Graphics.Color Color { get; }

        public float BlurRadius { get; }

        public float OffsetX { get; }

        public float OffsetY { get; }

        public float CornerRadius { get; }

        public int Width { get; }

        public int Height { get; }

        public string Hash { get; }

        public static ShadeInfo FromShade(Context context, Shade shade, float cornerRadius, View shadowsSource)
        {
            // float blurCoeff = 1f + (float)shade.BlurRadius / 20f;

            return new ShadeInfo(
                shade.Color.MultiplyAlpha((float)shade.Opacity).ToAndroid(),
                context.ToPixels(shade.BlurRadius) * 2,
                context.ToPixels(shade.Offset.X),
                context.ToPixels(shade.Offset.Y),
                cornerRadius,
                shadowsSource.MeasuredWidth + 2 * Padding,
                shadowsSource.MeasuredHeight + 2 * Padding);
        }

        public override string ToString() =>
            $"ShadeInfo( Offset: {OffsetX};{OffsetY}, Size: {Width}x{Height}, Color: {Color}, BlurRadius: {BlurRadius} )";
    }
}
