// See https://aka.ms/new-console-template for more information
using System.Text;
using lightningcss;
var targets = LightningCss.BrowserlistToTargets("last 2 versions, not IE <= 11"u8.ToArray());
Console.WriteLine(targets);
var result = LightningCss.Transform("""
    .foo {
        color: lch(50.998% 135.363 338);
    }
    """u8.ToArray(), new()
{
    Filename = "test.css"u8.ToArray(),
    CssModulesPattern = [],
    CssModules = true,
    CssModulesDashedIdents = true
}, new()
{
    UnusedSymbols = ["bar"u8.ToArray()],
    Targets = targets
}, new());
Console.WriteLine(Encoding.UTF8.GetString(result.Code));
foreach (var item in result.Exports)
{
    Console.WriteLine(Encoding.UTF8.GetString(item.Exported));
    Console.WriteLine(Encoding.UTF8.GetString(item.Local));
}