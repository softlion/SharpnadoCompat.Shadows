namespace ShadowsSample;

public partial class MainPageLandscape : ContentPage
{
    public MainPageLandscape()
    {
        SetValue(NavigationPage.HasNavigationBarProperty, false);
        InitializeComponent();

        ResourcesHelper.SetNeumorphismMode();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // BeCreative.OnAppearing();
    }
}