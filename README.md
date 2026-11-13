# MAUI RealParent GC repro (dotnet/maui#34177)

NEPREVERJENA minimalna reprodukcija opozorila
"The RealParent on ... has been Garbage Collected".

Element drži starša v WeakReference<Element> (_realParent). Če otrok preživi,
starš pa je pobran, `child.Parent` zapiše to opozorilo.

Dva scenarija:
 A) otrok dodan v VerticalStackLayout.Children
 B) `child.Parent = new ContentView()` (brez layouta/Children)
Za vsakega izpiše, ali je starš po GC še živ, nato prebere .Parent.

## Zagon (Windows, .NET 10 SDK + MAUI workload), RELEASE
    dotnet workload install maui
    dotnet run -c Release -f net10.0-windows10.0.19041.0
Opozorilo išči v Visual Studio Output (Debug) ali v dotnet izpisu.

Če je "parent alive = True", starša nekaj drži (to je že sama po sebi informacija);
pri "False" mora branje .Parent zapisati opozorilo.
