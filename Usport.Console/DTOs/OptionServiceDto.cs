namespace Usport.Console.DTOs;

/// <summary>
/// Données d'une option de service exposées par les services, sans révéler l'entité du domaine.
/// </summary>
public class OptionServiceDto
{
    public int Id { get; init; }
    public string NomOption { get; init; } = null!;
    public decimal PrixMensuel { get; init; }
    public DateTime DateCreation { get; init; }
}
