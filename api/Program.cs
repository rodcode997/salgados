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

var dbProvider = builder.Configuration["Database:Provider"] ?? "SqlServer";
var connStr = builder.Configuration.GetConnectionString("Default")
    ?? (dbProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase)
        ? "Data Source=salgados.db"
        : throw new InvalidOperationException("ConnectionStrings:Default não configurada para SQL Server."));

builder.Services.AddDbContext<AppDbContext>(o =>
{
    if (dbProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
        o.UseSqlite(connStr);
    else
        o.UseSqlServer(connStr, sql => sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null));
});

builder.Services.AddScoped<IMotivoRepository, MotivoRepository>();
builder.Services.AddScoped<IDividaRepository, DividaRepository>();
builder.Services.AddScoped<IPermissaoRepository, PermissaoRepository>();
builder.Services.AddScoped<IPerfilRepository, PerfilRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IConfiguracaoRepository, ConfiguracaoRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IGrupoPagamentoRepository, GrupoPagamentoRepository>();

builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddScoped<IMotivoService, MotivoService>();
builder.Services.AddScoped<IDividaService, DividaService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPerfilService, PerfilService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IConfiguracaoService, ConfiguracaoService>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IGrupoPagamentoService, GrupoPagamentoService>();
builder.Services.AddScoped<IAniversarioService, AniversarioService>();
builder.Services.AddHostedService<AniversarioHostedService>();

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

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173"];

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsSqlServer())
        db.Database.Migrate();
    else
        db.Database.EnsureCreated();

    // Garante colunas/tabelas incrementais em bancos SQLite locais já existentes
    if (db.Database.IsSqlite())
    {
        var conn = db.Database.GetDbConnection();
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "PRAGMA table_info(Usuarios);";
            var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    cols.Add(reader.GetString(1));
            }

            if (cols.Contains("Email") && !cols.Contains("Login"))
            {
                using var rename = conn.CreateCommand();
                rename.CommandText = "ALTER TABLE Usuarios RENAME COLUMN Email TO Login;";
                rename.ExecuteNonQuery();
                cols.Add("Login");
            }
            if (!cols.Contains("Avatar"))
            {
                using var alter = conn.CreateCommand();
                alter.CommandText = "ALTER TABLE Usuarios ADD COLUMN Avatar TEXT;";
                alter.ExecuteNonQuery();
            }
            if (!cols.Contains("DataNascimento"))
            {
                using var alter = conn.CreateCommand();
                alter.CommandText = "ALTER TABLE Usuarios ADD COLUMN DataNascimento TEXT;";
                alter.ExecuteNonQuery();
            }
            if (!cols.Contains("DeveAlterarSenha"))
            {
                using var alter = conn.CreateCommand();
                alter.CommandText = "ALTER TABLE Usuarios ADD COLUMN DeveAlterarSenha INTEGER NOT NULL DEFAULT 0;";
                alter.ExecuteNonQuery();
            }
            using var updAdmin = conn.CreateCommand();
            updAdmin.CommandText = "UPDATE Usuarios SET Login = 'admin' WHERE LOWER(Login) = 'admin@salgados.com' AND NOT EXISTS (SELECT 1 FROM Usuarios WHERE LOWER(Login) = 'admin');";
            updAdmin.ExecuteNonQuery();
        }
        using (var cfgCmd = conn.CreateCommand())
        {
            cfgCmd.CommandText = """
                CREATE TABLE IF NOT EXISTS Configuracoes (
                    Id INTEGER NOT NULL CONSTRAINT PK_Configuracoes PRIMARY KEY,
                    ExigirAceiteDivida INTEGER NOT NULL DEFAULT 1,
                    TipoDividaAniversario INTEGER NOT NULL DEFAULT 1
                );
                """;
            cfgCmd.ExecuteNonQuery();
        }
        using (var cfgColsCmd = conn.CreateCommand())
        {
            cfgColsCmd.CommandText = "PRAGMA table_info(Configuracoes);";
            var cfgCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var reader = cfgColsCmd.ExecuteReader())
            {
                while (reader.Read())
                    cfgCols.Add(reader.GetString(1));
            }
            if (!cfgCols.Contains("TipoDividaAniversario"))
            {
                using var alter = conn.CreateCommand();
                alter.CommandText = "ALTER TABLE Configuracoes ADD COLUMN TipoDividaAniversario INTEGER NOT NULL DEFAULT 1;";
                alter.ExecuteNonQuery();
            }
            if (!cfgCols.Contains("ValorMinimoQuitacaoConjunta"))
            {
                using var alter = conn.CreateCommand();
                alter.CommandText = "ALTER TABLE Configuracoes ADD COLUMN ValorMinimoQuitacaoConjunta TEXT NOT NULL DEFAULT '30';";
                alter.ExecuteNonQuery();
            }
        }
        using (var seedCfgCmd = conn.CreateCommand())
        {
            seedCfgCmd.CommandText = "INSERT OR IGNORE INTO Configuracoes (Id, ExigirAceiteDivida, TipoDividaAniversario, ValorMinimoQuitacaoConjunta) VALUES (1, 1, 1, '30');";
            seedCfgCmd.ExecuteNonQuery();
        }
        using (var gruposCmd = conn.CreateCommand())
        {
            gruposCmd.CommandText = """
                CREATE TABLE IF NOT EXISTS Produtos (
                    Id INTEGER NOT NULL CONSTRAINT PK_Produtos PRIMARY KEY AUTOINCREMENT,
                    Nome TEXT NOT NULL,
                    Valor TEXT NOT NULL,
                    Ativo INTEGER NOT NULL DEFAULT 1,
                    CriadoEm TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS GruposPagamento (
                    Id INTEGER NOT NULL CONSTRAINT PK_GruposPagamento PRIMARY KEY AUTOINCREMENT,
                    ProdutoId INTEGER NOT NULL,
                    ValorMeta TEXT NOT NULL,
                    ValorMinimoPorDivida TEXT NOT NULL,
                    DataCompra TEXT NULL,
                    Observacao TEXT NULL,
                    Status INTEGER NOT NULL DEFAULT 1,
                    CriadoPorId INTEGER NOT NULL,
                    CriadoEm TEXT NOT NULL,
                    FechadoEm TEXT NULL,
                    CONSTRAINT FK_GruposPagamento_Produtos_ProdutoId FOREIGN KEY (ProdutoId) REFERENCES Produtos (Id) ON DELETE RESTRICT,
                    CONSTRAINT FK_GruposPagamento_Usuarios_CriadoPorId FOREIGN KEY (CriadoPorId) REFERENCES Usuarios (Id) ON DELETE RESTRICT
                );
                CREATE TABLE IF NOT EXISTS GrupoPagamentoParticipacoes (
                    Id INTEGER NOT NULL CONSTRAINT PK_GrupoPagamentoParticipacoes PRIMARY KEY AUTOINCREMENT,
                    GrupoPagamentoId INTEGER NOT NULL,
                    UsuarioId INTEGER NOT NULL,
                    ValorContribuido TEXT NOT NULL,
                    CriadoEm TEXT NOT NULL,
                    CONSTRAINT FK_GrupoPagamentoParticipacoes_GruposPagamento_GrupoPagamentoId FOREIGN KEY (GrupoPagamentoId) REFERENCES GruposPagamento (Id) ON DELETE CASCADE,
                    CONSTRAINT FK_GrupoPagamentoParticipacoes_Usuarios_UsuarioId FOREIGN KEY (UsuarioId) REFERENCES Usuarios (Id) ON DELETE RESTRICT
                );
                CREATE TABLE IF NOT EXISTS GrupoPagamentoDividas (
                    GrupoPagamentoParticipacaoId INTEGER NOT NULL,
                    DividaId INTEGER NOT NULL,
                    CONSTRAINT PK_GrupoPagamentoDividas PRIMARY KEY (GrupoPagamentoParticipacaoId, DividaId),
                    CONSTRAINT FK_GrupoPagamentoDividas_GrupoPagamentoParticipacoes_GrupoPagamentoParticipacaoId FOREIGN KEY (GrupoPagamentoParticipacaoId) REFERENCES GrupoPagamentoParticipacoes (Id) ON DELETE CASCADE,
                    CONSTRAINT FK_GrupoPagamentoDividas_Dividas_DividaId FOREIGN KEY (DividaId) REFERENCES Dividas (Id) ON DELETE RESTRICT
                );
                """;
            gruposCmd.ExecuteNonQuery();
        }
        using (var grpColsCmd = conn.CreateCommand())
        {
            grpColsCmd.CommandText = "PRAGMA table_info(GruposPagamento);";
            var temDataCompra = false;
            using (var reader = grpColsCmd.ExecuteReader())
            {
                while (reader.Read())
                    if (string.Equals(reader.GetString(1), "DataCompra", StringComparison.OrdinalIgnoreCase))
                        temDataCompra = true;
            }
            if (!temDataCompra)
            {
                using var alter = conn.CreateCommand();
                alter.CommandText = "ALTER TABLE GruposPagamento ADD COLUMN DataCompra TEXT NULL;";
                alter.ExecuteNonQuery();
            }
        }
        conn.Close();
    }

    if (!db.Usuarios.Any())
    {
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();
        var adminLogin = builder.Configuration["SeedAdmin:Login"] ?? "admin";
        var adminSenha = builder.Configuration["SeedAdmin:SenhaInicial"] ?? "Admin@123";
        var admin = new Usuario
        {
            Nome = "Administrador",
            Login = adminLogin.Trim().ToLowerInvariant(),
            PerfilId = 1,
            Ativo = true,
            DeveAlterarSenha = false
        };
        admin.SenhaHash = hasher.HashPassword(admin, adminSenha);
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
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

if (File.Exists(Path.Combine(app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot"), "index.html")))
    app.MapFallbackToFile("index.html");

app.Run();
