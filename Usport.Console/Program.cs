using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Usport.Console.Data;
using Usport.Console.Services;
using Usport.Console.Menus;
using Usport.Console.IU;
using Spectre.Console;

namespace Usport.Console;

/// <summary>
/// Point d'entrée de l'application console Usport.
/// </summary>
internal class Program
{
    private const string RetourPrincipal = "Retour au menu principal";
    private const string NavHint = "[dim]Flèches pour naviguer, ENTRÉE pour valider[/]";

    static void Main(string[] args)
    {
        // Force l'UTF-8 pour un affichage correct des bordures.
        try { System.Console.OutputEncoding = Encoding.UTF8; } catch { /* sortie redirigée */ }

        AfficherSplashScreen();

        // Initialisation du conteneur de services avec le StockDonnees en mémoire.
        var services = new ServiceCollection();

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("blue"))
            .Start("Chargement...", ctx =>
            {
                ctx.Status("Initialisation services...");
                ConfigureServices(services);
            });

        var serviceProvider = services.BuildServiceProvider();

        // Chargement des données de démarrage (clubs, plans, options de service).
        var stockDonnees = serviceProvider.GetRequiredService<StockDonnees>();
        stockDonnees.Amorcer();

        AnsiConsole.WriteLine();
        InterfaceConsole.AfficherSeparateur();
        InterfaceConsole.AttendreEntree();

        RunApplication(serviceProvider);
    }

    private static void AfficherSplashScreen()
    {
        SafeClearConsole();

        var titre = new Panel(
            new FigletText("U S P O R T")
                .Centered()
                .Color(Color.Blue))
            .Border(BoxBorder.Double)
            .Padding(1, 0)
            .Expand();

        AnsiConsole.Write(titre);
        AnsiConsole.WriteLine();

        var sousTitre = new Panel(
            new Markup("[bold]Système de gestion de clubs de fitness[/]"))
            .Border(BoxBorder.Rounded)
            .Padding(1, 0)
            .Expand();

        AnsiConsole.Write(sousTitre);

        var mention = new Panel(
            new Markup("[dim]Application Console — TFE[/]"))
            .Border(BoxBorder.Rounded)
            .Padding(1, 0)
            .Expand();

        AnsiConsole.Write(mention);
        AnsiConsole.WriteLine();
    }

    private static void SafeClearConsole()
    {
        try
        {
            AnsiConsole.Clear();
        }
        catch (IOException)
        {
            // Certains hôtes de débogage n'exposent pas de buffer console valide.
        }
        catch (InvalidOperationException)
        {
            // Ignore les contextes sans console interactive.
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // StockDonnees partagé entre tous les services via l'injection de dépendances.
        services.AddSingleton<StockDonnees>();

        services.AddSingleton<ServiceMembre>();
        services.AddSingleton<ServiceContrat>();
        services.AddSingleton<ServiceEmployee>();
        services.AddSingleton<ServiceOptionService>();
        services.AddSingleton<ServiceClub>();

        services.AddSingleton<MenuMembre>();
        services.AddSingleton<MenuContrat>();
        services.AddSingleton<MenuEmployee>();
        services.AddSingleton<MenuOptionService>();
        services.AddSingleton<MenuClub>();
    }

    private static void RunApplication(ServiceProvider serviceProvider)
    {
        var menuMembre        = serviceProvider.GetRequiredService<MenuMembre>();
        var menuContrat       = serviceProvider.GetRequiredService<MenuContrat>();
        var menuEmployee      = serviceProvider.GetRequiredService<MenuEmployee>();
        var menuOptionService = serviceProvider.GetRequiredService<MenuOptionService>();
        var menuClub          = serviceProvider.GetRequiredService<MenuClub>();

        bool enCours = true;
        while (enCours)
        {
            SafeClearConsole();

            var menuTitre = new Panel(
                new Markup("[bold]MENU PRINCIPAL[/]"))
                .Border(BoxBorder.Double)
                .Padding(1, 0)
                .Expand();

            AnsiConsole.Write(menuTitre);
            AnsiConsole.WriteLine();

            var choix = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(NavHint)
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .PageSize(10)
                    .AddChoices(
                        "1. Gestion des membres",
                        "2. Gestion des contrats",
                        "3. Gestion des employés",
                        "4. Gestion des clubs",
                        "5. Options de service",
                        "6. Quitter"
                    )
            );

            try
            {
                switch (choix)
                {
                    case "1. Gestion des membres":
                        AfficherSousMenuMembres(menuMembre);
                        break;

                    case "2. Gestion des contrats":
                        AfficherSousMenuContrats(menuContrat);
                        break;

                    case "3. Gestion des employés":
                        AfficherSousMenuEmployees(menuEmployee);
                        break;

                    case "4. Gestion des clubs":
                        AfficherSousMenuClubs(menuClub);
                        break;

                    case "5. Options de service":
                        AfficherSousMenuOptionsService(menuOptionService);
                        break;

                    case "6. Quitter":
                        enCours = false;
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[bold blue]Au revoir ![/]");
                        break;
                }
            }
            catch (Exception ex)
            {
                InterfaceConsole.AfficherErreur(ex.Message);
                InterfaceConsole.RetourMenu();
            }
        }
    }

    private static void AfficherSousMenuMembres(MenuMembre menuMembre)
    {
        bool actif = true;
        while (actif)
        {
            SafeClearConsole();
            InterfaceConsole.AfficherTitre("GESTION DES MEMBRES");

            var choix = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(NavHint)
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .AddChoices(
                        "1. Ajouter un membre",
                        "2. Lister les membres",
                        "3. Modifier un membre",
                        "4. Supprimer un membre",
                        RetourPrincipal
                    )
            );

            switch (choix)
            {
                case "1. Ajouter un membre":
                    menuMembre.AfficherMenuAjouterMembre();
                    InterfaceConsole.RetourMenu();
                    break;

                case "2. Lister les membres":
                    menuMembre.AfficherMenuListeMembres();
                    break;

                case "3. Modifier un membre":
                    menuMembre.AfficherMenuModifierMembre();
                    InterfaceConsole.RetourMenu();
                    break;

                case "4. Supprimer un membre":
                    menuMembre.AfficherMenuSupprimerMembre();
                    InterfaceConsole.RetourMenu();
                    break;

                case RetourPrincipal:
                    actif = false;
                    break;
            }
        }
    }

    private static void AfficherSousMenuContrats(MenuContrat menuContrat)
    {
        bool actif = true;
        while (actif)
        {
            SafeClearConsole();
            InterfaceConsole.AfficherTitre("GESTION DES CONTRATS");

            var choix = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(NavHint)
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .AddChoices(
                        "1. Créer un contrat",
                        "2. Lister les contrats",
                        "3. Modifier un contrat",
                        "4. Annuler un contrat",
                        "5. Supprimer un contrat",
                        RetourPrincipal
                    )
            );

            switch (choix)
            {
                case "1. Créer un contrat":
                    menuContrat.AfficherMenuCreerContrat();
                    InterfaceConsole.RetourMenu();
                    break;

                case "2. Lister les contrats":
                    menuContrat.AfficherMenuListeContrats();
                    break;

                case "3. Modifier un contrat":
                    menuContrat.AfficherMenuModifierContrat();
                    InterfaceConsole.RetourMenu();
                    break;

                case "4. Annuler un contrat":
                    menuContrat.AfficherMenuAnnulerContrat();
                    InterfaceConsole.RetourMenu();
                    break;

                case "5. Supprimer un contrat":
                    menuContrat.AfficherMenuSupprimerContrat();
                    InterfaceConsole.RetourMenu();
                    break;

                case RetourPrincipal:
                    actif = false;
                    break;
            }
        }
    }

    private static void AfficherSousMenuEmployees(MenuEmployee menuEmployee)
    {
        bool actif = true;
        while (actif)
        {
            SafeClearConsole();
            InterfaceConsole.AfficherTitre("GESTION DES EMPLOYÉS");

            var choix = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(NavHint)
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .AddChoices(
                        "1. Ajouter un employé",
                        "2. Lister les employés",
                        "3. Modifier un employé",
                        "4. Supprimer un employé",
                        RetourPrincipal
                    )
            );

            switch (choix)
            {
                case "1. Ajouter un employé":
                    menuEmployee.AfficherMenuAjouterEmployee();
                    InterfaceConsole.RetourMenu();
                    break;

                case "2. Lister les employés":
                    menuEmployee.AfficherMenuListeEmployees();
                    break;

                case "3. Modifier un employé":
                    menuEmployee.AfficherMenuModifierEmployee();
                    InterfaceConsole.RetourMenu();
                    break;

                case "4. Supprimer un employé":
                    menuEmployee.AfficherMenuSupprimerEmployee();
                    InterfaceConsole.RetourMenu();
                    break;

                case RetourPrincipal:
                    actif = false;
                    break;
            }
        }
    }

    private static void AfficherSousMenuClubs(MenuClub menuClub)
    {
        bool actif = true;
        while (actif)
        {
            SafeClearConsole();
            InterfaceConsole.AfficherTitre("GESTION DES CLUBS");

            var choix = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(NavHint)
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .AddChoices(
                        "1. Lister les clubs",
                        "2. Fermer un club",
                        "3. Rouvrir un club",
                        RetourPrincipal
                    )
            );

            switch (choix)
            {
                case "1. Lister les clubs":
                    menuClub.AfficherMenuListeClubs();
                    break;

                case "2. Fermer un club":
                    menuClub.AfficherMenuFermerClub();
                    break;

                case "3. Rouvrir un club":
                    menuClub.AfficherMenuRouvrirClub();
                    break;

                case RetourPrincipal:
                    actif = false;
                    break;
            }
        }
    }

    // Sous-menu de gestion des options de service
    private static void AfficherSousMenuOptionsService(MenuOptionService menuOptionService)
    {
        bool actif = true;
        while (actif)
        {
            SafeClearConsole();
            InterfaceConsole.AfficherTitre("GESTION DES OPTIONS DE SERVICE");

            var choix = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(NavHint)
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .AddChoices(
                        "1. Ajouter une option",
                        "2. Lister les options",
                        "3. Modifier une option",
                        "4. Supprimer une option",
                        RetourPrincipal
                    )
            );

            switch (choix)
            {
                case "1. Ajouter une option":
                    menuOptionService.AfficherMenuAjouterOption();
                    InterfaceConsole.RetourMenu();
                    break;

                case "2. Lister les options":
                    menuOptionService.AfficherMenuListeOptions();
                    InterfaceConsole.RetourMenu();
                    break;

                case "3. Modifier une option":
                    menuOptionService.AfficherMenuModifierOption();
                    InterfaceConsole.RetourMenu();
                    break;

                case "4. Supprimer une option":
                    menuOptionService.AfficherMenuSupprimerOption();
                    InterfaceConsole.RetourMenu();
                    break;

                case RetourPrincipal:
                    actif = false;
                    break;
            }
        }
    }
}