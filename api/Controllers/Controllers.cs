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
