using Usport.Domaine.Enumerations;

namespace Usport.Console.DTOs;

/// <summary>
/// Données d'un contrat exposées par les services, sans révéler l'entité du domaine.
/// </summary>
public class ContratDto
{
    public int Id { get; init; }
    public int IdMembre { get; init; }
    public int IdPlan { get; init; }
    public StatutContrat Statut { get; init; }
    public DateOnly DateDebut { get; init; }
    public DateOnly? DateFin { get; init; }
    public decimal PrixMensuel { get; init; }
}