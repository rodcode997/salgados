const BASE = import.meta.env.VITE_API_URL || (import.meta.env.DEV ? 'http://localhost:5000/api' : '/api')
const TOKEN_KEY = 'salgados_token'

export const getToken = () => localStorage.getItem(TOKEN_KEY)
export const setToken = (t) => (t ? localStorage.setItem(TOKEN_KEY, t) : localStorage.removeItem(TOKEN_KEY))

let onUnauthorized = null
export const setOnUnauthorized = (fn) => { onUnauthorized = fn }

async function req(path, options = {}) {
  const token = getToken()
  const headers = { 'Content-Type': 'application/json', ...(options.headers || {}) }
  if (token) headers.Authorization = `Bearer ${token}`

  const res = await fetch(BASE + path, {
    ...options,
    headers,
    body: options.body ? JSON.stringify(options.body) : undefined
  })
  if (!res.ok) {
    if (res.status === 401 && path !== '/auth/login' && onUnauthorized) onUnauthorized()
    let msg = res.status === 403 ? 'Você não tem permissão para esta ação.' : `Erro ${res.status}`
    try {
      const j = await res.json()
      msg = j.erro || j.title || msg
      if (j.errors) msg += ': ' + Object.values(j.errors).flat().join(' ')
    } catch { }
    throw new Error(msg)
  }
  return res.status === 204 ? null : res.json()
}

export const api = {
  // Auth
  login: (login, senha) => req('/auth/login', { method: 'POST', body: { login, senha } }),
  me: () => req('/auth/me'),
  alterarSenha: (d) => req('/auth/alterar-senha', { method: 'POST', body: d }),
  alterarAvatar: (avatar) => req('/auth/avatar', { method: 'PUT', body: { avatar } }),
  alterarPerfil: (d) => req('/auth/perfil', { method: 'PUT', body: d }),
  primeiroAcesso: (d) => req('/auth/primeiro-acesso', { method: 'POST', body: d }),
  aceitarLei: () => req('/auth/aceitar-lei', { method: 'POST' }),

  // Usuários (cada usuário é uma pessoa da empresa)
  usuarios: () => req('/usuarios'),
  situacao: (id) => req(`/usuarios/${id}/situacao`),
  criarUsuario: (d) => req('/usuarios', { method: 'POST', body: d }),
  atualizarUsuario: (id, d) => req(`/usuarios/${id}`, { method: 'PUT', body: d }),
  ativoUsuario: (id, ativo) => req(`/usuarios/${id}/ativo/${ativo}`, { method: 'PATCH' }),

  // Motivos
  motivos: () => req('/motivos'),
  criarMotivo: (descricao) => req('/motivos', { method: 'POST', body: { descricao } }),
  ativoMotivo: (id, ativo) => req(`/motivos/${id}/ativo/${ativo}`, { method: 'PATCH' }),

  // Dívidas e Fluxo de Aceite / Contestação / Pagamento
  dividas: (q = '') => req('/dividas' + q),
  criarDivida: (d) => req('/dividas', { method: 'POST', body: d }),
  aceitarDivida: (id) => req(`/dividas/${id}/aceitar`, { method: 'POST' }),
  contestarDivida: (id, mensagem) => req(`/dividas/${id}/contestar`, { method: 'POST', body: { mensagem } }),
  complementarDivida: (id, mensagem) => req(`/dividas/${id}/complementar`, { method: 'POST', body: { mensagem } }),
  desistirDivida: (id, mensagem) => req(`/dividas/${id}/desistir`, { method: 'POST', body: { mensagem: mensagem || null } }),
  registrarPagamento: (dividaId, d) => req(`/dividas/${dividaId}/pagamentos`, { method: 'POST', body: d }),
  aprovarPagamento: (pagamentoId) => req(`/dividas/pagamentos/${pagamentoId}/aprovar`, { method: 'POST' }),
  recusarPagamento: (pagamentoId, mensagem) => req(`/dividas/pagamentos/${pagamentoId}/recusar`, { method: 'POST', body: { mensagem } }),

  // Perfis e Permissões
  permissoes: () => req('/permissoes'),
  perfis: () => req('/perfis'),
  criarPerfil: (d) => req('/perfis', { method: 'POST', body: d }),
  atualizarPerfil: (id, d) => req(`/perfis/${id}`, { method: 'PUT', body: d }),
  ativoPerfil: (id, ativo) => req(`/perfis/${id}/ativo/${ativo}`, { method: 'PATCH' }),

  // Configurações do sistema
  configuracoes: () => req('/configuracoes'),
  atualizarConfiguracoes: (d) => req('/configuracoes', { method: 'PUT', body: d }),

  // Produtos (para pagamento em conjunto)
  produtos: () => req('/produtos'),
  criarProduto: (d) => req('/produtos', { method: 'POST', body: d }),
  atualizarProduto: (id, d) => req(`/produtos/${id}`, { method: 'PUT', body: d }),
  ativoProduto: (id, ativo) => req(`/produtos/${id}/ativo/${ativo}`, { method: 'PATCH' }),

  // Grupos de pagamento em conjunto
  gruposPagamento: () => req('/grupos-pagamento'),
  criarGrupoPagamento: (d) => req('/grupos-pagamento', { method: 'POST', body: d }),
  participarGrupo: (id, d) => req(`/grupos-pagamento/${id}/participar`, { method: 'POST', body: d }),
  sairGrupo: (id) => req(`/grupos-pagamento/${id}/participar`, { method: 'DELETE' }),
  fecharGrupo: (id) => req(`/grupos-pagamento/${id}/fechar`, { method: 'POST' }),
  cancelarGrupo: (id) => req(`/grupos-pagamento/${id}/cancelar`, { method: 'POST' }),

  // Lei Base (Termo de Uso)
  leiBase: () => req('/lei-base'),
  atualizarLeiBase: (d) => req('/lei-base', { method: 'PUT', body: d }),
  enviarLeiPdf: async (titulo, arquivo) => {
    const fd = new FormData()
    fd.append('titulo', titulo)
    fd.append('arquivo', arquivo)
    const token = getToken()
    const res = await fetch(BASE + '/lei-base/pdf', {
      method: 'POST',
      headers: token ? { Authorization: `Bearer ${token}` } : {},
      body: fd
    })
    if (!res.ok) {
      let msg = `Erro ${res.status}`
      try { const j = await res.json(); msg = j.erro || j.title || msg } catch { }
      throw new Error(msg)
    }
    return res.json()
  },
  obterLeiPdfBlob: async () => {
    const token = getToken()
    const res = await fetch(BASE + '/lei-base/pdf', {
      headers: token ? { Authorization: `Bearer ${token}` } : {}
    })
    if (!res.ok) throw new Error('Não foi possível carregar o PDF da Lei Base.')
    return res.blob()
  }
}
