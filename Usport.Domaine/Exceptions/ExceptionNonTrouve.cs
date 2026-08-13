namespace Usport.Domaine.Exceptions;

public class ExceptionNonTrouve : ExceptionDomaine
{
    public ExceptionNonTrouve(string entite, object cle)
        : base("NON_TROUVE", $"{entite} avec la clé '{cle}' n'a pas été trouvé(e).") { }
}