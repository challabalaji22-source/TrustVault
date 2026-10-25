# TrustVault — Secure Document Storage and Sharing

TrustVault is a secure document management application built with Angular and ASP.NET Core. It allows users to upload, download, share, and revoke access to documents, with audit history for tracking document-related activities.

## Features

- User authentication and sign-in
- Secure document upload and download
- Share documents with authorized users
- Revoke document access
- Shared With Me view
- Audit history for document activities
- PostgreSQL database integration
- Angular frontend and ASP.NET Core backend

## Technology Stack

- **Frontend:** Angular, TypeScript, HTML, CSS
- **Backend:** ASP.NET Core Web API, C#
- **Database:** PostgreSQL
- **ORM:** Entity Framework Core
- **Version Control:** Git and GitHub

## Project Structure

```text
TrustVault/
├── backend/
│   └── TrustVault.Api/
│       ├── Data/
│       ├── Domain/
│       ├── Endpoints/
│       ├── Migrations/
│       ├── Models/
│       ├── Services/
│       └── Storage/
├── frontend/
│   └── src/
└── TrustVault.slnx
```

## Getting Started

### Prerequisites

Install the following tools:

- .NET SDK compatible with the backend project
- Node.js and npm
- Angular CLI compatible with the frontend project
- PostgreSQL
- Visual Studio Code (recommended)

### Backend Setup

1. Open a terminal in the project root.
2. Navigate to the backend project:

   ```powershell
   cd backend/TrustVault.Api
   ```

3. Configure the PostgreSQL connection string securely using your local development configuration or environment variables.
4. Restore dependencies:

   ```powershell
   dotnet restore
   ```

5. Apply database migrations if needed:

   ```powershell
   dotnet ef database update
   ```

6. Start the backend:

   ```powershell
   dotnet run
   ```

### Frontend Setup

1. Open another terminal.
2. Navigate to the frontend:

   ```powershell
   cd frontend
   ```

3. Install dependencies:

   ```powershell
   npm install
   ```

4. Start the Angular development server using the script configured in `package.json`:

   ```powershell
   npm start
   ```

If `npm start` is not configured, use the project's configured Angular CLI serve command.

## Security Notes

- Do not commit passwords, API keys, access tokens, or real database credentials.
- Keep uploaded documents and other private user data out of version control.
- Use appropriate authentication and authorization before allowing document access.
- Configure production secrets through a secure secret-management mechanism.

## Project Status

The application has implemented document upload/download, sharing, access revocation, and audit-history functionality. See the source code for the current implementation.

## Repository

[TrustVault on GitHub](https://github.com/challabalaji22-source/TrustVault)
