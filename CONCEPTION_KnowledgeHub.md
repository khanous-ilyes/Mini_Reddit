# KnowledgeHub — Plateforme Interne de Partage des Connaissances
## Document de Conception Technique Détaillée

> **Stack :** ASP.NET Core 8 Web API + Blazor WebAssembly  
> **Architecture :** 5 projets (Server / ServerLibrary / BaseLibrary / ClientLibrary / Client)  
> **Cible :** Grande entreprise, utilisateurs internes (employés)

---

## 1. Vision & Concept

KnowledgeHub est une plateforme interne inspirée de Reddit, dédiée au partage d'expertises, de solutions techniques et d'informations entre employés d'une grande entreprise. Elle est organisée en **groupes thématiques** créés par l'administrateur, dans lesquels les utilisateurs publient et consomment du contenu structuré par **types de postes**.

### Analogie Reddit adaptée à l'entreprise

| Reddit | KnowledgeHub |
|--------|-------------|
| Subreddit | Groupe |
| Post | Poste (typé) |
| Upvote/Downvote | Vote de commentaire |
| Moderator | Super Admin |
| User | Employé (via SSO) |

---

## 2. Authentification & Intégration SSO

### 2.1 Principe du SSO simulé

Le système reçoit un **token JWT** externe contenant exactement deux claims :

```json
{
  "user_id": "EMP001234",
  "id_structure": "STR042"
}
```

L'application ne gère pas la création des comptes — elle **lit les données depuis deux tables de synonymes** fournies par le DBA en lecture seule.

### 2.2 Tables Synonymes (lecture seule, fournies par DBA)

```sql
-- Table 1 : Structures organisationnelles
Structures (
  id_structure   VARCHAR(20) PRIMARY KEY,
  structure_name VARCHAR(200) NOT NULL
)

-- Table 2 : Employés
Employees (
  id_user      VARCHAR(20) PRIMARY KEY,
  nom          VARCHAR(100) NOT NULL,
  prenom       VARCHAR(100) NOT NULL,
  grade        VARCHAR(100),
  id_structure VARCHAR(20) REFERENCES Structures(id_structure)
)
```

### 2.3 Table de profil applicatif (gérée par l'app)

```sql
UserProfiles (
  id_user        VARCHAR(20) PRIMARY KEY,  -- FK vers Employees.id_user
  telephone      VARCHAR(20),
  bio            TEXT,
  avatar_url     VARCHAR(500),
  is_active      BOOLEAN DEFAULT TRUE,
  created_at     TIMESTAMP DEFAULT NOW(),
  updated_at     TIMESTAMP DEFAULT NOW()
)
```

### 2.4 Flow d'authentification simulé

```
[Client Blazor]
     │
     ▼
POST /api/auth/login  { username, password }  ← simulation SSO
     │
     ▼
[AuthController]
     ├── Valide credentials
     ├── Lit Employees WHERE id_user = ?
     ├── Lit Structures WHERE id_structure = ?
     ├── Crée/met à jour UserProfile si première connexion
     └── Génère JWT interne { user_id, id_structure, roles[] }
     │
     ▼
[Client reçoit le token et le stocke]
```

### 2.5 Rôles système

| Rôle | Description |
|------|-------------|
| `SuperAdmin` | Gère les groupes, domaines, configuration des types de postes |
| `ContentManager` | Peut publier News, Quiz, Sondage |
| `User` | Peut publier Solution, Problème ; consulter tout |

---

## 3. Modèle de Données Complet

### 3.1 Groupes & Domaines

```sql
Groups (
  id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name           VARCHAR(200) NOT NULL,
  description    TEXT,
  icon_url       VARCHAR(500),
  is_active      BOOLEAN DEFAULT TRUE,
  created_by     VARCHAR(20) REFERENCES Employees(id_user),
  created_at     TIMESTAMP DEFAULT NOW()
)

Domains (
  id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  group_id       UUID REFERENCES Groups(id) ON DELETE CASCADE,
  name           VARCHAR(200) NOT NULL,   -- ex: "Bug Informatique", "Vulnérabilité"
  description    TEXT,
  is_active      BOOLEAN DEFAULT TRUE
)

GroupMembers (
  id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  group_id       UUID REFERENCES Groups(id),
  user_id        VARCHAR(20) REFERENCES Employees(id_user),
  joined_at      TIMESTAMP DEFAULT NOW(),
  UNIQUE(group_id, user_id)
)
```

### 3.2 Types de Postes (configuration par Admin)

```sql
PostTypes (
  id                    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  code                  VARCHAR(50) UNIQUE NOT NULL,
  -- Valeurs: SOLUTION, PROBLEM, NEWS, QUIZ, SURVEY
  label                 VARCHAR(100) NOT NULL,
  allow_comments        BOOLEAN DEFAULT TRUE,
  requires_privilege    BOOLEAN DEFAULT FALSE,
  -- Si TRUE → seul ContentManager ou SuperAdmin peut créer
  is_active             BOOLEAN DEFAULT TRUE
)
```

**Seed initial :**

| code | label | allow_comments | requires_privilege |
|------|-------|----------------|-------------------|
| SOLUTION | Partage de Solution | true | false |
| PROBLEM | Poser un Problème | true | false |
| NEWS | Actualité / Info | true | true |
| QUIZ | Quiz | false | true |
| SURVEY | Sondage | false | true |

### 3.3 Postes

```sql
Posts (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  group_id        UUID REFERENCES Groups(id),
  domain_id       UUID REFERENCES Domains(id) NULLABLE,
  -- NULL si l'utilisateur choisit "Autre"
  post_type_id    UUID REFERENCES PostTypes(id),
  author_id       VARCHAR(20) REFERENCES Employees(id_user),
  title           VARCHAR(500) NOT NULL,
  status          VARCHAR(20) DEFAULT 'ACTIVE',
  -- ACTIVE, ARCHIVED, DELETED
  views_count     INTEGER DEFAULT 0,
  created_at      TIMESTAMP DEFAULT NOW(),
  updated_at      TIMESTAMP DEFAULT NOW(),
  deleted_at      TIMESTAMP NULLABLE
)

-- Section structurée : varie selon le type de poste
PostSections (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  post_id         UUID REFERENCES Posts(id) ON DELETE CASCADE,
  section_type    VARCHAR(50) NOT NULL,
  -- PROBLEM_DESC, SOLUTION_DESC, CONTEXT, STEPS, RESULT, CONTENT
  content         TEXT NOT NULL,
  order_index     INTEGER DEFAULT 0
)

PostHashtags (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  post_id         UUID REFERENCES Posts(id) ON DELETE CASCADE,
  tag             VARCHAR(100) NOT NULL
)

-- Index pour la recherche rapide par hashtag
CREATE INDEX idx_post_hashtags_tag ON PostHashtags(LOWER(tag));

PostMedia (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  post_id         UUID REFERENCES Posts(id) ON DELETE CASCADE,
  file_url        VARCHAR(500) NOT NULL,
  file_type       VARCHAR(50),   -- IMAGE, PDF, VIDEO
  file_name       VARCHAR(200),
  order_index     INTEGER DEFAULT 0
)
```

### 3.4 Structure d'un poste selon son type

**Type SOLUTION (Partage de Solution)**

```
Post
├── title           : Titre de la solution
├── group_id        : Groupe concerné (obligatoire)
├── domain_id       : Domaine du groupe (ou NULL = "Autre")
└── PostSections
    ├── section_type = PROBLEM_DESC  : Description du problème résolu
    ├── section_type = CONTEXT       : Contexte / environnement
    ├── section_type = SOLUTION_DESC : La solution détaillée
    └── section_type = RESULT        : Résultat / validation
└── PostHashtags    : Tags pour la recherche
```

**Type PROBLEM (Poser un Problème)**

```
Post
├── title           : Titre du problème
├── group_id        : Groupe concerné
├── domain_id       : Domaine (ou "Autre")
└── PostSections
    ├── section_type = PROBLEM_DESC  : Description détaillée du problème
    └── section_type = CONTEXT       : Contexte / ce qui a été essayé
└── PostHashtags    : Tags pour la recherche
```

**Type NEWS**

```
Post
├── title           : Titre de l'actualité
└── PostSections
    └── section_type = CONTENT : Corps de l'article (rich text)
└── PostMedia       : Images optionnelles
```

**Type QUIZ**

```
Post
├── title           : Intitulé du quiz
└── QuizQuestions   (table séparée, voir ci-dessous)
```

**Type SURVEY (Sondage)**

```
Post
├── title           : Question du sondage
└── SurveyOptions   (table séparée, voir ci-dessous)
```

### 3.5 Quiz & Sondage

```sql
QuizQuestions (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  post_id         UUID REFERENCES Posts(id) ON DELETE CASCADE,
  question_text   TEXT NOT NULL,
  order_index     INTEGER DEFAULT 0
)

QuizOptions (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  question_id     UUID REFERENCES QuizQuestions(id) ON DELETE CASCADE,
  option_text     TEXT NOT NULL,
  is_correct      BOOLEAN DEFAULT FALSE,
  order_index     INTEGER DEFAULT 0
)

QuizResponses (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  post_id         UUID REFERENCES Posts(id),
  user_id         VARCHAR(20) REFERENCES Employees(id_user),
  question_id     UUID REFERENCES QuizQuestions(id),
  selected_option UUID REFERENCES QuizOptions(id),
  answered_at     TIMESTAMP DEFAULT NOW()
)

SurveyOptions (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  post_id         UUID REFERENCES Posts(id) ON DELETE CASCADE,
  option_text     TEXT NOT NULL,
  order_index     INTEGER DEFAULT 0
)

SurveyVotes (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  post_id         UUID REFERENCES Posts(id),
  option_id       UUID REFERENCES SurveyOptions(id),
  user_id         VARCHAR(20) REFERENCES Employees(id_user),
  voted_at        TIMESTAMP DEFAULT NOW(),
  UNIQUE(post_id, user_id)  -- Un seul vote par sondage
)
```

### 3.6 Commentaires & Votes

```sql
Comments (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  post_id         UUID REFERENCES Posts(id) ON DELETE CASCADE,
  parent_id       UUID REFERENCES Comments(id) NULLABLE,
  -- NULL = commentaire racine, sinon = réponse imbriquée
  author_id       VARCHAR(20) REFERENCES Employees(id_user),
  content         TEXT NOT NULL,
  is_best_answer  BOOLEAN DEFAULT FALSE,
  -- Marqué par l'auteur du poste comme meilleure réponse
  status          VARCHAR(20) DEFAULT 'ACTIVE',
  created_at      TIMESTAMP DEFAULT NOW(),
  updated_at      TIMESTAMP DEFAULT NOW()
)

CommentVotes (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  comment_id      UUID REFERENCES Comments(id) ON DELETE CASCADE,
  user_id         VARCHAR(20) REFERENCES Employees(id_user),
  vote_type       SMALLINT NOT NULL CHECK (vote_type IN (1, -1)),
  -- 1 = upvote, -1 = downvote
  voted_at        TIMESTAMP DEFAULT NOW(),
  UNIQUE(comment_id, user_id)  -- Un seul vote par commentaire par user
)

-- Vue calculée du score d'un commentaire
-- score = SUM(vote_type) de CommentVotes WHERE comment_id = ?
```

---

## 4. Architecture du Projet (5 Projets)

### 4.1 Structure des solutions

```
KnowledgeHub.sln
├── Server/                   (ASP.NET Core 8 Web API)
├── ServerLibrary/            (Class Library — logique métier)
├── BaseLibrary/              (Class Library — partagé par tous)
├── ClientLibrary/            (Class Library — services client)
└── Client/                   (Blazor WebAssembly)
```

### 4.2 BaseLibrary

```
BaseLibrary/
├── Entities/
│   ├── Base/
│   │   └── BaseEntity.cs           (Id, CreatedAt, UpdatedAt, DeletedAt)
│   ├── Auth/
│   │   └── UserProfile.cs
│   ├── Groups/
│   │   ├── Group.cs
│   │   ├── Domain.cs
│   │   └── GroupMember.cs
│   ├── Posts/
│   │   ├── Post.cs
│   │   ├── PostSection.cs
│   │   ├── PostHashtag.cs
│   │   ├── PostMedia.cs
│   │   ├── PostType.cs
│   │   ├── Comment.cs
│   │   └── CommentVote.cs
│   ├── Quiz/
│   │   ├── QuizQuestion.cs
│   │   ├── QuizOption.cs
│   │   └── QuizResponse.cs
│   └── Survey/
│       ├── SurveyOption.cs
│       └── SurveyVote.cs
├── DTOs/
│   ├── Auth/
│   │   ├── LoginRequestDto.cs
│   │   ├── LoginResponseDto.cs
│   │   └── UserProfileDto.cs
│   ├── Groups/
│   │   ├── CreateGroupDto.cs
│   │   ├── GroupDto.cs
│   │   └── GroupSummaryDto.cs
│   ├── Posts/
│   │   ├── CreateSolutionPostDto.cs
│   │   ├── CreateProblemPostDto.cs
│   │   ├── CreateNewsPostDto.cs
│   │   ├── CreateQuizDto.cs
│   │   ├── CreateSurveyDto.cs
│   │   ├── PostDetailDto.cs
│   │   └── PostSummaryDto.cs
│   ├── Comments/
│   │   ├── CreateCommentDto.cs
│   │   ├── CommentDto.cs
│   │   └── VoteDto.cs
│   └── Search/
│       ├── SearchRequestDto.cs
│       └── SearchResultDto.cs
├── Helpers/
│   ├── ApiResponse.cs              (wrapper générique: Success, Data, Message, Errors)
│   ├── PaginatedResponse.cs        (Data[], Total, Page, PageSize)
│   ├── Constants.cs
│   ├── Enums.cs                    (PostTypeCode, UserRole, VoteType, PostStatus, SectionType)
│   ├── JwtSettings.cs
│   └── AutoMapperProfile.cs
└── Interfaces/
    └── IExternalDataService.cs     (lecture tables Employees + Structures)
```

### 4.3 ServerLibrary

```
ServerLibrary/
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── ExternalDbContext.cs         (readonly context pour tables Employees/Structures)
│   ├── Configurations/
│   │   ├── GroupConfiguration.cs
│   │   ├── PostConfiguration.cs
│   │   └── CommentConfiguration.cs
│   ├── Migrations/
│   └── Seeders/
│       ├── PostTypeSeeder.cs
│       └── AdminUserSeeder.cs
├── Repositories/
│   ├── Interfaces/
│   │   ├── IGenericRepository.cs
│   │   ├── IGroupRepository.cs
│   │   ├── IPostRepository.cs
│   │   ├── ICommentRepository.cs
│   │   └── ISearchRepository.cs
│   └── Implementations/
│       ├── GenericRepository.cs
│       ├── GroupRepository.cs
│       ├── PostRepository.cs
│       ├── CommentRepository.cs
│       └── SearchRepository.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IGroupService.cs
│   │   ├── IPostService.cs
│   │   ├── ICommentService.cs
│   │   ├── ISearchService.cs
│   │   └── IFileService.cs
│   └── Implementations/
│       ├── AuthService.cs
│       ├── GroupService.cs
│       ├── PostService.cs
│       ├── CommentService.cs
│       ├── SearchService.cs
│       └── FileService.cs
└── Handlers/
    ├── ExceptionHandler.cs
    ├── PaginationHandler.cs
    └── ValidationHandler.cs
```

### 4.4 Server (API)

```
Server/
├── Controllers/
│   ├── AuthController.cs
│   ├── GroupsController.cs
│   ├── PostsController.cs
│   ├── CommentsController.cs
│   ├── SearchController.cs
│   ├── ProfileController.cs
│   └── AdminController.cs          (gestion types de postes, config)
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   └── JwtAuthMiddleware.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
├── Program.cs
└── appsettings.json
```

### 4.5 ClientLibrary

```
ClientLibrary/
├── Helpers/
│   ├── JwtHelper.cs
│   ├── HttpClientHelper.cs
│   └── AuthStateProvider.cs        (Blazor AuthenticationStateProvider)
└── Services/
    ├── Interfaces/
    │   ├── IAuthClientService.cs
    │   ├── IGroupClientService.cs
    │   ├── IPostClientService.cs
    │   ├── ICommentClientService.cs
    │   └── ISearchClientService.cs
    └── Implementations/
        ├── AuthClientService.cs
        ├── GroupClientService.cs
        ├── PostClientService.cs
        ├── CommentClientService.cs
        └── SearchClientService.cs
```

### 4.6 Client (Blazor WebAssembly)

```
Client/
└── wwwroot/
└── Pages/
    ├── Auth/
    │   └── Login.razor
    ├── Home/
    │   └── Index.razor             (Feed global avec postes récents)
    ├── Groups/
    │   ├── GroupList.razor         (liste des groupes disponibles)
    │   ├── GroupDetail.razor       (postes d'un groupe)
    │   └── GroupAdmin.razor        (SuperAdmin: créer/éditer groupe)
    ├── Posts/
    │   ├── PostDetail.razor        (détail d'un poste + commentaires)
    │   ├── CreatePost.razor        (formulaire dynamique selon type)
    │   └── EditPost.razor
    ├── Search/
    │   └── SearchResults.razor
    ├── Profile/
    │   └── UserProfile.razor
    └── Admin/
        ├── PostTypeConfig.razor    (activer/désactiver, toggle commentaires)
        └── UserManagement.razor
└── Shared/
    ├── MainLayout.razor
    ├── NavMenu.razor
    ├── PostCard.razor              (card réutilisable dans le feed)
    ├── CommentThread.razor         (commentaires imbriqués + votes)
    ├── HashtagInput.razor          (input avec autocomplete de tags)
    └── SearchBar.razor
└── Components/
    ├── PostSections/
    │   ├── SolutionForm.razor
    │   ├── ProblemForm.razor
    │   ├── NewsForm.razor
    │   ├── QuizForm.razor
    │   └── SurveyForm.razor
    └── Votes/
        └── VoteButtons.razor
```

---

## 5. Endpoints API

### Auth

| Méthode | Route | Description | Rôle |
|---------|-------|-------------|------|
| POST | `/api/auth/login` | Login simulé SSO, retourne JWT | Public |
| POST | `/api/auth/refresh` | Refresh token | Authentifié |
| GET | `/api/auth/me` | Infos utilisateur courant | Authentifié |

### Groupes

| Méthode | Route | Description | Rôle |
|---------|-------|-------------|------|
| GET | `/api/groups` | Liste tous les groupes actifs | Authentifié |
| POST | `/api/groups` | Créer un groupe | SuperAdmin |
| PUT | `/api/groups/{id}` | Modifier un groupe | SuperAdmin |
| DELETE | `/api/groups/{id}` | Désactiver un groupe | SuperAdmin |
| POST | `/api/groups/{id}/join` | Rejoindre un groupe | User |
| POST | `/api/groups/{id}/leave` | Quitter un groupe | User |
| GET | `/api/groups/{id}/domains` | Lister les domaines d'un groupe | Authentifié |
| POST | `/api/groups/{id}/domains` | Ajouter un domaine | SuperAdmin |

### Postes

| Méthode | Route | Description | Rôle |
|---------|-------|-------------|------|
| GET | `/api/posts` | Feed paginé (filtrable) | Authentifié |
| GET | `/api/posts/{id}` | Détail d'un poste | Authentifié |
| POST | `/api/posts/solution` | Créer un poste Solution | User |
| POST | `/api/posts/problem` | Créer un poste Problème | User |
| POST | `/api/posts/news` | Créer un poste News | ContentManager |
| POST | `/api/posts/quiz` | Créer un Quiz | ContentManager |
| POST | `/api/posts/survey` | Créer un Sondage | ContentManager |
| PUT | `/api/posts/{id}` | Modifier un poste | Auteur / Admin |
| DELETE | `/api/posts/{id}` | Supprimer un poste | Auteur / Admin |
| POST | `/api/posts/{id}/view` | Incrémenter vues | Authentifié |

### Commentaires

| Méthode | Route | Description | Rôle |
|---------|-------|-------------|------|
| GET | `/api/posts/{id}/comments` | Commentaires d'un poste | Authentifié |
| POST | `/api/posts/{id}/comments` | Ajouter un commentaire | User |
| PUT | `/api/comments/{id}` | Modifier un commentaire | Auteur |
| DELETE | `/api/comments/{id}` | Supprimer un commentaire | Auteur / Admin |
| POST | `/api/comments/{id}/vote` | Voter pour un commentaire (body: `{voteType: 1 ou -1}`) | User |
| POST | `/api/comments/{id}/best` | Marquer comme meilleure réponse | Auteur du poste |

### Recherche

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/search?q=&type=&groupId=&domainId=&hashtag=&structureId=&authorName=&dateFrom=&dateTo=&page=&pageSize=` | Recherche multi-critères |
| GET | `/api/search/hashtags?q=` | Autocomplete hashtags |

### Profil

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/profile/{userId}` | Profil public d'un utilisateur |
| PUT | `/api/profile` | Mettre à jour son profil (téléphone, bio, avatar) |

### Admin

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/admin/post-types` | Lister les types de postes |
| PUT | `/api/admin/post-types/{id}` | Configurer (allow_comments, is_active) |

---

## 6. Logique de Recherche

La recherche multi-critères est construite dynamiquement côté `SearchRepository` :

```csharp
// Paramètres de recherche
public class SearchRequestDto
{
    public string? Q { get; set; }           // Recherche texte libre (titre + sections)
    public string? Hashtag { get; set; }     // Ex: #aspnetcore
    public Guid? GroupId { get; set; }
    public Guid? DomainId { get; set; }
    public string? StructureId { get; set; } // id_structure de l'auteur
    public string? AuthorName { get; set; }  // nom ou prénom
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? PostTypeCode { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
```

**Critères de recherche supportés :**

| Critère | Table(s) joinées |
|---------|-----------------|
| Texte libre | Posts.title + PostSections.content |
| Hashtag | PostHashtags.tag |
| Groupe | Posts.group_id → Groups.name |
| Domaine | Posts.domain_id → Domains.name |
| Structure de l'auteur | Posts.author_id → Employees.id_structure → Structures.structure_name |
| Nom/prénom de l'auteur | Posts.author_id → Employees.nom, Employees.prenom |
| Date | Posts.created_at |

---

## 7. Sécurité & Autorisation

### 7.1 JWT Claims

```json
{
  "sub": "EMP001234",
  "id_structure": "STR042",
  "roles": ["User"],
  "exp": 1700000000,
  "iss": "KnowledgeHub",
  "aud": "KnowledgeHub"
}
```

### 7.2 Policies dans Program.cs

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminOnly",
        p => p.RequireRole("SuperAdmin"));
    options.AddPolicy("ContentManager",
        p => p.RequireRole("SuperAdmin", "ContentManager"));
    options.AddPolicy("AuthenticatedUser",
        p => p.RequireAuthenticatedUser());
});
```

### 7.3 Règles d'accès

- Un user ne peut commenter que si `PostType.allow_comments = true`
- Un user ne peut créer NEWS/QUIZ/SURVEY que s'il a le rôle `ContentManager` ou `SuperAdmin`
- Un user ne peut rejoindre un groupe que s'il est actif
- L'auteur d'un poste peut marquer **une seule** réponse comme "best answer"
- Un user ne vote qu'une fois par commentaire (UPSERT : re-voter change le vote)

---

## 8. Profil Utilisateur

La page de profil affiche la fusion entre les données externes (lecture seule) et les données applicatives :

```
┌────────────────────────────────────────┐
│  [Avatar]  Nom Prénom                  │
│            Grade : Ingénieur Principal │
│            Structure : DSI - Sous-dir  │
│            Téléphone : 0555...         │
│            Bio : ...                   │
├────────────────────────────────────────┤
│  Groupes rejoints : [IT] [RH] [Sécu]  │
│  Postes publiés : 24                   │
│  Solutions partagées : 15              │
└────────────────────────────────────────┘
```

---

## 9. Interface Blazor — Pages Clés

### 9.1 Feed Principal (Index.razor)

- Liste des postes paginés, filtrables par groupe / type
- Card par poste : titre, auteur + structure, groupe, domaine, date, nb commentaires, hashtags
- Bouton "Rejoindre le groupe" si l'utilisateur n'est pas membre
- Barre de recherche globale en haut

### 9.2 Création de Poste (CreatePost.razor)

Formulaire **dynamique** selon le type sélectionné :

```
Étape 1 : Choisir le groupe
Étape 2 : Choisir le type de poste (uniquement les types auxquels l'utilisateur a droit)
Étape 3 : Formulaire adapté au type
          → SOLUTION : Titre + Problème rencontré + Contexte + Solution + Résultat + Hashtags
          → PROBLEM  : Titre + Description du problème + Ce qui a été essayé + Hashtags
          → NEWS     : Titre + Corps de l'article + Images
          → QUIZ     : Titre + Questions + Options (cocher la bonne réponse)
          → SURVEY   : Question principale + Options de réponse
```

### 9.3 Détail d'un Poste (PostDetail.razor)

- Affichage des sections structurées selon le type
- Hashtags cliquables (→ recherche par hashtag)
- Section commentaires (si `allow_comments = true`) :
  - Arbre de commentaires (2 niveaux : commentaire + réponses)
  - Flèches ▲ ▼ avec score visible
  - Badge "✓ Meilleure Réponse" sur le commentaire sélectionné
  - Pour QUIZ : affichage des résultats après réponse
  - Pour SONDAGE : barre de progression par option après vote

---

## 10. Configuration & Déploiement

### 10.1 appsettings.json

```json
{
  "ConnectionStrings": {
    "AppDb": "Host=...;Database=knowledge_hub;Username=...;Password=...",
    "ExternalDb": "Host=...;Database=enterprise_db;Username=...;Password=..."
  },
  "JwtSettings": {
    "SecretKey": "...",
    "Issuer": "KnowledgeHub",
    "Audience": "KnowledgeHub",
    "ExpiryMinutes": 480,
    "RefreshExpiryDays": 7
  },
  "FileStorage": {
    "BasePath": "/var/khub/uploads",
    "MaxFileSizeMb": 10,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".pdf"]
  }
}
```

### 10.2 Program.cs — Points clés

```csharp
// Deux DbContext
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("AppDb")));
builder.Services.AddDbContext<ExternalDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("ExternalDb"))
       .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
       // ReadOnly, pas de migrations sur ce contexte

// CORS pour Blazor WASM
builder.Services.AddCors(opt => opt.AddPolicy("BlazorClient",
    p => p.WithOrigins("https://localhost:7200").AllowAnyHeader().AllowAnyMethod()));

// Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(/* ... */);
builder.Services.AddAuthorization(/* policies */);

// AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// Services
builder.Services.AddScoped<IPostService, PostService>();
// ...
```

---

## 11. Roadmap d'Implémentation (avec Agent IA)

L'implémentation est découpée en **sprints** logiques pour travailler efficacement avec un agent IA :

### Sprint 1 — Fondations
1. Créer la solution et les 5 projets
2. Implémenter `BaseLibrary` : Entities, Enums, ApiResponse, PaginatedResponse
3. Configurer `ApplicationDbContext` + `ExternalDbContext`
4. Générer les migrations initiales
5. Implémenter le `PostTypeSeeder`

### Sprint 2 — Authentification
1. Implémenter `AuthService` (login simulé + lecture tables externes)
2. Implémenter `AuthController`
3. Implémenter `AuthStateProvider` côté Blazor
4. Page `Login.razor`

### Sprint 3 — Groupes & Domaines
1. CRUD Groupes (SuperAdmin)
2. Rejoindre/quitter un groupe
3. CRUD Domaines
4. Page `GroupList.razor` + `GroupDetail.razor`

### Sprint 4 — Postes (SOLUTION + PROBLEM)
1. `PostRepository` + `PostService`
2. Endpoints création Solution / Problème
3. `CreatePost.razor` (formulaire dynamique)
4. `PostDetail.razor`

### Sprint 5 — Commentaires & Votes
1. `CommentRepository` + `CommentService`
2. Endpoints commentaires + votes
3. `CommentThread.razor` avec votes et best answer

### Sprint 6 — Contenu Privilégié (NEWS, QUIZ, SURVEY)
1. Endpoints NEWS / QUIZ / SURVEY
2. Forms dédiés
3. Affichage résultats Quiz/Sondage

### Sprint 7 — Recherche
1. `SearchRepository` (requête dynamique multi-critères)
2. Endpoint `/api/search`
3. Autocomplete hashtags
4. `SearchResults.razor`

### Sprint 8 — Profil & Admin
1. Page `UserProfile.razor`
2. `AdminController` + `PostTypeConfig.razor`
3. Gestion rôles

---

## 12. Points d'Attention pour l'Agent IA

- **ExternalDbContext** : ne jamais appeler `SaveChanges()` sur ce context ; il est strictement en lecture
- **Commentaires imbriqués** : limiter à 2 niveaux maximum pour éviter la complexité récursive en Blazor
- **Vote upsert** : `CommentVote` doit faire un UPSERT (INSERT OR UPDATE) — un seul enregistrement par (comment_id, user_id)
- **PostType.allow_comments** : vérifier côté API avant d'accepter un `POST /api/posts/{id}/comments`
- **Autorisation création de poste** : vérifier `PostType.requires_privilege` côté API controller
- **domain_id nullable** : si l'utilisateur choisit "Autre", `domain_id = null` — ne pas rejeter côté validation
- **ExternalDb migrations** : ne pas inclure les tables `Employees` et `Structures` dans les migrations EF Core

---

*Document généré pour démarrage du projet KnowledgeHub — Version 1.0*
