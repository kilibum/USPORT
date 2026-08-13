namespace Usport.Domaine.Tarification;

/// <summary>
/// Tarif standard : plein tarif au mois, remise de 10 % sur l'engagement annuel.
/// </summary>
public class TarificationStandard : IStrategieTarification
{
    public string NomStrategie => "Standard";

    public decimal CalculerMensuel(decimal prixBaseParMois) => prixBaseParMois;

    // Remise de fidélité de 10 % pour l'engagement annuel.
    public decimal CalculerAnnuel(decimal prixBaseParMois)
        => prixBaseParMois * 12 * 0.90m;
}