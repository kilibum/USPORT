using Usport.Console.Services;
using Usport.Console.IU;
using Usport.Domaine.Enumerations;
using Spectre.Console;

namespace Usport.Console.Menus;

/// <summary>
/// Menus interactifs liés à la gestion des clubs (liste, fermeture, réouverture).
/// </summary>
public class MenuClub
{
    private readonly ServiceClub _serviceClub;

    public MenuClub(ServiceClub serviceClub)
    {
        _serviceClub = serviceClub ?? throw new ArgumentNullException(nameof(serviceClub));
    }

    public void AfficherMenuListeClubs()
    {
        AnsiConsole.Clear();
        InterfaceConsole.AfficherTitre("LISTE DES CLUBS");

        var clubs = _serviceClub.ObtenirTousLesClubs();
        if (clubs.Count == 0)
        {
            AnsiConsole.Write(new Panel(new Markup("[dim][[ État vide ]] — Aucun club enregistré.[/]"))
                .Border(BoxBorder.Rounded).BorderStyle(new Style(Color.Grey)).Padding(1, 0));
            InterfaceConsole.RetourMenu();
            return;
        }

        var table = new Table()
            .AddColumn(new TableColumn("[bold]ID[/]").Centered())
            .AddColumn(new TableColumn("[bold]Nom[/]"))
            .AddColumn(new TableColumn("[bold]Adresse[/]"))
            .AddColumn(new TableColumn("[bold]Ville[/]"))
            .AddColumn(new TableColumn("[bold]Code postal[/]").Centered())
            .AddColumn(new TableColumn("[bold]Ouvert le[/]"))
            .AddColumn(new TableColumn("[bold]24h/24[/]").Centered())
            .AddColumn(new TableColumn("[bold]Statut[/]").Centered())
            .Border(TableBorder.Rounded)
            .BorderStyle(new Style(Color.Grey));

        foreach (var club in clubs)
        {
            var couleur = club.StatutOperationnel switch
            {
                StatutOperationnelClub.Ouvert              => "green",
                StatutOperationnelClub.FermeTemporairement => "yellow",
                StatutOperationnelClub.FermeDefinitivement => "red",
                _                                          => "white"
            };

            var statutLabel = club.StatutOperationnel switch
            {
                StatutOperationnelClub.Ouvert              => "OUVERT",
                StatutOperationnelClub.FermeTemporairement => "FERMÉ TEMP.",
                StatutOperationnelClub.FermeDefinitivement => "FERMÉ DÉF.",
                _                                          => club.StatutOperationnel.ToString()
            };

            table.AddRow(
                club.Id.ToString("D3"),
                Markup.Escape(club.Nom),
                Markup.Escape(club.AdresseRue),
                Markup.Escape(club.AdresseVille),
                Markup.Escape(club.AdresseCodePostal),
                club.DateOuverture.ToString("yyyy-MM-dd"),
                club.EstOuvert247 ? "[green]Oui[/]" : "Non",
                $"[{couleur}]{statutLabel}[/]");
        }

        AnsiConsole.Write(table);
        InterfaceConsole.RetourMenu();
    }

    public void AfficherMenuFermerClub()
    {
        AnsiConsole.Clear();
        InterfaceConsole.AfficherTitre("FERMER UN CLUB");

        var clubsOuverts = _serviceClub.ObtenirClubsOuverts();
        if (clubsOuverts.Count == 0)
        {
            InterfaceConsole.AfficherAlerte("Aucun club ouvert à fermer.");
            InterfaceConsole.RetourMenu();
            return;
        }

        var club = AnsiConsole.Prompt(
            new SelectionPrompt<Domaine.Entites.GestionClub.Club>()
                .Title(" [bold]Sélectionnez le club à fermer :[/]")
                .HighlightStyle(new Style(Color.Yellow, decoration: Decoration.Bold))
                .PageSize(10)
                .UseConverter(c => $"{Markup.Escape(c.Nom)}  [dim]({Markup.Escape(c.AdresseVille)})[/]")
                .AddChoices(clubsOuverts));

        _serviceClub.FermerClub(club.Id);
        InterfaceConsole.AfficherSucces($"Club {club.Nom} fermé temporairement.", $"#{club.Id:D4}");
        InterfaceConsole.RetourMenu();
    }

    public void AfficherMenuRouvrirClub()
    {
        AnsiConsole.Clear();
        InterfaceConsole.AfficherTitre("ROUVRIR UN CLUB");

        var clubsFermes = _serviceClub.ObtenirClubsFermes();
        if (clubsFermes.Count == 0)
        {
            InterfaceConsole.AfficherAlerte("Aucun club fermé à rouvrir.");
            InterfaceConsole.RetourMenu();
            return;
        }

        var club = AnsiConsole.Prompt(
            new SelectionPrompt<Domaine.Entites.GestionClub.Club>()
                .Title(" [bold]Sélectionnez le club à rouvrir :[/]")
                .HighlightStyle(new Style(Color.Green, decoration: Decoration.Bold))
                .PageSize(10)
                .UseConverter(c => $"{Markup.Escape(c.Nom)}  [dim]({Markup.Escape(c.AdresseVille)})[/]")
                .AddChoices(clubsFermes));

        _serviceClub.RouvrirClub(club.Id);
        InterfaceConsole.AfficherSucces($"Club {club.Nom} réouvert avec succès.", $"#{club.Id:D4}");
        InterfaceConsole.RetourMenu();
    }
}
