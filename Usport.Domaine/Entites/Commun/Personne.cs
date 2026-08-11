using System;

namespace Usport.Domaine.Entites.Commun;

/// <summary>
/// Classe de base abstraite pour les personnes du système.
/// </summary>
public abstract class Personne
{
    public int Id {get; private set;}
    public string Prenom {get; private set;}
    public string Nom {get; private set;}
    public string Email {get; private set;}
    public DateTime DateDeCreation {get; private set;}
    public DateTime DateDeModification {get; private set;}


    public string NomComplet => $"{Prenom} {Nom}";

    protected Personne(string prenom, string nom, string email)
    {
        if (string.IsNullOrWhiteSpace(prenom))
            throw new ArgumentException("Le prénom ne peut pas être vide.", nameof(prenom));
        if (string.IsNullOrWhiteSpace(nom))
            throw new ArgumentException("Le nom ne peut pas être vide.", nameof(nom));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("L'email ne peut pas être vide.", nameof(email));   

        Prenom = prenom;
        Nom = nom;
        Email = email;
        DateDeCreation = DateTime.UtcNow;
        DateDeModification = DateTime.UtcNow;
    }

    protected Personne(int id, string prenom, string nom, string email, DateTime dateDeCreation, DateTime dateDeModification)
        {
            Id = id;
            Prenom = prenom;
            Nom = nom;
            Email = email;
            DateDeCreation = dateDeCreation;
            DateDeModification = dateDeModification;
        }

        protected void DefinirId(int id) => Id = id;
        public abstract string ObtenirInfosAffichage();
}