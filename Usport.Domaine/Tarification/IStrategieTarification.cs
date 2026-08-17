namespace Usport.Domaine.Tarification;

/// <summary>
/// Contrat de calcul de tarif d'un abonnement.
/// </summary>
public interface IStrategieTarification
{
    decimal CalculerMensuel(decimal prixBaseParMois);

    decimal CalculerAnnuel(decimal prixBaseParMois);

    string NomStrategie { get; }
}