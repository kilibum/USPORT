using Usport.Domaine.Enumerations;

namespace Usport.Console.DTOs;

/// <summary>
/// Données d'un membre exposées par les services, sans révéler l'entité du domaine.
/// </summary>
public class MembreDto
{
    public int Id { get; init; }
    public string NomComplet { get; init; } = null!;
    public int Age { get; init; }
    public string Email { get; init; } = null!;
    public string? AdresseVille { get; init; }
    public Genre Genre { get; init; }
    public ObjectifPrincipal? ObjectifPrincipal { get; init; }
    public StatutMembre Statut { get; init; }
    public int NombreVisitesTotal { get; init; }
    public string InfosAffichage { get; init; } = null!;
}