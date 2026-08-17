using Usport.Domaine.Entites.Adhesion;

namespace Tests.Domaine;

public class PlanAbonnementTests
{
    [Fact]
    public void Constructeur_ParametresValides_CreeLePlan()
    {
        var plan = new PlanAbonnement("Premium", 34.99m, 12);

        Assert.Equal("Premium", plan.NomPlan);
        Assert.Equal(34.99m, plan.PrixDeBase);
        Assert.Equal(12, plan.MoisEngagement);
        Assert.Equal(19.99m, plan.FraisInscription);
    }

    [Fact]
    public void Constructeur_AvecTousLesParametres_InitialiseCorrectement()
    {
        var plan = new PlanAbonnement("VIP", 49.99m, 24,
                                       fraisInscription: 0m,
                                       accesClubLimite: true,
                                       passDuoAutorise: true);

        Assert.Equal(0m, plan.FraisInscription);
        Assert.True(plan.AccesClubLimite);
        Assert.True(plan.PassDuoAutorise);
    }

    [Fact]
    public void Constructeur_NomVide_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new PlanAbonnement("", 19.99m));
    }

    [Fact]
    public void Constructeur_PrixNegatif_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new PlanAbonnement("Test", -1m));
    }

    [Fact]
    public void Constructeur_PrixZero_EstAccepte()
    {
        var plan = new PlanAbonnement("Gratuit", 0m);
        Assert.Equal(0m, plan.PrixDeBase);
    }

    [Fact]
    public void Constructeur_ValeursParDefaut_Correctes()
    {
        var plan = new PlanAbonnement("Basique", 19.99m);

        Assert.Equal(12, plan.MoisEngagement);
        Assert.Equal(19.99m, plan.FraisInscription);
        Assert.False(plan.AccesClubLimite);
        Assert.False(plan.PassDuoAutorise);
    }
}
