using Usport.Domaine.Entites.Adhesion;
using Usport.Domaine.Enumerations;

namespace Tests.Domaine;

public class MembreTests
{
    // Création 

    [Fact]
    public void Creer_ParametresValides_CreeLeMembre()
    {
        var membre = Membre.Creer("Jean", "Dupont", "jean@test.be",
                                   new DateOnly(2000, 5, 15));

        Assert.Equal("Jean", membre.Prenom);
        Assert.Equal("Dupont", membre.Nom);
        Assert.Equal("jean@test.be", membre.Email);
        Assert.Equal("Jean Dupont", membre.NomComplet);
        Assert.Equal(StatutMembre.Actif, membre.Statut);
    }

    [Fact]
    public void Creer_AvecOptionsCompletes_InitialiseCorrectement()
    {
        var membre = Membre.Creer("Marie", "Martin", "marie@test.be",
                                   new DateOnly(1995, 3, 20),
                                   telephone: "0470123456",
                                   ville: "Liège",
                                   genre: Genre.Femme,
                                   objectifPrincipal: ObjectifPrincipal.PerteDePoids,
                                   sourceAcquisition: SourceAcquisition.ReseauxSociaux);

        Assert.Equal(Genre.Femme, membre.Genre);
        Assert.Equal(ObjectifPrincipal.PerteDePoids, membre.ObjectifPrincipal);
        Assert.Equal(SourceAcquisition.ReseauxSociaux, membre.SourceAcquisition);
        Assert.Equal("0470123456", membre.TelephoneMobile);
        Assert.Equal("Liège", membre.AdresseVille);
    }

    // Validations Personne 

    [Fact]
    public void Creer_PrenomVide_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Membre.Creer("", "Dupont", "jean@test.be", new DateOnly(2000, 1, 1)));
    }

    [Fact]
    public void Creer_NomVide_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Membre.Creer("Jean", "", "jean@test.be", new DateOnly(2000, 1, 1)));
    }

    [Fact]
    public void Creer_EmailVide_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Membre.Creer("Jean", "Dupont", "", new DateOnly(2000, 1, 1)));
    }

    [Fact]
    public void Creer_PrenomEspaces_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Membre.Creer("   ", "Dupont", "jean@test.be", new DateOnly(2000, 1, 1)));
    }

    // Calcul d'âge

    [Fact]
    public void ObtenirAge_DateNaissanceConnue_RetourneAgeCorrect()
    {
        var dateNaissance = DateOnly.FromDateTime(DateTime.Today.AddYears(-25));
        var membre = Membre.Creer("Test", "User", "test@test.be", dateNaissance);

        Assert.Equal(25, membre.ObtenirAge());
    }

    [Fact]
    public void ObtenirAge_AnniversairePasPasse_RetourneAgeMoinsUn()
    {
        // Anniversaire dans 6 mois — l'âge ne devrait pas encore être incrémenté.
        var aujourdHui = DateTime.Today;
        var dateNaissance = new DateOnly(aujourdHui.Year - 30, aujourdHui.Month, aujourdHui.Day)
                              .AddMonths(6);

        // Si on dépasse décembre, on ajuste pour rester dans l'année correcte.
        if (dateNaissance > DateOnly.FromDateTime(aujourdHui))
        {
            var membre = Membre.Creer("Test", "User", "test@test.be", dateNaissance);
            Assert.Equal(29, membre.ObtenirAge());
        }
    }

    // Assignation d'ID

    [Fact]
    public void AssignerId_DefinitLIdCorrectement()
    {
        var membre = Membre.Creer("Jean", "Dupont", "jean@test.be", new DateOnly(2000, 1, 1));
        membre.AssignerId(99);

        Assert.Equal(99, membre.Id);
    }

    // Valeurs par défaut

    [Fact]
    public void Creer_SansOptionsOptionnelles_UtiliseValeursParDefaut()
    {
        var membre = Membre.Creer("Test", "User", "test@test.be", new DateOnly(2000, 1, 1));

        Assert.Equal(Genre.Autre, membre.Genre);
        Assert.Null(membre.ObjectifPrincipal);
        Assert.Null(membre.SourceAcquisition);
        Assert.Null(membre.TelephoneMobile);
        Assert.Null(membre.AdresseVille);
        Assert.Equal(0, membre.NombreVisitesTotal);
    }

    // Reconstituer

    [Fact]
    public void Reconstituer_RecreeUnMembreAvecTousLesChamps()
    {
        var dateCreation = DateTime.UtcNow.AddDays(-30);
        var dateModification = DateTime.UtcNow;
        var membre = Membre.Reconstituer(5, "Jean", "Dupont", "jean@test.be",
                                          new DateOnly(2000, 1, 1), "Namur",
                                          StatutMembre.Actif, 42,
                                          dateCreation, dateModification);

        Assert.Equal(5, membre.Id);
        Assert.Equal("Namur", membre.AdresseVille);
        Assert.Equal(StatutMembre.Actif, membre.Statut);
        Assert.Equal(42, membre.NombreVisitesTotal);
    }
}
