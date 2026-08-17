using Usport.Domaine.Enumerations;
using Usport.Domaine.Entites.Commun;

namespace Usport.Domaine.Entites.Adhesion;

/// <summary>
/// Représente un membre (client) du club de fitness.
/// </summary>
public class Membre : Personne
{
    public Genre Genre { get; private set; } = Genre.Autre;
    public DateOnly DateNaissance { get; private set; }
    public string Nationalite { get; private set; } = "Belge";
    public string? TelephoneMobile { get; private set; }
    public string? AdresseRue { get; private set; }
    public string? AdresseVille { get; private set; }
    public string? AdresseCodePostal { get; private set; }
    public StatutMembre Statut { get; private set; }
    public int? IdMembreParrain { get; private set; }
    public ObjectifPrincipal? ObjectifPrincipal { get; private set; }
    public SourceAcquisition? SourceAcquisition { get; private set; }
    public bool CertificatMedicalFourni { get; private set; }
    public DateTime DateConsentementRgpd { get; private set; }
    public bool ConsentementMarketing { get; private set; }
    public DateOnly? DateDerniereVisite { get; private set; }
    public int NombreVisitesTotal { get; private set; }
    public DateTime DateChangementStatut { get; private set; }

    private Membre(string prenom, string nom, string email, DateOnly dateNaissance,
                   string? telephone, string? ville,
                   Genre genre, ObjectifPrincipal? objectifPrincipal, SourceAcquisition? sourceAcquisition)
        : base(prenom, nom, email)
    {
        DateNaissance      = dateNaissance;
        TelephoneMobile    = telephone;
        AdresseVille       = ville;
        Genre              = genre;
        ObjectifPrincipal  = objectifPrincipal;
        SourceAcquisition  = sourceAcquisition;
        Statut             = StatutMembre.Actif;
        DateConsentementRgpd = DateTime.UtcNow;
        DateChangementStatut = DateTime.UtcNow;
    }

    // Reconstruit un membre à partir des données persistées en base.
    private Membre(int id, string prenom, string nom, string email,
                   DateOnly dateNaissance, string? ville, StatutMembre statut, int nombreVisitesTotal,
                   DateTime dateCreation, DateTime dateModification)
        : base(id, prenom, nom, email, dateCreation, dateModification)
    {
        DateNaissance         = dateNaissance;
        AdresseVille          = ville;
        Statut                = statut;
        NombreVisitesTotal    = nombreVisitesTotal;
        DateConsentementRgpd  = DateTime.UtcNow;
        DateChangementStatut  = DateTime.UtcNow;
    }

    public static Membre Creer(string prenom, string nom, string email,
                               DateOnly dateNaissance, string? telephone = null, string? ville = null,
                               Genre genre = Genre.Autre,
                               ObjectifPrincipal? objectifPrincipal = null,
                               SourceAcquisition? sourceAcquisition = null)
        => new(prenom, nom, email, dateNaissance, telephone, ville, genre, objectifPrincipal, sourceAcquisition);

    public static Membre Reconstituer(int id, string prenom, string nom, string email,
                                      DateOnly dateNaissance, string? ville,
                                      StatutMembre statut, int nombreVisitesTotal,
                                      DateTime dateCreation, DateTime dateModification)
        => new(id, prenom, nom, email, dateNaissance, ville, statut, nombreVisitesTotal, dateCreation, dateModification);

    public void AssignerId(int id) => DefinirId(id);

    public int ObtenirAge()
    {
        var aujourdHui = DateOnly.FromDateTime(DateTime.Today);
        var age = aujourdHui.Year - DateNaissance.Year;
        if (DateNaissance > aujourdHui.AddYears(-age)) age--;
        return age;
    }

    public override string ObtenirInfosAffichage()
        => $"Membre: {NomComplet} (Âge: {ObtenirAge()}, Statut: {Statut}, Email: {Email})";
}