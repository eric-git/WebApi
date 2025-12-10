using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

// === CONFIG ===
var issuerUrl = "https://localhost:5001"; // WebApi.Issuer
var tokenEndpoint = $"{issuerUrl}/connect/token";
var serviceUrl = "https://localhost:5002/WeatherForecast"; // WebApi.Service
var clientId = "webapi-client";
var kid = "client-key-1";

// === STEP 1: Generate client_assertion JWT ===
using var rsa = RSA.Create(2048); // In real use, load your private key instead
var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa)
{
    KeyId = kid
}, SecurityAlgorithms.RsaSha256);

var handler = new JwtSecurityTokenHandler();
var descriptor = new SecurityTokenDescriptor
{
    Issuer = clientId,
    Subject = new ClaimsIdentity(new[] { new Claim("sub", clientId) }),
    Audience = tokenEndpoint,
    Expires = DateTime.UtcNow.AddMinutes(5),
    SigningCredentials = signingCredentials
};

var clientAssertion = handler.CreateEncodedJwt(descriptor);

// === STEP 2: Request token from issuer ===
using var http = new HttpClient();
var form = new Dictionary<string, string>
{
    ["grant_type"] = "client_credentials",
    ["client_id"] = clientId,
    ["scope"] = "api.read",
    ["client_assertion_type"] = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
    ["client_assertion"] = clientAssertion
};

var tokenResponse = await http.PostAsync(tokenEndpoint, new FormUrlEncodedContent(form));
var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
Console.WriteLine("Token response:");
Console.WriteLine(tokenJson);

// Extract access_token
var accessToken = System.Text.Json.JsonDocument.Parse(tokenJson)
    .RootElement.GetProperty("access_token").GetString();

// === STEP 3: Call WeatherForecast API ===
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
var forecastResponse = await http.GetStringAsync(serviceUrl);

Console.WriteLine("\nWeather forecast:");
Console.WriteLine(forecastResponse);