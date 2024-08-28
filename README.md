# ShadowsCompat 

===================
STATUS:
- Android Handler: NOT WORKING (yet)
- Android Handler: uses RenderScript, which is obsolete and may not work on newer devices
- iOS Handler: NOT TESTED

Closed items:
- Release nuget using github action
- Both the library and the sample project compile and deploy ok, and use only net8 nugets
===================

Forked from the amazingly popular original Sharpnado.Shadows Library, this Compat version aims to ease your migration from Xamarin.Forms to .NET MAUI with a compatible implementation to get you up and running without rewriting the parts of your app that relied on the original library.

Get it from NuGet:

[![Nuget](https://img.shields.io/nuget/v/ShadowsCompat.svg)](https://www.nuget.org/packages/Softlion.ShadowsCompat)

| Supported platforms        |
|----------------------------|
| :heavy_check_mark: Android |
| :heavy_check_mark: iOS     |

![Presentation](Docs/github_banner.png)

## Initialization
Add `.UseSharpnadoShadowsCompat()` to your MAUI app builder.


## Original documentation (obsolete)
https://github.com/roubachof/Sharpnado.Shadows/wiki/Shadows-for-Xamarin.Forms-components-builders.
