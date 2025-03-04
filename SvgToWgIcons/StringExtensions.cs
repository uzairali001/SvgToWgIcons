using System.Globalization;

namespace SvgToWgIcons;

public static class StringExtensions
{
    public static string ToCamelCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }
        return char.ToLowerInvariant(value[0]) + value.Substring(1);
    }

    public static string ToPascalCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
        return string.Join("", value.Split("-").Select(textInfo.ToTitleCase)).Replace(" ", "");
    }

    public static string ToKebabCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }
        return string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x.ToString() : x.ToString())).ToLower();
    }

    public static string ToSnakeCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }
        return string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
    }

    public static string ToTitleCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }
        return char.ToUpperInvariant(value[0]) + value.Substring(1).ToLower();
    }

    public static string ToConstantCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }
        return string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToUpper();
    }


}
