namespace Usport.Domaine.Tarification;

/// <summary>
/// Tarif promotionnel à remise configurable (0 à 100 %), appliquée au mois comme à l'année.
/// </summary>
public class TarificationPromotionnelle : IStrategieTarification
{
    private readonly decimal _pourcentageRemise;

    // La remise est exprimée en décimal (0.25 = 25 %) et validée à la construction.
    public TarificationPromotionnelle(decimal pourcentageRemise)
    {
        if (pourcentageRemise < 0m || pourcentageRemise > 1m)
            throw new ArgumentException("La remise doit être comprise entre 0 et 1.", nameof(pourcentageRemise));

        _pourcentageRemise = pourcentageRemise;
    }

    public string NomStrategie => $"Promo ({_pourcentageRemise * 100:F0}% de réduction)";

    public decimal CalculerMensuel(decimal prixBaseParMois)
        => prixBaseParMois * (1 - _pourcentageRemise);

    public decimal CalculerAnnuel(decimal prixBaseParMois)
        => prixBaseParMois * (1 - _pourcentageRemise) * 12;
}