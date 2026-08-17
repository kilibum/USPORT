using Usport.Domaine.Enumerations;
using Usport.Domaine.Entites.Commun;

namespace Usport.Domaine.Entites.GestionClub;

/// <summary>
/// Représente un employé (membre du personnel) d'un club.
/// </summary>
public class Employe : Personne
{
    public int ClubId { get; private set; }
    public RoleEmploye Role { get; private set; }
    public decimal? SalaireMensuel { get; private set; }
    public string? Qualifications { get; private set; }
    public DateOnly DateEmbauche { get; private set; }

    private Employe(int clubId, string prenom, string nom, string email,
                     RoleEmploye role, DateOnly dateEmbauche, decimal? salaireMensuel = null,
                     string? qualifications = null)
        : base(prenom, nom, email)
    {
        ClubId         = clubId;
        Role           = role;
        DateEmbauche   = dateEmbauche;
        SalaireMensuel = salaireMensuel;
        Qualifications = qualifications;
    }

    public static Employe Creer(int clubId, string prenom, string nom, string email,
                                 RoleEmploye role, DateOnly dateEmbauche,
                                 decimal? salaireMensuel = null, string? qualifications = null)
        => new(clubId, prenom, nom, email, role, dateEmbauche, salaireMensuel, qualifications);

    public void AssignerId(int id) => DefinirId(id);

    public override string ObtenirInfosAffichage()
        => $"Employé: {NomComplet} ({Role} au Club {ClubId}, €{SalaireMensuel:F2}/mois, Email: {Email})";
}