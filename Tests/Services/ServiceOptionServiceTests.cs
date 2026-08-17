using Usport.Console.Data;
using Usport.Console.Services;

namespace Tests.Services;

public class ServiceOptionServiceTests
{
    private readonly StockDonnees _stock;
    private readonly ServiceOptionService _service;

    public ServiceOptionServiceTests()
    {
        _stock = new StockDonnees();
        _service = new ServiceOptionService(_stock);
    }

    // Constructeur 

    [Fact]
    public void Constructeur_StockNull_LeveArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ServiceOptionService(null!));
    }

    // CreerOptionService

    [Fact]
    public void CreerOptionService_ParametresValides_RetourneDtoAvecId()
    {
        var dto = _service.CreerOptionService("Coach privé", 25m);

        Assert.True(dto.Id > 0);
        Assert.Equal("Coach privé", dto.NomOption);
        Assert.Equal(25m, dto.PrixMensuel);
    }

    [Fact]
    public void CreerOptionService_PrixZero_EstAccepte()
    {
        var dto = _service.CreerOptionService("Gratuit", 0m);
        Assert.Equal(0m, dto.PrixMensuel);
    }

    [Fact]
    public void CreerOptionService_NomEnDouble_LeveInvalidOperationException()
    {
        _service.CreerOptionService("Coach privé", 25m);

        Assert.Throws<InvalidOperationException>(() =>
            _service.CreerOptionService("Coach privé", 30m));
    }

    [Fact]
    public void CreerOptionService_NomEnDoubleCasseInsensible_LeveInvalidOperationException()
    {
        _service.CreerOptionService("Coach privé", 25m);

        Assert.Throws<InvalidOperationException>(() =>
            _service.CreerOptionService("COACH PRIVÉ", 30m));
    }

    [Fact]
    public void CreerOptionService_NomVide_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            _service.CreerOptionService("", 10m));
    }

    [Fact]
    public void CreerOptionService_PrixNegatif_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            _service.CreerOptionService("Test", -1m));
    }

    [Fact]
    public void CreerOptionService_IdsAutoIncrementes()
    {
        var dto1 = _service.CreerOptionService("Option A", 10m);
        var dto2 = _service.CreerOptionService("Option B", 20m);

        Assert.True(dto2.Id > dto1.Id);
    }

    // ObtenirToutesLesOptions 

    [Fact]
    public void ObtenirToutesLesOptions_AucuneOption_RetourneListeVide()
    {
        Assert.Empty(_service.ObtenirToutesLesOptions());
    }

    [Fact]
    public void ObtenirToutesLesOptions_TrieesParNom()
    {
        _service.CreerOptionService("Sauna", 10m);
        _service.CreerOptionService("Coach", 25m);
        _service.CreerOptionService("Yoga", 15m);

        var options = _service.ObtenirToutesLesOptions();

        Assert.Equal("Coach", options[0].NomOption);
        Assert.Equal("Sauna", options[1].NomOption);
        Assert.Equal("Yoga", options[2].NomOption);
    }

    [Fact]
    public void ObtenirToutesLesOptions_ContientDateCreation()
    {
        _service.CreerOptionService("Test", 10m);

        var options = _service.ObtenirToutesLesOptions();

        Assert.True(options[0].DateCreation <= DateTime.UtcNow);
        Assert.True(options[0].DateCreation > DateTime.UtcNow.AddMinutes(-1));
    }

    // ObtenirOptionsSelectionnables 

    [Fact]
    public void ObtenirOptionsSelectionnables_RetourneRecordsAvecIdNomPrix()
    {
        _service.CreerOptionService("Coach", 25m);

        var options = _service.ObtenirOptionsSelectionnables();

        Assert.Single(options);
        Assert.Equal("Coach", options[0].NomOption);
        Assert.Equal(25m, options[0].PrixMensuel);
    }

    // ObtenirInfosModification

    [Fact]
    public void ObtenirInfosModification_Existant_RetourneTuple()
    {
        var dto = _service.CreerOptionService("Coach", 25m);

        var infos = _service.ObtenirInfosModification(dto.Id);

        Assert.NotNull(infos);
        Assert.Equal("Coach", infos.Value.Nom);
        Assert.Equal(25m, infos.Value.Prix);
    }

    [Fact]
    public void ObtenirInfosModification_Inexistant_RetourneNull()
    {
        Assert.Null(_service.ObtenirInfosModification(999));
    }

    // MettreAJourOptionService 

    [Fact]
    public void MettreAJourOptionService_Existant_RetourneTrueEtModifie()
    {
        var dto = _service.CreerOptionService("Ancien", 10m);

        var succes = _service.MettreAJourOptionService(dto.Id, "Nouveau", 20m);

        Assert.True(succes);

        var infos = _service.ObtenirInfosModification(dto.Id);
        Assert.Equal("Nouveau", infos!.Value.Nom);
        Assert.Equal(20m, infos.Value.Prix);
    }

    [Fact]
    public void MettreAJourOptionService_Inexistant_RetourneFalse()
    {
        Assert.False(_service.MettreAJourOptionService(999, "Test", 10m));
    }

    [Fact]
    public void MettreAJourOptionService_NomDejaExistant_LeveInvalidOperationException()
    {
        _service.CreerOptionService("Coach", 25m);
        var dto2 = _service.CreerOptionService("Sauna", 10m);

        Assert.Throws<InvalidOperationException>(() =>
            _service.MettreAJourOptionService(dto2.Id, "Coach", 10m));
    }

    [Fact]
    public void MettreAJourOptionService_MemeNomSansChangement_NeLeveRien()
    {
        var dto = _service.CreerOptionService("Coach", 25m);

        // Le même nom est accepté (on modifie uniquement le prix).
        var succes = _service.MettreAJourOptionService(dto.Id, "Coach", 30m);
        Assert.True(succes);
    }

    [Fact]
    public void MettreAJourOptionService_NomVide_LeveArgumentException()
    {
        var dto = _service.CreerOptionService("Test", 10m);

        Assert.Throws<ArgumentException>(() =>
            _service.MettreAJourOptionService(dto.Id, "", 10m));
    }

    [Fact]
    public void MettreAJourOptionService_PrixNegatif_LeveArgumentException()
    {
        var dto = _service.CreerOptionService("Test", 10m);

        Assert.Throws<ArgumentException>(() =>
            _service.MettreAJourOptionService(dto.Id, "Test", -5m));
    }

    // SupprimerOptionService

    [Fact]
    public void SupprimerOptionService_Existant_RetourneTrueEtSupprime()
    {
        var dto = _service.CreerOptionService("Coach", 25m);

        Assert.True(_service.SupprimerOptionService(dto.Id));
        Assert.Empty(_service.ObtenirToutesLesOptions());
    }

    [Fact]
    public void SupprimerOptionService_Inexistant_RetourneFalse()
    {
        Assert.False(_service.SupprimerOptionService(999));
    }

    [Fact]
    public void SupprimerOptionService_PuisRecreerAvecMemeNom_Fonctionne()
    {
        var dto = _service.CreerOptionService("Coach", 25m);
        _service.SupprimerOptionService(dto.Id);

        // Doit pouvoir recréer avec le même nom après suppression.
        var nouveau = _service.CreerOptionService("Coach", 30m);
        Assert.Equal("Coach", nouveau.NomOption);
    }

    // Intégration avec StockDonnees.Amorcer

    [Fact]
    public void OptionsAmorcees_SontRecuperablesParLeService()
    {
        var stockAmorce = new StockDonnees();
        stockAmorce.Amorcer();
        var service = new ServiceOptionService(stockAmorce);

        var options = service.ObtenirToutesLesOptions();

        Assert.Equal(4, options.Count);
    }

    [Fact]
    public void OptionAmorcee_NomEnDouble_LeveInvalidOperationException()
    {
        var stockAmorce = new StockDonnees();
        stockAmorce.Amorcer();
        var service = new ServiceOptionService(stockAmorce);

        Assert.Throws<InvalidOperationException>(() =>
            service.CreerOptionService("Coach privé", 50m));
    }
}
