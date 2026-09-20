# 🧠 KnowledgeHub (Mini-Reddit)

A complete and modern platform acting as a **Mini-Reddit**, designed to foster collaboration, knowledge sharing, and problem-solving within an organization.

![KnowledgeHub Logo](./Client/wwwroot/images/logo-reddit.png) <!-- Adjust the path to your logo if necessary -->

## ✨ Key Features

### 👥 User Features
*   **Diverse Content:** Share your knowledge through various interactive post types:
    *   **Problems (Questions) & Solutions:** Ask questions and propose validated solutions.
    *   **News:** Share important updates (with image upload support).
    *   **Quizzes & Surveys:** Test knowledge and gather community feedback.
*   **Engagement System (Gamification):**
    *   Comment, interact, and participate in discussions.
    *   Rate the best content (Star Rating).
*   **Discussion Groups:** Join specific groups to filter your news feed and only see relevant content (Integrated search privacy).
*   **Customizable Profiles:** Edit your biography, add contact information, and keep a precise track of your contributions.
*   **Multilingual Support:** Fully translated interface, instantly switchable between **Arabic (RTL)** and **French (LTR)**.
*   **Premium "Glassmorphism" Interface:** A dark, ultra-modern, and fluid design with polished animations for an optimal User Experience (UX).

### 🛡️ Administrator Features (SuperAdmin & Moderators)
*   **Analytics Dashboard:** A comprehensive dashboard displaying real-time Key Performance Indicators (KPIs) (Total posts, registered users, comment volume).
*   **Rankings & Detection:**
    *   Identify top contributors (Top Ratings, Solutions Champions).
    *   Automatically detect problematic behavior (Poorly rated authors, abusive reports).
*   **Advanced Moderation:** Manage reports to keep the platform healthy.
*   **Pin Posts:** Highlight crucial announcements and strategic news by pinning them to the top of the news feed.
*   **Group and Content Management:** Full creation and administration of groups and permissions.

## 🛠️ Technical Stack

*   **Frontend:** [Blazor WebAssembly](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor) (C# / .NET 8)
*   **Backend:** ASP.NET Core 8 Web API
*   **Database:** SQLite (via Entity Framework Core)
*   **Architecture:** Clean Architecture solution separated into multiple projects (`Client`, `Server`, `BaseLibrary`, `ServerLibrary`, `ClientLibrary`).

## 🚀 Installation and Launch

### Prerequisites
*   [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) installed on your machine.

### Steps to run the project locally
1.  **Clone the repository:**
    ```bash
    git clone <your-github-url>
    cd OurProject
    ```

2.  **Run the Server (Backend & Database):**
    The SQLite database will be created automatically on the first launch (via EF Core migrations).
    ```bash
    cd Server
    dotnet run
    ```

3.  **Run the Client (Blazor Frontend):**
    Open a new terminal and run the client.
    ```bash
    cd Client
    dotnet run
    ```
4.  Open your browser to the address indicated by the `Client` terminal (e.g., `http://localhost:5168` or the configured address).

## 💡 Perspectives (Future Roadmap)
*   **AI Recommendation System:** Suggest relevant posts based on user history.
*   **Real-time Notifications:** SignalR integration for instant alerts (new comments, mentions).
*   **Integrated AI Assistant:** Summarize long discussion threads or automatically suggest solutions.

---
*Developed with passion for knowledge exchange.*
