using Usport.Domaine.Entites.GestionClub;
using Usport.Domaine.Enumerations;

namespace Tests.Domaine;

public class ClubTests
{
    // Construction

    [Fact]
    public void Constructeur_ParametresValides_CreeLClubCorrectement()
    {
        var club = new Club("Usport Test", "Rue Test 1", "Bruxelles", "1000",
                            DateOnly.FromDateTime(new DateTime(2023, 1, 1)));

        Assert.Equal("Usport Test", club.Nom);
        Assert.Equal("Bruxelles", club.AdresseVille);
        Assert.Equal("1000", club.AdresseCodePostal);
        Assert.Equal("Belgique", club.Pays);
        Assert.True(club.EstOuvert247);
        Assert.Equal(StatutOperationnelClub.Ouvert, club.StatutOperationnel);
    }

    [Fact]
    public void Constructeur_NomVide_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Club("", "Rue Test", "Bruxelles", "1000", DateOnly.FromDateTime(DateTime.Now)));
    }

    [Fact]
    public void Constructeur_NomNull_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Club(null!, "Rue Test", "Bruxelles", "1000", DateOnly.FromDateTime(DateTime.Now)));
    }

    [Fact]
    public void Constructeur_NomEspaces_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Club("   ", "Rue Test", "Bruxelles", "1000", DateOnly.FromDateTime(DateTime.Now)));
    }

    // Comportements

    [Fact]
    public void EstOperationnel_ClubOuvert_RetourneTrue()
    {
        var club = CreerClubValide();
        Assert.True(club.EstOperationnel());
    }

    [Fact]
    public void Fermer_ClubOuvert_PasseEnFermeTemporairement()
    {
        var club = CreerClubValide();
        club.Fermer();

        Assert.Equal(StatutOperationnelClub.FermeTemporairement, club.StatutOperationnel);
        Assert.False(club.EstOperationnel());
    }

    [Fact]
    public void Reouvrir_ClubFerme_PasseEnOuvert()
    {
        var club = CreerClubValide();
        club.Fermer();
        club.Reouvrir();

        Assert.Equal(StatutOperationnelClub.Ouvert, club.StatutOperationnel);
        Assert.True(club.EstOperationnel());
    }

    [Fact]
    public void Fermer_PuisReouvrir_PuisFermer_StatutCorrect()
    {
        var club = CreerClubValide();
        club.Fermer();
        club.Reouvrir();
        club.Fermer();

        Assert.False(club.EstOperationnel());
    }

    [Fact]
    public void AssignerId_DefinitLIdCorrectement()
    {
        var club = CreerClubValide();
        club.AssignerId(42);

        Assert.Equal(42, club.Id);
    }

    [Fact]
    public void ObtenirInfosAffichage_ContientNomEtVille()
    {
        var club = CreerClubValide();
        var infos = club.ObtenirInfosAffichage();

        Assert.Contains("Usport Test", infos);
        Assert.Contains("Bruxelles", infos);
    }

    // Helper

    private static Club CreerClubValide()
        => new("Usport Test", "Rue Test 1", "Bruxelles", "1000",
               DateOnly.FromDateTime(new DateTime(2023, 1, 1)));
}
