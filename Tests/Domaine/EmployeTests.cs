using Usport.Domaine.Entites.GestionClub;
using Usport.Domaine.Enumerations;

namespace Tests.Domaine;

public class EmployeTests
{
    [Fact]
    public void Creer_ParametresValides_CreeLEmploye()
    {
        var employe = Employe.Creer(1, "Jean", "Dupont", "jean@club.be",
                                     RoleEmploye.Instructeur, new DateOnly(2023, 6, 1));

        Assert.Equal("Jean Dupont", employe.NomComplet);
        Assert.Equal(1, employe.ClubId);
        Assert.Equal(RoleEmploye.Instructeur, employe.Role);
    }

    [Fact]
    public void Creer_AvecSalaireEtQualifications_InitialiseCorrectement()
    {
        var employe = Employe.Creer(1, "Marie", "Martin", "marie@club.be",
                                     RoleEmploye.Receptionniste, new DateOnly(2023, 1, 1),
                                     salaireMensuel: 2500m,
                                     qualifications: "Accueil client, gestion planning");

        Assert.Equal(2500m, employe.SalaireMensuel);
        Assert.Equal("Accueil client, gestion planning", employe.Qualifications);
    }

    [Fact]
    public void Creer_SansSalaire_SalaireMensuelEstNull()
    {
        var employe = Employe.Creer(1, "Test", "User", "test@club.be",
                                     RoleEmploye.Instructeur, new DateOnly(2023, 1, 1));

        Assert.Null(employe.SalaireMensuel);
    }

    [Fact]
    public void Creer_PrenomVide_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Employe.Creer(1, "", "Dupont", "jean@club.be",
                          RoleEmploye.Instructeur, new DateOnly(2023, 1, 1)));
    }

    [Fact]
    public void Creer_NomVide_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Employe.Creer(1, "Jean", "", "jean@club.be",
                          RoleEmploye.Instructeur, new DateOnly(2023, 1, 1)));
    }

    [Fact]
    public void Creer_EmailVide_LeveArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Employe.Creer(1, "Jean", "Dupont", "",
                          RoleEmploye.Instructeur, new DateOnly(2023, 1, 1)));
    }

    [Fact]
    public void AssignerId_DefinitLIdCorrectement()
    {
        var employe = Employe.Creer(1, "Test", "User", "test@club.be",
                                     RoleEmploye.Instructeur, new DateOnly(2023, 1, 1));
        employe.AssignerId(77);

        Assert.Equal(77, employe.Id);
    }

    [Fact]
    public void ObtenirInfosAffichage_ContientNomEtRole()
    {
        var employe = Employe.Creer(1, "Jean", "Dupont", "jean@club.be",
                                     RoleEmploye.Instructeur, new DateOnly(2023, 1, 1),
                                     salaireMensuel: 3000m);
        var infos = employe.ObtenirInfosAffichage();

        Assert.Contains("Jean Dupont", infos);
        Assert.Contains("Instructeur", infos);
    }
}
