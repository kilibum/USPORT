namespace Usport.Domaine.Exceptions;

public class ExceptionAccesRefuse : ExceptionDomaine
{
    public string? RaisonRefus { get; }

    public ExceptionAccesRefuse(string raison)
        : base("ACCES_REFUSE", raison)
    {
        RaisonRefus = raison;
    }
}