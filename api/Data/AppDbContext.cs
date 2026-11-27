using Microsoft.EntityFrameworkCore;
using Salgados.Api.Entities;

namespace Salgados.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Motivo> Motivos => Set<Motivo>();
    public DbSet<Divida> Dividas => Set<Divida>();
    public DbSet<DividaInteracao> DividaInteracoes => Set<DividaInteracao>();
    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();
    public DbSet<Permissao> Permissoes => Set<Permissao>();
    public DbSet<Perfil> Perfis => Set<Perfil>();
    public DbSet<PerfilPermissao> PerfilPermissoes => Set<PerfilPermissao>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<ConfiguracaoSistema> Configuracoes => Set<ConfiguracaoSistema>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Motivo>(e =>
        {
            e.Property(m => m.Descricao).HasMaxLength(200).IsRequired();
            e.HasData(
                new Motivo { Id = 1, Descricao = "Aniversário" },
                new Motivo { Id = 2, Descricao = "Atraso em reunião" },
                new Motivo { Id = 3, Descricao = "Quebrou o build" },
                new Motivo { Id = 4, Descricao = "Bug em produção" },
                new Motivo { Id = 5, Descricao = "Esqueceu de bloquear o computador" });
        });

        b.Entity<Divida>(e =>
        {
            e.Property(d => d.ValorPago).HasPrecision(10, 2);
            e.Property(d => d.Observacao).HasMaxLength(500);
            e.Ignore(d => d.Quitada);
            e.Ignore(d => d.Dias);
            e.HasOne(d => d.Usuario).WithMany(u => u.Dividas).HasForeignKey(d => d.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(d => d.CriadoPor).WithMany().HasForeignKey(d => d.CriadoPorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(d => d.QuitadoPor).WithMany().HasForeignKey(d => d.QuitadoPorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(d => d.Motivo).WithMany().HasForeignKey(d => d.MotivoId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<DividaInteracao>(e =>
        {
            e.Property(i => i.Mensagem).HasMaxLength(600);
            e.HasOne(i => i.Divida).WithMany(d => d.Interacoes).HasForeignKey(i => i.DividaId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(i => i.Usuario).WithMany().HasForeignKey(i => i.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Pagamento>(e =>
        {
            e.Property(p => p.ValorPago).HasPrecision(10, 2);
            e.Property(p => p.Observacao).HasMaxLength(500);
            e.Property(p => p.MotivoRecusa).HasMaxLength(500);
            e.HasOne(p => p.Divida).WithMany(d => d.Pagamentos).HasForeignKey(p => p.DividaId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.CriadoPor).WithMany().HasForeignKey(p => p.CriadoPorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.AvaliadoPor).WithMany().HasForeignKey(p => p.AvaliadoPorId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Permissao>(e =>
        {
            e.Property(p => p.Codigo).HasMaxLength(80).IsRequired();
            e.Property(p => p.Descricao).HasMaxLength(200).IsRequired();
            e.Property(p => p.Modulo).HasMaxLength(60).IsRequired();
            e.HasIndex(p => p.Codigo).IsUnique();
            e.HasData(
                new Permissao { Id = 1, Codigo = PermissoesCodigos.DividasVer, Descricao = "Visualizar dívidas e painel", Modulo = "Dívidas" },
                new Permissao { Id = 2, Codigo = PermissoesCodigos.DividasCriar, Descricao = "Registrar novas dívidas", Modulo = "Dívidas" },
                new Permissao { Id = 3, Codigo = PermissoesCodigos.DividasQuitar, Descricao = "Aprovar/recusar ou cadastrar pagamento aprovado", Modulo = "Dívidas" },
                new Permissao { Id = 4, Codigo = PermissoesCodigos.UsuariosVer, Descricao = "Visualizar equipe e histórico de dívidas", Modulo = "Usuários" },
                new Permissao { Id = 5, Codigo = PermissoesCodigos.UsuariosGerenciar, Descricao = "Cadastrar, editar e ativar/desativar usuários", Modulo = "Usuários" },
                new Permissao { Id = 6, Codigo = PermissoesCodigos.MotivosVer, Descricao = "Visualizar motivos", Modulo = "Motivos" },
                new Permissao { Id = 7, Codigo = PermissoesCodigos.MotivosGerenciar, Descricao = "Cadastrar e ativar/desativar motivos", Modulo = "Motivos" },
                new Permissao { Id = 8, Codigo = PermissoesCodigos.PerfisGerenciar, Descricao = "Gerenciar perfis e permissões", Modulo = "Segurança" }
            );
        });

        b.Entity<Perfil>(e =>
        {
            e.Property(p => p.Nome).HasMaxLength(100).IsRequired();
            e.Property(p => p.Descricao).HasMaxLength(300);
            e.HasIndex(p => p.Nome).IsUnique();
            e.HasData(
                new Perfil { Id = 1, Nome = "Administrador", Descricao = "Acesso total ao sistema, usuários, perfis e aprovação de pagamentos", Ativo = true },
                new Perfil { Id = 2, Nome = "Gestor", Descricao = "Gerencia dívidas, pagamentos e motivos", Ativo = true },
                new Perfil { Id = 3, Nome = "Colaborador", Descricao = "Registra dívidas, aceita/contesta e informa pagamentos", Ativo = true }
            );
        });

        b.Entity<PerfilPermissao>(e =>
        {
            e.HasKey(pp => new { pp.PerfilId, pp.PermissaoId });
            e.HasOne(pp => pp.Perfil).WithMany(p => p.Permissoes).HasForeignKey(pp => pp.PerfilId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(pp => pp.Permissao).WithMany().HasForeignKey(pp => pp.PermissaoId).OnDelete(DeleteBehavior.Cascade);
            e.HasData(
                // Administrador: todas (1..8)
                new PerfilPermissao { PerfilId = 1, PermissaoId = 1 },
                new PerfilPermissao { PerfilId = 1, PermissaoId = 2 },
                new PerfilPermissao { PerfilId = 1, PermissaoId = 3 },
                new PerfilPermissao { PerfilId = 1, PermissaoId = 4 },
                new PerfilPermissao { PerfilId = 1, PermissaoId = 5 },
                new PerfilPermissao { PerfilId = 1, PermissaoId = 6 },
                new PerfilPermissao { PerfilId = 1, PermissaoId = 7 },
                new PerfilPermissao { PerfilId = 1, PermissaoId = 8 },
                // Gestor: 1, 2, 3, 4, 6, 7
                new PerfilPermissao { PerfilId = 2, PermissaoId = 1 },
                new PerfilPermissao { PerfilId = 2, PermissaoId = 2 },
                new PerfilPermissao { PerfilId = 2, PermissaoId = 3 },
                new PerfilPermissao { PerfilId = 2, PermissaoId = 4 },
                new PerfilPermissao { PerfilId = 2, PermissaoId = 6 },
                new PerfilPermissao { PerfilId = 2, PermissaoId = 7 },
                // Colaborador: 1, 2, 4, 6 (pode ver e registrar dívidas para outros)
                new PerfilPermissao { PerfilId = 3, PermissaoId = 1 },
                new PerfilPermissao { PerfilId = 3, PermissaoId = 2 },
                new PerfilPermissao { PerfilId = 3, PermissaoId = 4 },
                new PerfilPermissao { PerfilId = 3, PermissaoId = 6 }
            );
        });

        b.Entity<Usuario>(e =>
        {
            e.Property(u => u.Nome).HasMaxLength(150).IsRequired();
            e.Property(u => u.Login).HasMaxLength(100).IsRequired();
            e.Property(u => u.SenhaHash).IsRequired();
            e.Property(u => u.Avatar).HasMaxLength(500);
            e.HasIndex(u => u.Login).IsUnique();
            e.HasOne(u => u.Perfil).WithMany(p => p.Usuarios).HasForeignKey(u => u.PerfilId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ConfiguracaoSistema>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasData(new ConfiguracaoSistema { Id = 1, ExigirAceiteDivida = true, TipoDividaAniversario = TipoDivida.CentoSalgado });
        });
    }
}
