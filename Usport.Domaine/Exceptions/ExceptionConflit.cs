namespace Usport.Domaine.Exceptions;

public class ExceptionConflit : ExceptionDomaine
{
    public ExceptionConflit(string message)
        : base("CONFLIT", message) { }
}