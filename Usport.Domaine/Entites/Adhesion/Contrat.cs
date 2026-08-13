using Usport.Domaine.Enumerations;
using Usport.Domaine.Entites.GestionClub;
using Usport.Domaine.Exceptions;

namespace Usport.Domaine.Entites.Adhesion;

/// <summary>
/// Représente un contrat d'abonnement entre un membre et un club.
/// </summary>
public class Contrat
{
    public int Id { get; private set; }
    public int IdMembre { get; private set; }
    public int IdPlan { get; private set; }
    public int IdClubPrincipal { get; private set; }
    public DateOnly DateDebut { get; private set; }
    public DateOnly? DateFin { get; private set; }
    public TypeContrat TypeContrat { get; private set; }
    public StatutContrat Statut { get; private set; }
    public DateOnly? DateAnnulation { get; private set; }
    public string? MotifAnnulation { get; private set; }
    public DateOnly? DateDebutGel { get; private set; }
    public DateOnly? DateFinGel { get; private set; }
    public DateTime DateChangementStatut { get; private set; }
    public DateTime DateCreation { get; private set; }
    public DateTime DateModification { get; private set; }

    // Navigation
    public Membre Membre { get; private set; } = null!;
    public PlanAbonnement Plan { get; private set; } = null!;
    public Club ClubPrincipal { get; private set; } = null!;

    public Contrat(int idMembre, int idPlan, int idClubPrincipal,
                    DateOnly dateDebut, DateOnly? dateFin,
                    TypeContrat typeContrat = TypeContrat.DureeDeterminee)
    {
        IdMembre              = idMembre;
        IdPlan                = idPlan;
        IdClubPrincipal       = idClubPrincipal;
        DateDebut             = dateDebut;
        DateFin               = dateFin;
        TypeContrat           = typeContrat;
        Statut                = StatutContrat.Actif;
        DateChangementStatut  = DateTime.UtcNow;
        DateCreation          = DateTime.UtcNow;
        DateModification      = DateTime.UtcNow;
    }

    // ── Comportements ────────────────────────────────────────────────────

    /// <summary>Indique si le contrat est actif.</summary>
    public bool EstActif() => Statut == StatutContrat.Actif;

    /// <summary>
    /// Annule le contrat. Lève une ExceptionRegleMetier si déjà annulé ou expiré.
    /// </summary>
    public void Annuler(string? motif = null)
    {
        if (Statut == StatutContrat.Annule)
            throw new ExceptionRegleMetier("CONTRAT_DEJA_ANNULE",
                "Ce contrat est déjà annulé.");
        if (Statut == StatutContrat.Expire)
            throw new ExceptionRegleMetier("CONTRAT_EXPIRE",
                "Un contrat expiré ne peut pas être annulé.");

        Statut                = StatutContrat.Annule;
        DateAnnulation        = DateOnly.FromDateTime(DateTime.UtcNow);
        MotifAnnulation       = motif;
        DateChangementStatut  = DateTime.UtcNow;
        DateModification      = DateTime.UtcNow;
    }

    /// <summary>
    /// Gèle le contrat pour une période donnée.
    /// Seul un contrat actif peut être gelé ; la date de fin doit être postérieure au début.
    /// </summary>
    public void Geler(DateOnly debut, DateOnly fin)
    {
        if (!EstActif())
            throw new ExceptionRegleMetier("CONTRAT_NON_ACTIF",
                "Seul un contrat actif peut être gelé.");
        if (fin <= debut)
            throw new ArgumentException(
                "La date de fin du gel doit être postérieure à la date de début.", nameof(fin));

        DateDebutGel     = debut;
        DateFinGel       = fin;
        DateModification = DateTime.UtcNow;
    }
}