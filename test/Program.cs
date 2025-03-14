// See https://aka.ms/new-console-template for more information
using lightningcss;
try
{
    var targets = LightningCss.BrowserlistToTargets("last 2 versions, not IE <= 11"u8);
    Console.WriteLine(targets);
    var result = LightningCss.Transform(new()
    {
        Code = /* language=css */ """
       .foo {
           color: lch(50.998% 135.363 338);
       }
       """u8,
        Filename = "test.css"u8,
        CssModulesPattern = [],
        CssModules = true,
        CssModulesDashedIdents = true,
        UnusedSymbols = ["bar"u8.ToArray()],
        Targets = targets

    });
    using var cout = Console.OpenStandardOutput();
    cout.Write(result.Code);

    foreach (var item in result.Exports)
    {
        cout.Write(item.Exported.Span);
        cout.Write("\n"u8);
        cout.Write(item.Local.Span);
        cout.Write("\n"u8);
    }
}
catch (Exception e)
{
    Console.WriteLine("error");
    Console.WriteLine(e);
}