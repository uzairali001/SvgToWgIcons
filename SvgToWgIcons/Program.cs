
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
    Console.WriteLine($"Processing SVG files from {sourceDir} to {outputDir}");

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

            Console.Write($"{subDirectory}: ");
            await ProcessFolder(directory, dest);
        }
    }
    stopwatch.Stop();

    Console.WriteLine($"Took {stopwatch.Elapsed.TotalSeconds}s");
}

static async Task ProcessFolder(string source, string output)
{
    var files = Directory.EnumerateFiles(source, "*.svg", SearchOption.TopDirectoryOnly);
    Directory.CreateDirectory(output);
    using StreamWriter? indexWriter = new(new FileStream(Path.Combine(output, "index.js"), FileMode.Create, FileAccess.Write));
    using StreamWriter? typeWriter = new(new FileStream(Path.Combine(output, "index.d.ts"), FileMode.Create, FileAccess.Write));

    Console.WriteLine($"Processing {files.Count()} Files");
    typeWriter.WriteLine("import WgIconDefinition from \"../WgIconDefinition\";");

    foreach (var file in files)
    {
        SvgFileResult svgContent = ReadSvgFile(file);
        string iconName = Path.GetFileNameWithoutExtension(file);
        string componentString = GetComponentString(iconName, svgContent);

        indexWriter.WriteLine(componentString);
        typeWriter.WriteLine($"export const Wgi{iconName.ToPascalCase()}: WgIconDefinition;");
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

    var paths = doc.DocumentNode
        .SelectNodes("//svg/path")
        .Select(x => x.Attributes["d"].Value)
        .Where(x => x.Length > 0);

    return new SvgFileResult()
    {
        Paths = paths,
        ViewBoxWidth = int.Parse(viewbox[2]),
        ViewBoxHeight = int.Parse(viewbox[3])
    };
}

static string GetComponentString(string iconName, SvgFileResult svg)
{
    string pathsString = svg.Paths.Count() > 1
        ? $"[{string.Join(", ", svg.Paths.Select(x => $"\"{x}\""))}]"
        : $"\"{svg.Paths.First()}\"";

    return $$"""
    export const Wgi{{iconName.ToPascalCase()}} = {
      iconName: "{{iconName}}",
      icon: [{{svg.ViewBoxWidth}}, {{svg.ViewBoxHeight}}, {{pathsString}}]
    }
    """;
}
