using Usport.Console.DTOs;
using Usport.Console.Services;
using Usport.Console.IU;
using Usport.Domaine.Enumerations;
using Spectre.Console;

namespace Usport.Console.Menus;

/// <summary>
/// Menus interactifs liés à la gestion des employés (ajout, liste).
/// Le club et le rôle sont choisis dans des listes pour éviter la saisie d'un code.
/// </summary>
public class MenuEmployee
{
    private readonly ServiceEmployee _serviceEmployee;

    public MenuEmployee(ServiceEmployee serviceEmployee)
    {
        _serviceEmployee = serviceEmployee ?? throw new ArgumentNullException(nameof(serviceEmployee));
    }

    public void AfficherMenuAjouterEmployee()
    {
        AnsiConsole.Clear();
        InterfaceConsole.AfficherTitre("AJOUTER UN EMPLOYÉ");
        InterfaceConsole.AfficherSeparateur("saisie séquentielle");
        AnsiConsole.WriteLine();

        try
        {
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

            var clubs = _serviceEmployee.ObtenirClubsSelectionnables();
            if (clubs.Count == 0)
            {
                InterfaceConsole.AfficherAlerte("Aucun club disponible — créez d'abord un club.");
                return;
            }

            var club = AnsiConsole.Prompt(
                new SelectionPrompt<OptionClub>()
                    .Title(" [bold]Club d'affectation :[/]")
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .PageSize(10)
                    .UseConverter(c => $"{Markup.Escape(c.Nom)}  [dim]({Markup.Escape(c.Ville)}) — {StatutClubLabel(c.Statut)}[/]")
                    .AddChoices(clubs));

            var role = AnsiConsole.Prompt(
                new SelectionPrompt<RoleEmploye>()
                    .Title(" [bold]Rôle :[/]")
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .UseConverter(RoleLabel)
                    .AddChoices(Enum.GetValues<RoleEmploye>()));

            var embaucheStr = AnsiConsole.Ask<string>("[bold]Date d'embauche :[/] [dim](yyyy-MM-dd, vide = aujourd'hui)[/] ", string.Empty);
            DateOnly dateEmbauche;
            if (string.IsNullOrWhiteSpace(embaucheStr))
            {
                dateEmbauche = DateOnly.FromDateTime(DateTime.Today);
            }
            else if (!DateOnly.TryParseExact(embaucheStr, "yyyy-MM-dd",
                         System.Globalization.CultureInfo.InvariantCulture,
                         System.Globalization.DateTimeStyles.None, out dateEmbauche))
            {
                InterfaceConsole.AfficherAlerte("Format de date invalide (attendu : yyyy-MM-dd).");
                return;
            }

            var salaireStr = AnsiConsole.Ask<string>("[bold]Salaire mensuel :[/] [dim](optionnel, ex : 2350.00)[/] ", string.Empty);
            decimal? salaire = null;
            if (!string.IsNullOrWhiteSpace(salaireStr))
            {
                if (!decimal.TryParse(salaireStr, System.Globalization.NumberStyles.Number,
                        System.Globalization.CultureInfo.InvariantCulture, out var s) || s < 0)
                {
                    InterfaceConsole.AfficherAlerte("Salaire invalide.");
                    return;
                }
                salaire = s;
            }

            AnsiConsole.Markup("[bold]Qualifications :[/] [dim](optionnel)[/] ");
            var qualifications = System.Console.ReadLine()?.Trim();

            AnsiConsole.WriteLine();
            InterfaceConsole.AfficherSeparateur("résultat");

            var dto = _serviceEmployee.CreerEmploye(
                prenom, nom, email, club.Id, role, dateEmbauche,
                salaire, string.IsNullOrWhiteSpace(qualifications) ? null : qualifications);

            InterfaceConsole.AfficherSucces(
                $"Employé créé — {dto.NomComplet} ({RoleLabel(dto.Role)}) au club {dto.NomClub}.",
                $"#{dto.Id:D4}");
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

    public void AfficherMenuListeEmployees()
    {
        AnsiConsole.Clear();
        InterfaceConsole.AfficherTitre("LISTE DES EMPLOYÉS");

        try
        {
            var employees = _serviceEmployee.ObtenirTousLesEmployes();

            if (employees.Count == 0)
            {
                var videPanel = new Panel(
                    new Markup("[dim][[ État vide ]] — Aucun employé trouvé.[/]"))
                    .Border(BoxBorder.Rounded)
                    .BorderStyle(new Style(Color.Grey))
                    .Padding(1, 0);

                AnsiConsole.Write(videPanel);
                InterfaceConsole.RetourMenu();
                return;
            }

            var table = new Table()
                .AddColumn(new TableColumn("[bold]ID[/]").Centered())
                .AddColumn(new TableColumn("[bold]Nom[/]"))
                .AddColumn(new TableColumn("[bold]Email[/]"))
                .AddColumn(new TableColumn("[bold]Rôle[/]"))
                .AddColumn(new TableColumn("[bold]Club[/]"))
                .AddColumn(new TableColumn("[bold]Salaire[/]").Centered())
                .Border(TableBorder.Rounded)
                .BorderStyle(new Style(Color.Grey));

            foreach (var e in employees)
            {
                table.AddRow(
                    e.Id.ToString("D3"),
                    Markup.Escape(e.NomComplet),
                    Markup.Escape(e.Email),
                    Markup.Escape(RoleLabel(e.Role)),
                    Markup.Escape(e.NomClub),
                    e.SalaireMensuel.HasValue ? $"{e.SalaireMensuel:F2} EUR" : "—");
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

    // Sélection d'un employé dans une liste, puis mise à jour de son affectation.
    public void AfficherMenuModifierEmployee()
    {
        AnsiConsole.Clear();
        InterfaceConsole.AfficherTitre("MODIFIER UN EMPLOYÉ");

        try
        {
            var employee = ChoisirEmployee(" [bold]Sélectionnez l'employé à modifier :[/]", Color.Blue);
            if (employee is null) return;

            AnsiConsole.MarkupLine($" [dim]Actuellement : {Markup.Escape(RoleLabel(employee.Role))} au club {Markup.Escape(employee.NomClub)}[/]");
            AnsiConsole.WriteLine();

            var clubs = _serviceEmployee.ObtenirClubsSelectionnables();
            var club = AnsiConsole.Prompt(
                new SelectionPrompt<OptionClub>()
                    .Title(" [bold]Nouveau club :[/]")
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .PageSize(10)
                    .UseConverter(c => $"{Markup.Escape(c.Nom)}  [dim]({Markup.Escape(c.Ville)}) — {StatutClubLabel(c.Statut)}[/]")
                    .AddChoices(clubs));

            var role = AnsiConsole.Prompt(
                new SelectionPrompt<RoleEmploye>()
                    .Title(" [bold]Nouveau rôle :[/]")
                    .HighlightStyle(new Style(Color.Blue, decoration: Decoration.Bold))
                    .UseConverter(RoleLabel)
                    .AddChoices(Enum.GetValues<RoleEmploye>()));

            var infos = _serviceEmployee.ObtenirInfosModificationEmploye(employee.Id);
            var salaireActuel = infos?.Salaire?.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            var qualifActuelles = infos?.Qualifications ?? string.Empty;

            AnsiConsole.WriteLine();
            InterfaceConsole.AfficherSeparateur("valeurs actuelles — ENTRÉE pour conserver");
            AnsiConsole.WriteLine();

            var salaireStr = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold]Salaire mensuel :[/] ").DefaultValue(salaireActuel).AllowEmpty());
            decimal? salaire = null;
            if (!string.IsNullOrWhiteSpace(salaireStr))
            {
                if (!decimal.TryParse(salaireStr, System.Globalization.NumberStyles.Number,
                        System.Globalization.CultureInfo.InvariantCulture, out var s) || s < 0)
                {
                    InterfaceConsole.AfficherAlerte("Salaire invalide.");
                    return;
                }
                salaire = s;
            }

            var qualifications = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold]Qualifications :[/] ").DefaultValue(qualifActuelles).AllowEmpty());

            InterfaceConsole.AfficherSeparateur("résultat");

            bool succes = _serviceEmployee.MettreAJourEmploye(
                employee.Id, club.Id, role, salaire,
                string.IsNullOrWhiteSpace(qualifications) ? null : qualifications.Trim());

            if (succes)
                InterfaceConsole.AfficherSucces($"Employé mis à jour — {employee.NomComplet} ({RoleLabel(role)}) au club {club.Nom}.", $"#{employee.Id:D4}");
            else
                InterfaceConsole.AfficherAlerte("Aucune modification effectuée.");
        }
        catch (Exception ex)
        {
            InterfaceConsole.AfficherErreur(ex.Message);
        }
    }

    // Sélection d'un employé dans une liste, puis suppression après confirmation.
    public void AfficherMenuSupprimerEmployee()
    {
        AnsiConsole.Clear();
        InterfaceConsole.AfficherTitre("SUPPRIMER UN EMPLOYÉ");

        try
        {
            var employee = ChoisirEmployee(" [bold]Sélectionnez l'employé à supprimer :[/]", Color.Red);
            if (employee is null) return;

            var confirmer = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold red]Supprimer définitivement {Markup.Escape(employee.NomComplet)} ?[/]")
                    .HighlightStyle(new Style(Color.Red, decoration: Decoration.Bold))
                    .AddChoices("[[ Oui, supprimer ]]", "[[ Annuler ]]"));

            if (confirmer == "[[ Annuler ]]")
            {
                InterfaceConsole.AfficherAlerte("Suppression annulée.");
                return;
            }

            InterfaceConsole.AfficherSeparateur("résultat");
            bool succes = _serviceEmployee.SupprimerEmploye(employee.Id);
            if (succes)
                InterfaceConsole.AfficherSucces($"Employé supprimé — {employee.NomComplet}.", $"#{employee.Id:D4}");
            else
                InterfaceConsole.AfficherAlerte("Aucune suppression effectuée.");
        }
        catch (Exception ex)
        {
            InterfaceConsole.AfficherErreur(ex.Message);
        }
    }

    // Demande le choix d'un employé dans la liste ; retourne null si aucun employé.
    private EmployeDto? ChoisirEmployee(string titre, Color couleur)
    {
        var employees = _serviceEmployee.ObtenirTousLesEmployes();
        if (employees.Count == 0)
        {
            InterfaceConsole.AfficherAlerte("Aucun employé disponible.");
            return null;
        }

        return AnsiConsole.Prompt(
            new SelectionPrompt<EmployeDto>()
                .Title(titre)
                .HighlightStyle(new Style(couleur, decoration: Decoration.Bold))
                .PageSize(10)
                .MoreChoicesText("[dim]Flèches pour faire défiler[/]")
                .UseConverter(e => $"#{e.Id:D4} — {Markup.Escape(e.NomComplet)}  [dim]({Markup.Escape(RoleLabel(e.Role))}, {Markup.Escape(e.NomClub)})[/]")
                .AddChoices(employees));
    }

    // Libellé lisible d'un rôle d'employé.
    private static string RoleLabel(RoleEmploye role) => role switch
    {
        RoleEmploye.Instructeur     => "Instructeur",
        RoleEmploye.Gerant          => "Gérant",
        RoleEmploye.Receptionniste  => "Réceptionniste",
        RoleEmploye.Technicien      => "Technicien",
        RoleEmploye.Stagiaire       => "Stagiaire",
        _                           => role.ToString()
    };

    private static string StatutClubLabel(StatutOperationnelClub s) => s switch
    {
        StatutOperationnelClub.Ouvert              => "Ouvert",
        StatutOperationnelClub.FermeTemporairement => "Fermé temporairement",
        StatutOperationnelClub.FermeDefinitivement => "Fermé définitivement",
        _                                          => s.ToString()
    };
}