# 📝 Blog Platform API

A RESTful Blog Platform API built with **ASP.NET Core Web API**, **Entity Framework Core**, **SQL Server**, and **JWT Authentication**. The project follows a clean, feature-based Git workflow and is designed to demonstrate backend development best practices.

---

## 📖 Overview

This API allows users to register, log in, and manage blog posts. Authentication is secured using JSON Web Tokens (JWT), and passwords are hashed with BCrypt before being stored in the database.

---

## ✨ Features

- 👤 User Registration
- 🔐 User Login with JWT Authentication
- 🔒 Password Hashing using BCrypt
- ⚠️ Global Exception Handling
- 🗄️ SQL Server Database with Entity Framework Core
- 📦 DTO-based Request and Response Models
- 🌱 Entity Framework Core Migrations
- 🚀 RESTful API Architecture

---

## 🛠️ Technologies Used

- ASP.NET Core Web API
- C#
- Entity Framework Core
- SQL Server
- JWT Authentication
- BCrypt.Net
- Swagger (OpenAPI)

---

## 📂 Project Structure

```
BlogPlatform.API
│
├── Controllers
├── Data
├── DTO
├── Exceptions
├── Middleware
├── Models
├── Services
│   ├── Interfaces
│   └── AuthService.cs
├── Migrations
├── Program.cs
└── appsettings.json
```

---

## ⚙️ Installation

1. Clone the repository

```bash
git clone <repository-url>
```

2. Navigate to the project

```bash
cd BlogPlatform.API
```

3. Restore packages

```bash
dotnet restore
```

4. Apply database migrations

```bash
dotnet ef database update
```

5. Run the application

```bash
dotnet run
```

---

## 🔑 Authentication

The API uses **JWT (JSON Web Tokens)** for authentication.

After successful registration or login, the API returns a JWT token that should be included in the Authorization header.

```
Authorization: Bearer <your_token>
```

---

## 📚 Available Endpoints

### Authentication

| Method | Endpoint | Description |
|---------|----------|-------------|
| POST | `/api/Auth/register` | Register a new user |
| POST | `/api/Auth/login` | Login and receive JWT |

---

## 🗄️ Database

Current Entities:

- Users
- Blogs
- Comments

Database Provider:

- SQL Server

ORM:

- Entity Framework Core

---

## 🚀 Future Improvements

- Blog CRUD Operations
- Comment CRUD Operations
- Role-Based Authorization
- Pagination
- Search & Filtering
- Refresh Tokens
- Unit Testing

---

## 👨‍💻 Author

**Hamza**

Backend Developer | ASP.NET Core | C#
