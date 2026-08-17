using Usport.Console.Data;
using Usport.Console.DTOs;
using Usport.Domaine.Entites.GestionClub;
using Usport.Domaine.Enumerations;

namespace Usport.Console.Services;

/// <summary>
/// Service de gestion des employés.
/// </summary>
public class ServiceEmployee
{
    private readonly StockDonnees _stockDonnees;

    public ServiceEmployee(StockDonnees stockDonnees)
    {
        _stockDonnees = stockDonnees ?? throw new ArgumentNullException(nameof(stockDonnees));
    }

    // Crée un employé après validation de l'email unique.
    public EmployeDto CreerEmploye(
        string prenom, string nom, string email, int idClub,
        RoleEmploye role, DateOnly dateEmbauche, decimal? salaireMensuel, string? qualifications)
    {
        if (_stockDonnees.Employes.Any(e => e.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"L'adresse email '{email}' est déjà utilisée.");

        var employe = Employe.Creer(idClub, prenom, nom, email, role, dateEmbauche, salaireMensuel, qualifications);
        employe.AssignerId(_stockDonnees.ProchainIdEmploye());
        _stockDonnees.Employes.Add(employe);

        var nomClub = _stockDonnees.Clubs.FirstOrDefault(c => c.Id == idClub)?.Nom ?? "—";
        return VersDto(employe, nomClub);
    }

    // Liste les employés avec le nom de leur club, triés par nom.
    public List<EmployeDto> ObtenirTousLesEmployes()
    {
        return _stockDonnees.Employes
            .OrderBy(e => e.Nom)
            .ThenBy(e => e.Prenom)
            .Select(e =>
            {
                var nomClub = _stockDonnees.Clubs.FirstOrDefault(c => c.Id == e.ClubId)?.Nom ?? "—";
                return VersDto(e, nomClub);
            })
            .ToList();
    }

    // Clubs proposés à la sélection lors de la création d'un employé.
    public List<OptionClub> ObtenirClubsSelectionnables()
    {
        return _stockDonnees.Clubs
            .OrderBy(c => c.Nom)
            .Select(c => new OptionClub(c.Id, c.Nom, c.AdresseVille, c.StatutOperationnel))
            .ToList();
    }

    // Champs complémentaires pour pré-remplir le formulaire de modification.
    public (decimal? Salaire, string? Qualifications)? ObtenirInfosModificationEmploye(int id)
    {
        var employe = _stockDonnees.Employes.FirstOrDefault(e => e.Id == id);
        if (employe is null) return null;
        return (employe.SalaireMensuel, employe.Qualifications);
    }

    // Met à jour l'affectation et la rémunération d'un employé ainsi que la recréation de l'entité.
    public bool MettreAJourEmploye(
        int id, int idClub, RoleEmploye role, decimal? salaireMensuel, string? qualifications)
    {
        var ancien = _stockDonnees.Employes.FirstOrDefault(e => e.Id == id);
        if (ancien is null) return false;

        _stockDonnees.Employes.Remove(ancien);
        var misAJour = Employe.Creer(idClub, ancien.Prenom, ancien.Nom, ancien.Email,
                                      role, ancien.DateEmbauche, salaireMensuel, qualifications);
        misAJour.AssignerId(id);
        _stockDonnees.Employes.Add(misAJour);
        return true;
    }

    // Supprime définitivement un employé.
    public bool SupprimerEmploye(int id)
    {
        var employe = _stockDonnees.Employes.FirstOrDefault(e => e.Id == id);
        if (employe is null) return false;
        _stockDonnees.Employes.Remove(employe);
        return true;
    }

    private static EmployeDto VersDto(Employe employe, string nomClub) => new EmployeDto
    {
        Id             = employe.Id,
        NomComplet     = employe.NomComplet,
        Email          = employe.Email,
        Role           = employe.Role,
        NomClub        = nomClub,
        SalaireMensuel = employe.SalaireMensuel,
        InfosAffichage = employe.ObtenirInfosAffichage()
    };
}