namespace Lunar.Console.Presentation;

public sealed class OutputWriter
{
    public void WriteLine(string text = "") => System.Console.WriteLine(text);

    public void WriteSeparator(char c = '=', int width = 40) =>
        WriteLine(new string(c, width));

    public void WriteHeader(string title)
    {
        WriteSeparator();
        WriteLine($"  {title}");
        WriteSeparator();
    }

    public void Pause() => ReadLineInternal("\nPress Enter to continue...");

    private static string ReadLineInternal(string prompt)
    {
        System.Console.Write(prompt);
        return System.Console.ReadLine() ?? "";
    }
}
