using Usport.Console.DTOs;
using Usport.Console.Services;
using Usport.Console.IU;
using Spectre.Console;

namespace Usport.Console.Menus;

/// <summary>
/// Menus interactifs liés à la gestion des options de service (CRUD complet).
/// </summary>
public class MenuOptionService
{
    private readonly ServiceOptionService _serviceOption;

    public MenuOptionService(ServiceOptionService serviceOption)
    {
        _serviceOption = serviceOption ?? throw new ArgumentNullException(nameof(serviceOption));
    }

    // Saisie séquentielle du nom et du prix, puis création via le service.
    public void AfficherMenuAjouterOption()
    {
        AnsiConsole.Clear();
        InterfaceConsole.AfficherTitre("AJOUTER UNE OPTION DE SERVICE");
        InterfaceConsole.AfficherSeparateur("saisie séquentielle");
        AnsiConsole.WriteLine();

        var nom = AnsiConsole.Ask<string>("     [bold]Nom de l'option :[/] ");
        if (string.IsNullOrWhiteSpace(nom))
        {
            InterfaceConsole.AfficherAlerte("Le nom de l'option est obligatoire.");
            return;
        }

        var prixStr = AnsiConsole.Ask<string>(" [bold]Prix mensuel (EUR) :[/] ");
        if (!decimal.TryParse(prixStr, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var prix) || prix < 0)
        {
            InterfaceConsole.AfficherAlerte("Prix invalide — saisir un nombre positif (ex : 15.00).");
            return;
        }

        AnsiConsole.WriteLine();
        InterfaceConsole.AfficherSeparateur("résultat");

        try
        {
            var dto = _serviceOption.CreerOptionService(nom.Trim(), prix);

            InterfaceConsole.AfficherSucces(
                $"Option créée — ID #{dto.Id:D4} — {dto.NomOption} ({dto.PrixMensuel:F2} EUR/mois)",
                $"#{dto.Id:D4}");
        }
        catch (InvalidOperationException ex)
        {
            InterfaceConsole.AfficherAlerte(ex.Message);
        }
        catch (ArgumentException ex)
        {
            InterfaceConsole.AfficherAlerte("validation échouée", new List<string> { ex.Message });
        }
        catch (Exception ex)
        {
            InterfaceConsole.AfficherErreur(ex.Message);
        }
    }

    // Tableau listant toutes les options de service.
    public void AfficherMenuListeOptions()
    {
        AnsiConsole.Clear();
        InterfaceConsole.AfficherTitre("LISTE DES OPTIONS DE SERVICE");

        try
        {
            var options = _serviceOption.ObtenirToutesLesOptions();

            if (options.Count == 0)
            {
                AnsiConsole.Write(new Panel(
                    new Markup("[dim][[ État vide ]] — Aucune option de service.[/]"))
                    .Border(BoxBorder.Rounded)
                    .BorderStyle(new Style(Color.Grey))
                    .Padding(1, 0));
                return;
            }

            var table = new Table()
                .AddColumn(new TableColumn("[bold]ID[/]").Centered())
                .AddColumn(new TableColumn("[bold]Option[/]"))
                .AddColumn(new TableColumn("[bold]Prix mensuel[/]").Centered())
                .AddColumn(new TableColumn("[bold]Créée le[/]").Centered())
                .Border(TableBorder.Rounded)
                .BorderStyle(new Style(Color.Grey));

            foreach (var opt in options)
            {
                table.AddRow(
                    opt.Id.ToString("D3"),
                    Markup.Escape(opt.NomOption),
                    $"{opt.PrixMensuel:F2} EUR",
                    opt.DateCreation.ToString("yyyy-MM-dd"));
            }

            AnsiConsole.Write(table);
        }
        catch (Exception ex)
        {
            InterfaceConsole.AfficherErreur(ex.Message);
        }
    }

    // Sélection d'une option, puis modification du nom et du prix.
    public void AfficherMenuModifierOption()
    {
        AnsiConsole.Clear();
        InterfaceConsole.AfficherTitre("MODIFIER UNE OPTION DE SERVICE");

        try
        {
            var options = _serviceOption.ObtenirOptionsSelectionnables();
            if (options.Count == 0)
            {
                InterfaceConsole.AfficherAlerte("Aucune option de service à modifier.");
                return;
            }

            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<OptionServiceSelection>()
                    .Title(" [bold]Sélectionnez l'option à modifier :[/]")
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .PageSize(10)
                    .MoreChoicesText("[dim]Flèches pour faire défiler[/]")
                    .UseConverter(o => $"#{o.Id:D4} — {Markup.Escape(o.NomOption)}  [dim]({o.PrixMensuel:F2} EUR)[/]")
                    .AddChoices(options));

            var infos = _serviceOption.ObtenirInfosModification(selection.Id);
            if (infos is null)
            {
                InterfaceConsole.AfficherAlerte("Option introuvable.");
                return;
            }

            AnsiConsole.WriteLine();
            InterfaceConsole.AfficherSeparateur("valeurs actuelles — ENTRÉE pour conserver");
            AnsiConsole.WriteLine();

            var nom = AnsiConsole.Prompt(
                new TextPrompt<string>("     [bold]Nom :[/] ")
                    .DefaultValue(infos.Value.Nom)
                    .AllowEmpty());

            var prixStr = AnsiConsole.Prompt(
                new TextPrompt<string>(" [bold]Prix (EUR) :[/] ")
                    .DefaultValue(infos.Value.Prix.ToString("F2"))
                    .AllowEmpty());

            if (!decimal.TryParse(prixStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var prix) || prix < 0)
            {
                InterfaceConsole.AfficherAlerte("Prix invalide — saisir un nombre positif.");
                return;
            }

            AnsiConsole.WriteLine();
            InterfaceConsole.AfficherSeparateur("résultat");

            bool succes = _serviceOption.MettreAJourOptionService(
                selection.Id,
                string.IsNullOrWhiteSpace(nom) ? infos.Value.Nom : nom.Trim(),
                prix);

            if (succes)
                InterfaceConsole.AfficherSucces($"Option mise à jour — {nom.Trim()} ({prix:F2} EUR/mois).", $"#{selection.Id:D4}");
            else
                InterfaceConsole.AfficherAlerte("Aucune modification effectuée.");
        }
        catch (InvalidOperationException ex)
        {
            InterfaceConsole.AfficherAlerte(ex.Message);
        }
        catch (Exception ex)
        {
            InterfaceConsole.AfficherErreur(ex.Message);
        }
    }

    // Sélection d'une option puis suppression après confirmation.
    public void AfficherMenuSupprimerOption()
    {
        AnsiConsole.Clear();
        InterfaceConsole.AfficherTitre("SUPPRIMER UNE OPTION DE SERVICE");

        try
        {
            var options = _serviceOption.ObtenirOptionsSelectionnables();
            if (options.Count == 0)
            {
                InterfaceConsole.AfficherAlerte("Aucune option de service à supprimer.");
                return;
            }

            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<OptionServiceSelection>()
                    .Title(" [bold]Sélectionnez l'option à supprimer :[/]")
                    .HighlightStyle(new Style(Color.Red, decoration: Decoration.Bold))
                    .PageSize(10)
                    .MoreChoicesText("[dim]Flèches pour faire défiler[/]")
                    .UseConverter(o => $"#{o.Id:D4} — {Markup.Escape(o.NomOption)}  [dim]({o.PrixMensuel:F2} EUR)[/]")
                    .AddChoices(options));

            var confirmer = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold red]Supprimer l'option {Markup.Escape(selection.NomOption)} ?[/]")
                    .HighlightStyle(new Style(Color.Red, decoration: Decoration.Bold))
                    .AddChoices("[[ Oui, supprimer ]]", "[[ Annuler ]]"));

            if (confirmer == "[[ Annuler ]]")
            {
                InterfaceConsole.AfficherAlerte("Suppression annulée.");
                return;
            }

            InterfaceConsole.AfficherSeparateur("résultat");

            bool succes = _serviceOption.SupprimerOptionService(selection.Id);
            if (succes)
                InterfaceConsole.AfficherSucces($"Option supprimée — {selection.NomOption}.", $"#{selection.Id:D4}");
            else
                InterfaceConsole.AfficherAlerte("Aucune suppression effectuée.");
        }
        catch (Exception ex)
        {
            InterfaceConsole.AfficherErreur(ex.Message);
        }
    }
}
