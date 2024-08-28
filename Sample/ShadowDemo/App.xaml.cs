using ShadowsSample;

namespace ShadowDemo;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        UserAppTheme = PlatformAppTheme;

        MainPage = new MainPage();
        if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            MainPage = new MainPageLandscape();
        else
            MainPage = new NavigationPage(new MainPage());
    }
}