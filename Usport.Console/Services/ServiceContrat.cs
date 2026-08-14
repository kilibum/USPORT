using Usport.Console.Data;
using Usport.Console.DTOs;
using Usport.Domaine.Enumerations;
using Usport.Domaine.Tarification;

namespace Usport.Console.Services;

/// <summary>
/// Service de gestion des contrats (stockage en mémoire).
/// </summary>
public class ServiceContrat
{
    private readonly StockDonnees _stockDonnees;

    public ServiceContrat(StockDonnees stockDonnees)
    {
        _stockDonnees = stockDonnees ?? throw new ArgumentNullException(nameof(stockDonnees));
    }

    // Retourne le nom complet du membre actif, ou null s'il est introuvable.
    public string? ObtenirNomCompletMembre(int idMembre)
    {
        var membre = _stockDonnees.Membres.FirstOrDefault(m => m.Id == idMembre);
        return membre?.NomComplet;
    }

    // Retourne le nom et le prix de base du plan, ou null s'il est introuvable.
    public (string NomPlan, decimal PrixDeBase)? ObtenirInfosPlan(int idPlan)
    {
        var plan = _stockDonnees.Plans.FirstOrDefault(p => p.Id == idPlan);
        if (plan is null) return null;
        return (plan.NomPlan, plan.PrixDeBase);
    }

    // Le prix mensuel est calculé par la stratégie de tarification fournie.
    public ContratDto CreerContrat(int idMembre, int idPlan, int idClub, IStrategieTarification strategie)
    {
        if (!_stockDonnees.Membres.Any(m => m.Id == idMembre && m.Statut == StatutMembre.Actif))
            throw new InvalidOperationException($"Membre introuvable (ID : {idMembre}).");

        var plan = _stockDonnees.Plans.FirstOrDefault(p => p.Id == idPlan)
            ?? throw new InvalidOperationException($"Plan introuvable (ID : {idPlan}).");

        if (!_stockDonnees.Clubs.Any(c => c.Id == idClub))
            throw new InvalidOperationException($"Club introuvable (ID : {idClub}).");

        decimal prixMensuel = strategie.CalculerMensuel(plan.PrixDeBase);
        var dateDebut = DateOnly.FromDateTime(DateTime.UtcNow);
        var dateFin   = dateDebut.AddMonths(plan.MoisEngagement);

        var contrat = new ContratDto
        {
            Id           = _stockDonnees.ProchainIdContrat(),
            IdMembre     = idMembre,
            IdPlan       = idPlan,
            Statut       = StatutContrat.Actif,
            DateDebut    = dateDebut,
            DateFin      = dateFin,
            PrixMensuel  = prixMensuel
        };

        _stockDonnees.Contrats.Add(contrat);
        return contrat;
    }

    // Annule un contrat en passant son statut à Annule.
    // Retourne false si le contrat est introuvable ou déjà annulé.
    public bool AnnulerContrat(int idContrat)
    {
        var index = _stockDonnees.Contrats.FindIndex(c => c.Id == idContrat);
        if (index < 0) return false;

        var ancien = _stockDonnees.Contrats[index];
        if (ancien.Statut is StatutContrat.Annule or StatutContrat.Expire)
            return false;

        _stockDonnees.Contrats[index] = new ContratDto
        {
            Id           = ancien.Id,
            IdMembre     = ancien.IdMembre,
            IdPlan       = ancien.IdPlan,
            Statut       = StatutContrat.Annule,
            DateDebut    = ancien.DateDebut,
            DateFin      = ancien.DateFin,
            PrixMensuel  = ancien.PrixMensuel
        };

        return true;
    }

    // Change l'abonnement d'un contrat actif et son prix mensuel recalculé.
    public bool ChangerPlanContrat(int idContrat, int idPlan, decimal prixMensuel)
    {
        if (!_stockDonnees.Plans.Any(p => p.Id == idPlan))
            throw new InvalidOperationException($"Plan introuvable (ID : {idPlan}).");

        var index = _stockDonnees.Contrats.FindIndex(c => c.Id == idContrat && c.Statut == StatutContrat.Actif);
        if (index < 0) return false;

        var ancien = _stockDonnees.Contrats[index];
        _stockDonnees.Contrats[index] = new ContratDto
        {
            Id           = ancien.Id,
            IdMembre     = ancien.IdMembre,
            IdPlan       = idPlan,
            Statut       = ancien.Statut,
            DateDebut    = ancien.DateDebut,
            DateFin      = ancien.DateFin,
            PrixMensuel  = prixMensuel
        };

        return true;
    }

    // Retourne les contrats actifs les plus récents.
    public List<ContratDto> ObtenirContratsActifs(int nombreMax = 20)
    {
        return _stockDonnees.Contrats
            .Where(c => c.Statut == StatutContrat.Actif)
            .Take(nombreMax)
            .ToList();
    }

    // Membres actifs proposés à la sélection lors de la création d'un contrat.
    public List<OptionMembre> ObtenirMembresSelectionnables()
    {
        return _stockDonnees.Membres
            .Where(m => m.Statut == StatutMembre.Actif)
            .OrderBy(m => m.Nom)
            .ThenBy(m => m.Prenom)
            .Select(m => new OptionMembre(m.Id, m.NomComplet, m.AdresseVille))
            .ToList();
    }

    // Plans d'abonnement proposés à la sélection.
    public List<OptionPlan> ObtenirPlansSelectionnables()
    {
        return _stockDonnees.Plans.OrderBy(p => p.PrixDeBase).ToList();
    }

    // Clubs proposés à la sélection.
    public List<OptionClub> ObtenirClubsSelectionnables()
    {
        return _stockDonnees.Clubs
            .OrderBy(c => c.Nom)
            .Select(c => new OptionClub(c.Id, c.Nom, c.AdresseVille, c.StatutOperationnel))
            .ToList();
    }

    // Tous les contrats (tous statuts).
    public List<ContratDto> ObtenirTousLesContrats(int nombreMax = 50)
    {
        return _stockDonnees.Contrats.Take(nombreMax).ToList();
    }

    // Supprime définitivement un contrat.
    public bool SupprimerContrat(int idContrat)
    {
        var contrat = _stockDonnees.Contrats.FirstOrDefault(c => c.Id == idContrat);
        if (contrat is null) return false;
        _stockDonnees.Contrats.Remove(contrat);
        return true;
    }
}