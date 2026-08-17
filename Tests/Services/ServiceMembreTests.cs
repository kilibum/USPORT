using Usport.Console.Data;
using Usport.Console.Services;
using Usport.Domaine.Enumerations;

namespace Tests.Services;

public class ServiceMembreTests
{
    private readonly StockDonnees _stock;
    private readonly ServiceMembre _service;

    public ServiceMembreTests()
    {
        _stock = new StockDonnees();
        _service = new ServiceMembre(_stock);
    }

    // Constructeur

    [Fact]
    public void Constructeur_StockNull_LeveArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ServiceMembre(null!));
    }

    // CreerMembre

    [Fact]
    public void CreerMembre_ParametresValides_RetourneDtoAvecId()
    {
        var dto = _service.CreerMembre("Jean", "Dupont", "jean@test.be",
                                        new DateOnly(2000, 1, 1), null, null);

        Assert.True(dto.Id > 0);
        Assert.Equal("Jean Dupont", dto.NomComplet);
        Assert.Equal("jean@test.be", dto.Email);
        Assert.Equal(StatutMembre.Actif, dto.Statut);
    }

    [Fact]
    public void CreerMembre_IdsAutoIncrementes_IdsDifferents()
    {
        var dto1 = _service.CreerMembre("Jean", "A", "a@test.be",
                                         new DateOnly(2000, 1, 1), null, null);
        var dto2 = _service.CreerMembre("Marie", "B", "b@test.be",
                                         new DateOnly(2000, 1, 1), null, null);

        Assert.NotEqual(dto1.Id, dto2.Id);
        Assert.True(dto2.Id > dto1.Id);
    }

    [Fact]
    public void CreerMembre_EmailEnDouble_LeveInvalidOperationException()
    {
        _service.CreerMembre("Jean", "Dupont", "jean@test.be",
                              new DateOnly(2000, 1, 1), null, null);

        Assert.Throws<InvalidOperationException>(() =>
            _service.CreerMembre("Marie", "Martin", "jean@test.be",
                                  new DateOnly(2000, 1, 1), null, null));
    }

    [Fact]
    public void CreerMembre_EmailEnDoubleCasseInsensible_LeveInvalidOperationException()
    {
        _service.CreerMembre("Jean", "Dupont", "jean@test.be",
                              new DateOnly(2000, 1, 1), null, null);

        Assert.Throws<InvalidOperationException>(() =>
            _service.CreerMembre("Marie", "Martin", "JEAN@TEST.BE",
                                  new DateOnly(2000, 1, 1), null, null));
    }

    [Fact]
    public void CreerMembre_AgeMoins16Ans_LeveArgumentException()
    {
        var dateNaissanceTropRecente = DateOnly.FromDateTime(DateTime.Today.AddYears(-10));

        Assert.Throws<ArgumentException>(() =>
            _service.CreerMembre("Enfant", "Test", "enfant@test.be",
                                  dateNaissanceTropRecente, null, null));
    }

    [Fact]
    public void CreerMembre_Age16AnsExact_EstAccepte()
    {
        var dateNaissance16Ans = DateOnly.FromDateTime(DateTime.Today.AddYears(-16));
        var dto = _service.CreerMembre("Jeune", "Test", "jeune@test.be",
                                        dateNaissance16Ans, null, null);

        Assert.NotNull(dto);
    }

    [Fact]
    public void CreerMembre_AvecGenreEtObjectif_ValeursDansLeDto()
    {
        var dto = _service.CreerMembre("Marie", "Test", "marie@test.be",
                                        new DateOnly(2000, 1, 1), null, "Liège",
                                        Genre.Femme, ObjectifPrincipal.PerteDePoids,
                                        SourceAcquisition.ReseauxSociaux);

        Assert.Equal(Genre.Femme, dto.Genre);
        Assert.Equal(ObjectifPrincipal.PerteDePoids, dto.ObjectifPrincipal);
        Assert.Equal("Liège", dto.AdresseVille);
    }

    // ObtenirTousLesMembres

    [Fact]
    public void ObtenirTousLesMembres_AucunMembre_RetourneListeVide()
    {
        var membres = _service.ObtenirTousLesMembres(1, 20);
        Assert.Empty(membres);
    }

    [Fact]
    public void ObtenirTousLesMembres_TriesParNom()
    {
        _service.CreerMembre("Zoé", "Zara", "z@test.be", new DateOnly(2000, 1, 1), null, null);
        _service.CreerMembre("Alice", "Albert", "a@test.be", new DateOnly(2000, 1, 1), null, null);

        var membres = _service.ObtenirTousLesMembres(1, 20);

        Assert.Equal("Alice Albert", membres[0].NomComplet);
        Assert.Equal("Zoé Zara", membres[1].NomComplet);
    }

    [Fact]
    public void ObtenirTousLesMembres_PaginationFonctionne()
    {
        for (int i = 0; i < 5; i++)
            _service.CreerMembre($"User{i}", "Test", $"user{i}@test.be",
                                  new DateOnly(2000, 1, 1), null, null);

        var page1 = _service.ObtenirTousLesMembres(1, 3);
        var page2 = _service.ObtenirTousLesMembres(2, 3);

        Assert.Equal(3, page1.Count);
        Assert.Equal(2, page2.Count);
    }

    // ObtenirMembresSelectionnables

    [Fact]
    public void ObtenirMembresSelectionnables_RetourneOptionsAvecIdEtNom()
    {
        _service.CreerMembre("Jean", "Test", "jean@test.be",
                              new DateOnly(2000, 1, 1), null, "Bruxelles");

        var options = _service.ObtenirMembresSelectionnables();

        Assert.Single(options);
        Assert.Equal("Jean Test", options[0].NomComplet);
        Assert.Equal("Bruxelles", options[0].Ville);
    }

    // ObtenirContactMembre

    [Fact]
    public void ObtenirContactMembre_MembreExistant_RetourneContact()
    {
        var dto = _service.CreerMembre("Jean", "Test", "jean@test.be",
                                        new DateOnly(2000, 1, 1), "0470000000", "Namur");

        var contact = _service.ObtenirContactMembre(dto.Id);

        Assert.NotNull(contact);
        Assert.Equal("0470000000", contact.Value.Telephone);
        Assert.Equal("Namur", contact.Value.Ville);
    }

    [Fact]
    public void ObtenirContactMembre_MembreInexistant_RetourneNull()
    {
        var contact = _service.ObtenirContactMembre(999);
        Assert.Null(contact);
    }

    // MettreAJourContactMembre

    [Fact]
    public void MettreAJourContactMembre_MembreExistant_RetourneTrue()
    {
        var dto = _service.CreerMembre("Jean", "Test", "jean@test.be",
                                        new DateOnly(2000, 1, 1), null, null);

        var succes = _service.MettreAJourContactMembre(dto.Id, "0470111111", "Liège");

        Assert.True(succes);

        var contact = _service.ObtenirContactMembre(dto.Id);
        Assert.Equal("0470111111", contact!.Value.Telephone);
        Assert.Equal("Liège", contact.Value.Ville);
    }

    [Fact]
    public void MettreAJourContactMembre_MembreInexistant_RetourneFalse()
    {
        var succes = _service.MettreAJourContactMembre(999, "0470000000", "Test");
        Assert.False(succes);
    }

    // SupprimerMembre

    [Fact]
    public void SupprimerMembre_MembreExistant_RetourneTrueEtSupprime()
    {
        var dto = _service.CreerMembre("Jean", "Test", "jean@test.be",
                                        new DateOnly(2000, 1, 1), null, null);

        var succes = _service.SupprimerMembre(dto.Id);

        Assert.True(succes);
        Assert.Empty(_service.ObtenirTousLesMembres(1, 20));
    }

    [Fact]
    public void SupprimerMembre_MembreInexistant_RetourneFalse()
    {
        var succes = _service.SupprimerMembre(999);
        Assert.False(succes);
    }

    [Fact]
    public void SupprimerMembre_SupprimerAussiLesContrats()
    {
        _stock.Amorcer();
        var dto = _service.CreerMembre("Jean", "Test", "jean@test.be",
                                        new DateOnly(2000, 1, 1), null, null);

        // Crée un contrat pour ce membre.
        var serviceContrat = new ServiceContrat(_stock);
        serviceContrat.CreerContrat(dto.Id, _stock.Plans[0].Id, _stock.Clubs[0].Id,
                                     new Usport.Domaine.Tarification.TarificationStandard());

        Assert.Single(_stock.Contrats);

        _service.SupprimerMembre(dto.Id);

        Assert.Empty(_stock.Contrats);
    }
}
