# Bissaye.JwtAuth

Bissaye.JwtAuth is a .NET package that facilitates the implementation of JWT (JSON Web Token based on microsoft.aspnetcore.authentication.jwtbearer) authentication and the management of access and refresh tokens.

## Features

- JWT generation with custom claims

- JWT token verification and validation

- Access and refresh token management

- Customization of configuration options via JwtAuthConfigs in appsettings

- Support for cookie authentication in addition to JWT

## Installation

Add the package to your .NET project:
```bash
dotnet add package Bissaye.JwtAuth
```

## Configuration

Add the configuration settings to appsettings.json:

```json
{
  "JwtAuthConfigs": {
	"SecretKey": "your_secret_key",
	"Issuer": "your_issuer",
	"Audience": "your_audience",
	"AccessTokenExpirationMinutes": 15,
	"RefreshTokenExpirationMinutes": 30,
  }
}
```

Add the configuration settings to Program.cs:
```csharp
builder.Services.AddBissayeJwtAuth(builder.Configuration);
```
you can also add your own configs on the jwtbearerevents (or cookie configs) of your choice like this:
```csharp
builder.Services.AddBissayeJwtAuth(builder.Configuration, configureJwtBearerEvents: options =>
{
	options = new JwtBearerEvents {
	{
		// your code here
		OnChallenge = (context) => {...}
		OnAuthenticationFailed = (context) => {...}
		...
	};
});
```


## Utilisation de ITokenServices

Inject the ITokenServices interface into your controller:

```csharp
using Bissaye.JwtAuth.Services;
using System.Security.Claims;

private readonly ITokenServices _tokenServices;

public AuthController(ITokenServices tokenServices)
{
	_tokenServices = tokenServices;
}
```

### Generate a token
```csharp
List<Claim> claims = _tokenServices.GetClaims<T>(T obj);
string accessToken = tokenService.GenerateToken(claims);
string refreshToken = tokenService.GenerateToken(claims, refresh: true);
```

### Verify a refresh token
```csharp
bool isRefreshToken = tokenService.CheckRefreshToken(User);
```

### Retrieve a user's claims
```csharp
var userClaims = tokenService.GetClaimsValue<MyUserClass>(User);
```

## Contribution

Contributions are welcome! Feel free to suggest improvements via PRs or issues.