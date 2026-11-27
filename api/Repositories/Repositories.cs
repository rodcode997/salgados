using Microsoft.EntityFrameworkCore;
using Salgados.Api.Data;
using Salgados.Api.Entities;

namespace Salgados.Api.Repositories;

public interface IMotivoRepository
{
    Task<List<Motivo>> ListarAsync();
    Task<Motivo?> ObterAsync(int id);
    Task<Motivo?> ObterPorDescricaoAsync(string descricao);
    Task AdicionarAsync(Motivo m);
    Task SalvarAsync();
}

public interface IDividaRepository
{
    Task<List<Divida>> ListarAsync(int? usuarioId, StatusDivida? status);
    Task<List<Divida>> ListarPorIdsAsync(IEnumerable<int> ids);
    Task<Divida?> ObterAsync(int id);
    Task<Pagamento?> ObterPagamentoAsync(int pagamentoId);
    Task<bool> ExisteAniversarioNoAnoAsync(int usuarioId, int motivoId, int ano);
    Task AdicionarAsync(Divida d);
    Task SalvarAsync();
}

public interface IPermissaoRepository
{
    Task<List<Permissao>> ListarAsync();
    Task<List<Permissao>> ListarPorIdsAsync(IEnumerable<int> ids);
}

public interface IPerfilRepository
{
    Task<List<Perfil>> ListarAsync();
    Task<Perfil?> ObterAsync(int id);
    Task<Perfil?> ObterPorNomeAsync(string nome);
    Task AdicionarAsync(Perfil p);
    Task SalvarAsync();
}

public interface IUsuarioRepository
{
    Task<List<Usuario>> ListarAsync();
    Task<List<Usuario>> ListarAtivosComNascimentoAsync();
    Task<Usuario?> ObterAsync(int id);
    Task<Usuario?> ObterComDividasAsync(int id);
    Task<Usuario?> ObterPorLoginAsync(string login);
    Task<Usuario?> ObterPrimeiroAdminAsync();
    Task AdicionarAsync(Usuario u);
    Task SalvarAsync();
}

public interface IConfiguracaoRepository
{
    Task<ConfiguracaoSistema> ObterAsync();
    Task SalvarAsync();
}

public interface IProdutoRepository
{
    Task<List<Produto>> ListarAsync();
    Task<Produto?> ObterAsync(int id);
    Task AdicionarAsync(Produto p);
    Task SalvarAsync();
}

public interface IGrupoPagamentoRepository
{
    Task<List<GrupoPagamento>> ListarAsync();
    Task<GrupoPagamento?> ObterAsync(int id);
    Task<bool> AlgumaDividaEmOutroGrupoAbertoAsync(IEnumerable<int> dividaIds, int? ignorarGrupoId = null);
    Task AdicionarAsync(GrupoPagamento g);
    void RemoverParticipacao(GrupoPagamentoParticipacao p);
    Task SalvarAsync();
}

public class MotivoRepository(AppDbContext db) : IMotivoRepository
{
    public Task<List<Motivo>> ListarAsync() => db.Motivos.OrderBy(m => m.Descricao).ToListAsync();
    public Task<Motivo?> ObterAsync(int id) => db.Motivos.FindAsync(id).AsTask();
    public Task<Motivo?> ObterPorDescricaoAsync(string descricao) =>
        db.Motivos.FirstOrDefaultAsync(m => m.Descricao.ToLower() == descricao.ToLower());
    public async Task AdicionarAsync(Motivo m) => await db.Motivos.AddAsync(m);
    public Task SalvarAsync() => db.SaveChangesAsync();
}

public class DividaRepository(AppDbContext db) : IDividaRepository
{
    private IQueryable<Divida> QueryCompleta() => db.Dividas
        .Include(d => d.Usuario)
        .Include(d => d.CriadoPor)
        .Include(d => d.QuitadoPor)
        .Include(d => d.Motivo)
        .Include(d => d.Interacoes).ThenInclude(i => i.Usuario)
        .Include(d => d.Pagamentos).ThenInclude(p => p.CriadoPor)
        .Include(d => d.Pagamentos).ThenInclude(p => p.AvaliadoPor)
        .AsSplitQuery();

    public Task<List<Divida>> ListarAsync(int? usuarioId, StatusDivida? status)
    {
        var q = QueryCompleta();
        if (usuarioId.HasValue) q = q.Where(d => d.UsuarioId == usuarioId);
        if (status.HasValue) q = q.Where(d => d.Status == status.Value);
        return q.OrderByDescending(d => d.CriadoEm).ThenByDescending(d => d.Id).ToListAsync();
    }

    public Task<List<Divida>> ListarPorIdsAsync(IEnumerable<int> ids) =>
        db.Dividas.Include(d => d.Pagamentos).Where(d => ids.Contains(d.Id)).ToListAsync();

    public Task<Divida?> ObterAsync(int id) =>
        QueryCompleta().FirstOrDefaultAsync(d => d.Id == id);

    public Task<Pagamento?> ObterPagamentoAsync(int pagamentoId) =>
        db.Pagamentos
            .Include(p => p.CriadoPor)
            .Include(p => p.AvaliadoPor)
            .Include(p => p.Divida!).ThenInclude(d => d.Usuario)
            .Include(p => p.Divida!).ThenInclude(d => d.CriadoPor)
            .Include(p => p.Divida!).ThenInclude(d => d.QuitadoPor)
            .Include(p => p.Divida!).ThenInclude(d => d.Motivo)
            .Include(p => p.Divida!).ThenInclude(d => d.Interacoes).ThenInclude(i => i.Usuario)
            .Include(p => p.Divida!).ThenInclude(d => d.Pagamentos).ThenInclude(pg => pg.CriadoPor)
            .Include(p => p.Divida!).ThenInclude(d => d.Pagamentos).ThenInclude(pg => pg.AvaliadoPor)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == pagamentoId);

    public Task<bool> ExisteAniversarioNoAnoAsync(int usuarioId, int motivoId, int ano) =>
        db.Dividas.AnyAsync(d =>
            d.UsuarioId == usuarioId &&
            d.MotivoId == motivoId &&
            d.DataAquisicao.Year == ano);

    public async Task AdicionarAsync(Divida d) => await db.Dividas.AddAsync(d);
    public Task SalvarAsync() => db.SaveChangesAsync();
}

public class PermissaoRepository(AppDbContext db) : IPermissaoRepository
{
    public Task<List<Permissao>> ListarAsync() =>
        db.Permissoes.OrderBy(p => p.Modulo).ThenBy(p => p.Id).ToListAsync();
    public Task<List<Permissao>> ListarPorIdsAsync(IEnumerable<int> ids) =>
        db.Permissoes.Where(p => ids.Contains(p.Id)).ToListAsync();
}

public class PerfilRepository(AppDbContext db) : IPerfilRepository
{
    public Task<List<Perfil>> ListarAsync() =>
        db.Perfis
            .Include(p => p.Permissoes).ThenInclude(pp => pp.Permissao)
            .Include(p => p.Usuarios)
            .OrderBy(p => p.Nome)
            .ToListAsync();
    public Task<Perfil?> ObterAsync(int id) =>
        db.Perfis
            .Include(p => p.Permissoes).ThenInclude(pp => pp.Permissao)
            .Include(p => p.Usuarios)
            .FirstOrDefaultAsync(p => p.Id == id);
    public Task<Perfil?> ObterPorNomeAsync(string nome) =>
        db.Perfis.FirstOrDefaultAsync(p => p.Nome.ToLower() == nome.ToLower());
    public async Task AdicionarAsync(Perfil p) => await db.Perfis.AddAsync(p);
    public Task SalvarAsync() => db.SaveChangesAsync();
}

public class UsuarioRepository(AppDbContext db) : IUsuarioRepository
{
    public Task<List<Usuario>> ListarAsync() =>
        db.Usuarios.Include(u => u.Perfil).Include(u => u.Dividas).OrderBy(u => u.Nome).ToListAsync();
    public Task<List<Usuario>> ListarAtivosComNascimentoAsync() =>
        db.Usuarios.Where(u => u.Ativo && u.DataNascimento != null).ToListAsync();
    public Task<Usuario?> ObterAsync(int id) =>
        db.Usuarios
            .Include(u => u.Perfil!).ThenInclude(p => p.Permissoes).ThenInclude(pp => pp.Permissao)
            .Include(u => u.Dividas)
            .FirstOrDefaultAsync(u => u.Id == id);
    public Task<Usuario?> ObterComDividasAsync(int id) =>
        db.Usuarios
            .Include(u => u.Perfil)
            .Include(u => u.Dividas).ThenInclude(d => d.Motivo)
            .Include(u => u.Dividas).ThenInclude(d => d.CriadoPor)
            .Include(u => u.Dividas).ThenInclude(d => d.QuitadoPor)
            .Include(u => u.Dividas).ThenInclude(d => d.Interacoes).ThenInclude(i => i.Usuario)
            .Include(u => u.Dividas).ThenInclude(d => d.Pagamentos).ThenInclude(p => p.CriadoPor)
            .Include(u => u.Dividas).ThenInclude(d => d.Pagamentos).ThenInclude(p => p.AvaliadoPor)
            .AsSplitQuery()
            .FirstOrDefaultAsync(u => u.Id == id);
    public Task<Usuario?> ObterPorLoginAsync(string login) =>
        db.Usuarios
            .Include(u => u.Perfil!).ThenInclude(p => p.Permissoes).ThenInclude(pp => pp.Permissao)
            .FirstOrDefaultAsync(u => u.Login.ToLower() == login.ToLower());
    public Task<Usuario?> ObterPrimeiroAdminAsync() =>
        db.Usuarios.OrderBy(u => u.PerfilId == 1 ? 0 : 1).ThenBy(u => u.Id).FirstOrDefaultAsync();
    public async Task AdicionarAsync(Usuario u) => await db.Usuarios.AddAsync(u);
    public Task SalvarAsync() => db.SaveChangesAsync();
}

public class ConfiguracaoRepository(AppDbContext db) : IConfiguracaoRepository
{
    public async Task<ConfiguracaoSistema> ObterAsync()
    {
        var cfg = await db.Configuracoes.FirstOrDefaultAsync(c => c.Id == 1);
        if (cfg is null)
        {
            cfg = new ConfiguracaoSistema
            {
                Id = 1,
                ExigirAceiteDivida = true,
                TipoDividaAniversario = TipoDivida.CentoSalgado,
                ValorMinimoQuitacaoConjunta = 30m
            };
            await db.Configuracoes.AddAsync(cfg);
            await db.SaveChangesAsync();
        }
        return cfg;
    }

    public Task SalvarAsync() => db.SaveChangesAsync();
}

public class ProdutoRepository(AppDbContext db) : IProdutoRepository
{
    public Task<List<Produto>> ListarAsync() => db.Produtos.OrderBy(p => p.Nome).ToListAsync();
    public Task<Produto?> ObterAsync(int id) => db.Produtos.FindAsync(id).AsTask();
    public async Task AdicionarAsync(Produto p) => await db.Produtos.AddAsync(p);
    public Task SalvarAsync() => db.SaveChangesAsync();
}

public class GrupoPagamentoRepository(AppDbContext db) : IGrupoPagamentoRepository
{
    private IQueryable<GrupoPagamento> QueryCompleta() => db.GruposPagamento
        .Include(g => g.Produto)
        .Include(g => g.CriadoPor)
        .Include(g => g.Participacoes).ThenInclude(p => p.Usuario)
        .Include(g => g.Participacoes).ThenInclude(p => p.Dividas).ThenInclude(d => d.Divida!).ThenInclude(d => d.Motivo)
        .Include(g => g.Participacoes).ThenInclude(p => p.Dividas).ThenInclude(d => d.Divida!).ThenInclude(d => d.Pagamentos)
        .AsSplitQuery();

    public Task<List<GrupoPagamento>> ListarAsync() =>
        QueryCompleta()
            .Where(g => g.Status == StatusGrupoPagamento.Aberto || g.Status == StatusGrupoPagamento.Fechado)
            .OrderBy(g => g.Status)
            .ThenByDescending(g => g.CriadoEm)
            .Take(20)
            .ToListAsync();

    public Task<GrupoPagamento?> ObterAsync(int id) =>
        QueryCompleta().FirstOrDefaultAsync(g => g.Id == id);

    public Task<bool> AlgumaDividaEmOutroGrupoAbertoAsync(IEnumerable<int> dividaIds, int? ignorarGrupoId = null) =>
        db.GrupoPagamentoDividas.AnyAsync(gd =>
            dividaIds.Contains(gd.DividaId) &&
            gd.Participacao!.GrupoPagamento!.Status == StatusGrupoPagamento.Aberto &&
            (!ignorarGrupoId.HasValue || gd.Participacao.GrupoPagamentoId != ignorarGrupoId.Value));

    public async Task AdicionarAsync(GrupoPagamento g) => await db.GruposPagamento.AddAsync(g);
    public void RemoverParticipacao(GrupoPagamentoParticipacao p) => db.GrupoPagamentoParticipacoes.Remove(p);
    public Task SalvarAsync() => db.SaveChangesAsync();
}
