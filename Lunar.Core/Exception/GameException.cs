namespace Lunar.Core.Exception;

public class GameException : System.Exception
{
    public GameException(string message) : base(message) { }

    public GameException(string message, System.Exception innerException)
        : base(message, innerException) { }
}
