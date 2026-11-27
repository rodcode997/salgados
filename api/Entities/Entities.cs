namespace Salgados.Api.Entities;

public static class PermissoesCodigos
{
    public const string DividasVer = "dividas.ver";
    public const string DividasCriar = "dividas.criar";
    public const string DividasQuitar = "dividas.quitar";
    public const string UsuariosVer = "usuarios.ver";
    public const string UsuariosGerenciar = "usuarios.gerenciar";
    public const string MotivosVer = "motivos.ver";
    public const string MotivosGerenciar = "motivos.gerenciar";
    public const string PerfisGerenciar = "perfis.gerenciar";

    public static readonly string[] Todos =
    [
        DividasVer, DividasCriar, DividasQuitar,
        UsuariosVer, UsuariosGerenciar,
        MotivosVer, MotivosGerenciar,
        PerfisGerenciar
    ];
}

public enum TipoDivida
{
    CentoSalgado = 1,
    CaixaChocolate = 2
}

public enum StatusDivida
{
    PendenteAceite = 1,
    Contestada = 2,
    Aceita = 3,
    Desistida = 4,
    Quitada = 5
}

public enum TipoInteracao
{
    Criada = 1,
    Contestada = 2,
    Complementada = 3,
    Aceita = 4,
    Desistida = 5
}

public enum StatusPagamento
{
    Pendente = 1,
    Aprovado = 2,
    Recusado = 3
}

public class Motivo
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}

public class Divida
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public int MotivoId { get; set; }
    public Motivo? Motivo { get; set; }
    public TipoDivida Tipo { get; set; }
    public string? Observacao { get; set; }
    public DateTime DataAquisicao { get; set; }

    public StatusDivida Status { get; set; } = StatusDivida.PendenteAceite;

    public int CriadoPorId { get; set; }
    public Usuario? CriadoPor { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.Now;

    public DateTime? DataQuitacao { get; set; }
    public decimal? ValorPago { get; set; }
    public int? QuitadoPorId { get; set; }
    public Usuario? QuitadoPor { get; set; }
    public DateTime? QuitadoEm { get; set; }

    public List<DividaInteracao> Interacoes { get; set; } = new();
    public List<Pagamento> Pagamentos { get; set; } = new();

    public bool Quitada => Status == StatusDivida.Quitada;

    /// <summary>Dias que levou para quitar (ou dias em aberto até hoje, se não quitada).</summary>
    public int Dias => ((DataQuitacao ?? DateTime.Today).Date - DataAquisicao.Date).Days;
}

public class DividaInteracao
{
    public int Id { get; set; }
    public int DividaId { get; set; }
    public Divida? Divida { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public TipoInteracao Tipo { get; set; }
    public string? Mensagem { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.Now;
}

public class Pagamento
{
    public int Id { get; set; }
    public int DividaId { get; set; }
    public Divida? Divida { get; set; }
    public DateTime DataPagamento { get; set; }
    public decimal ValorPago { get; set; }
    public string? Observacao { get; set; }
    public StatusPagamento Status { get; set; } = StatusPagamento.Pendente;

    public int CriadoPorId { get; set; }
    public Usuario? CriadoPor { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.Now;

    public int? AvaliadoPorId { get; set; }
    public Usuario? AvaliadoPor { get; set; }
    public DateTime? AvaliadoEm { get; set; }
    public string? MotivoRecusa { get; set; }
}

public class Permissao
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
}

public class Perfil
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; } = true;
    public List<PerfilPermissao> Permissoes { get; set; } = new();
    public List<Usuario> Usuarios { get; set; } = new();
}

public class PerfilPermissao
{
    public int PerfilId { get; set; }
    public Perfil? Perfil { get; set; }
    public int PermissaoId { get; set; }
    public Permissao? Permissao { get; set; }
}

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.Now;
    public int PerfilId { get; set; }
    public Perfil? Perfil { get; set; }
    public List<Divida> Dividas { get; set; } = new();
}

public class ConfiguracaoSistema
{
    public int Id { get; set; } = 1;
    public bool ExigirAceiteDivida { get; set; } = true;
}
