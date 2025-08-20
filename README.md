# UserManagementAPI

A simple **ASP.NET Core Web API** project for user management with custom middleware implementations.  
This project is part of my learning journey in backend development and demonstrates middleware usage, validation, and basic CRUD operations.

## Features

- **Global Error Handling Middleware**
  - Catches unhandled exceptions
  - Returns consistent JSON error responses (`{ "error": "Internal server error." }`)
  - Improves reliability and debugging

- **Token Authentication Middleware**
  - Validates `Authorization: Bearer <token>` header
  - Blocks access for invalid/missing tokens (401 Unauthorized)
  - Allows access only to requests with a valid token

- **HTTP Logging Middleware**
  - Logs HTTP method, request path, and response status code
  - Helps with auditing and monitoring

- **User CRUD Endpoints**
  - `GET /users` – List all users
  - `GET /users/{id}` – Get a user by ID
  - `POST /users` – Create a new user (with validation)
  - `PUT /users/{id}` – Update existing user
  - `DELETE /users/{id}` – Remove a user
  - Includes input validation (Name required, Age between 0–120, Gender required)

- **Swagger Integration**
  - Enabled in Development environment
  - Provides an interactive API documentation and testing interface

## Tech Stack

- .NET 8
- ASP.NET Core Minimal API
- C#
- Swagger / Swashbuckle

## Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/<AtalayOzcan>/UserManagementAPI.git
   cd UserManagementAPI
   
2. Run the application:
dotnet run

3. Access Swagger UI:
https://localhost:****/swagger

**Example Request**
POST /users
Authorization: Bearer my-secret-token
Content-Type: application/json

{
  "name": "Atalay",
  "age": 24,
  "gender": "Male"
}

**Example Response**
201 Created
{
  "id": 3,
  "name": "Atalay",
  "age": 24,
  "gender": "Male"
}

## License
This project is licensed under the MIT License.
