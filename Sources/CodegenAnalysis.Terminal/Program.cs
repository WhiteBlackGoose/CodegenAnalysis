using Terminal.Gui;
using NStack;

Application.Init();
var top = Application.Top;

// Creates the top-level window to show
var win = new Window("MyApp")
{
    X = 0,
    Y = 1, // Leave one row for the toplevel menu

    // By using Dim.Fill(), it will automatically resize without manual intervention
    Width = Dim.Fill(),
    Height = Dim.Fill()
};

top.Add(win);

var tv = new TextView();
tv.Text =
@"
public class ToBench
{
    public ToBench()
    {
        
    }


    [CAAnalyze(10)]
    public int Bench(int a)
    {
        return a * 2;
    }
}
";
tv.ReadOnly = false;
tv.Width = Dim.Fill();
tv.Height = Dim.Fill() - 10;


var output = new TextView();
output.ReadOnly = true;
output.Width = Dim.Fill();
output.Height = 10;

win.Add(tv, output);

// Creates a menubar, the item "New" has a help menu.
var menu = new MenuBar(new MenuBarItem[] {
            new MenuBarItem ("_File", new MenuItem [] {
                new MenuItem ("_New", "Creates new file", null),
                new MenuItem ("_Close", "",null),
                new MenuItem ("_Quit", "", () => { top.Running = false; })
            }),
            new MenuBarItem ("_Edit", new MenuItem [] {
                new MenuItem ("_Copy", "", null),
                new MenuItem ("C_ut", "", null),
                new MenuItem ("_Paste", "", null)
            })
        });
top.Add(menu);


Application.Run();