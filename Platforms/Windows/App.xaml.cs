namespace MauiRealParentRepro.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        this.InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => global::MauiRealParentRepro.MauiProgram.CreateMauiApp();
}
