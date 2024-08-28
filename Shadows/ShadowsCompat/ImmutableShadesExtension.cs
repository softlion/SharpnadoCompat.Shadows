using System.Collections.ObjectModel;

namespace Sharpnado.Shades;

[ContentProperty(nameof(Items))]
[AcceptEmptyServiceProvider]
public class ImmutableShadesExtension : IMarkupExtension<IReadOnlyCollection<Shade>>
{
    public List<Shade> Items { get; } = [];

    public IReadOnlyCollection<Shade> ProvideValue(IServiceProvider serviceProvider)
    {
        if (Items == null)
            return new List<Shade>();

        return new ReadOnlyCollection<Shade>(Items);
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}