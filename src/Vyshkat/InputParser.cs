using System.Globalization;

namespace Vyshkat;

// Разбор чисел из консоли и файла. Принимает и «1.5», и «1,5» независимо от языка системы.
public static class InputParser
{
    public static int ParseInt(string text, string fieldName)
    {
        if (int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            return value;
        throw new FormatException($"{fieldName}: «{text}» — не целое число.");
    }

    public static double ParseDouble(string text, string fieldName)
    {
        var normalized = text.Trim().Replace(',', '.');
        if (double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            return value;
        throw new FormatException($"{fieldName}: «{text}» — не число.");
    }
}
