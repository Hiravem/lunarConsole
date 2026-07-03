namespace Lunar.Console.UI;

public sealed class InputReader
{
    public string ReadLine(string prompt)
    {
        System.Console.Write(prompt);
        return System.Console.ReadLine()?.Trim() ?? "";
    }

    public int ReadChoice(string prompt, int min, int max)
    {
        while (true)
        {
            var input = ReadLine(prompt);
            if (int.TryParse(input, out var choice) && choice >= min && choice <= max)
                return choice;

            System.Console.WriteLine($"Please enter a number between {min} and {max}.");
        }
    }
}
