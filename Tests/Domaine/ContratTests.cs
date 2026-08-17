using Usport.Domaine.Entites.Adhesion;
using Usport.Domaine.Enumerations;
using Usport.Domaine.Exceptions;

namespace Tests.Domaine;

public class ContratTests
{
    // Construction

    [Fact]
    public void Constructeur_ParametresValides_CreLeContrat()
    {
        var contrat = new Contrat(1, 2, 3,
                                   new DateOnly(2024, 1, 1), new DateOnly(2025, 1, 1));

        Assert.Equal(1, contrat.IdMembre);
        Assert.Equal(2, contrat.IdPlan);
        Assert.Equal(3, contrat.IdClubPrincipal);
        Assert.Equal(StatutContrat.Actif, contrat.Statut);
        Assert.Equal(TypeContrat.DureeDeterminee, contrat.TypeContrat);
    }

    [Fact]
    public void EstActif_ContratNeuf_RetourneTrue()
    {
        var contrat = CreerContratValide();
        Assert.True(contrat.EstActif());
    }

    // Annulation 

    [Fact]
    public void Annuler_ContratActif_PasseEnAnnule()
    {
        var contrat = CreerContratValide();
        contrat.Annuler("Déménagement");

        Assert.Equal(StatutContrat.Annule, contrat.Statut);
        Assert.False(contrat.EstActif());
        Assert.Equal("Déménagement", contrat.MotifAnnulation);
        Assert.NotNull(contrat.DateAnnulation);
    }

    [Fact]
    public void Annuler_SansMotif_AccepteMotifNull()
    {
        var contrat = CreerContratValide();
        contrat.Annuler();

        Assert.Equal(StatutContrat.Annule, contrat.Statut);
        Assert.Null(contrat.MotifAnnulation);
    }

    [Fact]
    public void Annuler_ContratDejaAnnule_LeveExceptionRegleMetier()
    {
        var contrat = CreerContratValide();
        contrat.Annuler();

        var ex = Assert.Throws<ExceptionRegleMetier>(() => contrat.Annuler());
        Assert.Equal("CONTRAT_DEJA_ANNULE", ex.Code);
    }

    // Gel

    [Fact]
    public void Geler_ContratActif_DefinitLesDatesDeGel()
    {
        var contrat = CreerContratValide();
        var debut = new DateOnly(2024, 6, 1);
        var fin = new DateOnly(2024, 7, 1);

        contrat.Geler(debut, fin);

        Assert.Equal(debut, contrat.DateDebutGel);
        Assert.Equal(fin, contrat.DateFinGel);
    }

    [Fact]
    public void Geler_DateFinAvantDateDebut_LeveArgumentException()
    {
        var contrat = CreerContratValide();
        var debut = new DateOnly(2024, 7, 1);
        var fin = new DateOnly(2024, 6, 1);

        Assert.Throws<ArgumentException>(() => contrat.Geler(debut, fin));
    }

    [Fact]
    public void Geler_DateFinEgaleDateDebut_LeveArgumentException()
    {
        var contrat = CreerContratValide();
        var date = new DateOnly(2024, 6, 1);

        Assert.Throws<ArgumentException>(() => contrat.Geler(date, date));
    }

    [Fact]
    public void Geler_ContratAnnule_LeveExceptionRegleMetier()
    {
        var contrat = CreerContratValide();
        contrat.Annuler();

        Assert.Throws<ExceptionRegleMetier>(() =>
            contrat.Geler(new DateOnly(2024, 6, 1), new DateOnly(2024, 7, 1)));
    }

    // Helper

    private static Contrat CreerContratValide()
        => new(1, 1, 1, new DateOnly(2024, 1, 1), new DateOnly(2025, 1, 1));
}
