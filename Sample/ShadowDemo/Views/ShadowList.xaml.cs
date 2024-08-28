namespace ShadowsSample;

public partial class ShadowList : ContentPage
{
    public ShadowList()
    {
        InitializeComponent();

        ResourcesHelper.SetNeumorphismMode();
    }

    private void OnNavigateToMainPageClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new MainPage());
    }

    private void LogoOnTapped(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }
}