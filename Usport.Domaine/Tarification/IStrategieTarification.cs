namespace Usport.Domaine.Tarification;

/// <summary>
/// Contrat de calcul de tarif d'un abonnement.
/// Chaque implémentation encapsule une politique de prix interchangeable.
/// </summary>
public interface IStrategieTarification
{
    decimal CalculerMensuel(decimal prixBaseParMois);

    decimal CalculerAnnuel(decimal prixBaseParMois);

    string NomStrategie { get; }
}