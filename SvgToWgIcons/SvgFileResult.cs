namespace SvgToWgIcons;

public class SvgFileResult
{
    public required int ViewBoxWidth { get; set; }
    public required int ViewBoxHeight { get; set; }
    public required IEnumerable<string> Paths { get; set; }
}
