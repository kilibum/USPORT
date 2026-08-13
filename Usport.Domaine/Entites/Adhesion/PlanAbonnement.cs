namespace Usport.Domaine.Entites.Adhesion;

public class PlanAbonnement
{
    public int Id { get; private set; }
    public string NomPlan { get; private set; }
    public decimal PrixDeBase { get; private set; }
    public int MoisEngagement { get; private set; }
    public decimal FraisInscription { get; private set; }
    public bool AccesClubLimite { get; private set; }
    public bool PassDuoAutorise { get; private set; }
    public DateTime DateCreation { get; private set; }
    public DateTime DateModification { get; private set; }

    // Navigation
    public ICollection<Contrat> Contrats { get; } = [];

    public PlanAbonnement(string nomPlan, decimal prixDeBase, int moisEngagement = 12,
                          decimal fraisInscription = 19.99m, bool accesClubLimite = false,
                          bool passDuoAutorise = false)
    {
        if (string.IsNullOrWhiteSpace(nomPlan)) throw new ArgumentException("Le nom du plan est obligatoire.", nameof(nomPlan));
        if (prixDeBase < 0)                     throw new ArgumentException("Le prix ne peut pas être négatif.", nameof(prixDeBase));

        NomPlan           = nomPlan;
        PrixDeBase        = prixDeBase;
        MoisEngagement    = moisEngagement;
        FraisInscription  = fraisInscription;
        AccesClubLimite   = accesClubLimite;
        PassDuoAutorise   = passDuoAutorise;
        DateCreation      = DateTime.UtcNow;
        DateModification  = DateTime.UtcNow;
    }
}