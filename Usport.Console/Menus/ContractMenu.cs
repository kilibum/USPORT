using Usport.Console.DTOs;
using Usport.Console.Services;
using Usport.Console.IU;
using Usport.Domaine.Enumerations;
using Usport.Domaine.Tarification;
using Spectre.Console;

namespace Usport.Console.Menus;

/// <summary>
/// Menus interactifs liés à la gestion des contrats (création, liste, modification,
/// annulation, suppression). Chaque opération propose une liste nominative.
/// </summary>
public class MenuContrat
{
    private readonly ServiceContrat _serviceContrat;

    public MenuContrat(ServiceContrat serviceContrat)
    {
        _serviceContrat = serviceContrat ?? throw new ArgumentNullException(nameof(serviceContrat));
    }

    // Wizard en 4 étapes : Membre > Plan > Club > Stratégie, puis confirmation.
    public void AfficherMenuCreerContrat()
    {
        AnsiConsole.Clear();
        InterfaceConsole.AfficherTitre("CRÉER UN CONTRAT");

        try
        {
            AfficherBarreEtapes(1);
            var membres = _serviceContrat.ObtenirMembresSelectionnables();
            if (membres.Count == 0)
            {
                InterfaceConsole.AfficherAlerte("Aucun membre actif disponible — créez d'abord un membre.");
                return;
            }

            var membre = AnsiConsole.Prompt(
                new SelectionPrompt<OptionMembre>()
                    .Title(" [bold]Sélectionnez le membre :[/]")
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .PageSize(10)
                    .MoreChoicesText("[dim]Flèches pour faire défiler[/]")
                    .UseConverter(m => $"#{m.Id:D4} — {Markup.Escape(m.NomComplet)}{(m.Ville is null ? "" : $"  [dim]({Markup.Escape(m.Ville)})[/]")}")
                    .AddChoices(membres));

            AfficherBarreEtapes(2);
            var plan = ChoisirPlan(" [bold]Sélectionnez l'abonnement :[/]");
            if (plan is null) return;

            AfficherBarreEtapes(3);
            var clubs = _serviceContrat.ObtenirClubsSelectionnables();
            if (clubs.Count == 0)
            {
                InterfaceConsole.AfficherAlerte("Aucun club disponible.");
                return;
            }

            var club = AnsiConsole.Prompt(
                new SelectionPrompt<OptionClub>()
                    .Title(" [bold]Sélectionnez le club :[/]")
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .PageSize(10)
                    .UseConverter(c => $"{Markup.Escape(c.Nom)}  [dim]({Markup.Escape(c.Ville)}) — {StatutClubLabel(c.Statut)}[/]")
                    .AddChoices(clubs));

            AfficherBarreEtapes(4);
            var strategie = ChoisirStrategie();

            AfficherApercuPrix(plan, strategie, membre.NomComplet, club.Nom);

            if (!Confirmer("[bold]Confirmer la création ?[/]"))
            {
                InterfaceConsole.AfficherAlerte("Création annulée par l'utilisateur.");
                return;
            }

            var contratDto = _serviceContrat.CreerContrat(membre.Id, plan.Id, club.Id, strategie);
            InterfaceConsole.AfficherSucces(
                $"Contrat créé pour {membre.NomComplet} — {plan.NomPlan}.",
                $"C-{contratDto.Id:D4}");
        }
        catch (InvalidOperationException ex)
        {
            InterfaceConsole.AfficherAlerte("validation échouée", new List<string> { ex.Message });
        }
        catch (Exception ex)
        {
            InterfaceConsole.AfficherErreur(ex.Message);
        }
    }

    // Tableau de tous les contrats avec leur statut (actif, annulé, expiré...).
    public void AfficherMenuListeContrats()
    {
        AnsiConsole.Clear();
        InterfaceConsole.AfficherTitre("LISTE DES CONTRATS");

        try
        {
            var contrats = _serviceContrat.ObtenirTousLesContrats();
            if (contrats.Count == 0)
            {
                AnsiConsole.Write(new Panel(new Markup("[dim][[ État vide ]] — Aucun contrat enregistré.[/]"))
                    .Border(BoxBorder.Rounded).BorderStyle(new Style(Color.Grey)).Padding(1, 0));
                InterfaceConsole.RetourMenu();
                return;
            }

            var noms = ChargerNoms(contrats);

            var table = new Table()
                .AddColumn(new TableColumn("[bold]Contrat[/]"))
                .AddColumn(new TableColumn("[bold]Membre[/]"))
                .AddColumn(new TableColumn("[bold]Abonnement[/]"))
                .AddColumn(new TableColumn("[bold]Mensuel[/]").Centered())
                .AddColumn(new TableColumn("[bold]Début[/]"))
                .AddColumn(new TableColumn("[bold]Fin[/]"))
                .AddColumn(new TableColumn("[bold]Statut[/]").Centered())
                .Border(TableBorder.Rounded)
                .BorderStyle(new Style(Color.Grey));

            foreach (var c in contrats)
            {
                table.AddRow(
                    $"C-{c.Id:D4}",
                    Markup.Escape(noms[c.Id].Membre),
                    Markup.Escape(noms[c.Id].Plan),
                    c.PrixMensuel > 0 ? $"{c.PrixMensuel:F2} EUR" : "—",
                    c.DateDebut.ToString("yyyy-MM-dd"),
                    c.DateFin?.ToString("yyyy-MM-dd") ?? "—",
                    $"[{CouleurStatut(c.Statut)}]{StatutLabel(c.Statut)}[/]");
            }

            AnsiConsole.Write(table);
            InterfaceConsole.RetourMenu();
        }
        catch (Exception ex)
        {
            InterfaceConsole.AfficherErreur(ex.Message);
            InterfaceConsole.RetourMenu();
        }
    }

    // Sélection d'un contrat actif puis réaffectation de l'abonnement et de la stratégie.
    public void AfficherMenuModifierContrat()
    {
        AnsiConsole.Clear();
        InterfaceConsole.AfficherTitre("MODIFIER UN CONTRAT");

        try
        {
            var contrat = ChoisirContrat(
                _serviceContrat.ObtenirContratsActifs(nombreMax: 20),
                " [bold]Sélectionnez le contrat à modifier :[/]",
                "Aucun contrat actif à modifier.");
            if (contrat is null) return;

            var plan = ChoisirPlan(" [bold]Nouvel abonnement :[/]");
            if (plan is null) return;

            var strategie = ChoisirStrategie();
            AfficherApercuPrix(plan, strategie, null, null);

            if (!Confirmer("[bold]Confirmer la modification ?[/]"))
            {
                InterfaceConsole.AfficherAlerte("Modification annulée.");
                return;
            }

            InterfaceConsole.AfficherSeparateur("résultat");
            decimal nouveauPrix = strategie.CalculerMensuel(plan.PrixDeBase);
            bool succes = _serviceContrat.ChangerPlanContrat(contrat.Id, plan.Id, nouveauPrix);
            if (succes)
                InterfaceConsole.AfficherSucces($"Abonnement changé pour {plan.NomPlan}.", $"C-{contrat.Id:D4}");
            else
                InterfaceConsole.AfficherAlerte("Aucune modification effectuée (contrat inactif ?).");
        }
        catch (InvalidOperationException ex)
        {
            InterfaceConsole.AfficherAlerte("validation échouée", new List<string> { ex.Message });
        }
        catch (Exception ex)
        {
            InterfaceConsole.AfficherErreur(ex.Message);
        }
    }

    // Sélection d'un contrat actif puis passage de son statut à annulé.
    public void AfficherMenuAnnulerContrat()
    {
        AnsiConsole.Clear();
        InterfaceConsole.AfficherTitre("ANNULER UN CONTRAT");

        try
        {
            var contrat = ChoisirContrat(
                _serviceContrat.ObtenirContratsActifs(nombreMax: 20),
                " [bold]Sélectionnez le contrat à annuler :[/]",
                "Aucun contrat actif trouvé.");
            if (contrat is null) return;

            if (!Confirmer("[bold]Confirmer l'annulation ?[/]", "[[ Confirmer l'annulation ]]"))
            {
                InterfaceConsole.AfficherAlerte("Annulation abandonnée.");
                return;
            }

            InterfaceConsole.AfficherSeparateur("résultat");
            bool succes = _serviceContrat.AnnulerContrat(contrat.Id);
            if (succes)
                InterfaceConsole.AfficherSucces("Annulation du contrat effectuée.", $"C-{contrat.Id:D4}");
            else
                InterfaceConsole.AfficherAlerte("Contrat déjà annulé — aucune action effectuée.");
        }
        catch (Exception ex)
        {
            InterfaceConsole.AfficherErreur(ex.Message);
        }
    }

    // Sélection d'un contrat (tous statuts) puis suppression définitive.
    public void AfficherMenuSupprimerContrat()
    {
        AnsiConsole.Clear();
        InterfaceConsole.AfficherTitre("SUPPRIMER UN CONTRAT");

        try
        {
            var contrat = ChoisirContrat(
                _serviceContrat.ObtenirTousLesContrats(),
                " [bold]Sélectionnez le contrat à supprimer :[/]",
                "Aucun contrat à supprimer.",
                rouge: true);
            if (contrat is null) return;

            if (!Confirmer("[bold red]Supprimer définitivement ce contrat ?[/]", "[[ Oui, supprimer ]]", rouge: true))
            {
                InterfaceConsole.AfficherAlerte("Suppression annulée.");
                return;
            }

            InterfaceConsole.AfficherSeparateur("résultat");
            bool succes = _serviceContrat.SupprimerContrat(contrat.Id);
            if (succes)
                InterfaceConsole.AfficherSucces("Contrat supprimé.", $"C-{contrat.Id:D4}");
            else
                InterfaceConsole.AfficherAlerte("Aucune suppression effectuée.");
        }
        catch (Exception ex)
        {
            InterfaceConsole.AfficherErreur(ex.Message);
        }
    }

    // Helpers privés

    // Demande le choix d'un plan d'abonnement ; retourne null si aucun n'existe.
    private OptionPlan? ChoisirPlan(string titre)
    {
        var plans = _serviceContrat.ObtenirPlansSelectionnables();
        if (plans.Count == 0)
        {
            InterfaceConsole.AfficherAlerte("Aucun plan d'abonnement disponible.");
            return null;
        }

        return AnsiConsole.Prompt(
            new SelectionPrompt<OptionPlan>()
                .Title(titre)
                .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                .PageSize(10)
                .UseConverter(p => $"{Markup.Escape(p.NomPlan)}  —  [green]{p.PrixDeBase:F2} EUR[/]/mois  —  engagement {p.MoisEngagement} mois")
                .AddChoices(plans));
    }

    // Demande le choix d'un contrat dans une liste nominative ; null si liste vide.
    private ContratDto? ChoisirContrat(
        List<ContratDto> contrats, string titre, string messageVide, bool rouge = false)
    {
        if (contrats.Count == 0)
        {
            InterfaceConsole.AfficherAlerte(messageVide);
            return null;
        }

        var noms = ChargerNoms(contrats);
        var couleur = rouge ? Color.Red : Color.Blue;

        return AnsiConsole.Prompt(
            new SelectionPrompt<ContratDto>()
                .Title(titre)
                .HighlightStyle(new Style(couleur, decoration: Decoration.Bold))
                .PageSize(10)
                .MoreChoicesText("[dim]Flèches pour faire défiler[/]")
                .UseConverter(c => Markup.Escape(
                    $"C-{c.Id:D4} — {noms[c.Id].Membre} — {noms[c.Id].Plan} ({StatutLabel(c.Statut)})"))
                .AddChoices(contrats));
    }

    private static IStrategieTarification ChoisirStrategie()
    {
        var choix = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(" [bold]Stratégie de tarification :[/]")
                .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                .AddChoices("Standard", "Étudiant (-40%)", "Promotionnel (-30%)"));

        return choix switch
        {
            "Standard"            => new TarificationStandard(),
            "Étudiant (-40%)"     => new TarificationEtudiant(),
            "Promotionnel (-30%)" => new TarificationPromotionnelle(0.30m),
            _                     => new TarificationStandard()
        };
    }

    private static void AfficherApercuPrix(OptionPlan plan, IStrategieTarification strategie, string? membre, string? club)
    {
        decimal mensuel = strategie.CalculerMensuel(plan.PrixDeBase);
        decimal annuel  = strategie.CalculerAnnuel(plan.PrixDeBase);
        decimal economie = (plan.PrixDeBase * 12) - annuel;

        InterfaceConsole.AfficherSeparateur("récapitulatif");
        AnsiConsole.WriteLine();

        var table = new Table().Border(TableBorder.Rounded).BorderStyle(new Style(Color.Grey));
        if (membre is not null) table.AddColumn("[bold]Membre[/]");
        table.AddColumn("[bold]Abonnement[/]");
        if (club is not null) table.AddColumn("[bold]Club[/]");
        table.AddColumn("[bold]Stratégie[/]");
        table.AddColumn("[bold]Mensuel[/]");
        table.AddColumn("[bold]Annuel[/]");
        table.AddColumn("[bold]Économie[/]");

        var cells = new List<string>();
        if (membre is not null) cells.Add(Markup.Escape(membre));
        cells.Add(Markup.Escape(plan.NomPlan));
        if (club is not null) cells.Add(Markup.Escape(club));
        cells.Add(Markup.Escape(strategie.NomStrategie));
        cells.Add($"{mensuel:F2} EUR");
        cells.Add($"{annuel:F2} EUR");
        cells.Add($"{economie:F2} EUR");
        table.AddRow(cells.ToArray());

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private static bool Confirmer(string titre, string ouiLabel = "[[ Confirmer ]]", bool rouge = false)
    {
        var couleur = rouge ? Color.Red : Color.Blue;
        var choix = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(titre)
                .HighlightStyle(new Style(couleur, decoration: Decoration.Bold))
                .AddChoices(ouiLabel, "[[ Annuler ]]"));
        return choix == ouiLabel;
    }

    // Charge, pour chaque contrat, le nom du membre et de l'abonnement.
    private Dictionary<int, (string Membre, string Plan)> ChargerNoms(List<ContratDto> contrats)
    {
        var noms = new Dictionary<int, (string, string)>();
        foreach (var c in contrats)
        {
            var membre = c.IdMembre == 0
                ? "(membre supprimé)"
                : _serviceContrat.ObtenirNomCompletMembre(c.IdMembre) ?? "(membre inconnu)";
            var plan   = _serviceContrat.ObtenirInfosPlan(c.IdPlan)?.NomPlan ?? "(formule inconnue)";
            noms[c.Id] = (membre, plan);
        }
        return noms;
    }

    private static string StatutLabel(StatutContrat s) => s switch
    {
        StatutContrat.Actif    => "ACTIF",
        StatutContrat.Suspendu => "SUSPENDU",
        StatutContrat.Expire   => "EXPIRÉ",
        StatutContrat.Annule   => "ANNULÉ",
        _                      => s.ToString().ToUpper()
    };

    private static string CouleurStatut(StatutContrat s) => s switch
    {
        StatutContrat.Actif    => "green",
        StatutContrat.Suspendu => "grey",
        StatutContrat.Expire   => "yellow",
        StatutContrat.Annule   => "red",
        _                      => "white"
    };

    // Fil d'Ariane du wizard ; l'étape courante est encadrée et en surbrillance.
    private static void AfficherBarreEtapes(int etapeActive)
    {
        var etapes = new[] { "Membre", "Plan", "Club", "Stratégie" };
        var parts = etapes.Select((e, i) =>
        {
            var n = i + 1;
            return n == etapeActive
                ? $"[bold blue][[ {n}. {e} ]][/]"
                : $"[dim]{n}. {e}[/]";
        });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("  " + string.Join("   [dim]>[/]   ", parts));
        AnsiConsole.WriteLine();
    }

    private static string StatutClubLabel(StatutOperationnelClub s) => s switch
    {
        StatutOperationnelClub.Ouvert              => "Ouvert",
        StatutOperationnelClub.FermeTemporairement => "Fermé temporairement",
        StatutOperationnelClub.FermeDefinitivement => "Fermé définitivement",
        _                                          => s.ToString()
    };
}