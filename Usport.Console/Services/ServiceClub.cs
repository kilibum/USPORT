using Usport.Console.Data;
using Usport.Domaine.Entites.GestionClub;

namespace Usport.Console.Services;

/// <summary>
/// Service de gestion des clubs : consultation, fermeture et réouverture.
/// </summary>
public class ServiceClub
{
    private readonly StockDonnees _stockDonnees;

    public ServiceClub(StockDonnees stockDonnees)
    {
        _stockDonnees = stockDonnees ?? throw new ArgumentNullException(nameof(stockDonnees));
    }

    public List<Club> ObtenirTousLesClubs()
        => _stockDonnees.Clubs.OrderBy(c => c.Nom).ToList();

    public List<Club> ObtenirClubsOuverts()
        => _stockDonnees.Clubs.Where(c => c.EstOperationnel()).OrderBy(c => c.Nom).ToList();

    public List<Club> ObtenirClubsFermes()
        => _stockDonnees.Clubs.Where(c => !c.EstOperationnel()).OrderBy(c => c.Nom).ToList();

    public bool FermerClub(int id)
    {
        var club = _stockDonnees.Clubs.FirstOrDefault(c => c.Id == id);
        if (club is null) return false;
        club.Fermer();
        return true;
    }

    public bool RouvrirClub(int id)
    {
        var club = _stockDonnees.Clubs.FirstOrDefault(c => c.Id == id);
        if (club is null) return false;
        club.Reouvrir();
        return true;
    }
}
