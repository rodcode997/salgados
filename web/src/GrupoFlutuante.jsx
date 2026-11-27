import { useEffect, useState } from 'react'
import { ChevronDown, Plus, Users, Trash2, LogOut, CheckCircle2, Sparkles } from 'lucide-react'
import { api } from './api'
import { Avatar } from './avatar'

const hoje = () => new Date().toISOString().slice(0, 10)
const fmtValor = (v) => (v != null ? Number(v).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }) : 'R$ 0,00')
const fmtData = (d) => (d ? new Date(d).toLocaleDateString('pt-BR', { timeZone: 'UTC' }) : '—')
const fmtCurta = (d) => (d ? new Date(d).toLocaleDateString('pt-BR', { timeZone: 'UTC', day: '2-digit', month: '2-digit' }) : '—')
const TIPOS_EMO = { CentoSalgado: '🥟', CaixaChocolate: '🍫' }

export function GrupoFlutuante({ usuarioLogado, exec, tem, versao, onAtualizar }) {
  const [aberto, setAberto] = useState(false)
  const [criando, setCriando] = useState(false)
  const [grupos, setGrupos] = useState([])
  const [produtos, setProdutos] = useState([])
  const [valorMinimoCfg, setValorMinimoCfg] = useState(30)
  const [minhasDividas, setMinhasDividas] = useState([])
  const [grupoSelId, setGrupoSelId] = useState(null)

  // Form criar grupo
  const [produtoId, setProdutoId] = useState('')
  const [dataCompra, setDataCompra] = useState(hoje())
  const [obsGrupo, setObsGrupo] = useState('')

  // Form participar
  const [dividasSel, setDividasSel] = useState([])

  const carregar = async () => {
    try {
      const [gs, ps, cfg, ds] = await Promise.all([
        api.gruposPagamento(),
        api.produtos(),
        api.configuracoes().catch(() => null),
        api.dividas(`?usuarioId=${usuarioLogado.id}&status=Aceita`).catch(() => [])
      ])
      if (Array.isArray(gs)) {
        setGrupos(gs)
        const abertos = gs.filter((g) => g.status === 'Aberto')
        setGrupoSelId((prev) => (abertos.some((g) => g.id === prev) ? prev : (abertos[0]?.id ?? null)))
      }
      if (Array.isArray(ps)) {
        const ativos = ps.filter((p) => p.ativo)
        setProdutos(ativos)
        setProdutoId((prev) => prev || (ativos[0]?.id ? String(ativos[0].id) : ''))
      }
      if (cfg?.valorMinimoQuitacaoConjunta) setValorMinimoCfg(cfg.valorMinimoQuitacaoConjunta)
      if (Array.isArray(ds)) setMinhasDividas(ds)
    } catch { /* silencioso no widget flutuante */ }
  }

  useEffect(() => {
    carregar()
    const id = setInterval(carregar, 25000)
    return () => clearInterval(id)
  }, [usuarioLogado.id, versao])

  const abertos = grupos.filter((g) => g.status === 'Aberto')
  const grupoAtual = abertos.find((g) => g.id === grupoSelId) || abertos[0] || null
  const minhaPart = grupoAtual?.participacoes.find((p) => p.usuarioId === usuarioLogado.id) || null

  useEffect(() => {
    if (!grupoAtual) {
      setDividasSel([])
      return
    }
    setDividasSel(minhaPart ? minhaPart.dividas.map((d) => d.id) : [])
  }, [grupoAtual?.id, minhaPart?.id, minhaPart?.dividas?.length])

  const dividasOutros = grupoAtual
    ? grupoAtual.participacoes.filter((p) => p.usuarioId !== usuarioLogado.id).reduce((a, p) => a + p.dividas.length, 0)
    : 0
  const vagasDisponiveis = grupoAtual ? Math.max(0, grupoAtual.totalDividasMeta - dividasOutros) : 0

  const toggleDivida = (id) => {
    setDividasSel((prev) => {
      if (prev.includes(id)) return prev.filter((x) => x !== id)
      if (prev.length >= vagasDisponiveis) return prev
      return [...prev, id]
    })
  }

  const criarGrupo = async (e) => {
    e.preventDefault()
    if (!produtoId || !dataCompra) return
    const novo = await exec(
      () => api.criarGrupoPagamento({ produtoId: +produtoId, dataCompra, observacao: obsGrupo || null }),
      'Grupo de pagamento criado!'
    )
    if (novo?.id) {
      setCriando(false)
      setDataCompra(hoje())
      setObsGrupo('')
      setGrupoSelId(novo.id)
      await carregar()
    }
  }

  const participar = async (e) => {
    e.preventDefault()
    if (!grupoAtual) return
    const res = await exec(() => api.participarGrupo(grupoAtual.id, { dividaIds: dividasSel }), null)
    if (res?.id) {
      if (res.status === 'Fechado') {
        await exec(async () => true, '🎉 Grupo fechado e todas as dívidas quitadas!')
        if (onAtualizar) onAtualizar()
      } else {
        await exec(async () => true, minhaPart ? 'Participação atualizada!' : 'Você entrou no grupo!')
      }
      await carregar()
    }
  }

  const sairGrupo = async () => {
    if (!grupoAtual) return
    const res = await exec(() => api.sairGrupo(grupoAtual.id), 'Você saiu do grupo.')
    if (res) await carregar()
  }

  const cancelarGrupo = async () => {
    if (!grupoAtual) return
    if (await exec(() => api.cancelarGrupo(grupoAtual.id), 'Grupo cancelado.')) {
      await carregar()
    }
  }

  // Prévia ao montar novo grupo
  const prodSel = produtos.find((p) => String(p.id) === String(produtoId))
  const previaQtd = prodSel && valorMinimoCfg > 0 ? Math.floor(prodSel.valor / valorMinimoCfg) : 0
  const previaValorDivida = previaQtd > 0 && prodSel ? prodSel.valor / previaQtd : 0

  const pct = grupoAtual && grupoAtual.totalDividasMeta > 0
    ? Math.min(100, Math.round((grupoAtual.totalDividasSelecionadas / grupoAtual.totalDividasMeta) * 100))
    : 0
  const faltamDividas = grupoAtual ? Math.max(0, grupoAtual.totalDividasMeta - grupoAtual.totalDividasSelecionadas) : 0
  const vaiFechar = grupoAtual && dividasSel.length > 0 && (dividasOutros + dividasSel.length) >= grupoAtual.totalDividasMeta
  const meuValor = grupoAtual ? dividasSel.length * grupoAtual.valorPorDivida : 0

  return (
    <div className="grupo-float">
      {!aberto ? (
        <button type="button" className={'grupo-fab' + (abertos.length ? ' tem-aberto' : '')} onClick={() => setAberto(true)}>
          <span className="grupo-fab-ico">🤝</span>
          {grupoAtual ? (
            <div className="grupo-fab-info">
              <div className="grupo-fab-linha">
                <b>{grupoAtual.produtoNome}</b>
                {abertos.length > 1 && <span className="grupo-fab-qtd">+{abertos.length - 1}</span>}
              </div>
              <small>
                {grupoAtual.totalDividasSelecionadas}/{grupoAtual.totalDividasMeta} dívidas · {fmtValor(grupoAtual.valorPorDivida)}/dívida
                {grupoAtual.dataCompra ? ` · 📅 ${fmtCurta(grupoAtual.dataCompra)}` : ''}
              </small>
              <div className="grupo-barra mini"><div style={{ width: `${pct}%` }} /></div>
            </div>
          ) : (
            <span>Pagamento em grupo</span>
          )}
        </button>
      ) : (
        <div className="grupo-panel">
          <div className="grupo-panel-cab">
            <div>
              <h4>🤝 Pagamento em grupo</h4>
              <small>{abertos.length ? `${abertos.length} grupo(s) aberto(s)` : 'Junte com a equipe para quitar'}</small>
            </div>
            <div style={{ display: 'flex', gap: 6 }}>
              {!criando && (
                <button type="button" className="peq" onClick={() => setCriando(true)}>
                  <Plus size={14} />Novo
                </button>
              )}
              <button type="button" className="icone" title="Minimizar" onClick={() => { setAberto(false); setCriando(false) }}>
                <ChevronDown size={20} />
              </button>
            </div>
          </div>

          <div className="grupo-panel-corpo">
            {criando ? (
              <form className="coluna" onSubmit={criarGrupo}>
                <div className="grupo-subcab">
                  <b>Criar novo grupo</b>
                  <button type="button" className="sec peq" onClick={() => setCriando(false)}>Voltar</button>
                </div>
                {produtos.length === 0 ? (
                  <div className="grupo-aviso">
                    Nenhum produto ativo cadastrado. Cadastre um produto com valor na aba <b>Produtos</b> para criar um grupo.
                  </div>
                ) : (
                  <>
                    <label>
                      Produto
                      <select value={produtoId} onChange={(e) => setProdutoId(e.target.value)} required>
                        <option value="">Selecione o produto</option>
                        {produtos.map((p) => (
                          <option key={p.id} value={p.id}>{p.nome} — {fmtValor(p.valor)}</option>
                        ))}
                      </select>
                    </label>

                    {prodSel && (
                      <div className="grupo-aviso">
                        {previaQtd > 0 ? (
                          <>
                            Este grupo quitará <b>{previaQtd} dívida(s)</b> por <b>{fmtValor(previaValorDivida)} cada</b>
                            <div style={{ fontSize: 11, marginTop: 2 }}>(mínimo configurado: {fmtValor(valorMinimoCfg)}/dívida)</div>
                          </>
                        ) : (
                          <>O valor deste produto ({fmtValor(prodSel.valor)}) é menor que o mínimo configurado ({fmtValor(valorMinimoCfg)}).</>
                        )}
                      </div>
                    )}

                    <label>
                      Data da compra
                      <input type="date" value={dataCompra} min={hoje()} onChange={(e) => setDataCompra(e.target.value)} required />
                    </label>
                    <label>
                      Observação (opcional)
                      <input value={obsGrupo} onChange={(e) => setObsGrupo(e.target.value)} placeholder="Ex.: Pedido da sexta-feira" maxLength={300} />
                    </label>
                    <button disabled={previaQtd <= 0} style={{ justifyContent: 'center' }}><Plus size={16} />Criar grupo</button>
                  </>
                )}
              </form>
            ) : !grupoAtual ? (
              <div className="grupo-vazio">
                <div style={{ fontSize: 36 }}>🍕</div>
                <b>Nenhum grupo aberto agora</b>
                <small>Crie um grupo escolhendo um produto para dividir igualmente o valor por dívida.</small>
                <button onClick={() => setCriando(true)}><Plus size={16} />Criar grupo</button>
              </div>
            ) : (
              <>
                {abertos.length > 1 && (
                  <div className="grupo-tabs">
                    {abertos.map((g) => (
                      <button
                        type="button"
                        key={g.id}
                        className={g.id === grupoAtual.id ? 'ativo' : ''}
                        onClick={() => setGrupoSelId(g.id)}
                      >
                        #{g.id} {g.produtoNome}
                      </button>
                    ))}
                  </div>
                )}

                <div className="grupo-meta-card">
                  <div className="grupo-meta-top">
                    <div>
                      <b>{grupoAtual.produtoNome}</b>
                      <small>
                        {grupoAtual.dataCompra ? `📅 Compra em ${fmtData(grupoAtual.dataCompra)} · ` : ''}
                        Criado por {grupoAtual.criadoPor}
                        {grupoAtual.observacao ? ` · ${grupoAtual.observacao}` : ''}
                      </small>
                    </div>
                    <span className="badge b-aberta">{fmtValor(grupoAtual.valorMeta)}</span>
                  </div>
                  <div className="grupo-barra"><div style={{ width: `${pct}%` }} /></div>
                  <div className="grupo-meta-nums">
                    <span>Dívidas: <b>{grupoAtual.totalDividasSelecionadas} / {grupoAtual.totalDividasMeta}</b></span>
                    <span><b>{fmtValor(grupoAtual.valorPorDivida)}</b> / dívida</span>
                  </div>
                  <small className="grupo-min-hint">
                    Faltam <b>{faltamDividas} dívida(s)</b> para fechar o grupo automaticamente
                  </small>
                </div>

                <div className="grupo-sec">
                  <div className="grupo-sec-tit"><Users size={14} />Participantes ({grupoAtual.participacoes.length})</div>
                  {grupoAtual.participacoes.length > 0 ? (
                    <div className="grupo-parts">
                      {grupoAtual.participacoes.map((p) => (
                        <div key={p.id} className="grupo-part">
                          <Avatar nome={p.usuario} avatar={p.usuarioAvatar} />
                          <div className="grupo-part-info">
                            <b>{p.usuario}</b>
                            <small>{p.dividas.length} dívida(s)</small>
                          </div>
                          <b className="num">{fmtValor(p.valorContribuido)}</b>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <small style={{ color: 'var(--txt-2)' }}>Seja o primeiro a entrar com suas dívidas!</small>
                  )}
                </div>

                <form className="grupo-sec" onSubmit={participar}>
                  <div className="grupo-sec-tit">
                    <Sparkles size={14} />Minhas dívidas ({dividasSel.length}/{vagasDisponiveis} vaga(s))
                  </div>
                  {minhasDividas.length === 0 ? (
                    <small style={{ color: 'var(--txt-2)' }}>Você não possui dívidas em aberto no momento. 🎉</small>
                  ) : (
                    <>
                      <div className="grupo-dividas">
                        {minhasDividas.map((d) => {
                          const marcada = dividasSel.includes(d.id)
                          const bloqueada = !marcada && dividasSel.length >= vagasDisponiveis
                          return (
                            <label
                              key={d.id}
                              className={'grupo-divida-check' + (marcada ? ' sel' : '')}
                              style={bloqueada ? { opacity: .5, cursor: 'not-allowed' } : undefined}
                            >
                              <input
                                type="checkbox"
                                checked={marcada}
                                disabled={bloqueada}
                                onChange={() => toggleDivida(d.id)}
                              />
                              <span>{TIPOS_EMO[d.tipo]} <b>{d.motivo}</b></span>
                              <small>{fmtValor(grupoAtual.valorPorDivida)}</small>
                            </label>
                          )
                        })}
                      </div>

                      {dividasSel.length > 0 && (
                        <div className="grupo-aviso" style={{ padding: '8px 10px' }}>
                          Sua contribuição: <b>{dividasSel.length} × {fmtValor(grupoAtual.valorPorDivida)} = {fmtValor(meuValor)}</b>
                        </div>
                      )}

                      <div className="grupo-acoes">
                        <button
                          className={vaiFechar ? 'ok' : ''}
                          disabled={dividasSel.length === 0}
                          style={{ flex: 1, justifyContent: 'center' }}
                        >
                          <CheckCircle2 size={16} />
                          {vaiFechar ? 'Fechar grupo e quitar!' : (minhaPart ? 'Atualizar dívidas' : 'Entrar no grupo')}
                        </button>
                        {minhaPart && (
                          <button type="button" className="sec peq" title="Sair do grupo" onClick={sairGrupo}>
                            <LogOut size={14} />Sair
                          </button>
                        )}
                      </div>
                    </>
                  )}
                </form>

                {(grupoAtual.criadoPorId === usuarioLogado.id || tem('dividas.quitar')) && (
                  <div className="grupo-rodape">
                    <button type="button" className="sec peq" onClick={cancelarGrupo}>
                      <Trash2 size={13} />Cancelar grupo
                    </button>
                  </div>
                )}
              </>
            )}
          </div>
        </div>
      )}
    </div>
  )
}
