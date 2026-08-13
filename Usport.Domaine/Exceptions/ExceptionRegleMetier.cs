namespace Usport.Domaine.Exceptions;

public class ExceptionRegleMetier : ExceptionDomaine
{
    public ExceptionRegleMetier(string code, string message)
        : base(code, message) { }
}