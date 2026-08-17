using Usport.Domaine.Tarification;

namespace Tests.Domaine;

public class TarificationStandardTests
{
    private readonly TarificationStandard _strategie = new();

    [Fact]
    public void NomStrategie_RetourneStandard()
    {
        Assert.Equal("Standard", _strategie.NomStrategie);
    }

    [Fact]
    public void CalculerMensuel_RetournePrixSansRemise()
    {
        Assert.Equal(30m, _strategie.CalculerMensuel(30m));
    }

    [Fact]
    public void CalculerAnnuel_AppliqueRemise10Pourcent()
    {
        // 30 * 12 * 0.90 = 324
        Assert.Equal(324m, _strategie.CalculerAnnuel(30m));
    }

    [Fact]
    public void CalculerMensuel_PrixZero_RetourneZero()
    {
        Assert.Equal(0m, _strategie.CalculerMensuel(0m));
    }

    [Fact]
    public void CalculerAnnuel_PrixZero_RetourneZero()
    {
        Assert.Equal(0m, _strategie.CalculerAnnuel(0m));
    }
}

public class TarificationEtudiantTests
{
    private readonly TarificationEtudiant _strategie = new();

    [Fact]
    public void NomStrategie_ContientEtudiant()
    {
        Assert.Contains("tudiant", _strategie.NomStrategie);
    }

    [Fact]
    public void CalculerMensuel_AppliqueRemise40Pourcent()
    {
        // 50 * 0.60 = 30
        Assert.Equal(30m, _strategie.CalculerMensuel(50m));
    }

    [Fact]
    public void CalculerAnnuel_AppliqueRemise40PourcentSurDouze()
    {
        // 50 * 0.60 * 12 = 360
        Assert.Equal(360m, _strategie.CalculerAnnuel(50m));
    }

    [Fact]
    public void CalculerMensuel_PrixZero_RetourneZero()
    {
        Assert.Equal(0m, _strategie.CalculerMensuel(0m));
    }
}

public class TarificationPromotionnelleTests
{
    [Fact]
    public void Constructeur_RemiseValide_CreeLaStrategie()
    {
        var strategie = new TarificationPromotionnelle(0.25m);
        Assert.Contains("25", strategie.NomStrategie);
    }

    [Fact]
    public void Constructeur_RemiseZero_EstAcceptee()
    {
        var strategie = new TarificationPromotionnelle(0m);
        Assert.Equal(50m, strategie.CalculerMensuel(50m));
    }

    [Fact]
    public void Constructeur_Remise100Pourcent_EstAcceptee()
    {
        var strategie = new TarificationPromotionnelle(1m);
        Assert.Equal(0m, strategie.CalculerMensuel(50m));
    }

    [Fact]
    public void Constructeur_RemiseNegative_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new TarificationPromotionnelle(-0.1m));
    }

    [Fact]
    public void Constructeur_RemiseSuperieure1_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new TarificationPromotionnelle(1.1m));
    }

    [Fact]
    public void CalculerMensuel_Remise25Pourcent_CalculeCorrectement()
    {
        var strategie = new TarificationPromotionnelle(0.25m);
        // 40 * 0.75 = 30
        Assert.Equal(30m, strategie.CalculerMensuel(40m));
    }

    [Fact]
    public void CalculerAnnuel_Remise25Pourcent_CalculeCorrectement()
    {
        var strategie = new TarificationPromotionnelle(0.25m);
        // 40 * 0.75 * 12 = 360
        Assert.Equal(360m, strategie.CalculerAnnuel(40m));
    }

    [Fact]
    public void CalculerAnnuel_PasDeRemiseFideliteSupplementaire()
    {
        // Vérifie que la tarification promotionnelle n'ajoute PAS la remise de fidélité 10%
        // contrairement à la tarification standard.
        var promo = new TarificationPromotionnelle(0m);
        var standard = new TarificationStandard();

        // Promo 0% annuel = prix * 12 (pas de remise fidélité)
        // Standard annuel = prix * 12 * 0.90 (remise fidélité 10%)
        Assert.True(promo.CalculerAnnuel(100m) > standard.CalculerAnnuel(100m));
    }
}
