namespace Usport.Domaine.Entites.Adhesion;

public class OptionService
{
    public int Id { get; private set; }
    public string NomOption { get; private set; }
    public decimal PrixMensuel { get; private set; }
    public DateTime DateCreation { get; private set; }
    public DateTime DateModification { get; private set; }

    public OptionService(string nomOption, decimal prixMensuel)
    {
        if (string.IsNullOrWhiteSpace(nomOption)) throw new ArgumentException("Le nom de l'option est obligatoire.", nameof(nomOption));
        if (prixMensuel < 0)                      throw new ArgumentException("Le prix ne peut pas être négatif.", nameof(prixMensuel));

        NomOption         = nomOption;
        PrixMensuel       = prixMensuel;
        DateCreation      = DateTime.UtcNow;
        DateModification  = DateTime.UtcNow;
    }

    /// <summary>Attribue un identifiant à l'option (stockage en mémoire).</summary>
    public void AssignerId(int id) => Id = id;
}