using Usport.Console.Data;
using Usport.Console.DTOs;
using Usport.Domaine.Entites.Adhesion;

namespace Usport.Console.Services;

/// <summary>
/// Service de gestion des options de service (stockage en mémoire).
/// </summary>
public class ServiceOptionService
{
    private readonly StockDonnees _stockDonnees;

    public ServiceOptionService(StockDonnees stockDonnees)
    {
        _stockDonnees = stockDonnees ?? throw new ArgumentNullException(nameof(stockDonnees));
    }

    // Crée une option de service après validation de l'unicité du nom.
    public OptionServiceDto CreerOptionService(string nom, decimal prix)
    {
        if (_stockDonnees.OptionsService.Any(o => o.NomOption.Equals(nom, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"L'option '{nom}' existe déjà.");

        var option = new OptionService(nom, prix);
        option.AssignerId(_stockDonnees.ProchainIdOptionService());
        _stockDonnees.OptionsService.Add(option);

        return VersDto(option);
    }

    // Liste toutes les options triées par nom.
    public List<OptionServiceDto> ObtenirToutesLesOptions()
    {
        return _stockDonnees.OptionsService
            .OrderBy(o => o.NomOption)
            .Select(VersDto)
            .ToList();
    }

    // Options proposées à la sélection dans les menus interactifs.
    public List<OptionServiceSelection> ObtenirOptionsSelectionnables()
    {
        return _stockDonnees.OptionsService
            .OrderBy(o => o.NomOption)
            .Select(o => new OptionServiceSelection(o.Id, o.NomOption, o.PrixMensuel))
            .ToList();
    }

    // Récupère le nom et le prix actuels pour pré-remplir le formulaire de modification.
    public (string Nom, decimal Prix)? ObtenirInfosModification(int id)
    {
        var option = _stockDonnees.OptionsService.FirstOrDefault(o => o.Id == id);
        if (option is null) return null;
        return (option.NomOption, option.PrixMensuel);
    }

    // Met à jour le nom et le prix d'une option existante.
    public bool MettreAJourOptionService(int id, string nom, decimal prix)
    {
        var option = _stockDonnees.OptionsService.FirstOrDefault(o => o.Id == id);
        if (option is null) return false;

        // Vérifie l'unicité du nom si celui-ci change.
        if (!option.NomOption.Equals(nom, StringComparison.OrdinalIgnoreCase)
            && _stockDonnees.OptionsService.Any(o => o.NomOption.Equals(nom, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"L'option '{nom}' existe déjà.");

        option.MettreAJour(nom, prix);
        return true;
    }

    // Supprime définitivement une option de service.
    public bool SupprimerOptionService(int id)
    {
        var option = _stockDonnees.OptionsService.FirstOrDefault(o => o.Id == id);
        if (option is null) return false;
        _stockDonnees.OptionsService.Remove(option);
        return true;
    }

    private static OptionServiceDto VersDto(OptionService option) => new OptionServiceDto
    {
        Id           = option.Id,
        NomOption    = option.NomOption,
        PrixMensuel  = option.PrixMensuel,
        DateCreation = option.DateCreation
    };
}
