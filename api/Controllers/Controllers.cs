using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salgados.Api.Dtos;
using Salgados.Api.Entities;
using Salgados.Api.Services;

namespace Salgados.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService service) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public Task<LoginDto> Login(LoginInput input) => service.LoginAsync(input);

    [HttpGet("me")]
    [Authorize]
    public Task<UsuarioSessaoDto> Me() => service.ObterSessaoAsync(ObterUsuarioId());

    [HttpPost("alterar-senha")]
    [Authorize]
    public async Task<IActionResult> AlterarSenha(AlterarSenhaInput input)
    {
        await service.AlterarSenhaAsync(ObterUsuarioId(), input);
        return NoContent();
    }

    [HttpPut("avatar")]
    [Authorize]
    public Task<UsuarioSessaoDto> AlterarAvatar(AlterarAvatarInput input) =>
        service.AlterarAvatarAsync(ObterUsuarioId(), input);

    [HttpPut("perfil")]
    [Authorize]
    public Task<UsuarioSessaoDto> AlterarPerfil(AlterarPerfilInput input) =>
        service.AlterarPerfilAsync(ObterUsuarioId(), input);

    [HttpPost("primeiro-acesso")]
    [Authorize]
    public Task<UsuarioSessaoDto> PrimeiroAcesso(PrimeiroAcessoInput input) =>
        service.ConcluirPrimeiroAcessoAsync(ObterUsuarioId(), input);

    [HttpPost("aceitar-lei")]
    [Authorize]
    public Task<UsuarioSessaoDto> AceitarLei() =>
        service.AceitarLeiAsync(ObterUsuarioId());

    private int ObterUsuarioId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new NaoAutorizadoException("Token inválido."));
}

[ApiController]
[Route("api/motivos")]
[Authorize]
public class MotivosController(IMotivoService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissoesCodigos.MotivosVer)]
    public Task<List<MotivoDto>> Listar() => service.ListarAsync();

    [HttpPost]
    [Authorize(Policy = PermissoesCodigos.MotivosGerenciar)]
    public Task<MotivoDto> Criar(MotivoInput input) => service.CriarAsync(input);

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissoesCodigos.MotivosGerenciar)]
    public Task<MotivoDto> Atualizar(int id, MotivoInput input) => service.AtualizarAsync(id, input);

    [HttpPatch("{id:int}/ativo/{ativo:bool}")]
    [Authorize(Policy = PermissoesCodigos.MotivosGerenciar)]
    public async Task<IActionResult> Ativo(int id, bool ativo) { await service.AlterarAtivoAsync(id, ativo); return NoContent(); }
}

[ApiController]
[Route("api/dividas")]
[Authorize]
public class DividasController(IDividaService service) : ControllerBase
{
    private int ObterUsuarioId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new NaoAutorizadoException("Token inválido."));

    [HttpGet]
    [Authorize(Policy = PermissoesCodigos.DividasVer)]
    public Task<List<DividaDto>> Listar([FromQuery] int? usuarioId, [FromQuery] StatusDivida? status) =>
        service.ListarAsync(usuarioId, status);

    [HttpPost]
    [Authorize(Policy = PermissoesCodigos.DividasCriar)]
    public Task<DividaDto> Criar(DividaInput input) => service.CriarAsync(input, ObterUsuarioId());

    [HttpPost("{id:int}/aceitar")]
    public Task<DividaDto> Aceitar(int id) => service.AceitarAsync(id, ObterUsuarioId());

    [HttpPost("{id:int}/contestar")]
    public Task<DividaDto> Contestar(int id, MensagemInput input) => service.ContestarAsync(id, input, ObterUsuarioId());

    [HttpPost("{id:int}/complementar")]
    public Task<DividaDto> Complementar(int id, MensagemInput input) => service.ComplementarAsync(id, input, ObterUsuarioId());

    [HttpPost("{id:int}/desistir")]
    public Task<DividaDto> Desistir(int id, MensagemOpcionalInput input) => service.DesistirAsync(id, input, ObterUsuarioId());

    [HttpPost("{id:int}/pagamentos")]
    public Task<DividaDto> RegistrarPagamento(int id, PagamentoInput input)
    {
        var podeAprovar = User.HasClaim("perm", PermissoesCodigos.DividasQuitar);
        return service.RegistrarPagamentoAsync(id, input, ObterUsuarioId(), aprovarDireto: podeAprovar);
    }

    [HttpPost("pagamentos/{pagamentoId:int}/aprovar")]
    [Authorize(Policy = PermissoesCodigos.DividasQuitar)]
    public Task<DividaDto> AprovarPagamento(int pagamentoId) =>
        service.AprovarPagamentoAsync(pagamentoId, ObterUsuarioId());

    [HttpPost("pagamentos/{pagamentoId:int}/recusar")]
    [Authorize(Policy = PermissoesCodigos.DividasQuitar)]
    public Task<DividaDto> RecusarPagamento(int pagamentoId, MensagemInput input) =>
        service.RecusarPagamentoAsync(pagamentoId, input, ObterUsuarioId());
}

[ApiController]
[Route("api/permissoes")]
[Authorize(Policy = PermissoesCodigos.PerfisGerenciar)]
public class PermissoesController(IPerfilService service) : ControllerBase
{
    [HttpGet]
    public Task<List<PermissaoDto>> Listar() => service.ListarPermissoesAsync();
}

[ApiController]
[Route("api/perfis")]
[Authorize]
public class PerfisController(IPerfilService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "PerfisOuUsuarios")]
    public Task<List<PerfilDto>> Listar() => service.ListarAsync();

    [HttpPost]
    [Authorize(Policy = PermissoesCodigos.PerfisGerenciar)]
    public Task<PerfilDto> Criar(PerfilInput input) => service.CriarAsync(input);

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissoesCodigos.PerfisGerenciar)]
    public Task<PerfilDto> Atualizar(int id, PerfilInput input) => service.AtualizarAsync(id, input);

    [HttpPatch("{id:int}/ativo/{ativo:bool}")]
    [Authorize(Policy = PermissoesCodigos.PerfisGerenciar)]
    public async Task<IActionResult> Ativo(int id, bool ativo) { await service.AlterarAtivoAsync(id, ativo); return NoContent(); }
}

[ApiController]
[Route("api/usuarios")]
[Authorize]
public class UsuariosController(IUsuarioService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissoesCodigos.UsuariosVer)]
    public Task<List<UsuarioDto>> Listar() => service.ListarAsync();

    [HttpGet("{id:int}/situacao")]
    [Authorize(Policy = PermissoesCodigos.UsuariosVer)]
    public Task<SituacaoDto> Situacao(int id) => service.SituacaoAsync(id);

    [HttpPost]
    [Authorize(Policy = PermissoesCodigos.UsuariosGerenciar)]
    public Task<UsuarioDto> Criar(UsuarioCreateInput input) => service.CriarAsync(input);

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissoesCodigos.UsuariosGerenciar)]
    public Task<UsuarioDto> Atualizar(int id, UsuarioUpdateInput input) => service.AtualizarAsync(id, input);

    [HttpPatch("{id:int}/ativo/{ativo:bool}")]
    [Authorize(Policy = PermissoesCodigos.UsuariosGerenciar)]
    public async Task<IActionResult> Ativo(int id, bool ativo)
    {
        var logadoId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        await service.AlterarAtivoAsync(id, ativo, logadoId);
        return NoContent();
    }
}

[ApiController]
[Route("api/configuracoes")]
[Authorize]
public class ConfiguracoesController(IConfiguracaoService service) : ControllerBase
{
    [HttpGet]
    public Task<ConfiguracaoDto> Obter() => service.ObterAsync();

    [HttpPut]
    [Authorize(Policy = PermissoesCodigos.PerfisGerenciar)]
    public Task<ConfiguracaoDto> Atualizar(ConfiguracaoInput input) => service.AtualizarAsync(input);
}

[ApiController]
[Route("api/produtos")]
[Authorize]
public class ProdutosController(IProdutoService service) : ControllerBase
{
    [HttpGet]
    public Task<List<ProdutoDto>> Listar() => service.ListarAsync();

    [HttpPost]
    [Authorize(Policy = PermissoesCodigos.MotivosGerenciar)]
    public Task<ProdutoDto> Criar(ProdutoInput input) => service.CriarAsync(input);

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissoesCodigos.MotivosGerenciar)]
    public Task<ProdutoDto> Atualizar(int id, ProdutoInput input) => service.AtualizarAsync(id, input);

    [HttpPatch("{id:int}/ativo/{ativo:bool}")]
    [Authorize(Policy = PermissoesCodigos.MotivosGerenciar)]
    public async Task<IActionResult> Ativo(int id, bool ativo) { await service.AlterarAtivoAsync(id, ativo); return NoContent(); }
}

[ApiController]
[Route("api/grupos-pagamento")]
[Authorize]
public class GruposPagamentoController(IGrupoPagamentoService service) : ControllerBase
{
    private int ObterUsuarioId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new NaoAutorizadoException("Token inválido."));

    [HttpGet]
    public Task<List<GrupoPagamentoDto>> Listar() => service.ListarAsync();

    [HttpPost]
    public Task<GrupoPagamentoDto> Criar(GrupoPagamentoCreateInput input) => service.CriarAsync(input, ObterUsuarioId());

    [HttpPost("{id:int}/participar")]
    public Task<GrupoPagamentoDto> Participar(int id, GrupoParticiparInput input) =>
        service.ParticiparAsync(id, input, ObterUsuarioId());

    [HttpDelete("{id:int}/participar")]
    public Task<GrupoPagamentoDto> Sair(int id) => service.SairAsync(id, ObterUsuarioId());

    [HttpPost("{id:int}/fechar")]
    public Task<GrupoPagamentoDto> Fechar(int id)
    {
        var podeGerenciar = User.HasClaim("perm", PermissoesCodigos.DividasQuitar);
        return service.FecharAsync(id, ObterUsuarioId(), podeGerenciar);
    }

    [HttpPost("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(int id)
    {
        var podeGerenciar = User.HasClaim("perm", PermissoesCodigos.DividasQuitar);
        await service.CancelarAsync(id, ObterUsuarioId(), podeGerenciar);
        return NoContent();
    }
}

[ApiController]
[Route("api/lei-base")]
[Authorize]
public class LeiBaseController(ILeiBaseService service) : ControllerBase
{
    private int ObterUsuarioId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new NaoAutorizadoException("Token inválido."));

    [HttpGet]
    public Task<LeiBaseDto> Obter() => service.ObterAsync();

    [HttpPut]
    [Authorize(Policy = PermissoesCodigos.PerfisGerenciar)]
    public Task<LeiBaseDto> Atualizar(LeiBaseInput input) => service.AtualizarAsync(input, ObterUsuarioId());

    [HttpPost("pdf")]
    [Authorize(Policy = PermissoesCodigos.PerfisGerenciar)]
    [RequestSizeLimit(10_485_760)]
    public async Task<LeiBaseDto> AtualizarPdf([FromForm] string titulo, [FromForm] IFormFile arquivo)
    {
        if (arquivo is null || arquivo.Length == 0)
            throw new RegraNegocioException("Selecione um arquivo PDF.");
        using var ms = new MemoryStream();
        await arquivo.CopyToAsync(ms);
        return await service.AtualizarPdfAsync(titulo, arquivo.FileName, ms.ToArray(), ObterUsuarioId());
    }

    [HttpGet("pdf")]
    public async Task<IActionResult> BaixarPdf()
    {
        var (bytes, nome) = await service.ObterPdfAsync();
        Response.Headers.ContentDisposition = $"inline; filename=\"{nome}\"";
        return File(bytes, "application/pdf");
    }
}
