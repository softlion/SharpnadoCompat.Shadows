using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Sharpnado.Shades;

public interface IShadows : IContentView
{
    int CornerRadius { get; }
    IEnumerable<Shade> Shades { get; }
    
    internal string StyleId { get; }
    internal int InstanceNumber { get; }
    internal event EventHandler<NotifyCollectionChangedEventArgs> WeakCollectionChanged;
}

public class Shadows : ContentView, IShadows
{
    public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(int), typeof(Shadows), 0);
    public static readonly BindableProperty ShadesProperty = BindableProperty.Create(nameof(Shades), typeof(IEnumerable<Shade>), typeof(Shadows),
        propertyChanged: ShadesPropertyChanged, coerceValue: CoerceShades,
        defaultValueCreator: bo => new ObservableCollection<Shade> { new() { Parent = (Shadows)bo } },
        validateValue: (bo, v) => v is IEnumerable<Shade>);

    private static int instanceCount = 0;
    private readonly WeakEventManager _weakCollectionChangedSource = new ();

    int IShadows.InstanceNumber { get; } = ++instanceCount;

    event EventHandler<NotifyCollectionChangedEventArgs> IShadows.WeakCollectionChanged
    {
        add => _weakCollectionChangedSource.AddEventHandler(value);
        remove => _weakCollectionChangedSource.RemoveEventHandler(value);
    }

    public int CornerRadius
    {
        get => (int)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public IEnumerable<Shade> Shades
    {
        get => (IEnumerable<Shade>)GetValue(ShadesProperty);
        set => SetValue(ShadesProperty, value);
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        
        foreach (var shade in Shades)
            shade.BindingContext = BindingContext;
    }

    private static object CoerceShades(BindableObject bindable, object value)
    {
        if (value is not ReadOnlyCollection<Shade> readonlyCollection)
            return value;
        
        return new ReadOnlyCollection<Shade>(readonlyCollection.Select(s => s.Clone()).ToList());
    }

    private static void ShadesPropertyChanged(BindableObject bindable, object? oldValue, object newValue)
    {
        var shadows = (Shadows)bindable;

        if (oldValue is IEnumerable<Shade> oldShades)
        {
            if (oldValue is INotifyCollectionChanged oldCollection)
                oldCollection.CollectionChanged -= shadows.OnShadeCollectionChanged;

            foreach (var shade in oldShades)
            {
                shade.Parent = null;
                shade.BindingContext = null;
            }
        }

        var newShades = (IEnumerable<Shade>)newValue;
        foreach (var shade in newShades)
        {
            shade.Parent = shadows;
            shade.BindingContext = shadows.BindingContext;
        }

        if (newValue is INotifyCollectionChanged newCollection)
            newCollection.CollectionChanged += shadows.OnShadeCollectionChanged;
    }

    private void OnShadeCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var hasChanged = false;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    foreach (Shade newShade in e.NewItems)
                    {
                        newShade.Parent = this;
                        newShade.BindingContext = BindingContext;
                        hasChanged = true;
                    }
                }
                break;

            case NotifyCollectionChangedAction.Reset:
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                {
                    foreach (Shade oldShade in e.OldItems)
                    {
                        oldShade.Parent = null;
                        oldShade.BindingContext = null;
                        hasChanged = true;
                    }
                }
                break;
        }

        if(hasChanged)
            _weakCollectionChangedSource.HandleEvent(this, e, nameof(IShadows.WeakCollectionChanged));
    }
}