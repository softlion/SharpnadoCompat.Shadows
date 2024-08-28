using System.Reflection;

namespace ShadowsSample.Images;

[ContentProperty(nameof(Source))]
public class ImageResourceExtension : IMarkupExtension
{
    private static readonly Dictionary<string, ImageSource> Cache = new();

    public string Source { get; set; }

    public static ImageSource GetImageSource(string value)
    {
        if (value == null)
        {
            return null;
        }

        if (!value.StartsWith(nameof(Sharpnado)))
        {
            // Add namespace to lookup
            value = "ShadowsDemo.EmbeddedResources.Images." + value;
        }

        if (Cache.TryGetValue(value, out var imageSource))
        {
            return imageSource;
        }

        // Do your translation lookup here, using whatever method you require
        var newImageSource = ImageSource.FromResource(value, typeof(ImageResourceExtension).GetTypeInfo().Assembly);
        Cache.Add(value, newImageSource);
        return newImageSource;
    }

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        if (Source == null)
        {
            return null;
        }

        return GetImageSource(Source);
    }
}