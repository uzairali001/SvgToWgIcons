
using HtmlAgilityPack;

using SvgToWgIcons;

using System.Diagnostics;
using System.Text;


string? sourceDir = args.ElementAtOrDefault(0);

string outputDir = args.ElementAtOrDefault(1) ?? (Path.Combine(Environment.CurrentDirectory, "output"));

if (args.Length == 0 || args.ElementAt(0) == "help")
{
    Console.WriteLine("Usage: SvgToBlazorComponent <source directory> <output directory>");
    return 0;
}

if (string.IsNullOrEmpty(sourceDir))
{
    Console.Error.WriteLine("Source directory not set");
    return -1;
}

if (!Directory.Exists(sourceDir))
{
    Console.WriteLine($"Error: Source directory '{sourceDir}' does not exist.");
    return -1;
}

if (!Directory.Exists(outputDir))
{
    Directory.CreateDirectory(outputDir);
    Console.WriteLine($"Created output directory: '{outputDir}'.");
}

await ProcessSvgFiles(sourceDir, outputDir);

return 0;



static async Task ProcessSvgFiles(string sourceDir, string outputDir)
{
    Stopwatch stopwatch = Stopwatch.StartNew();

    var directories = Directory.EnumerateDirectories(sourceDir, "*", SearchOption.AllDirectories).ToList();
    if (directories.Count == 0)
    {
        await ProcessFolder(sourceDir, outputDir);
    }
    else
    {
        foreach (var directory in directories)
        {
            string subDirectory = ((directory.Replace(sourceDir + "\\", "")) ?? string.Empty).ToPascalCase();
            string dest = Path.Combine(outputDir, subDirectory);

            await ProcessFolder(directory, dest);
        }
    }
    stopwatch.Stop();

    Console.WriteLine($"Took {stopwatch.Elapsed.TotalSeconds}s");

    //var files = Directory.EnumerateFiles(sourceDir, "*.svg", SearchOption.AllDirectories);
    //Console.WriteLine($"Converting {files.Count()} Files");
    //await Task.Delay(1000);

    //StreamWriter? steamWriter = null;

    //foreach (var filePath in files)
    //{
    //    string subDirectory = (Path.GetDirectoryName(filePath.Replace(sourceDir + "\\", "")) ?? string.Empty).ToPascalCase();
    //    string outputDirectory = Path.Combine(outputDir, subDirectory);
    //    if (!Directory.Exists(outputDirectory))
    //    {
    //        Directory.CreateDirectory(outputDirectory);
    //        string file = Path.Combine(outputDirectory, "index.ts");

    //        steamWriter = new StreamWriter(file, true);
    //    }


    //    SvgFileResult svgContent = ReadSvgFile(filePath);
    //    string iconName = Path.GetFileNameWithoutExtension(filePath);
    //    string componentString = GetComponentString(iconName, svgContent);

    //    await steamWriter.WriteLineAsync(componentString);
    //}

    //stopwatch.Stop();

    //Console.WriteLine($"Took {stopwatch.Elapsed.TotalSeconds}s");
    //Console.WriteLine($"Converted {files.Count()} files to Blazor component. Output directory: '{outputDir}'.");

}

static async Task ProcessFolder(string source, string output)
{
    var files = Directory.EnumerateFiles(source, "*.svg", SearchOption.TopDirectoryOnly);
    Directory.CreateDirectory(output);
    using StreamWriter? steamWriter = new(new FileStream(Path.Combine(output, "index.ts"), FileMode.Create, FileAccess.Write));

    foreach (var file in files)
    {
        SvgFileResult svgContent = ReadSvgFile(file);
        string iconName = Path.GetFileNameWithoutExtension(file);
        string componentString = GetComponentString(iconName, svgContent);

        steamWriter.WriteLine(componentString);
    }
}


static SvgFileResult ReadSvgFile(string filePath)
{
    using StreamReader reader = new(filePath, Encoding.UTF8);

    HtmlDocument doc = new();
    doc.Load(reader);

    string[] viewbox = doc.DocumentNode
        .SelectSingleNode("//svg")
        .Attributes["viewBox"].Value.Split(" ");

    string path = doc.DocumentNode
        .SelectSingleNode("//svg/path")
        .Attributes["d"].Value;

    return new SvgFileResult()
    {
        Path = path,
        ViewBoxWidth = int.Parse(viewbox[2]),
        ViewBoxHeight = int.Parse(viewbox[3])
    };
}

static string GetComponentString(string iconName, SvgFileResult svg)
{
    return $$"""
        export const Wgi{{iconName.ToPascalCase()}} = {
          iconName: "{{iconName}}",
          icon: [{{svg.ViewBoxWidth}}, {{svg.ViewBoxHeight}}, "{{svg.Path}}"]
        }
        """;
}