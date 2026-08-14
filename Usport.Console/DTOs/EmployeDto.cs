using Usport.Domaine.Enumerations;

namespace Usport.Console.DTOs;

/// <summary>
/// Données d'un employé exposées par les services, sans révéler l'entité du domaine.
/// </summary>
public class EmployeDto
{
    public int Id { get; init; }
    public string NomComplet { get; init; } = null!;
    public string Email { get; init; } = null!;
    public RoleEmploye Role { get; init; }
    public string NomClub { get; init; } = null!;
    public decimal? SalaireMensuel { get; init; }
    public string InfosAffichage { get; init; } = null!;
}