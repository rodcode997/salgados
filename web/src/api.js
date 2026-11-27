const BASE = 'http://localhost:5000/api'
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
  login: (email, senha) => req('/auth/login', { method: 'POST', body: { email, senha } }),
  me: () => req('/auth/me'),
  alterarSenha: (d) => req('/auth/alterar-senha', { method: 'POST', body: d }),
  alterarAvatar: (avatar) => req('/auth/avatar', { method: 'PUT', body: { avatar } }),

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
  atualizarConfiguracoes: (d) => req('/configuracoes', { method: 'PUT', body: d })
}
