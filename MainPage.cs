using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Maui.Controls.Shapes;

// NOTE: set this namespace / class name to match your existing App.cs (MainPage is referenced there).
namespace MauiRealParentRepro;

public class MainPage : ContentPage
{
    // Children are kept alive here (stands in for a ViewModel, static resource, popup content, cache...).
    static readonly List<View> Keep = new();

    // One entry per child type seen in the linked dotnet/maui issues.
    static readonly (string Name, Func<View> Create)[] Cases =
    {
        ("Button",              () => new Button { Text = "x" }),
        ("Label",               () => new Label { Text = "x" }),
        ("BoxView",             () => new BoxView()),
        ("Grid",                () => new Grid()),
        ("StackLayout",         () => new StackLayout()),
        ("VerticalStackLayout", () => new VerticalStackLayout()),
        ("ScrollView",          () => new ScrollView()),
        ("Border",              () => new Border()),
        ("Rectangle",           () => new Microsoft.Maui.Controls.Shapes.Rectangle()),
        ("RoundRectangle",      () => new RoundRectangle()),
        ("Image",               () => new Image()),
        ("CollectionView",      () => new CollectionView()),
    };

    readonly Label _result = new() { Text = "Press the button (run in RELEASE)." };

    public MainPage()
    {
        var run = new Button { Text = "Run all (A + B)" };
        run.Clicked += async (_, _) => await RunAll();

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 12,
                Children = { new Label { Text = "RealParent GC repro", FontSize = 22 }, run, _result }
            }
        };
    }

    // Scenario A: child added via Children, parent then becomes unreachable.
    // Scenario B: child.Parent = new ContentView() (no Children involved).
    [MethodImpl(MethodImplOptions.NoInlining)]
    static (WeakReference<Element> ParentRef, View Child) Build(Func<View> create, bool viaChildren)
    {
        var child = create();
        Element parent;
        if (viaChildren)
        {
            var p = new VerticalStackLayout();
            p.Children.Add(child);
            parent = p;
        }
        else
        {
            parent = new ContentView();
            child.Parent = parent;
        }
        return (new WeakReference<Element>(parent), child);
    }

    async Task RunAll()
    {
        var sb = new StringBuilder();
        foreach (var (scenario, viaChildren) in new[] { ("A (Children.Add)", true), ("B (Parent = ...)", false) })
        {
            sb.AppendLine($"--- Scenario {scenario} ---");
            foreach (var (name, create) in Cases)
            {
                var (parentRef, child) = Build(create, viaChildren);
                Keep.Add(child);

                for (int i = 0; i < 3; i++)
                {
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true);
                    GC.WaitForPendingFinalizers();
                    await Task.Delay(50);
                }

                bool alive = parentRef.TryGetTarget(out _);
                var parentNow = child.Parent;   // dead weak ref -> MAUI logs the RealParent warning here

                var line = $"{name}: parent alive = {alive}, child.Parent = {(parentNow?.GetType().Name ?? "null")}"
                         + (!alive ? "  <-- warning expected" : "");
                sb.AppendLine(line);
                Debug.WriteLine(line);
                Console.WriteLine(line);
            }
        }
        _result.Text = sb.ToString();
    }
}
