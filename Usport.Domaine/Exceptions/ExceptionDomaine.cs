namespace Usport.Domaine.Exceptions;

public class ExceptionDomaine : Exception
{
    public string Code { get; }

    public ExceptionDomaine(string code, string message) : base(message)
    {
        Code = code;
    }
}