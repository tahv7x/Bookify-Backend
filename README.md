<div align="center">
  <img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white" alt="C#" />
  <img src="https://img.shields.io/badge/.NET_8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/Entity_Framework-007ACC?style=for-the-badge&logo=nuget&logoColor=white" alt="Entity Framework" />
  <img src="https://img.shields.io/badge/MySQL-4479A1?style=for-the-badge&logo=mysql&logoColor=white" alt="MySQL" />
  <img src="https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=JSON%20web%20tokens&logoColor=white" alt="JWT" />
</div>

<br />

<div align="center">
  <h1>⚙️ Bookify - Backend REST API</h1>
  <p><strong>Le cœur logique et sécurisé de la plateforme de réservation Bookify</strong></p>
  <p>Construit avec <b>ASP.NET Core Web API</b>, <b>Entity Framework Core</b> et <b>MySQL</b>.</p>
</div>

---

## 📖 Présentation

Le backend de **Bookify** est une API RESTful robuste et scalable chargée de gérer toute la logique métier de la plateforme. Il expose des endpoints sécurisés pour permettre aux applications front-end (web, mobile) d'interagir avec la base de données.

L'architecture est structurée de manière modulaire autour de plusieurs contrôleurs (Controllers), avec une forte emphase sur la **sécurité (JWT, Claims, RBAC)**, l'intégrité des données et les performances.

---

## 🚀 Fonctionnalités Principales

- **🛡️ Authentification & Autorisation (JWT)** : Sécurisation complète par JSON Web Tokens. Rôles stricts (`ADMIN`, `PRESTATAIRE`, `CLIENT`).
- **📅 Gestion Complexe des Rendez-vous** : Système de réservation avec validation de la disponibilité, prévention des conflits d'horaires et gestion de statuts (`EN_ATTENTE`, `ACCEPTE`, `REFUSE`, `TERMINE`, `ANNULE`).
- **💬 Messagerie en Temps Réel** : Endpoints dédiés à l'échange de messages privés entre clients et prestataires.
- **📊 Statistiques & Tableaux de Bord** : Agrégation de données pour fournir des KPI (Revenus, Taux d'annulation, Croissance) aux prestataires et à l'administrateur.
- **⭐ Système d'Avis & Favoris** : Notation multidimensionnelle des prestataires et gestion de listes de favoris pour les clients.
- **🎫 Support & Modération** : Système de tickets de support complet (Ouvert/Fermé, Messages) géré par les administrateurs.
- **☁️ Intégration Cloudinary** : Gestion des uploads d'images (Avatars, Portfolios) directement via le backend.

---

## 📂 Architecture du Projet

L'API suit le pattern **MVC (Modèles, Contrôleurs)** avec injection de dépendances.

```text
backend/Bookify-API/
├── Controllers/       # Points d'entrée de l'API (Endpoints)
├── DTOs/              # Data Transfer Objects (Validation & Formatage des données I/O)
├── Models/            # Entités de la base de données (Entity Framework)
├── Services/          # Logique métier spécifique (ex: CloudinaryService)
├── Data/              # Context de base de données (BookifyDbContext)
├── Migrations/        # Historique des schémas de la BDD (EF Core Migrations)
├── Program.cs         # Configuration de l'application et des middlewares
└── appsettings.json   # Configurations (Chaîne de connexion, Clé JWT, Cloudinary)
```

---

## 🌐 Documentation des Endpoints (API Reference)

*Note : La majorité des endpoints (sauf ceux marqués publics) nécessitent un Header `Authorization: Bearer <token>`.*

### 🔐 Authentification (`/api/Auth`)
- `POST /register/client` : Créer un compte client *(Public)*.
- `POST /register/prestataire` : Créer un compte prestataire *(Public)*.
- `POST /login` : Obtenir un jeton JWT *(Public)*.
- `POST /refresh-token` : Renouveler un jeton expiré.
- `POST /forgot-password` / `reset-password` : Récupération de compte.

### 👤 Utilisateurs (`/api/Utilisateur`)
- `GET /` : Liste de tous les utilisateurs (Filtres admin).
- `GET /{id}` : Profil d'un utilisateur spécifique.
- `PUT /{id}` : Mettre à jour les infos d'un utilisateur (Auto-gestion).
- `DELETE /{id}` : Supprimer un compte (Admin).
- `PATCH /{id}/block` : Bloquer/Débloquer un utilisateur (Admin).

### 💼 Prestataires & Services (`/api/Prestataires` / `/api/Services`)
- `GET /api/Prestataires` : Rechercher des prestataires (Filtres : ville, spécialité, note) *(Public)*.
- `GET /api/Prestataires/profile/{id}` : Profil complet public (Avis, Services, Portfolio) *(Public)*.
- `POST /api/Services` : Ajouter un nouveau service pour un prestataire.
- `GET /api/Disponibilites` : Gérer les horaires de travail du prestataire.

### 📅 Rendez-vous (`/api/RendezVous`)
- `POST /` : Réserver un nouveau rendez-vous (Vérification stricte des dispos).
- `GET /client/{id}` : Historique et rendez-vous à venir pour un client.
- `GET /prestataire/{id}` : Dashboard des rendez-vous du prestataire.
- `PUT /{id}/accept` : Accepter une demande (Prestataire).
- `PUT /{id}/refuse` : Refuser une demande avec motif (Prestataire).
- `PUT /{id}/cancel` : Annuler un rendez-vous (Client/Prestataire).
- `PUT /{id}/complete` : Marquer comme terminé (Prestataire).

### 💬 Messagerie (`/api/Message`)
- `GET /conversations/{userId}` : Récupérer la liste des conversations actives.
- `GET /history/{user1}/{user2}` : Charger l'historique d'une discussion.
- `POST /send` : Envoyer un nouveau message privé.
- `PUT /{messageId}/read` : Marquer un message comme lu.

### 🎫 Support & Tickets (`/api/Support`)
- `POST /` : Créer un nouveau ticket (Demande d'aide).
- `GET /my-tickets` : Voir ses propres tickets (Client/Prestataire).
- `GET /` : Lister tous les tickets (Admin).
- `POST /{id}/reply` : Répondre à un ticket.
- `PATCH /{id}/close` : Clôturer un ticket (Admin).

### 📊 Statistiques (`/api/Stats`)
- `GET /admin` : Chiffres clés de la plateforme (Total users, CA estimé, Top catégories).
- `GET /prestataire/{id}` : Chiffres du prestataire (Revenus mensuels, Taux d'acceptation, Évolution).

### ⭐ Avis & Favoris (`/api/Avis` / `/api/Favoris`)
- `POST /api/Avis` : Laisser un avis après un rendez-vous terminé.
- `GET /api/Avis/prestataire/{id}` : Lire les avis d'un prestataire *(Public)*.
- `POST /api/Favoris/{prestataireId}` : Ajouter/Retirer un prestataire de ses favoris.
- `GET /api/Favoris/client/{id}` : Lister les favoris d'un client.

---

## 🔄 Comment fonctionne l'API (Flux de Données et Intégration)

L'API Bookify n'est pas seulement une base de données passive ; elle orchestre la logique métier et pré-formate les données pour faciliter leur consommation par le frontend React.

### 1. Format des Réponses (JSON)
L'API retourne systématiquement les données au format JSON. Lorsqu'une erreur survient, elle renvoie des codes HTTP standards (`400 Bad Request`, `401 Unauthorized`, `404 Not Found`) avec un message clair :
```json
{
  "message": "Cet horaire n'est plus disponible."
}
```

### 2. Exemples de Flux de Données (Endpoints -> Frontend)

#### A. Le Dashboard du Prestataire (Statistiques)
- **Endpoint appelé** : `GET /api/Stats/prestataire/{id}`
- **Ce que fait le backend** : Il interroge la table `RendezVous`, compte les réservations du mois, fait la somme des revenus, calcule la note moyenne depuis la table `Avis`, et génère les données historiques.
- **Ce que retourne le backend** :
  ```json
  {
    "revenus": 12500,
    "rdvThisMonth": 42,
    "noteMoyenne": 4.8,
    "areaData": [{ "month": "Jan", "v1": 10, "v2": 3000 }, ...],
    "rdvDaysThisMonth": [{ "day": 12, "statut": "ACCEPTE" }]
  }
  ```
- **Où c'est utilisé** : Directement dans `Dashboard.tsx` (Espace Prestataire). Les graphiques interactifs (`AreaChartSVG`, calendrier dynamique) s'hydratent avec ces données.

#### B. La Prise de Rendez-vous (Vérification des disponibilités)
- **Endpoints appelés** : `GET /api/Disponibilites/prestataire/{id}` puis `POST /api/RendezVous`
- **Ce que fait le backend** : 
  1. Il retourne d'abord les créneaux horaires configurés par le prestataire.
  2. Lors de la réservation (`POST`), le contrôleur **bloque la transaction** si le créneau chevauche un rendez-vous existant ou si le prestataire ne travaille pas ce jour-là.
- **Où c'est utilisé** : Dans `ServiceBooking.tsx` (Espace Client). Le client voit uniquement les heures cliquables (libres) et reçoit une notification de succès ou d'erreur instantanée si quelqu'un a réservé entre-temps.

#### C. L'Exploration des Prestataires (Filtres et Recherche)
- **Endpoint appelé** : `GET /api/Prestataires?ville=Casablanca&categorie=Plomberie`
- **Ce que fait le backend** : Le contrôleur utilise Entity Framework (`.Include()`) pour joindre les tables `Utilisateur` (avatar, nom), `Services` (prix) et `Avis` (note). Il filtre dynamiquement les résultats via des requêtes LINQ optimisées.
- **Ce que retourne le backend** : Une liste de prestataires enrichie (avec leur note globale pré-calculée et le prix de leur service le moins cher).
- **Où c'est utilisé** : Dans `Explore.tsx` (Espace Client). La page React boucle sur ce tableau JSON pour générer les jolies cartes de profil (Glassmorphism).

#### D. Système de Support (Tickets)
- **Endpoint appelé** : `GET /api/Support` (par l'Admin) ou `GET /api/Support/my-tickets`
- **Flux** : Lorsqu'un utilisateur a un problème, il envoie un ticket. L'admin récupère la liste complète via l'API, avec une propriété `isResolved` (Ouvert/Fermé). L'admin peut envoyer un `POST /api/Support/{id}/reply` pour répondre au client.
- **Où c'est utilisé** : Dans `Admin/Support.tsx` et `Client/HelpSupport.tsx`.

---

## 🛠️ Installation et Exécution Locale

### 1. Prérequis
- **.NET 8.0 SDK** (ou ultérieur)
- **Serveur MySQL** (local ou distant comme XAMPP, WAMP, ou Docker)

### 2. Configuration de la Base de données
Modifiez le fichier `appsettings.json` (ou `appsettings.Development.json`) :
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=bookify_db;User=root;Password=;"
}
```

### 3. Appliquer les Migrations
Générez la structure de la base de données via Entity Framework :
```bash
dotnet ef database update
```
*(Assurez-vous que l'outil global EF Core est installé : `dotnet tool install --global dotnet-ef`)*

### 4. Démarrer l'API
```bash
dotnet run
```
> L'API s'exécutera généralement sur **https://localhost:5200** ou **http://localhost:5200**.
> Si Swagger est activé, accédez à `https://localhost:5200/swagger` pour tester directement les endpoints.

---

## 🔐 Sécurité & Bonnes Pratiques Implémentées

- **Mots de passe hachés** : Utilisation de `BCrypt` pour stocker les mots de passe de manière sécurisée.
- **Politiques CORS** : Seules les origines autorisées (comme le frontend Vite) peuvent faire des requêtes.
- **Protection par Rôles** : L'attribut `[Authorize(Roles = "ADMIN")]` est strictement utilisé pour les endpoints d'administration.
- **Cascade Delete gérée** : La suppression d'un utilisateur supprime proprement ses entités orphelines (Messages, Rendez-vous, Avis) ou les rend anonymes selon la règle métier définie dans `UtilisateurController`.

---
<div align="center">
  <p>API construite pour garantir performance, sécurité et fiabilité de l'écosystème <b>Bookify</b>.</p>
</div>
