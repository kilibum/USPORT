using Usport.Console.Data;
using Usport.Console.Services;
using Usport.Domaine.Enumerations;
using Usport.Domaine.Tarification;

namespace Tests.Services;

public class ServiceContratTests
{
    private readonly StockDonnees _stock;
    private readonly ServiceContrat _serviceContrat;
    private readonly ServiceMembre _serviceMembre;
    private readonly int _idMembre;
    private readonly int _idPlan;
    private readonly int _idClub;

    public ServiceContratTests()
    {
        _stock = new StockDonnees();
        _stock.Amorcer();
        _serviceContrat = new ServiceContrat(_stock);
        _serviceMembre = new ServiceMembre(_stock);

        // Crée un membre de test actif.
        var membreDto = _serviceMembre.CreerMembre("Jean", "Test", "jean@test.be",
                                                     new DateOnly(2000, 1, 1), null, null);
        _idMembre = membreDto.Id;
        _idPlan = _stock.Plans[0].Id;
        _idClub = _stock.Clubs[0].Id;
    }

    // Constructeur

    [Fact]
    public void Constructeur_StockNull_LeveArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ServiceContrat(null!));
    }

    // CreerContrat

    [Fact]
    public void CreerContrat_ParametresValides_RetourneDtoAvecId()
    {
        var dto = _serviceContrat.CreerContrat(_idMembre, _idPlan, _idClub,
                                                new TarificationStandard());

        Assert.True(dto.Id > 0);
        Assert.Equal(_idMembre, dto.IdMembre);
        Assert.Equal(_idPlan, dto.IdPlan);
        Assert.Equal(StatutContrat.Actif, dto.Statut);
    }

    [Fact]
    public void CreerContrat_TarificationStandard_PrixCorrect()
    {
        var prixBase = _stock.Plans[0].PrixDeBase;
        var dto = _serviceContrat.CreerContrat(_idMembre, _idPlan, _idClub,
                                                new TarificationStandard());

        Assert.Equal(prixBase, dto.PrixMensuel);
    }

    [Fact]
    public void CreerContrat_TarificationEtudiant_PrixAvecRemise()
    {
        var prixBase = _stock.Plans[0].PrixDeBase;
        var dto = _serviceContrat.CreerContrat(_idMembre, _idPlan, _idClub,
                                                new TarificationEtudiant());

        // Remise 40% : prix * 0.60
        Assert.Equal(prixBase * 0.60m, dto.PrixMensuel);
    }

    [Fact]
    public void CreerContrat_MembreInexistant_LeveInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _serviceContrat.CreerContrat(999, _idPlan, _idClub, new TarificationStandard()));
    }

    [Fact]
    public void CreerContrat_PlanInexistant_LeveInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _serviceContrat.CreerContrat(_idMembre, 999, _idClub, new TarificationStandard()));
    }

    [Fact]
    public void CreerContrat_ClubInexistant_LeveInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _serviceContrat.CreerContrat(_idMembre, _idPlan, 999, new TarificationStandard()));
    }

    [Fact]
    public void CreerContrat_DateFinCalculeeCorrectement()
    {
        var moisEngagement = _stock.Plans[0].MoisEngagement;
        var dto = _serviceContrat.CreerContrat(_idMembre, _idPlan, _idClub,
                                                new TarificationStandard());

        Assert.NotNull(dto.DateFin);
        var duree = dto.DateFin!.Value.ToDateTime(TimeOnly.MinValue)
                    - dto.DateDebut.ToDateTime(TimeOnly.MinValue);
        // Vérifie que la durée est approximativement correcte (±5 jours pour les mois variables).
        Assert.True(duree.TotalDays >= (moisEngagement * 28));
    }

    // AnnulerContrat

    [Fact]
    public void AnnulerContrat_ContratActif_RetourneTrueEtStatutAnnule()
    {
        var contrat = _serviceContrat.CreerContrat(_idMembre, _idPlan, _idClub,
                                                    new TarificationStandard());

        var succes = _serviceContrat.AnnulerContrat(contrat.Id);

        Assert.True(succes);
        var contrats = _serviceContrat.ObtenirTousLesContrats();
        Assert.Equal(StatutContrat.Annule, contrats[0].Statut);
    }

    [Fact]
    public void AnnulerContrat_ContratDejaAnnule_RetourneFalse()
    {
        var contrat = _serviceContrat.CreerContrat(_idMembre, _idPlan, _idClub,
                                                    new TarificationStandard());
        _serviceContrat.AnnulerContrat(contrat.Id);

        var succes = _serviceContrat.AnnulerContrat(contrat.Id);
        Assert.False(succes);
    }

    [Fact]
    public void AnnulerContrat_ContratInexistant_RetourneFalse()
    {
        Assert.False(_serviceContrat.AnnulerContrat(999));
    }

    // ChangerPlanContrat

    [Fact]
    public void ChangerPlanContrat_ContratActif_RetourneTrueEtModifie()
    {
        var contrat = _serviceContrat.CreerContrat(_idMembre, _idPlan, _idClub,
                                                    new TarificationStandard());
        var nouveauPlan = _stock.Plans[1];
        var nouveauPrix = new TarificationStandard().CalculerMensuel(nouveauPlan.PrixDeBase);

        var succes = _serviceContrat.ChangerPlanContrat(contrat.Id, nouveauPlan.Id, nouveauPrix);

        Assert.True(succes);
        var contrats = _serviceContrat.ObtenirTousLesContrats();
        Assert.Equal(nouveauPlan.Id, contrats[0].IdPlan);
        Assert.Equal(nouveauPrix, contrats[0].PrixMensuel);
    }

    [Fact]
    public void ChangerPlanContrat_PlanInexistant_LeveInvalidOperationException()
    {
        var contrat = _serviceContrat.CreerContrat(_idMembre, _idPlan, _idClub,
                                                    new TarificationStandard());

        Assert.Throws<InvalidOperationException>(() =>
            _serviceContrat.ChangerPlanContrat(contrat.Id, 999, 50m));
    }

    [Fact]
    public void ChangerPlanContrat_ContratInexistant_RetourneFalse()
    {
        Assert.False(_serviceContrat.ChangerPlanContrat(999, _idPlan, 50m));
    }

    // ObtenirContratsActifs

    [Fact]
    public void ObtenirContratsActifs_AucunContrat_RetourneListeVide()
    {
        Assert.Empty(_serviceContrat.ObtenirContratsActifs());
    }

    [Fact]
    public void ObtenirContratsActifs_ExclutLesAnnules()
    {
        var contrat = _serviceContrat.CreerContrat(_idMembre, _idPlan, _idClub,
                                                    new TarificationStandard());
        _serviceContrat.AnnulerContrat(contrat.Id);

        Assert.Empty(_serviceContrat.ObtenirContratsActifs());
    }

    // SupprimerContrat

    [Fact]
    public void SupprimerContrat_Existant_RetourneTrueEtSupprime()
    {
        var contrat = _serviceContrat.CreerContrat(_idMembre, _idPlan, _idClub,
                                                    new TarificationStandard());

        Assert.True(_serviceContrat.SupprimerContrat(contrat.Id));
        Assert.Empty(_serviceContrat.ObtenirTousLesContrats());
    }

    [Fact]
    public void SupprimerContrat_Inexistant_RetourneFalse()
    {
        Assert.False(_serviceContrat.SupprimerContrat(999));
    }

    // ObtenirNomCompletMembre

    [Fact]
    public void ObtenirNomCompletMembre_Existant_RetourneLeNom()
    {
        Assert.Equal("Jean Test", _serviceContrat.ObtenirNomCompletMembre(_idMembre));
    }

    [Fact]
    public void ObtenirNomCompletMembre_Inexistant_RetourneNull()
    {
        Assert.Null(_serviceContrat.ObtenirNomCompletMembre(999));
    }

    // ObtenirInfosPlan

    [Fact]
    public void ObtenirInfosPlan_Existant_RetourneNomEtPrix()
    {
        var infos = _serviceContrat.ObtenirInfosPlan(_idPlan);
        Assert.NotNull(infos);
        Assert.False(string.IsNullOrEmpty(infos.Value.NomPlan));
        Assert.True(infos.Value.PrixDeBase >= 0);
    }

    [Fact]
    public void ObtenirInfosPlan_Inexistant_RetourneNull()
    {
        Assert.Null(_serviceContrat.ObtenirInfosPlan(999));
    }
}
