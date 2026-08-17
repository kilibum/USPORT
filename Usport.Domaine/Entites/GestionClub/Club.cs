using Usport.Domaine.Enumerations;

namespace Usport.Domaine.Entites.GestionClub;

/// <summary>
/// Représente un club de fitness.
/// </summary>
public class Club
{
    public int Id { get; private set; }
    public string Nom { get; private set; }
    public string AdresseRue { get; private set; }
    public string AdresseVille { get; private set; }
    public string AdresseCodePostal { get; private set; }
    public string Pays { get; private set; }
    public bool EstOuvert247 { get; private set; }
    public DateOnly DateOuverture { get; private set; }
    public StatutOperationnelClub StatutOperationnel { get; private set; }
    public DateTime DateCreation { get; private set; }
    public DateTime DateModification { get; private set; }

    // Navigation
    public ICollection<Employe> Employes { get; } = [];

    public Club(string nom, string adresseRue, string adresseVille,
                string adresseCodePostal, DateOnly dateOuverture, string pays = "Belgique")
    {
        if (string.IsNullOrWhiteSpace(nom)) throw new ArgumentException("Le nom du club est obligatoire.", nameof(nom));

        Nom                = nom;
        AdresseRue         = adresseRue;
        AdresseVille       = adresseVille;
        AdresseCodePostal  = adresseCodePostal;
        Pays               = pays;
        DateOuverture      = dateOuverture;
        EstOuvert247       = true;
        StatutOperationnel = StatutOperationnelClub.Ouvert;
        DateCreation       = DateTime.UtcNow;
        DateModification   = DateTime.UtcNow;
    }

    // Comportements

    public bool EstOperationnel() => StatutOperationnel == StatutOperationnelClub.Ouvert;

    public void Fermer()
    {
        StatutOperationnel = StatutOperationnelClub.FermeTemporairement;
        DateModification    = DateTime.UtcNow;
    }

    public void Reouvrir()
    {
        StatutOperationnel = StatutOperationnelClub.Ouvert;
        DateModification    = DateTime.UtcNow;
    }

    public void AssignerId(int id) => Id = id;

    public string ObtenirInfosAffichage()
        => $"Club: {Nom} — {AdresseVille}, {Pays} ({StatutOperationnel})";
}