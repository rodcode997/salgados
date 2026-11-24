using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Salgados.Api.Data;
using Salgados.Api.Entities;
using Salgados.Api.Repositories;
using Salgados.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=salgados.db"));

builder.Services.AddScoped<IMotivoRepository, MotivoRepository>();
builder.Services.AddScoped<IDividaRepository, DividaRepository>();
builder.Services.AddScoped<IPermissaoRepository, PermissaoRepository>();
builder.Services.AddScoped<IPerfilRepository, PerfilRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddScoped<IMotivoService, MotivoService>();
builder.Services.AddScoped<IDividaService, DividaService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPerfilService, PerfilService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "SalgadosApp-ChaveSecretaSuperSegura-2026-DevOnly!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Salgados.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "Salgados.Web";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(o =>
{
    foreach (var codigo in PermissoesCodigos.Todos)
        o.AddPolicy(codigo, p => p.RequireClaim("perm", codigo));

    o.AddPolicy("PerfisOuUsuarios", p => p.RequireAssertion(ctx =>
        ctx.User.HasClaim("perm", PermissoesCodigos.PerfisGerenciar) ||
        ctx.User.HasClaim("perm", PermissoesCodigos.UsuariosGerenciar)));
});

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Usuarios.Any())
    {
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();
        var admin = new Usuario
        {
            Nome = "Administrador",
            Email = "admin@salgados.com",
            PerfilId = 1,
            Ativo = true
        };
        admin.SenhaHash = hasher.HashPassword(admin, "Admin@123");
        db.Usuarios.Add(admin);
        db.SaveChanges();
    }
}

// Tradução das exceções de negócio para respostas HTTP
app.Use(async (ctx, next) =>
{
    try { await next(); }
    catch (NaoAutorizadoException ex) { ctx.Response.StatusCode = 401; await ctx.Response.WriteAsJsonAsync(new { erro = ex.Message }); }
    catch (NaoEncontradoException ex) { ctx.Response.StatusCode = 404; await ctx.Response.WriteAsJsonAsync(new { erro = ex.Message }); }
    catch (RegraNegocioException ex) { ctx.Response.StatusCode = 400; await ctx.Response.WriteAsJsonAsync(new { erro = ex.Message }); }
});

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run("http://localhost:5000");
