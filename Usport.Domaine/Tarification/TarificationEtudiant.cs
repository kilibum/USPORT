namespace Usport.Domaine.Tarification;

/// <summary>
/// Tarif étudiant : remise de 40 % appliquée au mois comme à l'année.
/// </summary>
public class TarificationEtudiant : IStrategieTarification
{
    private const decimal RemiseEtudiant = 0.40m;

    public string NomStrategie => "Étudiant (40% de réduction)";

    public decimal CalculerMensuel(decimal prixBaseParMois)
        => prixBaseParMois * (1 - RemiseEtudiant);

    public decimal CalculerAnnuel(decimal prixBaseParMois)
        => prixBaseParMois * (1 - RemiseEtudiant) * 12;
}