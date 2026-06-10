# Bookify — Backend

API RESTful du projet Bookify, développée avec ASP.NET Core (ciblant .NET 10), Entity Framework Core et MySQL.

Cette API suit une architecture en couches : Controllers → Services → Models → Infrastructure. Les contrôleurs exposent des endpoints HTTP, la logique métier est encapsulée dans les services, et l'accès aux données est géré par EF Core. Les DTOs sont utilisés pour les transferts entre couches.

Technologies principales :
- ASP.NET Core Web API (C#)
- Entity Framework Core (MySQL)
- JWT pour l'authentification
- Cloudinary pour le stockage d'avatars
- Service d'envoi d'e-mails pour la réinitialisation de mot de passe

Organisation du projet
- Controllers/: points d'entrée HTTP (AuthController, UtilisateurController, PrestatairesController, RendezVousController, NotificationsController, StatsController)
- Services/: logique métier réutilisable (CloudinaryService, EmailService, etc.)
- Models/: entités et contexte EF (BookifyDbContext)
- DTOs/: objets de transfert pour requêtes/réponses

Endpoints principaux (préfixe : /api)
- POST /api/auth/register — créer un utilisateur
- POST /api/auth/login — authentification (retourne JWT)
- PUT /api/auth/change-password/{id} — changer le mot de passe (authentifié)
- POST /api/auth/forgot-password — demander un code de réinitialisation
- POST /api/auth/verify-reset-code — vérifier le code envoyé
- POST /api/auth/reset-password — réinitialiser le mot de passe

- GET /api/utilisateur — (ADMIN) obtenir la liste des utilisateurs
- GET /api/utilisateur/{id} — obtenir un utilisateur
- PUT /api/utilisateur/{id} — mettre à jour le profil
- DELETE /api/utilisateur/{id} — supprimer un utilisateur
- POST /api/utilisateur/{id}/avatar — téléverser un avatar (Cloudinary)
- DELETE /api/utilisateur/{id}/avatar — supprimer un avatar

- Routes supplémentaires : /api/prestataires, /api/rendezvous, /api/notifications, /api/stats — voir les contrôleurs pour les détails.

Sécurité
- Authentification : JWT. Le token contient les claims "id" et "role".
- Autorisations : certains endpoints sont restreints aux rôles (ex : ADMIN).

Configuration
Les paramètres importants se trouvent dans appsettings.json ou dans les secrets d'environnement :
- ConnectionStrings: chaîne de connexion MySQL
- Jwt: Key, Issuer, Audience, ExpireMinutes
- Cloudinary: credentials (cloud name, api key, api secret)
- Email service: configuration SMTP

Exemple minimal (appsettings.json) :

{
  "ConnectionStrings": { "DefaultConnection": "server=...;user=...;password=...;database=..." },
  "Jwt": { "Key": "votre_cle_secrete", "Issuer": "bookify", "Audience": "bookify_users", "ExpireMinutes": "60" }
}

Ne pas committer de secrets dans le dépôt. Utilisez User Secrets ou variables d'environnement en dev/production.

Base de données & migrations
- Utiliser les migrations EF Core :
  - dotnet ef migrations add NomMigration
  - dotnet ef database update

Lancer localement
1. dotnet restore
2. Configurer la chaîne de connexion et les secrets
3. dotnet ef database update
4. dotnet run

Tests et validation
Utiliser Postman ou curl pour tester les endpoints. Les routes protégées nécessitent l'en-tête Authorization: Bearer {token}.

Contribution
Ouvrir une issue ou proposer une Pull Request pour corriger/ajouter des fonctionnalités.

Pour plus de détails sur chaque endpoint, consulter les contrôleurs dans le dossier Controllers et les DTOs associés.
