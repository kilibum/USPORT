using Usport.Console.Data;
using Usport.Console.Services;
using Usport.Domaine.Enumerations;

namespace Tests.Services;

public class ServiceEmployeeTests
{
    private readonly StockDonnees _stock;
    private readonly ServiceEmployee _service;

    public ServiceEmployeeTests()
    {
        _stock = new StockDonnees();
        _stock.Amorcer();
        _service = new ServiceEmployee(_stock);
    }

    // Constructeur

    [Fact]
    public void Constructeur_StockNull_LeveArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ServiceEmployee(null!));
    }

    // CreerEmploye

    [Fact]
    public void CreerEmploye_ParametresValides_RetourneDtoAvecId()
    {
        var idClub = _stock.Clubs[0].Id;
        var dto = _service.CreerEmploye("Jean", "Dupont", "jean@club.be",
                                         idClub, RoleEmploye.Instructeur,
                                         new DateOnly(2023, 6, 1), 3000m, null);

        Assert.True(dto.Id > 0);
        Assert.Equal("Jean Dupont", dto.NomComplet);
        Assert.Equal(RoleEmploye.Instructeur, dto.Role);
        Assert.Equal(3000m, dto.SalaireMensuel);
    }

    [Fact]
    public void CreerEmploye_EmailEnDouble_LeveInvalidOperationException()
    {
        var idClub = _stock.Clubs[0].Id;
        _service.CreerEmploye("Jean", "Dupont", "jean@club.be",
                               idClub, RoleEmploye.Instructeur,
                               new DateOnly(2023, 6, 1), null, null);

        Assert.Throws<InvalidOperationException>(() =>
            _service.CreerEmploye("Marie", "Martin", "jean@club.be",
                                   idClub, RoleEmploye.Receptionniste,
                                   new DateOnly(2023, 6, 1), null, null));
    }

    [Fact]
    public void CreerEmploye_EmailDoubleCasseInsensible_LeveInvalidOperationException()
    {
        var idClub = _stock.Clubs[0].Id;
        _service.CreerEmploye("Jean", "Dupont", "jean@club.be",
                               idClub, RoleEmploye.Instructeur,
                               new DateOnly(2023, 6, 1), null, null);

        Assert.Throws<InvalidOperationException>(() =>
            _service.CreerEmploye("Marie", "Martin", "JEAN@CLUB.BE",
                                   idClub, RoleEmploye.Instructeur,
                                   new DateOnly(2023, 6, 1), null, null));
    }

    [Fact]
    public void CreerEmploye_NomDuClubDansLeDto()
    {
        var club = _stock.Clubs[0];
        var dto = _service.CreerEmploye("Test", "User", "test@club.be",
                                         club.Id, RoleEmploye.Instructeur,
                                         new DateOnly(2023, 6, 1), null, null);

        Assert.Equal(club.Nom, dto.NomClub);
    }

    // ObtenirTousLesEmployes

    [Fact]
    public void ObtenirTousLesEmployes_AucunEmploye_RetourneListeVide()
    {
        var stock = new StockDonnees();
        var service = new ServiceEmployee(stock);

        Assert.Empty(service.ObtenirTousLesEmployes());
    }

    [Fact]
    public void ObtenirTousLesEmployes_TriesParNom()
    {
        var idClub = _stock.Clubs[0].Id;
        _service.CreerEmploye("Zoé", "Zara", "z@club.be", idClub,
                               RoleEmploye.Instructeur, new DateOnly(2023, 1, 1), null, null);
        _service.CreerEmploye("Alice", "Albert", "a@club.be", idClub,
                               RoleEmploye.Instructeur, new DateOnly(2023, 1, 1), null, null);

        var employes = _service.ObtenirTousLesEmployes();

        Assert.Equal("Alice Albert", employes[0].NomComplet);
        Assert.Equal("Zoé Zara", employes[1].NomComplet);
    }

    // ObtenirClubsSelectionnables

    [Fact]
    public void ObtenirClubsSelectionnables_RetourneTousLesClubs()
    {
        var clubs = _service.ObtenirClubsSelectionnables();
        Assert.Equal(_stock.Clubs.Count, clubs.Count);
    }

    // ObtenirInfosModificationEmploye

    [Fact]
    public void ObtenirInfosModificationEmploye_Existant_RetourneTuple()
    {
        var idClub = _stock.Clubs[0].Id;
        var dto = _service.CreerEmploye("Jean", "Test", "jean@club.be",
                                         idClub, RoleEmploye.Instructeur,
                                         new DateOnly(2023, 1, 1), 2500m, "BPJEPS");

        var infos = _service.ObtenirInfosModificationEmploye(dto.Id);

        Assert.NotNull(infos);
        Assert.Equal(2500m, infos.Value.Salaire);
        Assert.Equal("BPJEPS", infos.Value.Qualifications);
    }

    [Fact]
    public void ObtenirInfosModificationEmploye_Inexistant_RetourneNull()
    {
        Assert.Null(_service.ObtenirInfosModificationEmploye(999));
    }

    // MettreAJourEmploye

    [Fact]
    public void MettreAJourEmploye_Existant_RetourneTrueEtModifie()
    {
        var idClub = _stock.Clubs[0].Id;
        var dto = _service.CreerEmploye("Jean", "Test", "jean@club.be",
                                         idClub, RoleEmploye.Instructeur,
                                         new DateOnly(2023, 1, 1), 2000m, null);

        var succes = _service.MettreAJourEmploye(dto.Id, idClub,
                                                   RoleEmploye.Gerant, 4000m, "MBA");

        Assert.True(succes);

        var infos = _service.ObtenirInfosModificationEmploye(dto.Id);
        Assert.Equal(4000m, infos!.Value.Salaire);
        Assert.Equal("MBA", infos.Value.Qualifications);
    }

    [Fact]
    public void MettreAJourEmploye_Inexistant_RetourneFalse()
    {
        var succes = _service.MettreAJourEmploye(999, 1, RoleEmploye.Instructeur, null, null);
        Assert.False(succes);
    }

    [Fact]
    public void MettreAJourEmploye_ConserveLeMemeId()
    {
        var idClub = _stock.Clubs[0].Id;
        var dto = _service.CreerEmploye("Jean", "Test", "jean@club.be",
                                         idClub, RoleEmploye.Instructeur,
                                         new DateOnly(2023, 1, 1), null, null);

        _service.MettreAJourEmploye(dto.Id, idClub, RoleEmploye.Gerant, 5000m, null);

        var employes = _service.ObtenirTousLesEmployes();
        Assert.Single(employes);
        Assert.Equal(dto.Id, employes[0].Id);
    }

    // SupprimerEmploye

    [Fact]
    public void SupprimerEmploye_Existant_RetourneTrueEtSupprime()
    {
        var idClub = _stock.Clubs[0].Id;
        var dto = _service.CreerEmploye("Jean", "Test", "jean@club.be",
                                         idClub, RoleEmploye.Instructeur,
                                         new DateOnly(2023, 1, 1), null, null);

        var succes = _service.SupprimerEmploye(dto.Id);

        Assert.True(succes);
        Assert.Empty(_service.ObtenirTousLesEmployes());
    }

    [Fact]
    public void SupprimerEmploye_Inexistant_RetourneFalse()
    {
        Assert.False(_service.SupprimerEmploye(999));
    }
}
