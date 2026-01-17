# SkillSync 🚀

> AI-Powered Skills Tracking and Learning Recommendation Platform

SkillSync is a modern full-stack web application that helps developers and professionals track their learning journey, get AI-powered recommendations, and automatically sync their GitHub activity to discover new skills.

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)
![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-blue.svg)

## 🌟 Features

### Core Features
- **📊 Skills Management** - Track and manage your technical skills with proficiency levels
- **📝 Activity Tracking** - Log learning activities (courses, projects, practice sessions)
- **🎯 Milestone Planning** - Set and track learning goals with deadlines
- **📈 Analytics Dashboard** - Visualize progress with interactive charts
- **🤖 AI Recommendations** - Get personalized learning paths powered by Google Gemini
- **🔗 GitHub Integration** - Automatically sync repositories and detect programming languages
- **📧 Email Notifications** - Weekly summaries and milestone reminders
- **⏰ Background Jobs** - Automated data synchronization with Hangfire

### Technical Highlights
- **Modern Architecture** - Clean, maintainable codebase following best practices
- **Responsive Design** - Beautiful UI built with TailwindCSS and MudBlazor
- **Real-time Updates** - Dynamic charts and live data visualization with ApexCharts
- **Secure Authentication** - JWT-based auth with refresh tokens
- **Comprehensive Testing** - Unit and integration tests with automated CI/CD
- **API Documentation** - Interactive documentation with Scalar

---

## 🏗️ Architecture

### Technology Stack

**Backend:**
- ASP.NET Core 10 Web API
- Entity Framework Core 10
- SQL Server
- Hangfire (Background Jobs)
- Google Gemini AI (gemini-2.5-flash-lite)
- JWT Authentication
- Serilog (Logging)
- FluentValidation

**Frontend:**
- Blazor WebAssembly
- TailwindCSS
- MudBlazor
- Blazor-ApexCharts
- Blazored LocalStorage
- Markdig (Markdown rendering)

**Infrastructure:**
- GitHub Actions (CI/CD)
- xUnit (Testing)
- Moq & FluentAssertions
- Octokit (GitHub API)

### Project Structure
```
SkillSync/
├── SkillSync.Api/              # Backend Web API
│   ├── Controllers/            # API endpoints
│   ├── Services/              # Business logic
│   │   ├── Activities/        # Activity management
│   │   ├── AI/                # AI recommendation service
│   │   ├── Analytics/         # Analytics & reporting
│   │   ├── Auth/              # Authentication & authorization
│   │   ├── BackgroundJobs/    # Hangfire background jobs
│   │   ├── GitHub/            # GitHub integration
│   │   ├── Milestones/        # Milestone management
│   │   ├── Notifications/     # Email notifications
│   │   └── Skills/            # Skills management
│   ├── Data/                  # Database entities & DbContext
│   ├── DTOs/                  # Data transfer objects
│   ├── Validators/            # FluentValidation rules
│   └── Migrations/            # EF Core migrations
├── SkillSync.Web/             # Blazor WebAssembly Frontend
│   ├── Pages/                 # Razor pages
│   ├── Services/              # API client services
│   ├── Models/                # Frontend models
│   ├── Auth/                  # Authentication providers
│   ├── Layout/                # Layout components
│   └── wwwroot/               # Static assets
├── Tests/
│   ├── SkillSync.Tests.Unit/          # Unit tests
│   └── SkillSync.Tests.Integration/   # Integration tests
└── .github/
    └── workflows/             # CI/CD pipelines
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) or SQL Server LocalDB
- [Node.js](https://nodejs.org/) (for TailwindCSS)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/bykeny/SkillSync.git
   cd SkillSync
   ```

2. **Configure Database**
   
   Update connection string in `SkillSync.Api/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SkillSyncDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
     }
   }
   ```

3. **Configure API Keys**
   
   Add your API keys to `SkillSync.Api/appsettings.json`:
   ```json
   {
     "Gemini": {
       "ApiKey": "your-gemini-api-key",
       "Model": "gemini-2.5-flash-lite"
     },
     "GitHub": {
       "ClientId": "your-github-client-id",
       "ClientSecret": "your-github-client-secret",
       "RedirectUri": "https://localhost:7065/api/github/callback"
     },
     "Email": {
       "SmtpHost": "smtp.gmail.com",
       "SmtpPort": 587,
       "SmtpUsername": "your-email@gmail.com",
       "SmtpPassword": "your-app-password",
       "FromEmail": "skillsync@gmail.com",
       "FromName": "SkillSync"
     },
     "JwtSettings": {
       "Secret": "your-jwt-secret-key-min-32-chars",
       "Issuer": "SkillSyncAPI",
       "Audience": "SkillSyncWeb"
     }
   }
   ```

   > 💡 **Tip:** Use User Secrets for development: `dotnet user-secrets set "Gemini:ApiKey" "your-key"`

4. **Apply Database Migrations**
   ```bash
   cd SkillSync.Api
   dotnet ef database update
   ```

5. **Build TailwindCSS**
   ```bash
   cd ../SkillSync.Web
   npm install
   npm run css:build
   ```

6. **Run the Application**
   
   In Visual Studio:
   - Set multiple startup projects (SkillSync.Api + SkillSync.Web)
   - Press F5
   
   Or via command line:
   ```bash
   # Terminal 1 - API
   cd SkillSync.Api
   dotnet run
   
   # Terminal 2 - Web
   cd SkillSync.Web
   dotnet run
   ```

7. **Access the Application**
   - Frontend: https://localhost:7074
   - API: https://localhost:7065/scalar/v1 (Scalar API Docs)
   - Hangfire Dashboard: https://localhost:7065/hangfire

---

## 📖 Usage Guide

### Getting Started

1. **Register an Account**
   - Navigate to https://localhost:7074
   - Click "Sign up" and create your account

2. **Add Your First Skill**
   - Go to Skills page
   - Click "Add Skill"
   - Enter skill details (name, proficiency level, target level)
   - Select a category

3. **Track Activities**
   - Navigate to Activities
   - Log learning activities (courses, projects, practice)
   - Link activities to skills

4. **Get AI Recommendations**
   - Go to Recommendations
   - Click "Learning Path" and select a skill
   - Get personalized step-by-step guidance powered by Google Gemini
   - Generate weekly schedules and gap analysis

5. **Connect GitHub**
   - Navigate to GitHub page
   - Click "Connect with GitHub"
   - Authorize the app
   - View your repositories and language stats
   - Skills are auto-created from detected languages

6. **View Analytics**
   - Check the Analytics dashboard
   - View progress charts, skill radar, and activity timeline
   - Filter by date range (7, 14, 30, or 90 days)

---

## 🧪 Testing

### Run All Tests
```bash
dotnet test
```

### Run Unit Tests Only
```bash
dotnet test Tests/SkillSync.Tests.Unit
```

### Run Integration Tests Only
```bash
dotnet test Tests/SkillSync.Tests.Integration
```

### Generate Code Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## 🔧 Configuration

### Environment Variables

For production deployment, use environment variables instead of appsettings.json:

```bash
ConnectionStrings__DefaultConnection="your-connection-string"
JwtSettings__Secret="your-jwt-secret"
JwtSettings__Issuer="SkillSyncAPI"
JwtSettings__Audience="SkillSyncWeb"
Gemini__ApiKey="your-gemini-key"
Gemini__Model="gemini-2.5-flash-lite"
GitHub__ClientId="your-github-client-id"
GitHub__ClientSecret="your-github-client-secret"
Email__SmtpHost="smtp.gmail.com"
Email__SmtpPort="587"
Email__SmtpUsername="your-email@gmail.com"
Email__SmtpPassword="your-app-password"
```

### JWT Configuration

The application uses JWT with refresh tokens:
- Access Token: 15 minutes
- Refresh Token: 7 days
- Tokens are stored in LocalStorage (frontend)
- Refresh tokens are stored in database with rotation

### Background Jobs

Scheduled jobs run automatically via Hangfire:
- **Weekly Summary**: Every Monday at 9 AM
- **Milestone Reminders**: Daily at 8 AM
- **GitHub Sync**: Daily at 2 AM

You can monitor and manage jobs at https://localhost:7065/hangfire

---

## 📊 API Documentation

### Authentication Endpoints

#### Register
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "confirmPassword": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh-token-here",
  "expiresIn": 900
}
```

#### Refresh Token
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "your-refresh-token"
}
```

### Skills Endpoints

All skills endpoints require authentication (Bearer token).

#### Get All Skills
```http
GET /api/skills
Authorization: Bearer {access_token}
```

#### Create Skill
```http
POST /api/skills
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "name": "C# Programming",
  "description": "Learning C# and .NET",
  "proficiencyLevel": 3,
  "targetLevel": 5,
  "categoryId": 1
}
```

#### Update Skill
```http
PUT /api/skills/{id}
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "name": "C# Programming",
  "proficiencyLevel": 4,
  "targetLevel": 5,
  "isActive": true
}
```

### AI Recommendations

#### Get Learning Path
```http
POST /api/recommendations/learning-path/{skillId}
Authorization: Bearer {access_token}
```

Returns a personalized learning path with steps, resources, and timeline.

For complete API documentation, visit https://localhost:7065/scalar/v1 when running the application.

---

## 🚀 Deployment

### Azure App Service

1. **Create Azure Resources**
   ```bash
   az group create --name SkillSyncRG --location eastus
   az appservice plan create --name SkillSyncPlan --resource-group SkillSyncRG --sku B1
   az webapp create --name skillsync-api --resource-group SkillSyncRG --plan SkillSyncPlan --runtime "DOTNET|10.0"
   ```

2. **Deploy API**
   ```bash
   cd SkillSync.Api
   dotnet publish -c Release
   az webapp deploy --resource-group SkillSyncRG --name skillsync-api --src-path bin/Release/net10.0/publish
   ```

3. **Configure Environment Variables**
   ```bash
   az webapp config appsettings set --resource-group SkillSyncRG --name skillsync-api --settings \
     ConnectionStrings__DefaultConnection="your-connection-string" \
     JwtSettings__Secret="your-jwt-secret" \
     Gemini__ApiKey="your-gemini-key"
   ```

### Docker (Optional)

```dockerfile
# Dockerfile for API
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["SkillSync.Api/SkillSync.Api.csproj", "SkillSync.Api/"]
RUN dotnet restore "SkillSync.Api/SkillSync.Api.csproj"
COPY . .
WORKDIR "/src/SkillSync.Api"
RUN dotnet build "SkillSync.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SkillSync.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SkillSync.Api.dll"]
```

---

## 🔒 Security

### Implemented Security Measures

- ✅ JWT authentication with refresh tokens
- ✅ Password hashing with ASP.NET Identity
- ✅ HTTPS enforcement
- ✅ CORS configuration
- ✅ SQL injection prevention (EF Core parameterized queries)
- ✅ XSS protection (Blazor auto-escaping)
- ✅ Input validation with FluentValidation
- ✅ User Secrets support for local development
- ✅ Rate limiting on authentication endpoints

### Security Best Practices

- Never commit API keys or secrets to version control
- Use User Secrets during development: `dotnet user-secrets`
- Use environment variables in production
- Rotate JWT secrets regularly
- Enable 2FA for GitHub OAuth
- Keep dependencies updated
- Review security advisories regularly

---

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Coding Standards

- Follow C# coding conventions
- Write unit tests for new features (aim for 70%+ coverage)
- Use FluentValidation for input validation
- Update documentation
- Keep commits atomic and descriptive
- Ensure all tests pass before submitting PR

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

Copyright (c) 2026 Kanan Ramazanov

---

## 🙏 Acknowledgments

- **Google Gemini** - AI-powered recommendations using gemini-2.5-flash-lite
- **GitHub** - OAuth and API integration via Octokit
- **TailwindCSS** - Beautiful, responsive styling
- **MudBlazor** - Comprehensive Blazor component library
- **ApexCharts** - Interactive data visualization
- **Hangfire** - Reliable background job processing
- **Scalar** - Modern API documentation

---

## 📧 Contact

**Kanan Ramazanov**

Project Link: [https://github.com/bykeny/SkillSync](https://github.com/bykeny/SkillSync)

---

## 🗺️ Roadmap

- [x] Core skills and activity tracking
- [x] AI-powered recommendations with Google Gemini
- [x] GitHub integration with Octokit
- [x] Analytics dashboard with ApexCharts
- [x] Background jobs and email notifications
- [x] CI/CD pipeline with GitHub Actions
- [ ] Mobile app (React Native or .NET MAUI)
- [ ] Social features (share progress, follow others)
- [ ] Gamification (badges, achievements, streaks)
- [ ] Team collaboration features
- [ ] Learning resource marketplace
- [ ] Browser extension for quick activity logging
- [ ] Export/import functionality
- [ ] Dark mode theme support

---

**Built with ❤️ using .NET 10 and Blazor WebAssembly**
