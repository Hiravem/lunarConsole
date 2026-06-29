namespace Lunar.Console.Presentation.Screens;

public enum MainMenuChoice
{
    NewGame = 1,
    Continue = 2,
    Exit = 3
}

public sealed class MainMenuScreen
{
    private readonly InputReader _input;
    private readonly OutputWriter _output;

    public MainMenuScreen(InputReader input, OutputWriter output)
    {
        _input = input;
        _output = output;
    }

    public MainMenuChoice Show(bool hasSave)
    {
        _output.WriteLine();
        _output.WriteLine("1. New Game");
        _output.WriteLine(hasSave ? "2. Continue" : "2. Continue (no save)");
        _output.WriteLine("3. Exit");

        var choice = _input.ReadChoice("> ", 1, 3);
        return (MainMenuChoice)choice;
    }
}
