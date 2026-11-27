using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Salgados.Api.Dtos;
using Salgados.Api.Entities;
using Salgados.Api.Repositories;

namespace Salgados.Api.Services;

public class RegraNegocioException(string msg) : Exception(msg);
public class NaoEncontradoException(string msg) : Exception(msg);
public class NaoAutorizadoException(string msg) : Exception(msg);

public static class Mapper
{
    public const string MensagemParabens = "🎉 Parabéns! Você não tem nenhuma dívida de salgado ou chocolate!";

    public static string Descricao(TipoDivida t) => t switch
    {
        TipoDivida.CentoSalgado => "Cento de salgados",
        TipoDivida.CaixaChocolate => "Caixa de chocolate",
        _ => t.ToString()
    };

    public static string Descricao(StatusDivida s) => s switch
    {
        StatusDivida.PendenteAceite => "Aguardando aceite",
        StatusDivida.Contestada => "Contestada",
        StatusDivida.Aceita => "Em aberto",
        StatusDivida.Desistida => "Desistida",
        StatusDivida.Quitada => "Quitada",
        _ => s.ToString()
    };

    public static string Descricao(TipoInteracao t) => t switch
    {
        TipoInteracao.Criada => "Dívida cadastrada",
        TipoInteracao.Contestada => "Contestada pelo devedor",
        TipoInteracao.Complementada => "Mais informações enviadas",
        TipoInteracao.Aceita => "Aceita pelo devedor",
        TipoInteracao.Desistida => "Desistida pelo criador",
        _ => t.ToString()
    };

    public static string Descricao(StatusPagamento s) => s switch
    {
        StatusPagamento.Pendente => "Aguardando aprovação",
        StatusPagamento.Aprovado => "Aprovado",
        StatusPagamento.Recusado => "Recusado",
        _ => s.ToString()
    };

    public static MotivoDto ToDto(Motivo m) => new(m.Id, m.Descricao, m.Ativo);

    public static DividaInteracaoDto ToDto(DividaInteracao i) => new(
        i.Id, i.UsuarioId, i.Usuario?.Nome ?? "", i.Tipo, Descricao(i.Tipo), i.Mensagem, i.CriadoEm);

    public static PagamentoDto ToDto(Pagamento p) => new(
        p.Id, p.DividaId, p.DataPagamento, p.ValorPago, p.Observacao,
        p.Status, Descricao(p.Status),
        p.CriadoPorId, p.CriadoPor?.Nome ?? "", p.CriadoEm,
        p.AvaliadoPorId, p.AvaliadoPor?.Nome, p.AvaliadoEm,
        p.MotivoRecusa);

    public static DividaDto ToDto(Divida d) => new(
        d.Id, d.UsuarioId, d.Usuario?.Nome ?? "", d.Usuario?.Avatar, d.MotivoId, d.Motivo?.Descricao ?? "",
        d.Tipo, Descricao(d.Tipo), d.Observacao,
        d.DataAquisicao,
        d.Status, Descricao(d.Status),
        d.CriadoPorId, d.CriadoPor?.Nome ?? "", d.CriadoEm,
        d.DataQuitacao, d.ValorPago,
        d.QuitadoPorId, d.QuitadoPor?.Nome, d.QuitadoEm,
        d.Quitada, d.Dias,
        d.Pagamentos.Any(p => p.Status == StatusPagamento.Pendente),
        d.Interacoes.OrderBy(i => i.CriadoEm).Select(ToDto).ToList(),
        d.Pagamentos.OrderByDescending(p => p.CriadoEm).Select(ToDto).ToList());

    public static PermissaoDto ToDto(Permissao p) => new(p.Id, p.Codigo, p.Descricao, p.Modulo);

    public static PerfilDto ToDto(Perfil p) => new(
        p.Id, p.Nome, p.Descricao, p.Ativo, p.Usuarios.Count,
        p.Permissoes.Where(pp => pp.Permissao != null).Select(pp => ToDto(pp.Permissao!)).OrderBy(x => x.Modulo).ThenBy(x => x.Id).ToList());

    public static UsuarioDto ToDto(Usuario u)
    {
        var abertas = u.Dividas.Count(d => d.Status == StatusDivida.Aceita);
        var totalValidas = u.Dividas.Count(d => d.Status is StatusDivida.Aceita or StatusDivida.Quitada);
        return new UsuarioDto(
            u.Id, u.Nome, u.Login, u.DataNascimento, u.Avatar, u.Ativo, u.CriadoEm, u.PerfilId, u.Perfil?.Nome ?? "",
            abertas, totalValidas, abertas == 0 ? MensagemParabens : null);
    }

    public static UsuarioSessaoDto ToSessaoDto(Usuario u) => new(
        u.Id, u.Nome, u.Login, u.Avatar, u.DeveAlterarSenha, u.PerfilId, u.Perfil?.Nome ?? "",
        u.Perfil?.Permissoes.Where(pp => pp.Permissao != null).Select(pp => pp.Permissao!.Codigo).Distinct().ToList() ?? []);
}

public interface IMotivoService
{
    Task<List<MotivoDto>> ListarAsync();
    Task<MotivoDto> CriarAsync(MotivoInput input);
    Task<MotivoDto> AtualizarAsync(int id, MotivoInput input);
    Task AlterarAtivoAsync(int id, bool ativo);
}

public class MotivoService(IMotivoRepository repo) : IMotivoService
{
    public async Task<List<MotivoDto>> ListarAsync() => (await repo.ListarAsync()).Select(Mapper.ToDto).ToList();

    public async Task<MotivoDto> CriarAsync(MotivoInput input)
    {
        var m = new Motivo { Descricao = input.Descricao.Trim() };
        await repo.AdicionarAsync(m);
        await repo.SalvarAsync();
        return Mapper.ToDto(m);
    }

    public async Task<MotivoDto> AtualizarAsync(int id, MotivoInput input)
    {
        var m = await repo.ObterAsync(id) ?? throw new NaoEncontradoException("Motivo não encontrado.");
        m.Descricao = input.Descricao.Trim();
        await repo.SalvarAsync();
        return Mapper.ToDto(m);
    }

    public async Task AlterarAtivoAsync(int id, bool ativo)
    {
        var m = await repo.ObterAsync(id) ?? throw new NaoEncontradoException("Motivo não encontrado.");
        m.Ativo = ativo;
        await repo.SalvarAsync();
    }
}

public interface IDividaService
{
    Task<List<DividaDto>> ListarAsync(int? usuarioId, StatusDivida? status);
    Task<DividaDto> CriarAsync(DividaInput input, int criadoPorId);
    Task<DividaDto> AceitarAsync(int id, int usuarioLogadoId);
    Task<DividaDto> ContestarAsync(int id, MensagemInput input, int usuarioLogadoId);
    Task<DividaDto> ComplementarAsync(int id, MensagemInput input, int usuarioLogadoId);
    Task<DividaDto> DesistirAsync(int id, MensagemOpcionalInput input, int usuarioLogadoId);
    Task<DividaDto> RegistrarPagamentoAsync(int dividaId, PagamentoInput input, int usuarioLogadoId, bool aprovarDireto);
    Task<DividaDto> AprovarPagamentoAsync(int pagamentoId, int avaliadorId);
    Task<DividaDto> RecusarPagamentoAsync(int pagamentoId, MensagemInput input, int avaliadorId);
}

public class DividaService(
    IDividaRepository repo,
    IUsuarioRepository usuarios,
    IMotivoRepository motivos,
    IConfiguracaoRepository configuracoes) : IDividaService
{
    public async Task<List<DividaDto>> ListarAsync(int? usuarioId, StatusDivida? status) =>
        (await repo.ListarAsync(usuarioId, status)).Select(Mapper.ToDto).ToList();

    public async Task<DividaDto> CriarAsync(DividaInput input, int criadoPorId)
    {
        if (!Enum.IsDefined(input.Tipo)) throw new RegraNegocioException("Tipo de dívida inválido.");
        if (input.DataAquisicao.Date > DateTime.Today) throw new RegraNegocioException("Data de aquisição não pode ser futura.");
        var usuario = await usuarios.ObterAsync(input.UsuarioId) ?? throw new RegraNegocioException("Usuário inválido.");
        if (!usuario.Ativo) throw new RegraNegocioException("Usuário inativo.");
        var motivo = await motivos.ObterAsync(input.MotivoId) ?? throw new RegraNegocioException("Motivo inválido.");
        if (!motivo.Ativo) throw new RegraNegocioException("Motivo inativo.");

        var config = await configuracoes.ObterAsync();
        var agora = DateTime.Now;
        var autoAceita = !config.ExigirAceiteDivida || input.UsuarioId == criadoPorId;
        var d = new Divida
        {
            UsuarioId = input.UsuarioId,
            MotivoId = input.MotivoId,
            Tipo = input.Tipo,
            DataAquisicao = input.DataAquisicao.Date,
            Observacao = input.Observacao?.Trim(),
            Status = autoAceita ? StatusDivida.Aceita : StatusDivida.PendenteAceite,
            CriadoPorId = criadoPorId,
            CriadoEm = agora
        };
        d.Interacoes.Add(new DividaInteracao
        {
            UsuarioId = criadoPorId,
            Tipo = TipoInteracao.Criada,
            Mensagem = string.IsNullOrWhiteSpace(input.Observacao) ? null : input.Observacao.Trim(),
            CriadoEm = agora
        });
        if (autoAceita)
        {
            d.Interacoes.Add(new DividaInteracao
            {
                UsuarioId = criadoPorId,
                Tipo = TipoInteracao.Aceita,
                Mensagem = !config.ExigirAceiteDivida
                    ? "Lançada diretamente em aberto (aceite de dívidas desativado)."
                    : "Aceite automático (cadastrada pelo próprio devedor).",
                CriadoEm = agora
            });
        }

        await repo.AdicionarAsync(d);
        await repo.SalvarAsync();
        return Mapper.ToDto((await repo.ObterAsync(d.Id))!);
    }

    public async Task<DividaDto> AceitarAsync(int id, int usuarioLogadoId)
    {
        var d = await repo.ObterAsync(id) ?? throw new NaoEncontradoException("Dívida não encontrada.");
        if (d.UsuarioId != usuarioLogadoId) throw new RegraNegocioException("Apenas o usuário que recebeu a dívida pode aceitá-la.");
        if (d.Status != StatusDivida.PendenteAceite) throw new RegraNegocioException("Esta dívida não está aguardando aceite.");

        d.Status = StatusDivida.Aceita;
        d.Interacoes.Add(new DividaInteracao
        {
            DividaId = d.Id,
            UsuarioId = usuarioLogadoId,
            Tipo = TipoInteracao.Aceita,
            CriadoEm = DateTime.Now
        });
        await repo.SalvarAsync();
        return Mapper.ToDto((await repo.ObterAsync(d.Id))!);
    }

    public async Task<DividaDto> ContestarAsync(int id, MensagemInput input, int usuarioLogadoId)
    {
        var d = await repo.ObterAsync(id) ?? throw new NaoEncontradoException("Dívida não encontrada.");
        if (d.UsuarioId != usuarioLogadoId) throw new RegraNegocioException("Apenas o usuário que recebeu a dívida pode contestá-la.");
        if (d.Status != StatusDivida.PendenteAceite) throw new RegraNegocioException("Esta dívida não está aguardando aceite.");

        d.Status = StatusDivida.Contestada;
        d.Interacoes.Add(new DividaInteracao
        {
            DividaId = d.Id,
            UsuarioId = usuarioLogadoId,
            Tipo = TipoInteracao.Contestada,
            Mensagem = input.Mensagem.Trim(),
            CriadoEm = DateTime.Now
        });
        await repo.SalvarAsync();
        return Mapper.ToDto((await repo.ObterAsync(d.Id))!);
    }

    public async Task<DividaDto> ComplementarAsync(int id, MensagemInput input, int usuarioLogadoId)
    {
        var d = await repo.ObterAsync(id) ?? throw new NaoEncontradoException("Dívida não encontrada.");
        if (d.CriadoPorId != usuarioLogadoId) throw new RegraNegocioException("Apenas quem cadastrou a dívida pode enviar mais informações.");
        if (d.Status != StatusDivida.Contestada) throw new RegraNegocioException("Só é possível complementar uma dívida contestada.");

        d.Status = StatusDivida.PendenteAceite;
        d.Interacoes.Add(new DividaInteracao
        {
            DividaId = d.Id,
            UsuarioId = usuarioLogadoId,
            Tipo = TipoInteracao.Complementada,
            Mensagem = input.Mensagem.Trim(),
            CriadoEm = DateTime.Now
        });
        await repo.SalvarAsync();
        return Mapper.ToDto((await repo.ObterAsync(d.Id))!);
    }

    public async Task<DividaDto> DesistirAsync(int id, MensagemOpcionalInput input, int usuarioLogadoId)
    {
        var d = await repo.ObterAsync(id) ?? throw new NaoEncontradoException("Dívida não encontrada.");
        if (d.CriadoPorId != usuarioLogadoId) throw new RegraNegocioException("Apenas quem cadastrou a dívida pode desistir dela.");
        if (d.Status is not (StatusDivida.Contestada or StatusDivida.PendenteAceite))
            throw new RegraNegocioException("Só é possível desistir de uma dívida pendente ou contestada.");

        d.Status = StatusDivida.Desistida;
        d.Interacoes.Add(new DividaInteracao
        {
            DividaId = d.Id,
            UsuarioId = usuarioLogadoId,
            Tipo = TipoInteracao.Desistida,
            Mensagem = string.IsNullOrWhiteSpace(input.Mensagem) ? null : input.Mensagem.Trim(),
            CriadoEm = DateTime.Now
        });
        await repo.SalvarAsync();
        return Mapper.ToDto((await repo.ObterAsync(d.Id))!);
    }

    public async Task<DividaDto> RegistrarPagamentoAsync(int dividaId, PagamentoInput input, int usuarioLogadoId, bool aprovarDireto)
    {
        var d = await repo.ObterAsync(dividaId) ?? throw new NaoEncontradoException("Dívida não encontrada.");
        if (d.Status != StatusDivida.Aceita)
            throw new RegraNegocioException("Só é possível informar pagamento de uma dívida aceita e em aberto.");
        if (!aprovarDireto && d.UsuarioId != usuarioLogadoId)
            throw new RegraNegocioException("Você só pode informar pagamento das suas próprias dívidas.");
        if (d.Pagamentos.Any(p => p.Status == StatusPagamento.Pendente))
            throw new RegraNegocioException("Já existe um pagamento aguardando aprovação para esta dívida.");
        if (input.DataPagamento.Date < d.DataAquisicao.Date)
            throw new RegraNegocioException("Data de pagamento anterior à data de aquisição.");
        if (input.DataPagamento.Date > DateTime.Today)
            throw new RegraNegocioException("Data de pagamento não pode ser futura.");

        var agora = DateTime.Now;
        var pg = new Pagamento
        {
            DividaId = d.Id,
            DataPagamento = input.DataPagamento.Date,
            ValorPago = input.ValorPago,
            Observacao = string.IsNullOrWhiteSpace(input.Observacao) ? null : input.Observacao.Trim(),
            Status = aprovarDireto ? StatusPagamento.Aprovado : StatusPagamento.Pendente,
            CriadoPorId = usuarioLogadoId,
            CriadoEm = agora,
            AvaliadoPorId = aprovarDireto ? usuarioLogadoId : null,
            AvaliadoEm = aprovarDireto ? agora : null
        };
        d.Pagamentos.Add(pg);

        if (aprovarDireto)
        {
            d.Status = StatusDivida.Quitada;
            d.DataQuitacao = pg.DataPagamento;
            d.ValorPago = pg.ValorPago;
            d.QuitadoPorId = usuarioLogadoId;
            d.QuitadoEm = agora;
        }

        await repo.SalvarAsync();
        return Mapper.ToDto((await repo.ObterAsync(d.Id))!);
    }

    public async Task<DividaDto> AprovarPagamentoAsync(int pagamentoId, int avaliadorId)
    {
        var pg = await repo.ObterPagamentoAsync(pagamentoId) ?? throw new NaoEncontradoException("Pagamento não encontrado.");
        if (pg.Status != StatusPagamento.Pendente)
            throw new RegraNegocioException("Este pagamento já foi avaliado.");
        var d = pg.Divida!;
        if (d.Status != StatusDivida.Aceita)
            throw new RegraNegocioException("A dívida não está mais em aberto.");

        var agora = DateTime.Now;
        pg.Status = StatusPagamento.Aprovado;
        pg.AvaliadoPorId = avaliadorId;
        pg.AvaliadoEm = agora;

        d.Status = StatusDivida.Quitada;
        d.DataQuitacao = pg.DataPagamento;
        d.ValorPago = pg.ValorPago;
        d.QuitadoPorId = avaliadorId;
        d.QuitadoEm = agora;

        await repo.SalvarAsync();
        return Mapper.ToDto((await repo.ObterAsync(d.Id))!);
    }

    public async Task<DividaDto> RecusarPagamentoAsync(int pagamentoId, MensagemInput input, int avaliadorId)
    {
        var pg = await repo.ObterPagamentoAsync(pagamentoId) ?? throw new NaoEncontradoException("Pagamento não encontrado.");
        if (pg.Status != StatusPagamento.Pendente)
            throw new RegraNegocioException("Este pagamento já foi avaliado.");

        pg.Status = StatusPagamento.Recusado;
        pg.AvaliadoPorId = avaliadorId;
        pg.AvaliadoEm = DateTime.Now;
        pg.MotivoRecusa = input.Mensagem.Trim();

        await repo.SalvarAsync();
        return Mapper.ToDto((await repo.ObterAsync(pg.DividaId))!);
    }
}

public interface IAuthService
{
    Task<LoginDto> LoginAsync(LoginInput input);
    Task<UsuarioSessaoDto> ObterSessaoAsync(int usuarioId);
    Task AlterarSenhaAsync(int usuarioId, AlterarSenhaInput input);
    Task<UsuarioSessaoDto> AlterarAvatarAsync(int usuarioId, AlterarAvatarInput input);
}

public class AuthService(IUsuarioRepository usuarios, IPasswordHasher<Usuario> hasher, IConfiguration config) : IAuthService
{
    public async Task<LoginDto> LoginAsync(LoginInput input)
    {
        var login = input.Login.Trim().ToLowerInvariant();
        var u = await usuarios.ObterPorLoginAsync(login)
            ?? throw new NaoAutorizadoException("Login ou senha inválidos.");

        if (!u.Ativo) throw new NaoAutorizadoException("Usuário inativo. Procure um administrador.");
        if (u.Perfil is null || !u.Perfil.Ativo) throw new NaoAutorizadoException("Perfil do usuário está inativo.");

        var verif = hasher.VerifyHashedPassword(u, u.SenhaHash, input.Senha);
        if (verif == PasswordVerificationResult.Failed)
            throw new NaoAutorizadoException("Login ou senha inválidos.");

        var sessao = Mapper.ToSessaoDto(u);
        var horas = config.GetValue("Jwt:ExpiraHoras", 12);
        var expira = DateTime.UtcNow.AddHours(horas);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, u.Id.ToString()),
            new(ClaimTypes.NameIdentifier, u.Id.ToString()),
            new(ClaimTypes.Name, u.Nome),
            new("login", u.Login),
            new(ClaimTypes.Role, sessao.Perfil)
        };
        foreach (var perm in sessao.Permissoes)
            claims.Add(new Claim("perm", perm));

        var keyStr = config["Jwt:Key"] ?? "SalgadosApp-ChaveSecretaSuperSegura-2026-DevOnly!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"] ?? "Salgados.Api",
            audience: config["Jwt:Audience"] ?? "Salgados.Web",
            claims: claims,
            expires: expira,
            signingCredentials: creds);

        return new LoginDto(new JwtSecurityTokenHandler().WriteToken(token), expira, sessao);
    }

    public async Task<UsuarioSessaoDto> ObterSessaoAsync(int usuarioId)
    {
        var u = await usuarios.ObterAsync(usuarioId) ?? throw new NaoAutorizadoException("Sessão inválida.");
        if (!u.Ativo || u.Perfil is null || !u.Perfil.Ativo)
            throw new NaoAutorizadoException("Usuário ou perfil inativo.");
        return Mapper.ToSessaoDto(u);
    }

    public async Task AlterarSenhaAsync(int usuarioId, AlterarSenhaInput input)
    {
        var u = await usuarios.ObterAsync(usuarioId) ?? throw new NaoEncontradoException("Usuário não encontrado.");
        var verif = hasher.VerifyHashedPassword(u, u.SenhaHash, input.SenhaAtual);
        if (verif == PasswordVerificationResult.Failed)
            throw new RegraNegocioException("Senha atual incorreta.");
        if (input.NovaSenha == input.SenhaAtual)
            throw new RegraNegocioException("A nova senha deve ser diferente da senha atual.");
        u.SenhaHash = hasher.HashPassword(u, input.NovaSenha);
        u.DeveAlterarSenha = false;
        await usuarios.SalvarAsync();
    }

    public async Task<UsuarioSessaoDto> AlterarAvatarAsync(int usuarioId, AlterarAvatarInput input)
    {
        var u = await usuarios.ObterAsync(usuarioId) ?? throw new NaoEncontradoException("Usuário não encontrado.");
        u.Avatar = string.IsNullOrWhiteSpace(input.Avatar) ? null : input.Avatar.Trim();
        await usuarios.SalvarAsync();
        return Mapper.ToSessaoDto(u);
    }
}

public interface IPerfilService
{
    Task<List<PermissaoDto>> ListarPermissoesAsync();
    Task<List<PerfilDto>> ListarAsync();
    Task<PerfilDto> CriarAsync(PerfilInput input);
    Task<PerfilDto> AtualizarAsync(int id, PerfilInput input);
    Task AlterarAtivoAsync(int id, bool ativo);
}

public class PerfilService(IPerfilRepository repo, IPermissaoRepository permissoes) : IPerfilService
{
    public async Task<List<PermissaoDto>> ListarPermissoesAsync() =>
        (await permissoes.ListarAsync()).Select(Mapper.ToDto).ToList();

    public async Task<List<PerfilDto>> ListarAsync() =>
        (await repo.ListarAsync()).Select(Mapper.ToDto).ToList();

    public async Task<PerfilDto> CriarAsync(PerfilInput input)
    {
        var nome = input.Nome.Trim();
        if (await repo.ObterPorNomeAsync(nome) != null)
            throw new RegraNegocioException("Já existe um perfil com este nome.");

        var ids = input.PermissaoIds.Distinct().ToList();
        var permsValidas = await permissoes.ListarPorIdsAsync(ids);
        if (permsValidas.Count != ids.Count)
            throw new RegraNegocioException("Uma ou mais permissões são inválidas.");

        var p = new Perfil
        {
            Nome = nome,
            Descricao = input.Descricao?.Trim(),
            Permissoes = permsValidas.Select(x => new PerfilPermissao { PermissaoId = x.Id }).ToList()
        };
        await repo.AdicionarAsync(p);
        await repo.SalvarAsync();
        return Mapper.ToDto((await repo.ObterAsync(p.Id))!);
    }

    public async Task<PerfilDto> AtualizarAsync(int id, PerfilInput input)
    {
        var p = await repo.ObterAsync(id) ?? throw new NaoEncontradoException("Perfil não encontrado.");
        var nome = input.Nome.Trim();
        var existente = await repo.ObterPorNomeAsync(nome);
        if (existente != null && existente.Id != id)
            throw new RegraNegocioException("Já existe outro perfil com este nome.");

        var ids = input.PermissaoIds.Distinct().ToList();
        var permsValidas = await permissoes.ListarPorIdsAsync(ids);
        if (permsValidas.Count != ids.Count)
            throw new RegraNegocioException("Uma ou mais permissões são inválidas.");

        p.Nome = nome;
        p.Descricao = input.Descricao?.Trim();
        p.Permissoes.Clear();
        foreach (var perm in permsValidas)
            p.Permissoes.Add(new PerfilPermissao { PerfilId = p.Id, PermissaoId = perm.Id });

        await repo.SalvarAsync();
        return Mapper.ToDto((await repo.ObterAsync(p.Id))!);
    }

    public async Task AlterarAtivoAsync(int id, bool ativo)
    {
        var p = await repo.ObterAsync(id) ?? throw new NaoEncontradoException("Perfil não encontrado.");
        if (!ativo && p.Id == 1)
            throw new RegraNegocioException("O perfil Administrador não pode ser desativado.");
        p.Ativo = ativo;
        await repo.SalvarAsync();
    }
}

public interface IUsuarioService
{
    Task<List<UsuarioDto>> ListarAsync();
    Task<SituacaoDto> SituacaoAsync(int id);
    Task<UsuarioDto> CriarAsync(UsuarioCreateInput input);
    Task<UsuarioDto> AtualizarAsync(int id, UsuarioUpdateInput input);
    Task AlterarAtivoAsync(int id, bool ativo, int usuarioLogadoId);
}

public class UsuarioService(
    IUsuarioRepository repo,
    IPerfilRepository perfis,
    IPasswordHasher<Usuario> hasher,
    IAniversarioService aniversarios) : IUsuarioService
{
    public async Task<List<UsuarioDto>> ListarAsync() =>
        (await repo.ListarAsync()).Select(Mapper.ToDto).ToList();

    public async Task<SituacaoDto> SituacaoAsync(int id)
    {
        var u = await repo.ObterComDividasAsync(id) ?? throw new NaoEncontradoException("Usuário não encontrado.");
        foreach (var d in u.Dividas) d.Usuario = u;
        var dividas = u.Dividas.OrderByDescending(d => d.CriadoEm).Select(Mapper.ToDto).ToList();
        var pendentes = dividas.Where(d => d.Status is StatusDivida.PendenteAceite or StatusDivida.Contestada).ToList();
        var abertas = dividas.Where(d => d.Status == StatusDivida.Aceita).ToList();
        var historico = dividas.Where(d => d.Status is StatusDivida.Quitada or StatusDivida.Desistida).ToList();
        return new SituacaoDto(Mapper.ToDto(u), pendentes, abertas, historico);
    }

    public async Task<UsuarioDto> CriarAsync(UsuarioCreateInput input)
    {
        var login = input.Login.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(login))
            throw new RegraNegocioException("Login inválido.");
        if (await repo.ObterPorLoginAsync(login) != null)
            throw new RegraNegocioException("Já existe um usuário com este login.");
        if (input.DataNascimento.Date > DateTime.Today)
            throw new RegraNegocioException("Data de nascimento não pode ser futura.");

        var perfil = await perfis.ObterAsync(input.PerfilId) ?? throw new RegraNegocioException("Perfil inválido.");
        if (!perfil.Ativo) throw new RegraNegocioException("Perfil inativo.");

        var u = new Usuario
        {
            Nome = input.Nome.Trim(),
            Login = login,
            DataNascimento = input.DataNascimento.Date,
            PerfilId = input.PerfilId,
            Avatar = string.IsNullOrWhiteSpace(input.Avatar) ? null : input.Avatar.Trim(),
            DeveAlterarSenha = true
        };
        u.SenhaHash = hasher.HashPassword(u, input.Senha);
        await repo.AdicionarAsync(u);
        await repo.SalvarAsync();

        await aniversarios.ProcessarUsuarioAsync(u.Id);
        return Mapper.ToDto((await repo.ObterAsync(u.Id))!);
    }

    public async Task<UsuarioDto> AtualizarAsync(int id, UsuarioUpdateInput input)
    {
        var u = await repo.ObterAsync(id) ?? throw new NaoEncontradoException("Usuário não encontrado.");
        var login = input.Login.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(login))
            throw new RegraNegocioException("Login inválido.");
        var existente = await repo.ObterPorLoginAsync(login);
        if (existente != null && existente.Id != id)
            throw new RegraNegocioException("Já existe outro usuário com este login.");
        if (input.DataNascimento.Date > DateTime.Today)
            throw new RegraNegocioException("Data de nascimento não pode ser futura.");

        var perfil = await perfis.ObterAsync(input.PerfilId) ?? throw new RegraNegocioException("Perfil inválido.");
        if (!perfil.Ativo) throw new RegraNegocioException("Perfil inativo.");

        u.Nome = input.Nome.Trim();
        u.Login = login;
        u.DataNascimento = input.DataNascimento.Date;
        u.PerfilId = input.PerfilId;
        u.Avatar = string.IsNullOrWhiteSpace(input.Avatar) ? null : input.Avatar.Trim();
        if (!string.IsNullOrWhiteSpace(input.NovaSenha))
        {
            if (input.NovaSenha.Length < 6)
                throw new RegraNegocioException("A nova senha deve ter pelo menos 6 caracteres.");
            u.SenhaHash = hasher.HashPassword(u, input.NovaSenha);
            u.DeveAlterarSenha = true;
        }

        await repo.SalvarAsync();
        await aniversarios.ProcessarUsuarioAsync(u.Id);
        return Mapper.ToDto((await repo.ObterAsync(u.Id))!);
    }

    public async Task AlterarAtivoAsync(int id, bool ativo, int usuarioLogadoId)
    {
        if (!ativo && id == usuarioLogadoId)
            throw new RegraNegocioException("Você não pode desativar o próprio usuário.");
        var u = await repo.ObterAsync(id) ?? throw new NaoEncontradoException("Usuário não encontrado.");
        u.Ativo = ativo;
        await repo.SalvarAsync();
    }
}

public interface IConfiguracaoService
{
    Task<ConfiguracaoDto> ObterAsync();
    Task<ConfiguracaoDto> AtualizarAsync(ConfiguracaoInput input);
}

public class ConfiguracaoService(IConfiguracaoRepository repo) : IConfiguracaoService
{
    public async Task<ConfiguracaoDto> ObterAsync()
    {
        var cfg = await repo.ObterAsync();
        return new ConfiguracaoDto(cfg.ExigirAceiteDivida, cfg.TipoDividaAniversario, cfg.ValorMinimoQuitacaoConjunta);
    }

    public async Task<ConfiguracaoDto> AtualizarAsync(ConfiguracaoInput input)
    {
        if (!Enum.IsDefined(input.TipoDividaAniversario))
            throw new RegraNegocioException("Tipo de dívida de aniversário inválido.");
        if (input.ValorMinimoQuitacaoConjunta <= 0)
            throw new RegraNegocioException("O valor mínimo de quitação em conjunto deve ser maior que zero.");

        var cfg = await repo.ObterAsync();
        cfg.ExigirAceiteDivida = input.ExigirAceiteDivida;
        cfg.TipoDividaAniversario = input.TipoDividaAniversario;
        cfg.ValorMinimoQuitacaoConjunta = input.ValorMinimoQuitacaoConjunta;
        await repo.SalvarAsync();
        return new ConfiguracaoDto(cfg.ExigirAceiteDivida, cfg.TipoDividaAniversario, cfg.ValorMinimoQuitacaoConjunta);
    }
}

public interface IAniversarioService
{
    Task ProcessarHojeAsync();
    Task ProcessarUsuarioAsync(int usuarioId);
}

public class AniversarioService(
    IUsuarioRepository usuarios,
    IDividaRepository dividas,
    IMotivoRepository motivos,
    IConfiguracaoRepository configuracoes) : IAniversarioService
{
    private static bool EhAniversarioHoje(DateTime nascimento, DateTime hoje)
    {
        if (nascimento.Month == hoje.Month && nascimento.Day == hoje.Day) return true;
        // Nascidos em 29/02 comemoram em 28/02 em anos não bissextos
        if (nascimento.Month == 2 && nascimento.Day == 29 && !DateTime.IsLeapYear(hoje.Year) && hoje.Month == 2 && hoje.Day == 28)
            return true;
        return false;
    }

    public async Task ProcessarHojeAsync()
    {
        var hoje = DateTime.Today;
        var ativos = await usuarios.ListarAtivosComNascimentoAsync();
        var aniversariantes = ativos.Where(u => u.DataNascimento.HasValue && EhAniversarioHoje(u.DataNascimento.Value, hoje)).ToList();
        if (aniversariantes.Count == 0) return;

        await CriarDividasAniversarioAsync(aniversariantes, hoje);
    }

    public async Task ProcessarUsuarioAsync(int usuarioId)
    {
        var hoje = DateTime.Today;
        var u = await usuarios.ObterAsync(usuarioId);
        if (u is null || !u.Ativo || !u.DataNascimento.HasValue || !EhAniversarioHoje(u.DataNascimento.Value, hoje))
            return;

        await CriarDividasAniversarioAsync([u], hoje);
    }

    private async Task CriarDividasAniversarioAsync(List<Usuario> aniversariantes, DateTime hoje)
    {
        var motivo = await motivos.ObterPorDescricaoAsync("Aniversário") ?? await motivos.ObterAsync(1);
        if (motivo is null)
        {
            motivo = new Motivo { Descricao = "Aniversário", Ativo = true };
            await motivos.AdicionarAsync(motivo);
            await motivos.SalvarAsync();
        }

        var config = await configuracoes.ObterAsync();
        var admin = await usuarios.ObterPrimeiroAdminAsync();
        var agora = DateTime.Now;
        var salvouAlgo = false;

        foreach (var u in aniversariantes)
        {
            if (await dividas.ExisteAniversarioNoAnoAsync(u.Id, motivo.Id, hoje.Year))
                continue;

            var criadoPorId = admin?.Id ?? u.Id;
            var autoAceita = !config.ExigirAceiteDivida || criadoPorId == u.Id;
            var d = new Divida
            {
                UsuarioId = u.Id,
                MotivoId = motivo.Id,
                Tipo = config.TipoDividaAniversario,
                DataAquisicao = hoje,
                Observacao = "🎂 Feliz aniversário! Dívida gerada automaticamente.",
                Status = autoAceita ? StatusDivida.Aceita : StatusDivida.PendenteAceite,
                CriadoPorId = criadoPorId,
                CriadoEm = agora
            };
            d.Interacoes.Add(new DividaInteracao
            {
                UsuarioId = criadoPorId,
                Tipo = TipoInteracao.Criada,
                Mensagem = d.Observacao,
                CriadoEm = agora
            });
            if (autoAceita)
            {
                d.Interacoes.Add(new DividaInteracao
                {
                    UsuarioId = criadoPorId,
                    Tipo = TipoInteracao.Aceita,
                    Mensagem = !config.ExigirAceiteDivida
                        ? "Lançada diretamente em aberto (aceite de dívidas desativado)."
                        : "Aceite automático (gerada para o próprio usuário).",
                    CriadoEm = agora
                });
            }

            await dividas.AdicionarAsync(d);
            salvouAlgo = true;
        }

        if (salvouAlgo)
            await dividas.SalvarAsync();
    }
}

public class AniversarioHostedService(IServiceScopeFactory scopeFactory, ILogger<AniversarioHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ExecutarVerificacaoAsync();

        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ExecutarVerificacaoAsync();
        }
    }

    private async Task ExecutarVerificacaoAsync()
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IAniversarioService>();
            await service.ProcessarHojeAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar dívidas automáticas de aniversário.");
        }
    }
}

public interface IProdutoService
{
    Task<List<ProdutoDto>> ListarAsync();
    Task<ProdutoDto> CriarAsync(ProdutoInput input);
    Task<ProdutoDto> AtualizarAsync(int id, ProdutoInput input);
    Task AlterarAtivoAsync(int id, bool ativo);
}

public class ProdutoService(IProdutoRepository repo) : IProdutoService
{
    private static ProdutoDto ToDto(Produto p) => new(p.Id, p.Nome, p.Valor, p.Ativo, p.CriadoEm);

    public async Task<List<ProdutoDto>> ListarAsync() =>
        (await repo.ListarAsync()).Select(ToDto).ToList();

    public async Task<ProdutoDto> CriarAsync(ProdutoInput input)
    {
        var nome = input.Nome.Trim();
        if (string.IsNullOrWhiteSpace(nome)) throw new RegraNegocioException("Nome do produto é obrigatório.");
        if (input.Valor <= 0) throw new RegraNegocioException("O valor do produto deve ser maior que zero.");

        var p = new Produto { Nome = nome, Valor = input.Valor };
        await repo.AdicionarAsync(p);
        await repo.SalvarAsync();
        return ToDto(p);
    }

    public async Task<ProdutoDto> AtualizarAsync(int id, ProdutoInput input)
    {
        var p = await repo.ObterAsync(id) ?? throw new NaoEncontradoException("Produto não encontrado.");
        var nome = input.Nome.Trim();
        if (string.IsNullOrWhiteSpace(nome)) throw new RegraNegocioException("Nome do produto é obrigatório.");
        if (input.Valor <= 0) throw new RegraNegocioException("O valor do produto deve ser maior que zero.");

        p.Nome = nome;
        p.Valor = input.Valor;
        await repo.SalvarAsync();
        return ToDto(p);
    }

    public async Task AlterarAtivoAsync(int id, bool ativo)
    {
        var p = await repo.ObterAsync(id) ?? throw new NaoEncontradoException("Produto não encontrado.");
        p.Ativo = ativo;
        await repo.SalvarAsync();
    }
}

public interface IGrupoPagamentoService
{
    Task<List<GrupoPagamentoDto>> ListarAsync();
    Task<GrupoPagamentoDto> CriarAsync(GrupoPagamentoCreateInput input, int criadoPorId);
    Task<GrupoPagamentoDto> ParticiparAsync(int grupoId, GrupoParticiparInput input, int usuarioId);
    Task<GrupoPagamentoDto> SairAsync(int grupoId, int usuarioId);
    Task<GrupoPagamentoDto> FecharAsync(int grupoId, int usuarioId, bool podeGerenciar);
    Task CancelarAsync(int grupoId, int usuarioId, bool podeGerenciar);
}

public class GrupoPagamentoService(
    IGrupoPagamentoRepository repo,
    IProdutoRepository produtos,
    IDividaRepository dividas,
    IConfiguracaoRepository configuracoes) : IGrupoPagamentoService
{
    private static (int TotalDividasMeta, decimal ValorPorDivida) CalcularRegraGrupo(GrupoPagamento g)
    {
        var totalDividas = g.ValorMinimoPorDivida > 0
            ? Math.Max(1, (int)Math.Floor(g.ValorMeta / g.ValorMinimoPorDivida))
            : 1;
        var valorPorDivida = Math.Round(g.ValorMeta / totalDividas, 2);
        return (totalDividas, valorPorDivida);
    }

    private static GrupoPagamentoDto ToDto(GrupoPagamento g)
    {
        var (totalDividasMeta, valorPorDivida) = CalcularRegraGrupo(g);
        var selecionadas = g.Participacoes.Sum(p => p.Dividas.Count);
        return new GrupoPagamentoDto(
            g.Id, g.ProdutoId, g.Produto?.Nome ?? "",
            g.ValorMeta, g.ValorMinimoPorDivida,
            totalDividasMeta, selecionadas, valorPorDivida,
            g.DataCompra,
            g.Observacao, g.Status,
            g.CriadoPorId, g.CriadoPor?.Nome ?? "", g.CriadoEm, g.FechadoEm,
            g.Participacoes.OrderBy(p => p.CriadoEm).Select(p => new GrupoParticipacaoDto(
                p.Id, p.UsuarioId, p.Usuario?.Nome ?? "", p.Usuario?.Avatar,
                p.ValorContribuido, p.CriadoEm,
                p.Dividas.Where(d => d.Divida != null).Select(d => new GrupoDividaResumoDto(
                    d.Divida!.Id, d.Divida.Tipo, Mapper.Descricao(d.Divida.Tipo),
                    d.Divida.Motivo?.Descricao ?? "", d.Divida.DataAquisicao
                )).ToList()
            )).ToList());
    }

    private static void FecharEQuitar(GrupoPagamento g, int usuarioId, decimal valorPorDivida)
    {
        var agora = DateTime.Now;
        var hoje = DateTime.Today;
        g.Status = StatusGrupoPagamento.Fechado;
        g.FechadoEm = agora;

        foreach (var part in g.Participacoes)
        {
            part.ValorContribuido = Math.Round(part.Dividas.Count * valorPorDivida, 2);
            foreach (var gd in part.Dividas)
            {
                var d = gd.Divida;
                if (d is null || d.Status != StatusDivida.Aceita) continue;
                d.Status = StatusDivida.Quitada;
                d.DataQuitacao = hoje;
                d.ValorPago = valorPorDivida;
                d.QuitadoPorId = usuarioId;
                d.QuitadoEm = agora;
                d.Pagamentos.Add(new Pagamento
                {
                    DividaId = d.Id,
                    DataPagamento = hoje,
                    ValorPago = valorPorDivida,
                    Observacao = $"Pagamento em grupo #{g.Id}: {g.Produto?.Nome} (R$ {valorPorDivida:N2} por dívida)",
                    Status = StatusPagamento.Aprovado,
                    CriadoPorId = part.UsuarioId,
                    CriadoEm = agora,
                    AvaliadoPorId = usuarioId,
                    AvaliadoEm = agora
                });
            }
        }
    }

    public async Task<List<GrupoPagamentoDto>> ListarAsync() =>
        (await repo.ListarAsync()).Select(ToDto).ToList();

    public async Task<GrupoPagamentoDto> CriarAsync(GrupoPagamentoCreateInput input, int criadoPorId)
    {
        var produto = await produtos.ObterAsync(input.ProdutoId) ?? throw new RegraNegocioException("Produto inválido.");
        if (!produto.Ativo) throw new RegraNegocioException("Este produto está inativo.");
        if (input.DataCompra.Date < DateTime.Today)
            throw new RegraNegocioException("A data da compra não pode ser anterior a hoje.");

        var cfg = await configuracoes.ObterAsync();
        if (produto.Valor < cfg.ValorMinimoQuitacaoConjunta)
            throw new RegraNegocioException($"O valor do produto (R$ {produto.Valor:N2}) é menor que o mínimo configurado para quitação em conjunto (R$ {cfg.ValorMinimoQuitacaoConjunta:N2}).");

        var g = new GrupoPagamento
        {
            ProdutoId = produto.Id,
            ValorMeta = produto.Valor,
            ValorMinimoPorDivida = cfg.ValorMinimoQuitacaoConjunta,
            DataCompra = input.DataCompra.Date,
            Observacao = string.IsNullOrWhiteSpace(input.Observacao) ? null : input.Observacao.Trim(),
            Status = StatusGrupoPagamento.Aberto,
            CriadoPorId = criadoPorId,
            CriadoEm = DateTime.Now
        };
        await repo.AdicionarAsync(g);
        await repo.SalvarAsync();
        return ToDto((await repo.ObterAsync(g.Id))!);
    }

    public async Task<GrupoPagamentoDto> ParticiparAsync(int grupoId, GrupoParticiparInput input, int usuarioId)
    {
        var g = await repo.ObterAsync(grupoId) ?? throw new NaoEncontradoException("Grupo de pagamento não encontrado.");
        if (g.Status != StatusGrupoPagamento.Aberto)
            throw new RegraNegocioException("Este grupo de pagamento já foi encerrado.");

        var ids = (input.DividaIds ?? []).Distinct().ToList();
        if (ids.Count == 0)
            throw new RegraNegocioException("Selecione ao menos uma dívida em aberto para participar.");

        var (totalDividasMeta, valorPorDivida) = CalcularRegraGrupo(g);
        var dividasOutros = g.Participacoes.Where(p => p.UsuarioId != usuarioId).Sum(p => p.Dividas.Count);
        var vagasRestantes = totalDividasMeta - dividasOutros;
        if (ids.Count > vagasRestantes)
            throw new RegraNegocioException($"Restam apenas {vagasRestantes} vaga(s) de dívida neste grupo.");

        var dividasSel = await dividas.ListarPorIdsAsync(ids);
        if (dividasSel.Count != ids.Count)
            throw new RegraNegocioException("Uma ou mais dívidas selecionadas não foram encontradas.");
        if (dividasSel.Any(d => d.UsuarioId != usuarioId))
            throw new RegraNegocioException("Você só pode incluir as suas próprias dívidas no grupo.");
        if (dividasSel.Any(d => d.Status != StatusDivida.Aceita))
            throw new RegraNegocioException("Apenas dívidas em aberto (aceitas) podem ser pagas em grupo.");
        if (await repo.AlgumaDividaEmOutroGrupoAbertoAsync(ids, ignorarGrupoId: grupoId))
            throw new RegraNegocioException("Uma ou mais dívidas já estão vinculadas a outro grupo aberto.");

        var valorUsuario = Math.Round(ids.Count * valorPorDivida, 2);
        var existente = g.Participacoes.FirstOrDefault(p => p.UsuarioId == usuarioId);
        if (existente != null)
        {
            existente.ValorContribuido = valorUsuario;
            existente.Dividas.Clear();
            foreach (var d in dividasSel)
                existente.Dividas.Add(new GrupoPagamentoDivida { GrupoPagamentoParticipacaoId = existente.Id, DividaId = d.Id, Divida = d });
        }
        else
        {
            var nova = new GrupoPagamentoParticipacao
            {
                GrupoPagamentoId = g.Id,
                UsuarioId = usuarioId,
                ValorContribuido = valorUsuario,
                CriadoEm = DateTime.Now,
                Dividas = dividasSel.Select(d => new GrupoPagamentoDivida { DividaId = d.Id, Divida = d }).ToList()
            };
            g.Participacoes.Add(nova);
        }

        // Fecha automaticamente quando todas as dívidas do grupo são preenchidas (valor do produto atingido)
        if (dividasOutros + ids.Count >= totalDividasMeta)
            FecharEQuitar(g, usuarioId, valorPorDivida);

        await repo.SalvarAsync();
        return ToDto((await repo.ObterAsync(g.Id))!);
    }

    public async Task<GrupoPagamentoDto> SairAsync(int grupoId, int usuarioId)
    {
        var g = await repo.ObterAsync(grupoId) ?? throw new NaoEncontradoException("Grupo de pagamento não encontrado.");
        if (g.Status != StatusGrupoPagamento.Aberto)
            throw new RegraNegocioException("Não é possível sair de um grupo já encerrado.");

        var part = g.Participacoes.FirstOrDefault(p => p.UsuarioId == usuarioId);
        if (part != null)
        {
            g.Participacoes.Remove(part);
            repo.RemoverParticipacao(part);
            await repo.SalvarAsync();
        }
        return ToDto((await repo.ObterAsync(g.Id))!);
    }

    public async Task<GrupoPagamentoDto> FecharAsync(int grupoId, int usuarioId, bool podeGerenciar)
    {
        var g = await repo.ObterAsync(grupoId) ?? throw new NaoEncontradoException("Grupo de pagamento não encontrado.");
        if (g.Status != StatusGrupoPagamento.Aberto)
            throw new RegraNegocioException("Este grupo já foi encerrado.");
        if (g.CriadoPorId != usuarioId && !podeGerenciar)
            throw new RegraNegocioException("Apenas quem criou o grupo (ou um gestor/admin) pode fechá-lo manualmente.");
        if (g.Participacoes.Count == 0)
            throw new RegraNegocioException("O grupo precisa ter ao menos um participante para ser fechado.");

        var (_, valorPorDivida) = CalcularRegraGrupo(g);
        FecharEQuitar(g, usuarioId, valorPorDivida);
        await repo.SalvarAsync();
        return ToDto((await repo.ObterAsync(g.Id))!);
    }

    public async Task CancelarAsync(int grupoId, int usuarioId, bool podeGerenciar)
    {
        var g = await repo.ObterAsync(grupoId) ?? throw new NaoEncontradoException("Grupo de pagamento não encontrado.");
        if (g.Status != StatusGrupoPagamento.Aberto)
            throw new RegraNegocioException("Este grupo já foi encerrado.");
        if (g.CriadoPorId != usuarioId && !podeGerenciar)
            throw new RegraNegocioException("Apenas quem criou o grupo (ou um gestor/admin) pode cancelá-lo.");

        g.Status = StatusGrupoPagamento.Cancelado;
        g.FechadoEm = DateTime.Now;
        await repo.SalvarAsync();
    }
}
