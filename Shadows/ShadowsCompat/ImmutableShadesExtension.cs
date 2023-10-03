using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace Sharpnado.Shades
{
    [ContentProperty(nameof(Items))]
    [AcceptEmptyServiceProvider]
    public class ImmutableShadesExtension : IMarkupExtension<IReadOnlyCollection<Shade>>
    {
        public ImmutableShadesExtension()
        {
            Items = new List<Shade>();
        }

        public List<Shade> Items { get; }

        public IReadOnlyCollection<Shade> ProvideValue(IServiceProvider serviceProvider)
        {
            if (Items == null)
            {
                return new List<Shade>().AsReadOnly();
            }

            return new ReadOnlyCollection<Shade>(Items);
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return (this as IMarkupExtension<IReadOnlyCollection<Shade>>).ProvideValue(serviceProvider);
        }
    }
}