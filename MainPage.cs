using System.Runtime.CompilerServices;
using System.Text;

namespace MauiRealParentRepro;

public class MainPage : ContentPage
{
    // Močne reference na otroke (starši NIMAJO močnih referenc iz kode).
    static View? _childA, _childB;
    // Šibki referenci na starše, da izmerimo, ali jih je GC res pobral.
    static WeakReference? _parentA, _parentB;

    readonly Label _result = new() { Text = "Klikni gumb (Release build!) in glej tudi Output/Debug okno." };

    public MainPage()
    {
        var btn = new Button { Text = "Sproži repro" };
        btn.Clicked += (_, _) => Run();

        Content = new VerticalStackLayout
        {
            Padding = 24,
            Spacing = 12,
            Children = { btn, _result }
        };
    }

    void Run()
    {
        CreateA();
        CreateB();

        for (int i = 0; i < 5; i++)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
        }

        var sb = new StringBuilder();
        // 1) najprej samo izmerimo, ali je starš živ (ne dotikamo se še .Parent)
        sb.AppendLine($"A (VerticalStackLayout.Children): parent alive = {_parentA!.IsAlive}");
        sb.AppendLine($"B (child.Parent = new ContentView()): parent alive = {_parentB!.IsAlive}");

        // 2) šele nato preberemo .Parent; če je starš pobran, mora MAUI zapisati opozorilo
        sb.AppendLine($"A Parent = {_childA!.Parent?.ToString() ?? "null"}");
        sb.AppendLine($"B Parent = {_childB!.Parent?.ToString() ?? "null"}");

        _result.Text = sb.ToString();
        System.Diagnostics.Debug.WriteLine(_result.Text);
    }

    // A: običajna pot prek Layout.Children
    [MethodImpl(MethodImplOptions.NoInlining)]
    static void CreateA()
    {
        var child = new Label { Text = "A" };
        var parent = new VerticalStackLayout { Children = { child } };
        _childA = child;
        _parentA = new WeakReference(parent);
    }

    // B: starš nastavljen neposredno prek Parent, brez Children/Content in layouta
    [MethodImpl(MethodImplOptions.NoInlining)]
    static void CreateB()
    {
        var child = new Label { Text = "B" };
        var parent = new ContentView();
        child.Parent = parent;
        _childB = child;
        _parentB = new WeakReference(parent);
    }
}
