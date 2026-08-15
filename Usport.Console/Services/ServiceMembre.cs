using Usport.Console.Data;
using Usport.Console.DTOs;
using Usport.Domaine.Entites.Adhesion;
using Usport.Domaine.Enumerations;

namespace Usport.Console.Services;

/// <summary>
/// Service de gestion des membres (stockage en mémoire).
/// </summary>
public class ServiceMembre
{
    private readonly StockDonnees _stockDonnees;

    public ServiceMembre(StockDonnees stockDonnees)
    {
        _stockDonnees = stockDonnees ?? throw new ArgumentNullException(nameof(stockDonnees));
    }

    public MembreDto CreerMembre(
        string prenom,
        string nom,
        string email,
        DateOnly dateNaissance,
        string? telephone,
        string? ville,
        Genre genre = Genre.Autre,
        ObjectifPrincipal? objectifPrincipal = null,
        SourceAcquisition? sourceAcquisition = null)
    {
        int age = CalculerAge(dateNaissance);
        if (age < 16)
            throw new ArgumentException($"L'âge minimum requis est 16 ans. Âge calculé : {age} ans.");

        if (_stockDonnees.Membres.Any(m => m.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"L'adresse email '{email}' est déjà utilisée.");

        var membre = Membre.Creer(prenom, nom, email, dateNaissance, telephone, ville,
                                   genre, objectifPrincipal, sourceAcquisition);
        membre.AssignerId(_stockDonnees.ProchainIdMembre());
        _stockDonnees.Membres.Add(membre);

        return VersDto(membre);
    }

    public List<MembreDto> ObtenirTousLesMembres(int page, int taillePage)
    {
        return _stockDonnees.Membres
            .Where(m => m.Statut == StatutMembre.Actif)
            .OrderBy(m => m.Nom)
            .ThenBy(m => m.Prenom)
            .Skip((page - 1) * taillePage)
            .Take(taillePage)
            .Select(VersDto)
            .ToList();
    }

    public List<OptionMembre> ObtenirMembresSelectionnables()
    {
        return _stockDonnees.Membres
            .Where(m => m.Statut == StatutMembre.Actif)
            .OrderBy(m => m.Nom)
            .ThenBy(m => m.Prenom)
            .Select(m => new OptionMembre(m.Id, m.NomComplet, m.AdresseVille))
            .ToList();
    }

    public (string? Telephone, string? Ville)? ObtenirContactMembre(int id)
    {
        var membre = _stockDonnees.Membres.FirstOrDefault(m => m.Id == id);
        if (membre is null) return null;
        return (membre.TelephoneMobile, membre.AdresseVille);
    }

    public bool MettreAJourContactMembre(int id, string? telephone, string? ville)
    {
        var ancien = _stockDonnees.Membres.FirstOrDefault(m => m.Id == id);
        if (ancien is null) return false;

        _stockDonnees.Membres.Remove(ancien);
        var misAJour = Membre.Creer(ancien.Prenom, ancien.Nom, ancien.Email,
                                    ancien.DateNaissance, telephone, ville,
                                    ancien.Genre, ancien.ObjectifPrincipal, ancien.SourceAcquisition);
        misAJour.AssignerId(id);
        _stockDonnees.Membres.Add(misAJour);
        return true;
    }

    public bool SupprimerMembre(int id)
    {
        var membre = _stockDonnees.Membres.FirstOrDefault(m => m.Id == id);
        if (membre is null) return false;

        _stockDonnees.Contrats.RemoveAll(c => c.IdMembre == id);
        _stockDonnees.Membres.Remove(membre);
        return true;
    }

    private static MembreDto VersDto(Membre membre) => new MembreDto
    {
        Id                 = membre.Id,
        NomComplet         = membre.NomComplet,
        Age                = membre.ObtenirAge(),
        Email              = membre.Email,
        AdresseVille       = membre.AdresseVille,
        Genre              = membre.Genre,
        ObjectifPrincipal  = membre.ObjectifPrincipal,
        Statut             = membre.Statut,
        NombreVisitesTotal = membre.NombreVisitesTotal,
        InfosAffichage     = membre.ObtenirInfosAffichage()
    };

    private static int CalculerAge(DateOnly dateNaissance)
    {
        var aujourdHui = DateOnly.FromDateTime(DateTime.Today);
        var age = aujourdHui.Year - dateNaissance.Year;
        if (dateNaissance > aujourdHui.AddYears(-age)) age--;
        return age;
    }
}