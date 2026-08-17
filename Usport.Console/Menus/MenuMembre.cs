using Usport.Console.DTOs;
using Usport.Console.Services;
using Usport.Console.IU;
using Usport.Domaine.Enumerations;
using Spectre.Console;

namespace Usport.Console.Menus;

/// <summary>
/// Menus interactifs liés à la gestion des membres (ajout, liste paginée).
/// </summary>
public class MenuMembre
{
    private readonly ServiceMembre _serviceMembre;

    public MenuMembre(ServiceMembre serviceMembre)
    {
        _serviceMembre = serviceMembre ?? throw new ArgumentNullException(nameof(serviceMembre));
    }

    // Saisie séquentielle des informations puis création via le service.
    public void AfficherMenuAjouterMembre()
    {
        AnsiConsole.Clear();
        InterfaceConsole.AfficherTitre("AJOUTER UN MEMBRE");
        InterfaceConsole.AfficherSeparateur("saisie séquentielle");
        AnsiConsole.WriteLine();

        var prenom = AnsiConsole.Ask<string>("     [bold]Prénom :[/] ");
        if (string.IsNullOrWhiteSpace(prenom))
        {
            InterfaceConsole.AfficherAlerte("Le prénom est obligatoire.");
            return;
        }

        var nom = AnsiConsole.Ask<string>("       [bold]Nom :[/] ");
        if (string.IsNullOrWhiteSpace(nom))
        {
            InterfaceConsole.AfficherAlerte("Le nom est obligatoire.");
            return;
        }

        var email = AnsiConsole.Ask<string>("     [bold]Email :[/] ");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            InterfaceConsole.AfficherAlerte("Email invalide.");
            return;
        }

        var dobStr = AnsiConsole.Ask<string>("[bold]Date de naissance :[/] [dim](yyyy-MM-dd)[/] ");
        if (!DateOnly.TryParseExact(dobStr, "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var dateNaissance))
        {
            InterfaceConsole.AfficherAlerte("Format invalide — saisir la date au format yyyy-MM-dd (ex : 2003-07-14).");
            return;
        }

        var aujourdHui = DateOnly.FromDateTime(DateTime.Today);
        if (dateNaissance >= aujourdHui)
        {
            InterfaceConsole.AfficherAlerte("La date de naissance doit être dans le passé.");
            return;
        }

        var age = aujourdHui.Year - dateNaissance.Year
                  - (aujourdHui < dateNaissance.AddYears(aujourdHui.Year - dateNaissance.Year) ? 1 : 0);
        if (age < 16)
        {
            InterfaceConsole.AfficherAlerte($"Âge calculé ({age} ans) trop faible — minimum 16 ans requis.");
            return;
        }
        if (age > 120)
        {
            InterfaceConsole.AfficherAlerte($"Âge calculé ({age} ans) non plausible — vérifiez la date saisie.");
            return;
        }

        AnsiConsole.Markup("       [bold]Ville :[/] ");
        AnsiConsole.Markup("[dim](facultatif — laisser vide pour passer)[/] ");
        var ville = System.Console.ReadLine()?.Trim();

        AnsiConsole.Markup("  [bold]Téléphone :[/] ");
        AnsiConsole.Markup("[dim](facultatif — laisser vide pour passer)[/] ");
        var telephone = System.Console.ReadLine()?.Trim();

        var genre = AnsiConsole.Prompt(
            new SelectionPrompt<Genre>()
                .Title(" [bold]Genre :[/]")
                .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                .UseConverter(GenreLabel)
                .AddChoices(Enum.GetValues<Genre>()));

        var objectifChoix = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(" [bold]Objectif principal :[/]")
                .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                .AddChoices("Perte de poids", "Prise de masse", "Remise en forme", "Autre", "Aucun (passer)"));

        ObjectifPrincipal? objectif = objectifChoix switch
        {
            "Perte de poids"  => Usport.Domaine.Enumerations.ObjectifPrincipal.PerteDePoids,
            "Prise de masse"  => Usport.Domaine.Enumerations.ObjectifPrincipal.PriseDeMasse,
            "Remise en forme" => Usport.Domaine.Enumerations.ObjectifPrincipal.RemiseEnForme,
            "Autre"           => Usport.Domaine.Enumerations.ObjectifPrincipal.Autre,
            _                 => null
        };

        var acquisitionChoix = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(" [bold]Comment nous avez-vous connu ?[/]")
                .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                .AddChoices("Publicité web", "Réseaux sociaux", "Recommandation", "Autre", "Non renseigné (passer)"));

        SourceAcquisition? source = acquisitionChoix switch
        {
            "Publicité web"    => SourceAcquisition.PubliciteWeb,
            "Réseaux sociaux"  => SourceAcquisition.ReseauxSociaux,
            "Recommandation"   => SourceAcquisition.Recommandation,
            "Autre"            => SourceAcquisition.Autre,
            _                  => null
        };

        AnsiConsole.WriteLine();
        InterfaceConsole.AfficherSeparateur("résultat");

        try
        {
            var membreDto = _serviceMembre.CreerMembre(
                prenom,
                nom,
                email,
                dateNaissance,
                string.IsNullOrWhiteSpace(telephone) ? null : telephone,
                string.IsNullOrWhiteSpace(ville) ? null : ville,
                genre,
                objectif,
                source
            );

            InterfaceConsole.AfficherSucces(
                $"Membre créé — ID #{membreDto.Id:D4} — {membreDto.NomComplet}",
                $"#{membreDto.Id:D4}");
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

    // Liste paginée des membres avec navigation.
    public void AfficherMenuListeMembres()
    {
        int taillePage = 20;
        int numeroPage = 1;
        bool enConsultation = true;

        while (enConsultation)
        {
            AnsiConsole.Clear();
            InterfaceConsole.AfficherTitre("LISTE DES MEMBRES");

            try
            {
                var membres = _serviceMembre.ObtenirTousLesMembres(numeroPage, taillePage);

                if (!membres.Any())
                {
                    var videPanel = new Panel(
                        new Markup("[dim][[ État vide ]] — Aucun membre trouvé.[/]"))
                        .Border(BoxBorder.Rounded)
                        .BorderStyle(new Style(Color.Grey))
                        .Padding(1, 0);

                    AnsiConsole.Write(videPanel);
                    InterfaceConsole.RetourMenu();
                    break;
                }

                var table = new Table()
                    .AddColumn(new TableColumn("[bold]ID[/]").Centered())
                    .AddColumn(new TableColumn("[bold]Nom[/]"))
                    .AddColumn(new TableColumn("[bold]Email[/]"))
                    .AddColumn(new TableColumn("[bold]Âge[/]").Centered())
                    .AddColumn(new TableColumn("[bold]Genre[/]").Centered())
                    .AddColumn(new TableColumn("[bold]Objectif[/]"))
                    .AddColumn(new TableColumn("[bold]Ville[/]"))
                    .AddColumn(new TableColumn("[bold]Statut[/]").Centered())
                    .AddColumn(new TableColumn("[bold]Visites[/]").Centered())
                    .Border(TableBorder.Rounded)
                    .BorderStyle(new Style(Color.Grey));

                foreach (var membre in membres)
                {
                    var statutColor = membre.Statut == StatutMembre.Actif ? "green" : "yellow";

                    table.AddRow(
                        membre.Id.ToString("D3"),
                        Markup.Escape(membre.NomComplet),
                        Markup.Escape(membre.Email),
                        membre.Age.ToString(),
                        Markup.Escape(GenreLabel(membre.Genre)),
                        Markup.Escape(ObjectifLabel(membre.ObjectifPrincipal)),
                        Markup.Escape(membre.AdresseVille ?? "---"),
                        $"[{statutColor}]{membre.Statut.ToString().ToUpper()}[/]",
                        membre.NombreVisitesTotal.ToString()
                    );
                }

                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();

                var choix = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[dim]Page {numeroPage} — {membres.Count} membres affichés ({taillePage} membres/page)[/]")
                        .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                        .AddChoices(
                            "Page précédente",
                            "Page suivante",
                            "Retour au menu"
                        )
                );

                switch (choix)
                {
                    case "Page précédente":
                        if (numeroPage > 1) numeroPage--;
                        break;
                    case "Page suivante":
                        if (membres.Count == taillePage) numeroPage++;
                        break;
                    case "Retour au menu":
                        enConsultation = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                InterfaceConsole.AfficherErreur(ex.Message);
                InterfaceConsole.RetourMenu();
                enConsultation = false;
            }
        }
    }

    // Sélection du membre dans une liste, puis mise à jour de ses coordonnées.
    public void AfficherMenuModifierMembre()
    {
        AnsiConsole.Clear();
        InterfaceConsole.AfficherTitre("MODIFIER UN MEMBRE");

        try
        {
            var membres = _serviceMembre.ObtenirMembresSelectionnables();
            if (membres.Count == 0)
            {
                InterfaceConsole.AfficherAlerte("Aucun membre actif à modifier.");
                return;
            }

            var membre = AnsiConsole.Prompt(
                new SelectionPrompt<OptionMembre>()
                    .Title(" [bold]Sélectionnez le membre à modifier :[/]")
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .PageSize(10)
                    .MoreChoicesText("[dim]Flèches pour faire défiler[/]")
                    .UseConverter(m => $"#{m.Id:D4} — {Markup.Escape(m.NomComplet)}{(m.Ville is null ? "" : $"  [dim]({Markup.Escape(m.Ville)})[/]")}")
                    .AddChoices(membres));

            var contact = _serviceMembre.ObtenirContactMembre(membre.Id);
            var villeActuelle = contact?.Ville ?? string.Empty;
            var telActuel     = contact?.Telephone ?? string.Empty;

            AnsiConsole.WriteLine();
            InterfaceConsole.AfficherSeparateur("valeurs actuelles — ENTRÉE pour conserver");
            AnsiConsole.WriteLine();

            var ville = AnsiConsole.Prompt(
                new TextPrompt<string>("     [bold]Ville :[/] ")
                    .DefaultValue(villeActuelle)
                    .AllowEmpty());

            var tel = AnsiConsole.Prompt(
                new TextPrompt<string>(" [bold]Téléphone :[/] ")
                    .DefaultValue(telActuel)
                    .AllowEmpty());

            AnsiConsole.WriteLine();
            InterfaceConsole.AfficherSeparateur("résultat");

            bool succes = _serviceMembre.MettreAJourContactMembre(
                membre.Id,
                string.IsNullOrWhiteSpace(tel) ? null : tel.Trim(),
                string.IsNullOrWhiteSpace(ville) ? null : ville.Trim());

            if (succes)
                InterfaceConsole.AfficherSucces($"Coordonnées mises à jour — {membre.NomComplet}.", $"#{membre.Id:D4}");
            else
                InterfaceConsole.AfficherAlerte("Aucune modification effectuée.");
        }
        catch (Exception ex)
        {
            InterfaceConsole.AfficherErreur(ex.Message);
        }
    }

    // Sélection du membre puis suppression de lui et de ses contrats, après confirmation.
    public void AfficherMenuSupprimerMembre()
    {
        AnsiConsole.Clear();
        InterfaceConsole.AfficherTitre("SUPPRIMER UN MEMBRE");

        try
        {
            var membres = _serviceMembre.ObtenirMembresSelectionnables();
            if (membres.Count == 0)
            {
                InterfaceConsole.AfficherAlerte("Aucun membre à supprimer.");
                return;
            }

            var membre = AnsiConsole.Prompt(
                new SelectionPrompt<OptionMembre>()
                    .Title(" [bold]Sélectionnez le membre à supprimer :[/]")
                    .HighlightStyle(new Style(Color.Red, decoration: Decoration.Bold))
                    .PageSize(10)
                    .MoreChoicesText("[dim]Flèches pour faire défiler[/]")
                    .UseConverter(m => $"#{m.Id:D4} — {Markup.Escape(m.NomComplet)}{(m.Ville is null ? "" : $"  [dim]({Markup.Escape(m.Ville)})[/]")}")
                    .AddChoices(membres));

            var confirmer = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold red]Supprimer {Markup.Escape(membre.NomComplet)} et tous ses contrats ?[/]")
                    .HighlightStyle(new Style(Color.Red, decoration: Decoration.Bold))
                    .AddChoices("[[ Oui, supprimer ]]", "[[ Annuler ]]"));

            if (confirmer == "[[ Annuler ]]")
            {
                InterfaceConsole.AfficherAlerte("Suppression annulée.");
                return;
            }

            InterfaceConsole.AfficherSeparateur("résultat");

            bool succes = _serviceMembre.SupprimerMembre(membre.Id);
            if (succes)
                InterfaceConsole.AfficherSucces($"Membre et contrats associés supprimés — {membre.NomComplet}.", $"#{membre.Id:D4}");
            else
                InterfaceConsole.AfficherAlerte("Aucune suppression effectuée.");
        }
        catch (Exception ex)
        {
            InterfaceConsole.AfficherErreur(ex.Message);
        }
    }

    // Helpers privés

    private static string GenreLabel(Genre g) => g switch
    {
        Genre.Homme => "Homme",
        Genre.Femme => "Femme",
        Genre.Autre => "Autre",
        _           => g.ToString()
    };

    private static string ObjectifLabel(ObjectifPrincipal? g) => g switch
    {
        Usport.Domaine.Enumerations.ObjectifPrincipal.PerteDePoids  => "Perte de poids",
        Usport.Domaine.Enumerations.ObjectifPrincipal.PriseDeMasse  => "Prise de masse",
        Usport.Domaine.Enumerations.ObjectifPrincipal.RemiseEnForme => "Remise en forme",
        Usport.Domaine.Enumerations.ObjectifPrincipal.Autre         => "Autre",
        null                                                        => "---",
        _                                                           => g.ToString()!
    };
}