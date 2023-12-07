# Getting Started
## Setting Up and Running the Project

### Prerequisites

Ensure you have the following installed:
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) 
### **Project Setup**

1. **Open the Project**: Open `Visual Studio 2022`, and select `Open a project or solution`. Navigate to the directory where you cloned or extracted `SimpleAuthApi`, and open the `.sln` file.

2. **Restore Dependencies**: Once the project is open in Visual Studio, right-click on the solution in the Solution Explorer and select `Restore NuGet Packages` to install all the required dependencies.
### **Running the Application**

1. **Start the Application**: 
   - In Visual Studio, ensure `SimpleAuthApi` is set as the startup project. 
   - Click on the `IIS Express` or `https` button to build and run the application. 

2. **Using a Command Line**: Navigate to the project's directory then run the following command:
```bash
 dotnet run
```

3. **Access the API**: After the application is running, you can access the API endpoints using a tool like [Postman](https://www.postman.com/) or other client.

## API Endpoints

### Authetnication

- **Login Endpoint**: To log in and obtain an access token:
```http
POST /api/Auth/Login
Content-Type: application/json

{
  "username": "thong.smith@test.com",
  "password": "P@55w0rd!"
}
```

Response: An `AccessTokenResponse` containing the access token, the expiry time, and a refresh token.

- **Refresh Token Endpoint**: To refresh your access token using a refresh token:
```http
POST /api/Auth/RefreshToken
Content-Type: application/json

{
  "refreshToken": "<your_refresh_token_here>"
}
```
Response: a new `AccessTokenResponse` with a new access token and refresh token.

### User Management Endpoints (Requires Authorization)

- **Create User** (No authorization required):
```http
POST /api/Users/Create
Content-Type: application/json

{
  "email" : "<email_to_created>",
  "firstName" : "<first_name>",
  "lastName" : "<last_name>",
  "password" : "<password>"
}
```

- **Update User**:
```http
PUT /api/Users/Update
Content-Type: application/json
Authorization: Bearer <your_access_token_here>

{
  "firstName" : "<first_name_to_update>",
  "lastName" : "<last_name_to_update>",
}
```

- **Delete User**:
```http
DELETE /api/Users/Delete
Content-Type: application/json
Authorization: Bearer <your_access_token_here>
```

- **Get User by ID**: To retrieve a specific user by their ID:
```http
GET /api/Users/{id}
Authorization: Bearer <your_access_token_here>
```

- **Get All Users**: To retrieve a list of all users:
```http
GET /api/Users
Authorization: Bearer <your_access_token_here>
```

-  **Search Users**: To search for users by a specific term:
```http
GET /api/Users/{term}
Authorization: Bearer <your_access_token_here>
```
