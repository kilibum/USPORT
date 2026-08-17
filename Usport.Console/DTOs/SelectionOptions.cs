using Usport.Domaine.Enumerations;

namespace Usport.Console.DTOs;

// Options proposées dans les listes de sélection interactives des menus.

public record OptionMembre(int Id, string NomComplet, string? Ville);

public record OptionPlan(int Id, string NomPlan, decimal PrixDeBase, int MoisEngagement);

public record OptionClub(int Id, string Nom, string Ville, StatutOperationnelClub Statut = StatutOperationnelClub.Ouvert);

public record OptionServiceSelection(int Id, string NomOption, decimal PrixMensuel);