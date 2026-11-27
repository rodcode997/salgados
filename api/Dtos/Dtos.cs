using System.ComponentModel.DataAnnotations;
using Salgados.Api.Entities;

namespace Salgados.Api.Dtos;

public record MotivoInput([Required, MaxLength(200)] string Descricao);
public record MotivoDto(int Id, string Descricao, bool Ativo);

public record DividaInput(
    [Range(1, int.MaxValue)] int UsuarioId,
    [Range(1, int.MaxValue)] int MotivoId,
    TipoDivida Tipo,
    DateTime DataAquisicao,
    string? Observacao);

public record MensagemInput([Required, MaxLength(600)] string Mensagem);
public record MensagemOpcionalInput([MaxLength(600)] string? Mensagem);

public record PagamentoInput(
    DateTime DataPagamento,
    [Range(0.01, 100000)] decimal ValorPago,
    [MaxLength(500)] string? Observacao);

public record DividaInteracaoDto(
    int Id, int UsuarioId, string Usuario,
    TipoInteracao Tipo, string TipoDescricao,
    string? Mensagem, DateTime CriadoEm);

public record PagamentoDto(
    int Id, int DividaId, DateTime DataPagamento, decimal ValorPago, string? Observacao,
    StatusPagamento Status, string StatusDescricao,
    int CriadoPorId, string CriadoPor, DateTime CriadoEm,
    int? AvaliadoPorId, string? AvaliadoPor, DateTime? AvaliadoEm,
    string? MotivoRecusa);

public record DividaDto(
    int Id, int UsuarioId, string Usuario, string? UsuarioAvatar, int MotivoId, string Motivo,
    TipoDivida Tipo, string TipoDescricao, string? Observacao,
    DateTime DataAquisicao,
    StatusDivida Status, string StatusDescricao,
    int CriadoPorId, string CriadoPor, DateTime CriadoEm,
    DateTime? DataQuitacao, decimal? ValorPago,
    int? QuitadoPorId, string? QuitadoPor, DateTime? QuitadoEm,
    bool Quitada, int Dias, bool TemPagamentoPendente,
    List<DividaInteracaoDto> Interacoes,
    List<PagamentoDto> Pagamentos);

// Autenticação, Usuários (Pessoas), Perfis e Permissões
public record LoginInput([Required, MaxLength(100)] string Login, [Required] string Senha);
public record AlterarSenhaInput([Required] string SenhaAtual, [Required, MinLength(6)] string NovaSenha);
public record AlterarAvatarInput([MaxLength(500)] string? Avatar);

public record UsuarioSessaoDto(int Id, string Nome, string Login, string? Avatar, bool DeveAlterarSenha, int PerfilId, string Perfil, List<string> Permissoes);
public record LoginDto(string Token, DateTime ExpiraEm, UsuarioSessaoDto Usuario);

public record PermissaoDto(int Id, string Codigo, string Descricao, string Modulo);

public record PerfilInput(
    [Required, MaxLength(100)] string Nome,
    [MaxLength(300)] string? Descricao,
    List<int> PermissaoIds);

public record PerfilDto(
    int Id, string Nome, string? Descricao, bool Ativo,
    int TotalUsuarios, List<PermissaoDto> Permissoes);

public record UsuarioCreateInput(
    [Required, MaxLength(150)] string Nome,
    [Required, MaxLength(100)] string Login,
    [Required, MinLength(6)] string Senha,
    [Range(1, int.MaxValue)] int PerfilId,
    [Required] DateTime DataNascimento,
    [MaxLength(500)] string? Avatar = null);

public record UsuarioUpdateInput(
    [Required, MaxLength(150)] string Nome,
    [Required, MaxLength(100)] string Login,
    [Range(1, int.MaxValue)] int PerfilId,
    [Required] DateTime DataNascimento,
    string? NovaSenha,
    [MaxLength(500)] string? Avatar = null);

public record UsuarioDto(
    int Id, string Nome, string Login, DateTime? DataNascimento, string? Avatar, bool Ativo,
    DateTime CriadoEm, int PerfilId, string Perfil,
    int DividasAbertas, int TotalDividas, string? Mensagem);

public record SituacaoDto(UsuarioDto Usuario, List<DividaDto> Pendentes, List<DividaDto> Abertas, List<DividaDto> Historico);

public record ConfiguracaoDto(bool ExigirAceiteDivida, TipoDivida TipoDividaAniversario, decimal ValorMinimoQuitacaoConjunta);
public record ConfiguracaoInput(
    bool ExigirAceiteDivida,
    TipoDivida TipoDividaAniversario,
    [Range(0.01, 100000)] decimal ValorMinimoQuitacaoConjunta);

public record ProdutoInput(
    [Required, MaxLength(150)] string Nome,
    [Range(0.01, 100000)] decimal Valor);

public record ProdutoDto(int Id, string Nome, decimal Valor, bool Ativo, DateTime CriadoEm);

public record GrupoPagamentoCreateInput(
    [Range(1, int.MaxValue)] int ProdutoId,
    [Required] DateTime DataCompra,
    [MaxLength(300)] string? Observacao);

public record GrupoParticiparInput([Required] List<int> DividaIds);

public record GrupoDividaResumoDto(int Id, TipoDivida Tipo, string TipoDescricao, string Motivo, DateTime DataAquisicao);

public record GrupoParticipacaoDto(
    int Id, int UsuarioId, string Usuario, string? UsuarioAvatar,
    decimal ValorContribuido, DateTime CriadoEm,
    List<GrupoDividaResumoDto> Dividas);

public record GrupoPagamentoDto(
    int Id, int ProdutoId, string ProdutoNome,
    decimal ValorMeta, decimal ValorMinimoPorDivida,
    int TotalDividasMeta, int TotalDividasSelecionadas, decimal ValorPorDivida,
    DateTime? DataCompra,
    string? Observacao, StatusGrupoPagamento Status,
    int CriadoPorId, string CriadoPor, DateTime CriadoEm, DateTime? FechadoEm,
    List<GrupoParticipacaoDto> Participacoes);
