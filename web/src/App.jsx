import { useEffect, useMemo, useState } from 'react'
import {
  Plus, Search, X, CheckCircle2, AlertCircle, ChevronRight, LogOut, KeyRound,
  Pencil, Shield, Eye, LayoutDashboard, ReceiptText, Users, Tag, ShieldCheck, Menu, Settings, ShoppingBag
} from 'lucide-react'
import { api, getToken, setToken, setOnUnauthorized } from './api'
import { Avatar, EditorAvatar, avatarPadrao, parseAvatar, serializeAvatar } from './avatar'
import { GrupoFlutuante } from './GrupoFlutuante'

const hoje = () => new Date().toISOString().slice(0, 10)
const fmtData = (d) => (d ? new Date(d).toLocaleDateString('pt-BR', { timeZone: 'UTC' }) : '—')
const fmtCurta = (d) => (d ? new Date(d).toLocaleDateString('pt-BR', { timeZone: 'UTC', day: '2-digit', month: '2-digit' }) : '—')
const fmtDataHora = (d) => (d ? new Date(d).toLocaleString('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }) : '—')
const fmtValor = (v) => (v != null ? v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }) : '—')

const TIPOS = {
  CentoSalgado: { emo: '🥟', nome: 'Cento de salgados' },
  CaixaChocolate: { emo: '🍫', nome: 'Caixa de chocolate' }
}

const STATUS_BADGE = {
  PendenteAceite: { cls: 'b-pendente', texto: 'Aguardando aceite' },
  Contestada: { cls: 'b-contestada', texto: 'Contestada' },
  Aceita: { cls: 'b-aberta', texto: 'Em aberto' },
  Quitada: { cls: 'b-quitada', texto: 'Quitada' },
  Desistida: { cls: 'b-desistida', texto: 'Desistida' }
}

const PGTO_BADGE = {
  Pendente: { cls: 'b-pendente', texto: 'Aguardando aprovação' },
  Aprovado: { cls: 'b-quitada', texto: 'Aprovado' },
  Recusado: { cls: 'b-contestada', texto: 'Recusado' }
}

const PERM = {
  DIVIDAS_VER: 'dividas.ver',
  DIVIDAS_CRIAR: 'dividas.criar',
  DIVIDAS_QUITAR: 'dividas.quitar',
  USUARIOS_VER: 'usuarios.ver',
  USUARIOS_GERENCIAR: 'usuarios.gerenciar',
  MOTIVOS_VER: 'motivos.ver',
  MOTIVOS_GERENCIAR: 'motivos.gerenciar',
  PERFIS_GERENCIAR: 'perfis.gerenciar'
}

function Coxinha({ size = 64 }) {
  return (
    <svg width={size} height={size} viewBox="0 0 64 64">
      <path d="M32 4C24 14 8 30 8 44c0 10 10 16 24 16s24-6 24-16C56 30 40 14 32 4z" fill="#f5b544" stroke="#c9852a" strokeWidth="2.5" />
      <path d="M20 40c4 2 8 1 10-1" stroke="#fff6" strokeWidth="3" fill="none" strokeLinecap="round" />
      <circle cx="24" cy="40" r="2.6" fill="#3a2216" />
      <circle cx="40" cy="40" r="2.6" fill="#3a2216" />
      <path d="M27 46c3 3 7 3 10 0" stroke="#3a2216" strokeWidth="2.4" fill="none" strokeLinecap="round" />
      <ellipse cx="19" cy="46" rx="3.5" ry="2" fill="#ff8a6b" opacity=".7" />
      <ellipse cx="45" cy="46" rx="3.5" ry="2" fill="#ff8a6b" opacity=".7" />
    </svg>
  )
}

const Tipo = ({ tipo }) => <span className="tipo-cel"><span>{TIPOS[tipo].emo}</span><span>{TIPOS[tipo].nome}</span></span>

function StatusBadge({ status, temPagamentoPendente }) {
  const s = STATUS_BADGE[status] || { cls: 'b-neutro', texto: status }
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 4, alignItems: 'flex-start' }}>
      <span className={'badge ' + s.cls}>{s.texto}</span>
      {temPagamentoPendente && <span className="badge b-pgto">💰 Pgto pendente</span>}
    </div>
  )
}

const Vazio = ({ texto, emo = '🍽️' }) => <div className="vazio"><div className="vazio-ico">{emo}</div>{texto}</div>

function Modal({ titulo, children, rodape, onClose, peq }) {
  return (
    <div className="fundo" onMouseDown={onClose}>
      <div className={'modal' + (peq ? ' peq' : '')} onMouseDown={(e) => e.stopPropagation()}>
        <div className="modal-cab"><h3>{titulo}</h3><button className="icone" onClick={onClose}><X size={22} /></button></div>
        <div className="modal-corpo">{children}</div>
        {rodape && <div className="modal-rodape">{rodape}</div>}
      </div>
    </div>
  )
}

const DECO = [
  ['🥟', '34%', '-14px', -20], ['🍫', '55%', '-10px', 25], ['🥟', '79%', '-16px', 15], ['🍫', '97%', '30px', -30],
  ['🥟', '-1%', '70px', 30], ['🍫', '33%', '62px', -15], ['🥟', '68%', '68px', 10], ['🍫', '81%', '70px', 20]
]

export default function App() {
  const [usuario, setUsuario] = useState(null)
  const [carregandoSessao, setCarregandoSessao] = useState(true)
  const [pag, setPag] = useState('painel')
  const [novaDivida, setNovaDivida] = useState(false)
  const [modalSenha, setModalSenha] = useState(false)
  const [modalAvatar, setModalAvatar] = useState(false)
  const [menuAberto, setMenuAberto] = useState(false)
  const [versaoAvatar, setVersaoAvatar] = useState(0)
  const [toast, setToast] = useState(null)

  const avisar = (msg, tipo = 'sucesso') => {
    setToast({ msg, tipo })
    setTimeout(() => setToast(null), 3500)
  }

  const sair = () => {
    setToken(null)
    setUsuario(null)
    setPag('painel')
  }

  useEffect(() => {
    setOnUnauthorized(() => {
      setToken(null)
      setUsuario(null)
      avisar('Sessão expirada. Faça login novamente.', 'erro')
    })
    if (!getToken()) { setCarregandoSessao(false); return }
    api.me()
      .then((u) => setUsuario(u))
      .catch(() => setToken(null))
      .finally(() => setCarregandoSessao(false))
  }, [])

  const exec = async (fn, ok) => {
    try {
      const r = await fn()
      if (ok) avisar(ok)
      return r ?? true
    } catch (e) { avisar(e.message, 'erro') }
  }

  const tem = (p) => !!usuario?.permissoes?.includes(p)

  const abas = useMemo(() => {
    if (!usuario) return []
    const lista = []
    if (tem(PERM.DIVIDAS_VER) && tem(PERM.USUARIOS_VER)) lista.push(['painel', 'Painel', LayoutDashboard])
    if (tem(PERM.DIVIDAS_VER)) lista.push(['dividas', 'Dívidas', ReceiptText])
    if (tem(PERM.USUARIOS_VER)) lista.push(['usuarios', 'Usuários', Users])
    lista.push(['produtos', 'Produtos', ShoppingBag])
    if (tem(PERM.MOTIVOS_VER)) lista.push(['motivos', 'Motivos', Tag])
    if (tem(PERM.PERFIS_GERENCIAR)) lista.push(['perfis', 'Perfis', ShieldCheck])
    if (tem(PERM.PERFIS_GERENCIAR)) lista.push(['configuracoes', 'Configurações', Settings])
    return lista
  }, [usuario])

  useEffect(() => {
    if (abas.length && !abas.some(([k]) => k === pag)) setPag(abas[0][0])
  }, [abas, pag])

  if (carregandoSessao) {
    return <div className="login-tela"><Vazio texto="Carregando..." emo="🥟" /></div>
  }

  if (!usuario) {
    return (
      <>
        <TelaLogin onLogin={(res) => { setToken(res.token); setUsuario(res.usuario); avisar(`Bem-vindo(a), ${res.usuario.nome}!`) }} avisar={avisar} />
        {toast && <div className={'toast ' + toast.tipo}>{toast.tipo === 'erro' ? <AlertCircle size={20} /> : <CheckCircle2 size={20} />}{toast.msg}</div>}
      </>
    )
  }

  if (usuario.deveAlterarSenha) {
    return (
      <>
        <TelaPrimeiroAcesso
          usuario={usuario}
          exec={exec}
          avisar={avisar}
          onConcluido={() => setUsuario((u) => (u ? { ...u, deveAlterarSenha: false } : u))}
          onSair={sair}
        />
        {toast && <div className={'toast ' + toast.tipo}>{toast.tipo === 'erro' ? <AlertCircle size={20} /> : <CheckCircle2 size={20} />}{toast.msg}</div>}
      </>
    )
  }

  const abrirNova = () => { setPag('dividas'); setNovaDivida(true); setMenuAberto(false) }
  const irPara = (k) => { setPag(k); setMenuAberto(false) }

  return (
    <div className="app-shell">
      {menuAberto && <div className="sidebar-backdrop" onClick={() => setMenuAberto(false)} />}

      <aside className={'sidebar' + (menuAberto ? ' aberta' : '')}>
        <div className="sidebar-topo">
          <div className="marca">
            <Coxinha size={44} />
            <div>
              <h1>Salgados</h1>
              <small>Controle de dívidas 🥟🍫</small>
            </div>
          </div>
          <button type="button" className="icone sidebar-fechar" onClick={() => setMenuAberto(false)} aria-label="Fechar menu">
            <X size={20} />
          </button>
        </div>

        {tem(PERM.DIVIDAS_CRIAR) && (
          <button className="grande sidebar-cta" onClick={abrirNova}>
            <Plus size={18} />Nova dívida
          </button>
        )}

        <nav className="sidebar-nav">
          {abas.map(([k, t, Icon]) => (
            <button key={k} className={pag === k ? 'ativo' : ''} onClick={() => irPara(k)}>
              <Icon size={19} />
              <span>{t}</span>
            </button>
          ))}
        </nav>

        <div className="sidebar-user">
          <div className="sidebar-user-top">
            <button type="button" className="avatar-btn" title="Personalizar avatar" onClick={() => { setModalAvatar(true); setMenuAberto(false) }}>
              <Avatar nome={usuario.nome} avatar={usuario.avatar} />
            </button>
            <div className="u-info">
              <b>{usuario.nome}</b>
              <small>{usuario.perfil}</small>
            </div>
          </div>
          <div className="sidebar-user-acoes">
            <button type="button" className="sec peq" title="Personalizar avatar" onClick={() => { setModalAvatar(true); setMenuAberto(false) }}>
              Avatar
            </button>
            <button type="button" className="icone" title="Alterar senha" onClick={() => { setModalSenha(true); setMenuAberto(false) }}>
              <KeyRound size={17} />
            </button>
            <button type="button" className="icone" title="Sair" onClick={sair}>
              <LogOut size={17} />
            </button>
          </div>
        </div>
      </aside>

      <div className="conteudo">
        <header className="mobile-bar">
          <button type="button" className="icone" onClick={() => setMenuAberto(true)} aria-label="Abrir menu">
            <Menu size={22} />
          </button>
          <div className="marca"><Coxinha size={34} /><h1>Salgados</h1></div>
          {tem(PERM.DIVIDAS_CRIAR) && (
            <button className="peq" onClick={abrirNova}><Plus size={16} />Nova</button>
          )}
        </header>

        <main className="pagina">
          {pag === 'painel' && <Painel key={versaoAvatar} exec={exec} irPara={irPara} usuarioLogado={usuario} tem={tem} />}
          {pag === 'dividas' && <Dividas key={versaoAvatar} exec={exec} nova={novaDivida} setNova={setNovaDivida} tem={tem} usuarioLogado={usuario} />}
          {pag === 'usuarios' && (
            <Usuarios
              key={versaoAvatar}
              exec={exec}
              tem={tem}
              usuarioLogado={usuario}
              onUpdateSessao={(u) => setUsuario((prev) => (prev && prev.id === u.id ? { ...prev, nome: u.nome, login: u.login, avatar: u.avatar } : prev))}
            />
          )}
          {pag === 'produtos' && <Produtos exec={exec} tem={tem} />}
          {pag === 'motivos' && <Motivos exec={exec} tem={tem} />}
          {pag === 'perfis' && <Perfis exec={exec} />}
          {pag === 'configuracoes' && <Configuracoes exec={exec} />}
          {!abas.length && <div className="card"><Vazio texto="Seu perfil não possui permissões de visualização." emo="🔒" /></div>}
        </main>
      </div>

      <GrupoFlutuante
        usuarioLogado={usuario}
        exec={exec}
        tem={tem}
        versao={versaoAvatar}
        onAtualizar={() => setVersaoAvatar((v) => v + 1)}
      />

      {modalSenha && <ModalAlterarSenha exec={exec} onClose={() => setModalSenha(false)} />}
      {modalAvatar && (
        <ModalAlterarAvatar
          usuario={usuario}
          exec={exec}
          onSaved={(u) => { setUsuario(u); setVersaoAvatar((v) => v + 1); setModalAvatar(false) }}
          onClose={() => setModalAvatar(false)}
        />
      )}

      {toast && (
        <div className={'toast ' + toast.tipo}>
          {toast.tipo === 'erro' ? <AlertCircle size={20} /> : <CheckCircle2 size={20} />}{toast.msg}
        </div>
      )}
    </div>
  )
}

function TelaLogin({ onLogin, avisar }) {
  const [login, setLogin] = useState('')
  const [senha, setSenha] = useState('')
  const [entrando, setEntrando] = useState(false)

  const entrar = async (e) => {
    e.preventDefault()
    setEntrando(true)
    try {
      const res = await api.login(login, senha)
      onLogin(res)
    } catch (err) {
      avisar(err.message, 'erro')
    } finally {
      setEntrando(false)
    }
  }

  return (
    <div className="login-tela">
      {DECO.map(([e, l, t, r], i) => (
        <span key={i} className="deco" style={{ left: l, top: t, transform: `rotate(${r}deg)` }}>{e}</span>
      ))}
      <div className="login-card">
        <div className="login-topo">
          <Coxinha size={72} />
          <h1>Dívidas de Salgados</h1>
          <p>Entre para acompanhar salgados e chocolates 🥟🍫</p>
        </div>
        <form className="coluna" onSubmit={entrar}>
          <label>Login<input type="text" value={login} onChange={(e) => setLogin(e.target.value)} placeholder="seu.login" required autoFocus /></label>
          <label>Senha<input type="password" value={senha} onChange={(e) => setSenha(e.target.value)} placeholder="••••••••" required /></label>
          <button className="grande" style={{ justifyContent: 'center', marginTop: 6 }} disabled={entrando}>
            {entrando ? 'Entrando...' : 'Entrar'}
          </button>
        </form>
        <div className="login-dica">
          Admin padrão: <code>admin</code> / <code>Admin@123</code>
        </div>
      </div>
    </div>
  )
}

function TelaPrimeiroAcesso({ usuario, exec, avisar, onConcluido, onSair }) {
  const [senhaAtual, setSenhaAtual] = useState('')
  const [novaSenha, setNovaSenha] = useState('')
  const [confirmacao, setConfirmacao] = useState('')
  const [salvando, setSalvando] = useState(false)

  const salvar = async (e) => {
    e.preventDefault()
    if (novaSenha !== confirmacao) {
      avisar('A confirmação não confere com a nova senha.', 'erro')
      return
    }
    setSalvando(true)
    const ok = await exec(() => api.alterarSenha({ senhaAtual, novaSenha }), 'Senha atualizada! Bem-vindo(a)!')
    setSalvando(false)
    if (ok) onConcluido()
  }

  return (
    <div className="login-tela">
      {DECO.map(([e, l, t, r], i) => (
        <span key={i} className="deco" style={{ left: l, top: t, transform: `rotate(${r}deg)` }}>{e}</span>
      ))}
      <div className="login-card">
        <div className="login-topo">
          <Coxinha size={64} />
          <h1>Primeiro acesso 🔐</h1>
          <p>Olá, <b>{usuario.nome}</b>! Por segurança, defina sua nova senha para continuar.</p>
        </div>
        <form className="coluna" onSubmit={salvar}>
          <label>Senha provisória (atual)<input type="password" value={senhaAtual} onChange={(e) => setSenhaAtual(e.target.value)} required autoFocus /></label>
          <label>Nova senha (mín. 6 caracteres)<input type="password" minLength={6} value={novaSenha} onChange={(e) => setNovaSenha(e.target.value)} required /></label>
          <label>Confirmar nova senha<input type="password" minLength={6} value={confirmacao} onChange={(e) => setConfirmacao(e.target.value)} required /></label>
          <button className="grande" style={{ justifyContent: 'center', marginTop: 6 }} disabled={salvando}>
            {salvando ? 'Salvando...' : 'Salvar nova senha e entrar'}
          </button>
          <button type="button" className="sec" style={{ justifyContent: 'center' }} onClick={onSair}>
            <LogOut size={16} />Sair
          </button>
        </form>
      </div>
    </div>
  )
}

function ModalAlterarSenha({ exec, onClose }) {
  const [senhaAtual, setSenhaAtual] = useState('')
  const [novaSenha, setNovaSenha] = useState('')
  const salvar = async (e) => {
    e.preventDefault()
    if (await exec(() => api.alterarSenha({ senhaAtual, novaSenha }), 'Senha alterada com sucesso!')) onClose()
  }
  return (
    <Modal peq titulo="Alterar senha 🔑" onClose={onClose}
      rodape={<><button className="sec" onClick={onClose}>Cancelar</button><button form="f-senha">Salvar</button></>}>
      <form id="f-senha" className="coluna" onSubmit={salvar}>
        <label>Senha atual<input type="password" value={senhaAtual} onChange={(e) => setSenhaAtual(e.target.value)} required /></label>
        <label>Nova senha (mín. 6 caracteres)<input type="password" minLength={6} value={novaSenha} onChange={(e) => setNovaSenha(e.target.value)} required /></label>
      </form>
    </Modal>
  )
}

function ModalAlterarAvatar({ usuario, exec, onSaved, onClose }) {
  const [avatar, setAvatar] = useState(() => parseAvatar(usuario.avatar) || avatarPadrao('M'))
  const salvar = async (e) => {
    e.preventDefault()
    const atualizado = await exec(() => api.alterarAvatar(serializeAvatar(avatar)), 'Avatar atualizado!')
    if (atualizado?.id) onSaved(atualizado)
  }
  return (
    <Modal peq titulo="Meu avatar 🎨" onClose={onClose}
      rodape={<><button className="sec" onClick={onClose}>Cancelar</button><button form="f-avatar">Salvar</button></>}>
      <form id="f-avatar" onSubmit={salvar}>
        <EditorAvatar nome={usuario.nome} value={avatar} onChange={setAvatar} />
      </form>
    </Modal>
  )
}

function Kpis({ dividas, usuarios }) {
  const abertas = dividas.filter((d) => d.status === 'Aceita').length
  const quitadas = dividas.filter((d) => d.status === 'Quitada')
  const salgadosPagos = quitadas.filter((d) => d.tipo === 'CentoSalgado').length
  const chocolatesPagos = quitadas.filter((d) => d.tipo === 'CaixaChocolate').length
  const media = quitadas.length ? Math.round(quitadas.reduce((a, d) => a + d.dias, 0) / quitadas.length) : 0
  const ativos = usuarios.filter((u) => u.ativo)
  const limpos = ativos.filter((u) => u.dividasAbertas === 0).length
  const itens = [
    ['Dívidas em aberto', abertas, '🥟', 'k-laranja'],
    [
      'Pagos (salgados / chocolates)',
      <span className="kpi-duplo">
        <span title="Centos de salgados pagos">🥟 {salgadosPagos}</span>
        <span title="Caixas de chocolate pagas">🍫 {chocolatesPagos}</span>
      </span>,
      '🎉',
      'k-choco'
    ],
    ['Média para quitar', `${media} dias`, '⏰', 'k-amarelo'],
    ['Pessoas sem dívida', `${limpos}/${ativos.length}`, '😇', 'k-coral']
  ]
  return (
    <div className="kpis">
      {itens.map(([t, v, e, c]) => (
        <div className={'kpi ' + c} key={t}>
          <div className="kpi-ico">{e}</div>
          <div><span>{t}</span><b className="num">{v}</b></div>
        </div>
      ))}
    </div>
  )
}

function Painel({ exec, irPara, usuarioLogado, tem }) {
  const [dividas, setDividas] = useState([])
  const [usuarios, setUsuarios] = useState([])
  const [detalhe, setDetalhe] = useState(null)
  const [pessoaSel, setPessoaSel] = useState(null)

  const carregar = () => {
    exec(() => api.dividas()).then((d) => Array.isArray(d) && setDividas(d))
    exec(api.usuarios).then((u) => Array.isArray(u) && setUsuarios(u))
  }
  useEffect(() => { carregar() }, [])

  const pendentesMeuAceite = dividas.filter((d) => d.status === 'PendenteAceite' && d.usuarioId === usuarioLogado.id)
  const contestadasMinhas = dividas.filter((d) => d.status === 'Contestada' && d.criadoPorId === usuarioLogado.id)
  const pgtosPendentes = tem(PERM.DIVIDAS_QUITAR) ? dividas.filter((d) => d.temPagamentoPendente) : []
  const semDividas = usuarios.filter((u) => u.ativo && u.dividasAbertas === 0)

  const ranking = useMemo(() => {
    const m = {}
    dividas.filter((d) => d.status === 'Aceita')
      .forEach((d) => {
        if (!m[d.usuario]) m[d.usuario] = { qtd: 0, avatar: d.usuarioAvatar }
        m[d.usuario].qtd += 1
      })
    return Object.entries(m).sort((a, b) => b[1].qtd - a[1].qtd).slice(0, 5)
  }, [dividas])
  const max = ranking[0]?.[1]?.qtd || 1

  const atualizarDetalhe = (nova) => {
    setDetalhe(nova)
    carregar()
  }

  return (
    <>
      {(pendentesMeuAceite.length > 0 || contestadasMinhas.length > 0 || pgtosPendentes.length > 0) && (
        <div className="pendencias">
          {pendentesMeuAceite.length > 0 && (
            <div className="alerta-card">
              <div><b>🔔 {pendentesMeuAceite.length} dívida(s) aguardando seu aceite</b><small>Aceite ou conteste na aba Dívidas</small></div>
              <button className="peq" onClick={() => setDetalhe(pendentesMeuAceite[0])}>Analisar</button>
            </div>
          )}
          {contestadasMinhas.length > 0 && (
            <div className="alerta-card">
              <div><b>💬 {contestadasMinhas.length} dívida(s) contestada(s)</b><small>Envie mais informações ou desista</small></div>
              <button className="peq" onClick={() => setDetalhe(contestadasMinhas[0])}>Responder</button>
            </div>
          )}
          {pgtosPendentes.length > 0 && (
            <div className="alerta-card">
              <div><b>💰 {pgtosPendentes.length} pagamento(s) aguardando aprovação</b><small>Aprove ou recuse o pagamento informado</small></div>
              <button className="ok peq" onClick={() => setDetalhe(pgtosPendentes[0])}>Avaliar</button>
            </div>
          )}
        </div>
      )}

      <Kpis dividas={dividas} usuarios={usuarios} />
      <div className="grid2">
        <div className="card">
          <div className="card-cab">
            <h3>Dívidas recentes</h3>
            <button className="sec peq" onClick={() => irPara('dividas')}>Ver todas <ChevronRight size={16} /></button>
          </div>
          {dividas.length
            ? <TabelaDividas dividas={dividas.slice(0, 6)} mostrarUsuario compacta onVer={setDetalhe} />
            : <Vazio texto="Nenhuma dívida ainda. Que equipe exemplar!" emo="🎉" />}
        </div>
        <div className="card amarelo">
          <div className="card-cab"><h3>Maiores devedores</h3></div>
          {ranking.length ? (
            <div className="rank">
              {ranking.map(([nome, info], i) => (
                <div className="rank-item" key={nome}>
                  <div className="pos">{i + 1}</div>
                  <Avatar nome={nome} avatar={info.avatar} />
                  <div className="info">
                    <div className="linha"><b>{nome}</b><small>{info.qtd} dívida{info.qtd > 1 ? 's' : ''}</small></div>
                    <div className="progresso"><div style={{ width: `${(info.qtd / max) * 100}%` }} /></div>
                  </div>
                </div>
              ))}
            </div>
          ) : <Vazio texto="Ninguém por aqui" emo="😇" />}
        </div>
      </div>

      <div className="card mural-ok">
        <div className="card-cab">
          <div>
            <h3>🎉 Mural de Parabéns — Sem dívidas ({semDividas.length})</h3>
            <p>Quem está com a ficha limpa e não deve nenhum salgado ou chocolate!</p>
          </div>
          <button className="sec peq" onClick={() => irPara('usuarios')}>Ver equipe <ChevronRight size={16} /></button>
        </div>
        {semDividas.length ? (
          <div className="sem-dividas-grid">
            {semDividas.map((u) => (
              <div key={u.id} className="limpo-card" onClick={() => setPessoaSel(u.id)}>
                <div className="limpo-top">
                  <Avatar nome={u.nome} avatar={u.avatar} />
                  <div>
                    <b>{u.nome}</b>
                    <small>{u.totalDividas ? `${u.totalDividas} quitada(s) no histórico` : 'Ficha 100% zerada!'}</small>
                  </div>
                </div>
                <div className="limpo-msg">{u.mensagem || '🎉 Parabéns! Sem dívidas!'}</div>
              </div>
            ))}
          </div>
        ) : <Vazio texto="Todo mundo está devendo salgado hoje! 😅" emo="🥟" />}
      </div>

      {detalhe && (
        <ModalDetalheDivida
          divida={detalhe}
          exec={exec}
          tem={tem}
          usuarioLogado={usuarioLogado}
          onUpdate={atualizarDetalhe}
          onClose={() => setDetalhe(null)}
        />
      )}
      {pessoaSel && (
        <Situacao
          id={pessoaSel}
          exec={exec}
          tem={tem}
          usuarioLogado={usuarioLogado}
          onClose={() => { setPessoaSel(null); carregar() }}
        />
      )}
    </>
  )
}

function TabelaDividas({ dividas, mostrarUsuario, compacta, onVer }) {
  return (
    <div className="tabela">
      <table>
        <thead>
          <tr>
            {mostrarUsuario && <th>Pessoa</th>}
            <th>Tipo</th>
            <th>Motivo / Cadastro</th>
            <th>Aquisição</th>
            {!compacta && <th>Quitação</th>}
            <th className="centro">Dias</th>
            {!compacta && <th>Valor pago</th>}
            <th>Status</th>
            {onVer && <th></th>}
          </tr>
        </thead>
        <tbody>
          {dividas.map((d) => (
            <tr key={d.id} style={onVer ? { cursor: 'pointer' } : undefined} onClick={() => onVer && onVer(d)}>
              {mostrarUsuario && <td><div className="pessoa-cel"><Avatar nome={d.usuario} avatar={d.usuarioAvatar} />{d.usuario}</div></td>}
              <td><Tipo tipo={d.tipo} /></td>
              <td>
                <b>{d.motivo}</b>
                {d.observacao && <div className="sub">{d.observacao}</div>}
                <div className="sub">Por {d.criadoPor} em {fmtDataHora(d.criadoEm)}</div>
              </td>
              <td className="num">{compacta ? fmtCurta(d.dataAquisicao) : fmtData(d.dataAquisicao)}</td>
              {!compacta && (
                <td className="num">
                  {d.dataQuitacao ? (
                    <>
                      {fmtData(d.dataQuitacao)}
                      {d.quitadoPor && <div className="sub">Por {d.quitadoPor} em {fmtDataHora(d.quitadoEm)}</div>}
                    </>
                  ) : '—'}
                </td>
              )}
              <td className={'num centro' + (d.status === 'Aceita' && d.dias > 15 ? ' alerta' : '')}>{d.status === 'Desistida' ? '—' : d.dias}</td>
              {!compacta && <td className="num">{fmtValor(d.valorPago)}</td>}
              <td><StatusBadge status={d.status} temPagamentoPendente={d.temPagamentoPendente} /></td>
              {onVer && (
                <td style={{ textAlign: 'right' }}>
                  <button className="sec peq" onClick={(e) => { e.stopPropagation(); onVer(d) }}><Eye size={14} />Detalhes</button>
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function ModalDetalheDivida({ divida: d, exec, tem, usuarioLogado, onUpdate, onClose }) {
  const [modoAcao, setModoAcao] = useState(null) // null | 'contestar' | 'complementar' | 'desistir' | 'pagar'
  const [msg, setMsg] = useState('')
  const [pgtoForm, setPgtoForm] = useState({ dataPagamento: hoje(), valorPago: '', observacao: '' })
  const [recusandoId, setRecusandoId] = useState(null)
  const [motivoRecusa, setMotivoRecusa] = useState('')

  const ehDevedor = d.usuarioId === usuarioLogado.id
  const ehCriador = d.criadoPorId === usuarioLogado.id
  const podeAprovar = tem(PERM.DIVIDAS_QUITAR)

  const aceitar = async () => {
    const r = await exec(() => api.aceitarDivida(d.id), 'Dívida aceita!')
    if (r?.id) onUpdate(r)
  }

  const enviarAcao = async (e) => {
    e.preventDefault()
    let r = null
    if (modoAcao === 'contestar') r = await exec(() => api.contestarDivida(d.id, msg), 'Dívida contestada!')
    if (modoAcao === 'complementar') r = await exec(() => api.complementarDivida(d.id, msg), 'Informações enviadas! Aguardando aceite.')
    if (modoAcao === 'desistir') r = await exec(() => api.desistirDivida(d.id, msg), 'Você desistiu da dívida.')
    if (r?.id) { setModoAcao(null); setMsg(''); onUpdate(r) }
  }

  const enviarPagamento = async (e) => {
    e.preventDefault()
    const okMsg = podeAprovar ? 'Pagamento cadastrado e aprovado! 🎉' : 'Pagamento informado! Aguardando aprovação do administrador.'
    const r = await exec(() => api.registrarPagamento(d.id, {
      dataPagamento: pgtoForm.dataPagamento,
      valorPago: +pgtoForm.valorPago,
      observacao: pgtoForm.observacao || null
    }), okMsg)
    if (r?.id) {
      setModoAcao(null)
      setPgtoForm({ dataPagamento: hoje(), valorPago: '', observacao: '' })
      onUpdate(r)
    }
  }

  const aprovarPgto = async (pgtoId) => {
    const r = await exec(() => api.aprovarPagamento(pgtoId), 'Pagamento aprovado e dívida quitada! 🎉')
    if (r?.id) onUpdate(r)
  }

  const recusarPgto = async (e) => {
    e.preventDefault()
    const r = await exec(() => api.recusarPagamento(recusandoId, motivoRecusa), 'Pagamento recusado.')
    if (r?.id) { setRecusandoId(null); setMotivoRecusa(''); onUpdate(r) }
  }

  return (
    <Modal titulo={`Dívida #${d.id} · ${d.usuario}`} onClose={onClose}>
      <div className="resumo-pessoa" style={{ justifyContent: 'space-between', flexWrap: 'wrap' }}>
        <div style={{ display: 'flex', gap: 14, alignItems: 'center' }}>
          <Avatar nome={d.usuario} avatar={d.usuarioAvatar} grande />
          <div>
            <h2>{d.usuario}</h2>
            <p><Tipo tipo={d.tipo} /> · <b>{d.motivo}</b></p>
          </div>
        </div>
        <StatusBadge status={d.status} temPagamentoPendente={d.temPagamentoPendente} />
      </div>

      <div className="meta-grid">
        <div className="meta-item"><small>Cadastrada por</small><b>{d.criadoPor}</b><small>{fmtDataHora(d.criadoEm)}</small></div>
        <div className="meta-item"><small>Data de aquisição</small><b>{fmtData(d.dataAquisicao)}</b><small>{d.status === 'Desistida' ? 'Cancelada' : `${d.dias} dia(s)`}</small></div>
        <div className="meta-item">
          <small>Quitação</small>
          <b>{d.dataQuitacao ? `${fmtData(d.dataQuitacao)} (${fmtValor(d.valorPago)})` : 'Não quitada'}</b>
          {d.quitadoPor && <small>Aprovada por {d.quitadoPor} em {fmtDataHora(d.quitadoEm)}</small>}
        </div>
        {d.observacao && <div className="meta-item"><small>Observação inicial</small><b>{d.observacao}</b></div>}
      </div>

      {/* Ação 1: Devedor precisa aceitar ou contestar */}
      {d.status === 'PendenteAceite' && ehDevedor && (
        <div className="acao-box">
          <h4>🔔 Esta dívida foi cadastrada para você. Você concorda?</h4>
          {modoAcao !== 'contestar' ? (
            <div className="acao-botoes">
              <button className="ok" onClick={aceitar}>✓ Aceitar dívida</button>
              <button className="btn-perigo" onClick={() => { setModoAcao('contestar'); setMsg('') }}>✕ Contestar</button>
            </div>
          ) : (
            <form className="coluna" onSubmit={enviarAcao}>
              <label>Motivo da contestação<textarea value={msg} onChange={(e) => setMsg(e.target.value)} placeholder="Explique por que você está contestando..." required /></label>
              <div className="acao-botoes">
                <button className="btn-perigo">Enviar contestação</button>
                <button type="button" className="sec" onClick={() => setModoAcao(null)}>Cancelar</button>
              </div>
            </form>
          )}
        </div>
      )}

      {/* Ação 2: Criador responde à contestação */}
      {d.status === 'Contestada' && ehCriador && (
        <div className="acao-box">
          <h4>💬 O devedor contestou esta dívida. O que deseja fazer?</h4>
          {!modoAcao ? (
            <div className="acao-botoes">
              <button onClick={() => { setModoAcao('complementar'); setMsg('') }}>➕ Enviar mais informações</button>
              <button className="btn-perigo" onClick={() => { setModoAcao('desistir'); setMsg('') }}>🏳️ Desistir da dívida</button>
            </div>
          ) : (
            <form className="coluna" onSubmit={enviarAcao}>
              <label>
                {modoAcao === 'complementar' ? 'Mais informações / evidências' : 'Motivo da desistência (opcional)'}
                <textarea value={msg} onChange={(e) => setMsg(e.target.value)} required={modoAcao === 'complementar'}
                  placeholder={modoAcao === 'complementar' ? 'Adicione detalhes para reenviar ao devedor...' : 'Ex.: Engano de pessoa...'} />
              </label>
              <div className="acao-botoes">
                <button className={modoAcao === 'desistir' ? 'btn-perigo' : ''}>
                  {modoAcao === 'complementar' ? 'Reenviar para aceite' : 'Confirmar desistência'}
                </button>
                <button type="button" className="sec" onClick={() => setModoAcao(null)}>Cancelar</button>
              </div>
            </form>
          )}
        </div>
      )}

      {/* Ação 3: Informar ou cadastrar pagamento */}
      {d.status === 'Aceita' && !d.temPagamentoPendente && (ehDevedor || podeAprovar) && (
        <div className="acao-box">
          <h4>💰 {podeAprovar ? 'Cadastrar pagamento (já fica aprovado)' : 'Informar pagamento da dívida'}</h4>
          {modoAcao !== 'pagar' ? (
            <div className="acao-botoes">
              <button className="ok" onClick={() => setModoAcao('pagar')}>
                {podeAprovar ? '✓ Cadastrar pagamento aprovado' : 'Informar pagamento'}
              </button>
            </div>
          ) : (
            <form className="coluna" onSubmit={enviarPagamento}>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <label>Data do pagamento<input type="date" value={pgtoForm.dataPagamento} min={d.dataAquisicao.slice(0, 10)} max={hoje()}
                  onChange={(e) => setPgtoForm({ ...pgtoForm, dataPagamento: e.target.value })} required /></label>
                <label>Valor pago (R$)<input type="number" step="0.01" min="0.01" value={pgtoForm.valorPago} placeholder="0,00"
                  onChange={(e) => setPgtoForm({ ...pgtoForm, valorPago: e.target.value })} required /></label>
              </div>
              <label>Observação (opcional)<input value={pgtoForm.observacao} onChange={(e) => setPgtoForm({ ...pgtoForm, observacao: e.target.value })} placeholder="Ex.: Trouxe 100 salgados da padaria..." /></label>
              <div className="acao-botoes">
                <button className="ok">{podeAprovar ? 'Salvar e aprovar' : 'Enviar para aprovação'}</button>
                <button type="button" className="sec" onClick={() => setModoAcao(null)}>Cancelar</button>
              </div>
            </form>
          )}
        </div>
      )}

      {/* Pagamentos */}
      <div className="sec-titulo">💰 Pagamentos ({d.pagamentos.length})</div>
      {d.pagamentos.length ? (
        <div className="pgto-lista">
          {d.pagamentos.map((p) => {
            const pb = PGTO_BADGE[p.status] || { cls: 'b-neutro', texto: p.status }
            return (
              <div key={p.id} className="pgto-card">
                <div className="pgto-top">
                  <div>
                    <b style={{ fontSize: 18 }}>{fmtValor(p.valorPago)}</b> · pago em <b>{fmtData(p.dataPagamento)}</b>
                  </div>
                  <span className={'badge ' + pb.cls}>{pb.texto}</span>
                </div>
                {p.observacao && <div>{p.observacao}</div>}
                <div className="pgto-meta">
                  <span>Cadastrado por <b>{p.criadoPor}</b> em {fmtDataHora(p.criadoEm)}</span>
                  {p.avaliadoPor && (
                    <span>
                      {p.status === 'Aprovado' ? 'Aprovado' : 'Recusado'} por <b>{p.avaliadoPor}</b> em {fmtDataHora(p.avaliadoEm)}
                    </span>
                  )}
                  {p.motivoRecusa && <span style={{ color: 'var(--coral)' }}><b>Motivo da recusa:</b> {p.motivoRecusa}</span>}
                </div>
                {p.status === 'Pendente' && podeAprovar && (
                  recusandoId === p.id ? (
                    <form className="coluna" onSubmit={recusarPgto} style={{ marginTop: 6 }}>
                      <label>Motivo da recusa<input value={motivoRecusa} onChange={(e) => setMotivoRecusa(e.target.value)} placeholder="Informe o motivo..." required /></label>
                      <div className="acao-botoes">
                        <button className="btn-perigo peq">Confirmar recusa</button>
                        <button type="button" className="sec peq" onClick={() => setRecusandoId(null)}>Cancelar</button>
                      </div>
                    </form>
                  ) : (
                    <div className="acao-botoes" style={{ marginTop: 4 }}>
                      <button className="ok peq" onClick={() => aprovarPgto(p.id)}>✓ Aprovar pagamento</button>
                      <button className="btn-perigo peq" onClick={() => { setRecusandoId(p.id); setMotivoRecusa('') }}>✕ Recusar</button>
                    </div>
                  )
                )}
              </div>
            )
          })}
        </div>
      ) : <div className="sub" style={{ color: 'var(--txt-2)' }}>Nenhum pagamento informado ainda.</div>}

      {/* Histórico de interações / contestações */}
      <div className="sec-titulo">📜 Histórico de aceite e contestações</div>
      <div className="timeline">
        {d.interacoes.map((i) => (
          <div key={i.id} className={'tl-item tl-' + i.tipo.toLowerCase()}>
            <div className="tl-top">
              <b>{i.tipoDescricao} · {i.usuario}</b>
              <small>{fmtDataHora(i.criadoEm)}</small>
            </div>
            {i.mensagem && <p className="tl-msg">{i.mensagem}</p>}
          </div>
        ))}
      </div>
    </Modal>
  )
}

function Dividas({ exec, nova, setNova, tem, usuarioLogado }) {
  const [dividas, setDividas] = useState([])
  const [usuarios, setUsuarios] = useState([])
  const [motivos, setMotivos] = useState([])
  const [filtro, setFiltro] = useState('')
  const [busca, setBusca] = useState('')
  const vazio = { usuarioId: '', motivoId: '', tipo: 'CentoSalgado', dataAquisicao: hoje(), observacao: '' }
  const [form, setForm] = useState(vazio)
  const [detalhe, setDetalhe] = useState(null)

  const carregar = () => exec(() => api.dividas(filtro ? `?status=${filtro}` : '')).then((d) => Array.isArray(d) && setDividas(d))
  useEffect(() => {
    if (tem(PERM.USUARIOS_VER)) exec(api.usuarios).then((u) => Array.isArray(u) && setUsuarios(u.filter((x) => x.ativo)))
    if (tem(PERM.MOTIVOS_VER)) exec(api.motivos).then((m) => Array.isArray(m) && setMotivos(m.filter((x) => x.ativo)))
  }, [])
  useEffect(() => { carregar() }, [filtro])

  const set = (k) => (e) => setForm({ ...form, [k]: e.target.value })
  const salvar = async (e) => {
    e.preventDefault()
    const r = await exec(() => api.criarDivida({ ...form, usuarioId: +form.usuarioId, motivoId: +form.motivoId }), 'Dívida registrada! 😈')
    if (r) { setForm(vazio); setNova(false); carregar() }
  }

  const lista = dividas.filter((d) => (d.usuario + d.motivo + d.criadoPor).toLowerCase().includes(busca.toLowerCase()))

  const atualizarDetalhe = (atualizada) => {
    setDetalhe(atualizada)
    carregar()
  }

  return (
    <>
      <div className="card">
        <div className="card-cab">
          <h3>Todas as dívidas</h3>
          <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap', alignItems: 'center' }}>
            <div className="seg">
              {[
                ['', 'Todas'],
                ['PendenteAceite', 'Aguardando aceite'],
                ['Contestada', 'Contestadas'],
                ['Aceita', 'Em aberto'],
                ['Quitada', 'Quitadas'],
                ['Desistida', 'Desistidas']
              ].map(([v, t]) => (
                <button key={v} className={filtro === v ? 'ativo' : ''} onClick={() => setFiltro(v)}>{t}</button>
              ))}
            </div>
            <div className="campo-busca"><Search size={18} /><input placeholder="Buscar pessoa ou motivo" value={busca} onChange={(e) => setBusca(e.target.value)} /></div>
          </div>
        </div>
        {lista.length
          ? <TabelaDividas dividas={lista} mostrarUsuario onVer={setDetalhe} />
          : <Vazio texto="Nenhuma dívida encontrada" />}
      </div>

      {nova && tem(PERM.DIVIDAS_CRIAR) && (
        <Modal peq titulo="Nova dívida 😈" onClose={() => setNova(false)}
          rodape={<><button className="sec" onClick={() => setNova(false)}>Cancelar</button><button form="f-nova">Registrar</button></>}>
          <form id="f-nova" className="coluna" onSubmit={salvar}>
            <div className="tipos">
              {Object.entries(TIPOS).map(([k, t]) => (
                <div key={k} className={'tipo' + (form.tipo === k ? ' sel' : '') + (k === 'CaixaChocolate' ? ' choco' : '')}
                  onClick={() => setForm({ ...form, tipo: k })}>
                  <span className="emo">{t.emo}</span>{t.nome}
                </div>
              ))}
            </div>
            <label>Quem vai pagar?
              <select value={form.usuarioId} onChange={set('usuarioId')} required>
                <option value="">Selecione</option>
                {usuarios.map((u) => <option key={u.id} value={u.id}>{u.nome}</option>)}
              </select>
            </label>
            <label>Motivo
              <select value={form.motivoId} onChange={set('motivoId')} required>
                <option value="">Selecione</option>
                {motivos.map((m) => <option key={m.id} value={m.id}>{m.descricao}</option>)}
              </select>
            </label>
            <label>Data de aquisição<input type="date" value={form.dataAquisicao} max={hoje()} onChange={set('dataAquisicao')} required /></label>
            <label>Observação<input value={form.observacao} onChange={set('observacao')} placeholder="Detalhes do ocorrido... (opcional)" /></label>
          </form>
        </Modal>
      )}

      {detalhe && (
        <ModalDetalheDivida
          divida={detalhe}
          exec={exec}
          tem={tem}
          usuarioLogado={usuarioLogado}
          onUpdate={atualizarDetalhe}
          onClose={() => setDetalhe(null)}
        />
      )}
    </>
  )
}

function Situacao({ id, exec, tem, usuarioLogado, onClose }) {
  const [s, setS] = useState(null)
  const [detalhe, setDetalhe] = useState(null)
  const carregar = () => exec(() => api.situacao(id)).then((r) => r?.usuario && setS(r))
  useEffect(() => { carregar() }, [id])
  if (!s) return null

  return (
    <>
      <Modal titulo="Situação" onClose={onClose}>
        <div className="resumo-pessoa">
          <Avatar nome={s.usuario.nome} avatar={s.usuario.avatar} grande />
          <div>
            <h2>{s.usuario.nome}</h2>
            <p>
              @{s.usuario.login}
              {s.usuario.dataNascimento && ` · 🎂 ${fmtCurta(s.usuario.dataNascimento)}`}
              {` · ${s.usuario.perfil} · ${s.abertas.length} em aberto · ${s.historico.length} no histórico`}
            </p>
          </div>
        </div>
        {s.abertas.length === 0 ? (
          <div className="parabens"><span className="emo">🎉</span><div><b>Parabéns!</b>{s.usuario.mensagem}</div></div>
        ) : (
          <><div className="sec-titulo">🔥 Em aberto</div><div className="sem-pad"><TabelaDividas dividas={s.abertas} onVer={setDetalhe} /></div></>
        )}
        {s.pendentes.length > 0 && (
          <><div className="sec-titulo">⏳ Aguardando aceite / Contestadas</div><div className="sem-pad"><TabelaDividas dividas={s.pendentes} onVer={setDetalhe} /></div></>
        )}
        <div className="sec-titulo">📜 Histórico</div>
        {s.historico.length
          ? <div className="sem-pad"><TabelaDividas dividas={s.historico} onVer={setDetalhe} /></div>
          : <Vazio texto="Nenhuma dívida finalizada ainda" emo="📭" />}
      </Modal>
      {detalhe && (
        <ModalDetalheDivida
          divida={detalhe}
          exec={exec}
          tem={tem}
          usuarioLogado={usuarioLogado}
          onUpdate={(u) => { setDetalhe(u); carregar() }}
          onClose={() => setDetalhe(null)}
        />
      )}
    </>
  )
}

function Usuarios({ exec, tem, usuarioLogado, onUpdateSessao }) {
  const [usuarios, setUsuarios] = useState([])
  const [perfis, setPerfis] = useState([])
  const [busca, setBusca] = useState('')
  const [sel, setSel] = useState(null)
  const [editando, setEditando] = useState(null)
  const [form, setForm] = useState({ nome: '', login: '', dataNascimento: '', senha: '', perfilId: '', avatar: avatarPadrao('M') })
  const podeGerenciar = tem(PERM.USUARIOS_GERENCIAR)

  const carregar = () => exec(api.usuarios).then((u) => Array.isArray(u) && setUsuarios(u))
  useEffect(() => {
    carregar()
    if (podeGerenciar) exec(api.perfis).then((p) => Array.isArray(p) && setPerfis(p.filter((x) => x.ativo)))
  }, [podeGerenciar])

  const abrirNovo = () => {
    setForm({ nome: '', login: '', dataNascimento: '', senha: '', perfilId: perfis[0]?.id ?? '', avatar: avatarPadrao('M') })
    setEditando('novo')
  }
  const abrirEdicao = (e, u) => {
    e.stopPropagation()
    setForm({
      nome: u.nome,
      login: u.login,
      dataNascimento: u.dataNascimento ? u.dataNascimento.slice(0, 10) : '',
      senha: '',
      perfilId: u.perfilId,
      avatar: parseAvatar(u.avatar) || avatarPadrao('M')
    })
    setEditando(u)
  }

  const salvar = async (e) => {
    e.preventDefault()
    const avatarJson = serializeAvatar(form.avatar)
    const ok = editando === 'novo'
      ? await exec(() => api.criarUsuario({ nome: form.nome, login: form.login, dataNascimento: form.dataNascimento, senha: form.senha, perfilId: +form.perfilId, avatar: avatarJson }), 'Usuário criado!')
      : await exec(() => api.atualizarUsuario(editando.id, { nome: form.nome, login: form.login, dataNascimento: form.dataNascimento, perfilId: +form.perfilId, novaSenha: form.senha || null, avatar: avatarJson }), 'Usuário atualizado!')
    if (ok) {
      if (editando !== 'novo' && ok?.id && onUpdateSessao) onUpdateSessao(ok)
      setEditando(null)
      carregar()
    }
  }

  const alternar = async (e, u) => {
    e.stopPropagation()
    if (await exec(() => api.ativoUsuario(u.id, !u.ativo), u.ativo ? 'Usuário desativado' : 'Usuário ativado')) carregar()
  }

  const lista = usuarios.filter((u) => (u.nome + u.login + u.perfil).toLowerCase().includes(busca.toLowerCase()))

  return (
    <>
      <div className="card">
        <div className="card-cab">
          <div><h3>Equipe / Usuários</h3><p>{usuarios.length} pessoa(s) cadastrada(s)</p></div>
          <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
            <div className="campo-busca"><Search size={18} /><input placeholder="Buscar pessoa ou login" value={busca} onChange={(e) => setBusca(e.target.value)} /></div>
            {podeGerenciar && <button onClick={abrirNovo}><Plus size={18} />Novo usuário</button>}
          </div>
        </div>
        {lista.length ? (
          <div className="pessoas">
            {lista.map((u) => (
              <div key={u.id} className="pessoa" style={{ opacity: u.ativo ? 1 : .55 }} onClick={() => setSel(u.id)}>
                <Avatar nome={u.nome} avatar={u.avatar} grande />
                <h4>{u.nome}</h4>
                <small>@{u.login}{u.dataNascimento ? ` · 🎂 ${fmtCurta(u.dataNascimento)}` : ''}</small>
                <span className="badge b-neutro"><Shield size={12} />{u.perfil}</span>
                <small>{u.totalDividas} dívida(s) no histórico</small>
                {u.dividasAbertas
                  ? <span className="badge b-aberta">{u.dividasAbertas} em aberto</span>
                  : <span className="badge b-quitada">🎉 Parabéns, sem dívidas!</span>}
                {podeGerenciar && (
                  <div style={{ display: 'flex', gap: 8, marginTop: 6 }}>
                    <button className="sec peq" onClick={(e) => abrirEdicao(e, u)}><Pencil size={13} />Editar</button>
                    {u.id !== usuarioLogado.id && (
                      <button className={u.ativo ? 'sec peq' : 'ok peq'} onClick={(e) => alternar(e, u)}>
                        {u.ativo ? 'Desativar' : 'Ativar'}
                      </button>
                    )}
                  </div>
                )}
              </div>
            ))}
          </div>
        ) : <Vazio texto="Nenhum usuário encontrado" emo="🙈" />}
      </div>

      {editando && (
        <Modal peq titulo={editando === 'novo' ? 'Novo usuário' : 'Editar usuário'} onClose={() => setEditando(null)}
          rodape={<><button className="sec" onClick={() => setEditando(null)}>Cancelar</button><button form="f-user">Salvar</button></>}>
          <form id="f-user" className="coluna" onSubmit={salvar}>
            <EditorAvatar nome={form.nome} value={form.avatar} onChange={(avatar) => setForm({ ...form, avatar })} />
            <label>Nome completo<input value={form.nome} onChange={(e) => setForm({ ...form, nome: e.target.value })} required /></label>
            <label>Login<input type="text" value={form.login} onChange={(e) => setForm({ ...form, login: e.target.value })} placeholder="ex.: joao.silva" required /></label>
            <label>Data de nascimento<input type="date" value={form.dataNascimento} max={hoje()} onChange={(e) => setForm({ ...form, dataNascimento: e.target.value })} required /></label>
            <label>Perfil
              <select value={form.perfilId} onChange={(e) => setForm({ ...form, perfilId: e.target.value })} required>
                <option value="">Selecione</option>
                {perfis.map((p) => <option key={p.id} value={p.id}>{p.nome}</option>)}
              </select>
            </label>
            <label>
              {editando === 'novo' ? 'Senha (mín. 6 caracteres)' : 'Nova senha (deixe em branco para manter)'}
              <input type="password" minLength={editando === 'novo' ? 6 : undefined} value={form.senha}
                onChange={(e) => setForm({ ...form, senha: e.target.value })} required={editando === 'novo'} />
            </label>
          </form>
        </Modal>
      )}
      {sel && <Situacao id={sel} exec={exec} tem={tem} usuarioLogado={usuarioLogado} onClose={() => setSel(null)} />}
    </>
  )
}

function Motivos({ exec, tem }) {
  const [motivos, setMotivos] = useState([])
  const [desc, setDesc] = useState('')
  const carregar = () => exec(api.motivos).then((m) => Array.isArray(m) && setMotivos(m))
  useEffect(() => { carregar() }, [])

  const salvar = async (e) => {
    e.preventDefault()
    if (await exec(() => api.criarMotivo(desc), 'Motivo adicionado!')) { setDesc(''); carregar() }
  }
  const alternar = async (m) => { await exec(() => api.ativoMotivo(m.id, !m.ativo)); carregar() }

  return (
    <div className="card">
      <div className="card-cab"><div><h3>Motivos</h3><p>{motivos.filter((m) => m.ativo).length} ativo(s)</p></div></div>
      {tem(PERM.MOTIVOS_GERENCIAR) && (
        <div className="card-corpo">
          <form className="linha" onSubmit={salvar}>
            <label>Novo motivo<input value={desc} onChange={(e) => setDesc(e.target.value)} placeholder="Ex.: Chegou atrasado na daily" required /></label>
            <button><Plus size={18} />Adicionar</button>
          </form>
        </div>
      )}
      <div className="motivos">
        {motivos.map((m) => (
          <div key={m.id} className={'motivo' + (m.ativo ? '' : ' inativo')}>
            {m.descricao}
            {tem(PERM.MOTIVOS_GERENCIAR) && (
              <button className={m.ativo ? 'sec peq' : 'peq'} onClick={() => alternar(m)}>{m.ativo ? 'Desativar' : 'Ativar'}</button>
            )}
          </div>
        ))}
      </div>
    </div>
  )
}

function Perfis({ exec }) {
  const [perfis, setPerfis] = useState([])
  const [permissoes, setPermissoes] = useState([])
  const [editando, setEditando] = useState(null)
  const [form, setForm] = useState({ nome: '', descricao: '', permissaoIds: [] })

  const carregar = () => exec(api.perfis).then((p) => Array.isArray(p) && setPerfis(p))
  useEffect(() => {
    carregar()
    exec(api.permissoes).then((p) => Array.isArray(p) && setPermissoes(p))
  }, [])

  const grupos = useMemo(() => {
    const m = {}
    permissoes.forEach((p) => { (m[p.modulo] = m[p.modulo] || []).push(p) })
    return Object.entries(m)
  }, [permissoes])

  const abrirNovo = () => {
    setForm({ nome: '', descricao: '', permissaoIds: [] })
    setEditando('novo')
  }
  const abrirEdicao = (p) => {
    setForm({ nome: p.nome, descricao: p.descricao || '', permissaoIds: p.permissoes.map((x) => x.id) })
    setEditando(p)
  }

  const togglePerm = (id) => {
    setForm((f) => ({
      ...f,
      permissaoIds: f.permissaoIds.includes(id) ? f.permissaoIds.filter((x) => x !== id) : [...f.permissaoIds, id]
    }))
  }

  const salvar = async (e) => {
    e.preventDefault()
    const payload = { nome: form.nome, descricao: form.descricao, permissaoIds: form.permissaoIds }
    const ok = editando === 'novo'
      ? await exec(() => api.criarPerfil(payload), 'Perfil criado!')
      : await exec(() => api.atualizarPerfil(editando.id, payload), 'Perfil atualizado!')
    if (ok) { setEditando(null); carregar() }
  }

  const alternar = async (p) => {
    if (await exec(() => api.ativoPerfil(p.id, !p.ativo), p.ativo ? 'Perfil desativado' : 'Perfil ativado')) carregar()
  }

  return (
    <>
      <div className="card">
        <div className="card-cab">
          <div><h3>Perfis e Permissões</h3><p>Defina o que cada perfil pode visualizar ou gerenciar</p></div>
          <button onClick={abrirNovo}><Plus size={18} />Novo perfil</button>
        </div>
        <div className="perfis-grid">
          {perfis.map((p) => (
            <div key={p.id} className={'perfil-card' + (p.ativo ? '' : ' inativo')}>
              <div className="p-top">
                <div>
                  <h4>{p.nome}</h4>
                  <p>{p.descricao || 'Sem descrição'}</p>
                </div>
                <span className={'badge ' + (p.ativo ? 'b-quitada' : 'b-neutro')}>{p.ativo ? 'Ativo' : 'Inativo'}</span>
              </div>
              <div className="perm-tags">
                {p.permissoes.length
                  ? p.permissoes.map((perm) => <span key={perm.id} className="perm-tag">{perm.descricao}</span>)
                  : <span className="perm-tag">Sem permissões</span>}
              </div>
              <div className="perfil-rodape">
                <small style={{ color: 'var(--txt-2)', fontWeight: 700 }}>{p.totalUsuarios} usuário(s)</small>
                <div style={{ display: 'flex', gap: 8 }}>
                  <button className="sec peq" onClick={() => abrirEdicao(p)}><Pencil size={14} />Editar</button>
                  {p.id !== 1 && (
                    <button className={p.ativo ? 'sec peq' : 'ok peq'} onClick={() => alternar(p)}>
                      {p.ativo ? 'Desativar' : 'Ativar'}
                    </button>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {editando && (
        <Modal titulo={editando === 'novo' ? 'Novo perfil' : `Editar perfil: ${editando.nome}`} onClose={() => setEditando(null)}
          rodape={<><button className="sec" onClick={() => setEditando(null)}>Cancelar</button><button form="f-perfil">Salvar</button></>}>
          <form id="f-perfil" className="coluna" onSubmit={salvar}>
            <label>Nome do perfil<input value={form.nome} onChange={(e) => setForm({ ...form, nome: e.target.value })} required /></label>
            <label>Descrição<input value={form.descricao} onChange={(e) => setForm({ ...form, descricao: e.target.value })} placeholder="Opcional" /></label>
            <div className="perm-grupos">
              {grupos.map(([modulo, lista]) => (
                <div key={modulo} className="perm-grupo">
                  <h5>{modulo}</h5>
                  <div className="perm-checks">
                    {lista.map((perm) => (
                      <label key={perm.id} className="perm-check">
                        <input type="checkbox" checked={form.permissaoIds.includes(perm.id)} onChange={() => togglePerm(perm.id)} />
                        <span>{perm.descricao}</span>
                        <small>{perm.codigo}</small>
                      </label>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          </form>
        </Modal>
      )}
    </>
  )
}

function Configuracoes({ exec }) {
  const [cfg, setCfg] = useState(null)
  const [valorMin, setValorMin] = useState('')
  const [salvando, setSalvando] = useState(false)

  useEffect(() => {
    exec(api.configuracoes).then((r) => {
      if (r) {
        setCfg(r)
        setValorMin(String(r.valorMinimoQuitacaoConjunta ?? 30))
      }
    })
  }, [])

  const salvarCfg = async (proximo, msg) => {
    setSalvando(true)
    const r = await exec(() => api.atualizarConfiguracoes(proximo), msg)
    if (r) {
      setCfg(r)
      setValorMin(String(r.valorMinimoQuitacaoConjunta))
    }
    setSalvando(false)
  }

  const salvarValorMin = (e) => {
    e.preventDefault()
    if (!cfg) return
    salvarCfg(
      { ...cfg, valorMinimoQuitacaoConjunta: Number(valorMin) },
      'Valor mínimo de quitação em conjunto atualizado!'
    )
  }

  return (
    <div className="card">
      <div className="card-cab">
        <div>
          <h3>Configurações</h3>
          <p>Ajuste regras e comportamentos do sistema</p>
        </div>
      </div>
      <div className="card-corpo">
        {!cfg ? (
          <Vazio texto="Carregando configurações..." emo="⚙️" />
        ) : (
          <div className="config-lista">
            <div className="config-item">
              <div className="config-info">
                <div className="config-titulo">
                  <h4>Exigir aceite de dívidas</h4>
                  <span className={'badge ' + (cfg.exigirAceiteDivida ? 'b-quitada' : 'b-neutro')}>
                    {cfg.exigirAceiteDivida ? 'Ativado' : 'Desativado'}
                  </span>
                </div>
                <p>
                  {cfg.exigirAceiteDivida
                    ? 'Quando ativado, novas dívidas cadastradas para outra pessoa ficam aguardando aceite (o devedor pode aceitar ou contestar).'
                    : 'Quando desativado, novas dívidas são lançadas diretamente como "Em aberto", sem etapa de aceite.'}
                </p>
              </div>
              <label className="switch" aria-label="Exigir aceite de dívidas">
                <input
                  type="checkbox"
                  checked={cfg.exigirAceiteDivida}
                  disabled={salvando}
                  onChange={(e) =>
                    salvarCfg(
                      { ...cfg, exigirAceiteDivida: e.target.checked },
                      e.target.checked ? 'Aceite de dívidas ativado!' : 'Aceite desativado — dívidas irão direto em aberto!'
                    )
                  }
                />
                <span className="switch-trilha" />
              </label>
            </div>

            <div className="config-item">
              <div className="config-info">
                <div className="config-titulo">
                  <h4>🎂 Dívida automática de aniversário</h4>
                  <span className="badge b-aberta">
                    {TIPOS[cfg.tipoDividaAniversario]?.emo} {TIPOS[cfg.tipoDividaAniversario]?.nome}
                  </span>
                </div>
                <p>
                  No dia do aniversário de cada usuário ativo, o sistema gera automaticamente uma dívida do tipo escolhido abaixo (no máximo uma por ano).
                </p>
              </div>
              <div className="seg">
                {Object.entries(TIPOS).map(([k, v]) => (
                  <button
                    key={k}
                    type="button"
                    disabled={salvando}
                    className={cfg.tipoDividaAniversario === k ? 'ativo' : ''}
                    onClick={() =>
                      cfg.tipoDividaAniversario !== k &&
                      salvarCfg(
                        { ...cfg, tipoDividaAniversario: k },
                        `Dívida de aniversário definida como ${v.nome}!`
                      )
                    }
                  >
                    {v.emo} {v.nome}
                  </button>
                ))}
              </div>
            </div>

            <div className="config-item">
              <div className="config-info">
                <div className="config-titulo">
                  <h4>🤝 Valor mínimo por dívida (pagamento em conjunto)</h4>
                  <span className="badge b-neutro">{fmtValor(cfg.valorMinimoQuitacaoConjunta)}</span>
                </div>
                <p>
                  Valor mínimo que cada participante precisa contribuir por dívida selecionada ao entrar em um grupo de pagamento.
                </p>
              </div>
              <form className="config-valor" onSubmit={salvarValorMin}>
                <input
                  type="number"
                  step="0.01"
                  min="0.01"
                  value={valorMin}
                  onChange={(e) => setValorMin(e.target.value)}
                  disabled={salvando}
                  required
                />
                <button disabled={salvando}>Salvar</button>
              </form>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}

function Produtos({ exec, tem }) {
  const [produtos, setProdutos] = useState([])
  const [editando, setEditando] = useState(null)
  const [form, setForm] = useState({ nome: '', valor: '' })
  const podeGerenciar = tem(PERM.MOTIVOS_GERENCIAR)

  const carregar = () => exec(api.produtos).then((p) => Array.isArray(p) && setProdutos(p))
  useEffect(() => { carregar() }, [])

  const abrirNovo = () => { setForm({ nome: '', valor: '' }); setEditando('novo') }
  const abrirEdicao = (p) => { setForm({ nome: p.nome, valor: String(p.valor) }); setEditando(p) }

  const salvar = async (e) => {
    e.preventDefault()
    const payload = { nome: form.nome, valor: Number(form.valor) }
    const ok = editando === 'novo'
      ? await exec(() => api.criarProduto(payload), 'Produto cadastrado!')
      : await exec(() => api.atualizarProduto(editando.id, payload), 'Produto atualizado!')
    if (ok) { setEditando(null); carregar() }
  }

  const alternar = async (p) => {
    if (await exec(() => api.ativoProduto(p.id, !p.ativo), p.ativo ? 'Produto desativado' : 'Produto ativado')) carregar()
  }

  return (
    <>
      <div className="card">
        <div className="card-cab">
          <div>
            <h3>Produtos para Pagamento em Grupo</h3>
            <p>{produtos.filter((p) => p.ativo).length} produto(s) ativo(s) disponíveis para criar grupos</p>
          </div>
          {podeGerenciar && <button onClick={abrirNovo}><Plus size={18} />Novo produto</button>}
        </div>
        {produtos.length ? (
          <div className="produtos-grid">
            {produtos.map((p) => (
              <div key={p.id} className={'produto-card' + (p.ativo ? '' : ' inativo')}>
                <div className="produto-top">
                  <h4>{p.nome}</h4>
                  <span className={'badge ' + (p.ativo ? 'b-quitada' : 'b-neutro')}>{p.ativo ? 'Ativo' : 'Inativo'}</span>
                </div>
                <div className="produto-valor num">{fmtValor(p.valor)}</div>
                {podeGerenciar && (
                  <div style={{ display: 'flex', gap: 8, marginTop: 'auto' }}>
                    <button className="sec peq" onClick={() => abrirEdicao(p)}><Pencil size={13} />Editar</button>
                    <button className={p.ativo ? 'sec peq' : 'ok peq'} onClick={() => alternar(p)}>
                      {p.ativo ? 'Desativar' : 'Ativar'}
                    </button>
                  </div>
                )}
              </div>
            ))}
          </div>
        ) : <Vazio texto="Nenhum produto cadastrado ainda. Cadastre um produto com valor para usar nos grupos de pagamento!" emo="🛍️" />}
      </div>

      {editando && (
        <Modal peq titulo={editando === 'novo' ? 'Novo produto' : 'Editar produto'} onClose={() => setEditando(null)}
          rodape={<><button className="sec" onClick={() => setEditando(null)}>Cancelar</button><button form="f-prod">Salvar</button></>}>
          <form id="f-prod" className="coluna" onSubmit={salvar}>
            <label>Nome do produto<input value={form.nome} onChange={(e) => setForm({ ...form, nome: e.target.value })} placeholder="Ex.: Bolo de chocolate + Refri" required autoFocus /></label>
            <label>Valor (R$)<input type="number" step="0.01" min="0.01" value={form.valor} onChange={(e) => setForm({ ...form, valor: e.target.value })} placeholder="120.00" required /></label>
          </form>
        </Modal>
      )}
    </>
  )
}
