using Usport.Domaine.Entites.Adhesion;

namespace Tests.Domaine;

public class OptionServiceTests
{
    // Construction

    [Fact]
    public void Constructeur_ParametresValides_CreeLOption()
    {
        var option = new OptionService("Coach privé", 25.00m);

        Assert.Equal("Coach privé", option.NomOption);
        Assert.Equal(25.00m, option.PrixMensuel);
        Assert.True(option.DateCreation <= DateTime.UtcNow);
    }

    [Fact]
    public void Constructeur_PrixZero_EstAccepte()
    {
        var option = new OptionService("Option gratuite", 0m);
        Assert.Equal(0m, option.PrixMensuel);
    }

    [Fact]
    public void Constructeur_NomVide_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new OptionService("", 10m));
    }

    [Fact]
    public void Constructeur_NomNull_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new OptionService(null!, 10m));
    }

    [Fact]
    public void Constructeur_NomEspaces_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new OptionService("   ", 10m));
    }

    [Fact]
    public void Constructeur_PrixNegatif_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new OptionService("Test", -1m));
    }

    // MettreAJour

    [Fact]
    public void MettreAJour_NouveauNomEtPrix_MetAJourCorrectement()
    {
        var option = new OptionService("Ancien nom", 10m);
        var dateModifAvant = option.DateModification;

        // Petit délai pour garantir que DateModification change.
        option.MettreAJour("Nouveau nom", 20m);

        Assert.Equal("Nouveau nom", option.NomOption);
        Assert.Equal(20m, option.PrixMensuel);
        Assert.True(option.DateModification >= dateModifAvant);
    }

    [Fact]
    public void MettreAJour_NomVide_LeveArgumentException()
    {
        var option = new OptionService("Test", 10m);
        Assert.Throws<ArgumentException>(() => option.MettreAJour("", 10m));
    }

    [Fact]
    public void MettreAJour_PrixNegatif_LeveArgumentException()
    {
        var option = new OptionService("Test", 10m);
        Assert.Throws<ArgumentException>(() => option.MettreAJour("Test", -5m));
    }

    [Fact]
    public void MettreAJour_PrixZero_EstAccepte()
    {
        var option = new OptionService("Test", 10m);
        option.MettreAJour("Test gratuit", 0m);

        Assert.Equal(0m, option.PrixMensuel);
    }

    [Fact]
    public void MettreAJour_NeChangePasDateCreation()
    {
        var option = new OptionService("Test", 10m);
        var dateCreation = option.DateCreation;

        option.MettreAJour("Modifié", 20m);

        Assert.Equal(dateCreation, option.DateCreation);
    }

    // AssignerId

    [Fact]
    public void AssignerId_DefinitLIdCorrectement()
    {
        var option = new OptionService("Test", 10m);
        option.AssignerId(42);

        Assert.Equal(42, option.Id);
    }
}
