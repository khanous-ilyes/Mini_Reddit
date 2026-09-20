# 🧠 KnowledgeHub (Mini-Reddit)

Une plateforme complète et moderne agissant comme un **Mini-Reddit**, conçue pour favoriser la collaboration, l'échange de connaissances et la résolution de problèmes au sein d'une organisation. 

![KnowledgeHub Logo](./Client/wwwroot/images/logo-reddit.png) <!-- Adaptez le chemin vers votre logo si nécessaire -->

## ✨ Fonctionnalités Principales

### 👥 Côté Utilisateur
*   **Diversité des contenus :** Partagez vos connaissances via différents types de postes interactifs :
    *   **Problèmes (Questions) & Solutions :** Posez des questions et proposez des solutions validées.
    *   **News :** Partagez les actualités importantes (avec support d'upload d'images).
    *   **Quiz & Sondages (Surveys) :** Testez les connaissances et recueillez les avis de la communauté.
*   **Système d'Engagement (Gamification) :** 
    *   Commentez, interagissez et participez aux discussions.
    *   Notez (Star Rating) les meilleurs contenus.
*   **Groupes de discussion :** Rejoignez des groupes spécifiques pour filtrer votre flux d'actualité et ne voir que le contenu pertinent (Confidentialité de recherche intégrée).
*   **Profils Personnalisables :** Éditez votre biographie, ajoutez vos informations de contact, et gardez un suivi précis de vos contributions.
*   **Support Multilingue :** Interface entièrement traduite et basculable instantanément entre l'**Arabe (RTL)** et le **Français (LTR)**.
*   **Interface "Glassmorphism" Premium :** Un design sombre, ultra-moderne et fluide avec des animations soignées pour une expérience utilisateur (UX) optimale.

### 🛡️ Côté Administrateur (SuperAdmin & Modérateurs)
*   **Dashboard Analytics (Statistiques) :** Un tableau de bord complet affichant les indicateurs clés (KPIs) en temps réel (Nombre total de postes, utilisateurs inscrits, volume de commentaires).
*   **Classements & Détection :** 
    *   Identifier les meilleurs contributeurs (Top Ratings, Champions Solutions).
    *   Détecter automatiquement les comportements problématiques (Auteurs mal notés, signalements abusifs).
*   **Modération Avancée :** Gestion des signalements (Reports) pour garder la plateforme saine.
*   **Épinglage (Pin Posts) :** Mettez en avant les annonces cruciales et les actualités stratégiques en les épinglant tout en haut du flux d'actualité.
*   **Gestion des Groupes et Contenus :** Création et administration complète des groupes et des droits.

## 🛠️ Stack Technique

*   **Frontend :** [Blazor WebAssembly](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor) (C# / .NET 8)
*   **Backend :** ASP.NET Core 8 Web API
*   **Base de données :** SQLite (via Entity Framework Core)
*   **Architecture :** Solution Clean Architecture séparée en projets multiples (`Client`, `Server`, `BaseLibrary`, `ServerLibrary`, `ClientLibrary`).

## 🚀 Installation et Lancement

### Prérequis
*   [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) installé sur votre machine.

### Étapes pour lancer le projet localement
1.  **Cloner le dépôt :**
    ```bash
    git clone <votre-url-github>
    cd OurProject
    ```

2.  **Lancer le Serveur (Backend & Base de données) :**
    La base de données SQLite se créera automatiquement au premier lancement (via les migrations EF Core).
    ```bash
    cd Server
    dotnet run
    ```

3.  **Lancer le Client (Frontend Blazor) :**
    Ouvrez un nouveau terminal et lancez le client.
    ```bash
    cd Client
    dotnet run
    ```
4.  Ouvrez votre navigateur sur l'adresse indiquée par le terminal du `Client` (ex: `http://localhost:5168` ou l'adresse configurée).

## 💡 Perspectives (Roadmap future)
*   **Système de Recommandation IA :** Suggérer des posts pertinents basés sur l'historique utilisateur.
*   **Notifications en Temps Réel :** Intégration de SignalR pour des alertes instantanées (nouveaux commentaires, mentions).
*   **Assistant IA Intégré :** Résumer de longs fils de discussion ou suggérer des solutions automatiques.

---
*Développé avec passion pour l'échange de connaissances.*
