using Usport.Console.DTOs;
using Usport.Domaine.Entites.Adhesion;
using Usport.Domaine.Entites.GestionClub;

namespace Usport.Console.Data;

/// <summary>
/// Stockage en mémoire de toutes les données de l'application.
/// Remplace la base de données par des listes en mémoire.
/// </summary>
public class StockDonnees
{
    // Entités du domaine.
    public List<Membre> Membres { get; } = new();
    public List<Employe> Employes { get; } = new();
    public List<Club> Clubs { get; } = new();
    public List<OptionService> OptionsService { get; } = new();

    // Données de référence (plans pré-chargés au démarrage).
    public List<OptionPlan> Plans { get; } = new();

    // Contrats stockés comme DTOs (incluent le prix mensuel calculé).
    public List<ContratDto> Contrats { get; } = new();

    // Compteurs auto-incrémentés pour simuler les IDs de base de données.
    private int _prochainIdMembre = 1;
    private int _prochainIdEmploye = 1;
    private int _prochainIdContrat = 1;
    private int _prochainIdClub = 1;
    private int _prochainIdOptionService = 1;

    public int ProchainIdMembre() => _prochainIdMembre++;
    public int ProchainIdEmploye() => _prochainIdEmploye++;
    public int ProchainIdContrat() => _prochainIdContrat++;
    public int ProchainIdClub() => _prochainIdClub++;
    public int ProchainIdOptionService() => _prochainIdOptionService++;

    /// <summary>
    /// Charge les données de démarrage : clubs, plans et options de service.
    /// Appelé une seule fois au lancement de l'application.
    /// </summary>
    public void Amorcer()
    {
        // Clubs de référence (entités du domaine avec comportements)
        var clubBruxelles = new Club("Usport Bruxelles", "Rue de la Loi 42", "Bruxelles", "1000", DateOnly.FromDateTime(new DateTime(2020, 1, 15)));
        clubBruxelles.AssignerId(ProchainIdClub());
        Clubs.Add(clubBruxelles);

        var clubLiege = new Club("Usport Liège", "Place Saint-Lambert 10", "Liège", "4000", DateOnly.FromDateTime(new DateTime(2021, 6, 1)));
        clubLiege.AssignerId(ProchainIdClub());
        Clubs.Add(clubLiege);

        var clubNamur = new Club("Usport Namur", "Rue de Fer 25", "Namur", "5000", DateOnly.FromDateTime(new DateTime(2022, 3, 20)));
        clubNamur.AssignerId(ProchainIdClub());
        Clubs.Add(clubNamur);

        // Plans d'abonnement
        Plans.Add(new OptionPlan(1, "Basique", 19.99m, 1));
        Plans.Add(new OptionPlan(2, "Premium", 34.99m, 12));
        Plans.Add(new OptionPlan(3, "VIP", 49.99m, 24));

        // Options de service
        var opt1 = new OptionService("Coach privé", 25.00m);
        opt1.AssignerId(ProchainIdOptionService());
        OptionsService.Add(opt1);

        var opt2 = new OptionService("Suivi nutritionnel", 15.00m);
        opt2.AssignerId(ProchainIdOptionService());
        OptionsService.Add(opt2);

        var opt3 = new OptionService("Accès sauna", 10.00m);
        opt3.AssignerId(ProchainIdOptionService());
        OptionsService.Add(opt3);

        var opt4 = new OptionService("Cours collectifs", 12.00m);
        opt4.AssignerId(ProchainIdOptionService());
        OptionsService.Add(opt4);
    }
}