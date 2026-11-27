import { useEffect, useRef, useState } from 'react'
import { FileText, Upload, ExternalLink, Bold, Italic, List, Heading1, Heading2, Minus, Eye } from 'lucide-react'
import { api } from './api'

function renderInline(texto) {
  const partes = []
  const regex = /(\*\*[^*]+\*\*|\*[^*]+\*)/g
  let last = 0
  let m
  let k = 0
  while ((m = regex.exec(texto)) !== null) {
    if (m.index > last) partes.push(texto.slice(last, m.index))
    const token = m[0]
    if (token.startsWith('**')) partes.push(<strong key={k++}>{token.slice(2, -2)}</strong>)
    else partes.push(<em key={k++}>{token.slice(1, -1)}</em>)
    last = m.index + token.length
  }
  if (last < texto.length) partes.push(texto.slice(last))
  return partes
}

export function RenderTextoFormatado({ texto }) {
  const linhas = (texto || '').split(/\r?\n/)
  const elementos = []
  let listaAtual = []
  let listaTipo = null

  const fecharLista = (idx) => {
    if (!listaAtual.length) return
    if (listaTipo === 'ol') elementos.push(<ol key={`l-${idx}`}>{listaAtual}</ol>)
    else elementos.push(<ul key={`l-${idx}`}>{listaAtual}</ul>)
    listaAtual = []
    listaTipo = null
  }

  linhas.forEach((raw, i) => {
    const l = raw.trim()
    if (!l) {
      fecharLista(i)
      return
    }
    if (/^-{3,}$/.test(l)) {
      fecharLista(i)
      elementos.push(<hr key={i} />)
      return
    }
    if (l.startsWith('### ')) {
      fecharLista(i)
      elementos.push(<h4 key={i}>{renderInline(l.slice(4))}</h4>)
      return
    }
    if (l.startsWith('## ')) {
      fecharLista(i)
      elementos.push(<h3 key={i}>{renderInline(l.slice(3))}</h3>)
      return
    }
    if (l.startsWith('# ')) {
      fecharLista(i)
      elementos.push(<h2 key={i}>{renderInline(l.slice(2))}</h2>)
      return
    }
    if (/^[-*]\s+/.test(l)) {
      if (listaTipo && listaTipo !== 'ul') fecharLista(i)
      listaTipo = 'ul'
      listaAtual.push(<li key={i}>{renderInline(l.replace(/^[-*]\s+/, ''))}</li>)
      return
    }
    if (/^\d+\.\s+/.test(l)) {
      if (listaTipo && listaTipo !== 'ol') fecharLista(i)
      listaTipo = 'ol'
      listaAtual.push(<li key={i}>{renderInline(l.replace(/^\d+\.\s+/, ''))}</li>)
      return
    }
    fecharLista(i)
    elementos.push(<p key={i}>{renderInline(l)}</p>)
  })
  fecharLista('fim')

  return <div className="lei-fmt">{elementos}</div>
}

export function VisualizadorLei({ lei, grande }) {
  const [pdfUrl, setPdfUrl] = useState(null)
  const [erroPdf, setErroPdf] = useState(false)

  useEffect(() => {
    if (!lei || lei.tipo !== 'Pdf' || !lei.temPdf) {
      setPdfUrl(null)
      return
    }
    let ativo = true
    let urlCriada = null
    setErroPdf(false)
    api.obterLeiPdfBlob()
      .then((blob) => {
        if (!ativo) return
        urlCriada = URL.createObjectURL(blob)
        setPdfUrl(urlCriada)
      })
      .catch(() => ativo && setErroPdf(true))
    return () => {
      ativo = false
      if (urlCriada) URL.revokeObjectURL(urlCriada)
    }
  }, [lei?.tipo, lei?.versao, lei?.temPdf])

  if (lei?.tipo === 'Pdf') {
    return (
      <div className="lei-pdf-wrap">
        <div className="lei-pdf-bar">
          <span><FileText size={16} /> <b>{lei.arquivoNome || 'lei-base.pdf'}</b></span>
          {pdfUrl && (
            <a href={pdfUrl} target="_blank" rel="noreferrer" download={lei.arquivoNome || 'lei-base.pdf'} className="lei-pdf-link">
              <ExternalLink size={14} />Abrir / Baixar PDF
            </a>
          )}
        </div>
        {erroPdf ? (
          <div className="grupo-aviso">Não foi possível carregar a pré-visualização do PDF.</div>
        ) : !pdfUrl ? (
          <div className="grupo-aviso">Carregando PDF...</div>
        ) : (
          <iframe className={'lei-pdf-frame' + (grande ? ' grande' : '')} src={pdfUrl} title={lei.titulo} />
        )}
      </div>
    )
  }

  return (
    <div className={'lei-texto-box' + (grande ? ' grande' : '')}>
      <RenderTextoFormatado texto={lei?.conteudo || ''} />
    </div>
  )
}

export function EditorLeiBase({ lei, exec, onSalvo, onCancelar }) {
  const [modo, setModo] = useState(lei?.tipo === 'Pdf' ? 'Pdf' : 'Texto')
  const [titulo, setTitulo] = useState(lei?.titulo || 'Lei Base dos Salgados')
  const [conteudo, setConteudo] = useState(lei?.conteudo || '')
  const [arquivo, setArquivo] = useState(null)
  const [preview, setPreview] = useState(false)
  const [salvando, setSalvando] = useState(false)
  const taRef = useRef(null)

  const aplicarFormato = (antes, depois = '', placeholder = 'texto') => {
    const el = taRef.current
    if (!el) return
    const start = el.selectionStart
    const end = el.selectionEnd
    const sel = conteudo.slice(start, end) || placeholder
    const novo = conteudo.slice(0, start) + antes + sel + depois + conteudo.slice(end)
    setConteudo(novo)
    setTimeout(() => {
      el.focus()
      el.setSelectionRange(start + antes.length, start + antes.length + sel.length)
    }, 0)
  }

  const salvar = async (e) => {
    e.preventDefault()
    setSalvando(true)
    const msg = 'Nova versão da Lei Base publicada! Os usuários precisarão aceitar no próximo login.'
    const r = modo === 'Pdf'
      ? await exec(() => api.enviarLeiPdf(titulo, arquivo), msg)
      : await exec(() => api.atualizarLeiBase({ titulo, conteudo }), msg)
    setSalvando(false)
    if (r) onSalvo(r)
  }

  return (
    <form className="coluna" onSubmit={salvar}>
      <div className="grupo-aviso">
        ⚠️ Ao publicar, a versão será incrementada para <b>v{(lei?.versao || 0) + 1}</b> e todos os usuários precisarão ler e aceitar novamente ao entrar no sistema.
      </div>

      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: 12, flexWrap: 'wrap' }}>
        <div className="seg">
          <button type="button" className={modo === 'Texto' ? 'ativo' : ''} onClick={() => setModo('Texto')}>
            📝 Texto formatado
          </button>
          <button type="button" className={modo === 'Pdf' ? 'ativo' : ''} onClick={() => setModo('Pdf')}>
            📄 Arquivo PDF
          </button>
        </div>
        {modo === 'Texto' && (
          <button type="button" className="sec peq" onClick={() => setPreview((v) => !v)}>
            <Eye size={14} />{preview ? 'Voltar ao editor' : 'Pré-visualizar'}
          </button>
        )}
      </div>

      <label>
        Título
        <input value={titulo} onChange={(e) => setTitulo(e.target.value)} maxLength={200} required />
      </label>

      {modo === 'Texto' ? (
        preview ? (
          <div className="lei-texto-box grande">
            <RenderTextoFormatado texto={conteudo || '*Nenhum texto digitado ainda.*'} />
          </div>
        ) : (
          <div className="coluna" style={{ gap: 8 }}>
            <div className="lei-toolbar">
              <button type="button" className="sec peq" title="Título" onClick={() => aplicarFormato('# ', '', 'Título')}><Heading1 size={14} />Título</button>
              <button type="button" className="sec peq" title="Subtítulo" onClick={() => aplicarFormato('## ', '', 'Subtítulo')}><Heading2 size={14} />Subtítulo</button>
              <button type="button" className="sec peq" title="Negrito" onClick={() => aplicarFormato('**', '**', 'negrito')}><Bold size={14} /></button>
              <button type="button" className="sec peq" title="Itálico" onClick={() => aplicarFormato('*', '*', 'itálico')}><Italic size={14} /></button>
              <button type="button" className="sec peq" title="Item de lista" onClick={() => aplicarFormato('- ', '', 'Regra')}><List size={14} />Lista</button>
              <button type="button" className="sec peq" title="Linha divisória" onClick={() => aplicarFormato('\n---\n', '', '')}><Minus size={14} /></button>
            </div>
            <textarea
              ref={taRef}
              rows={12}
              value={conteudo}
              onChange={(e) => setConteudo(e.target.value)}
              placeholder={'# Artigo 1º — Regras Gerais\n\n- **Atrasos em reuniões** geram 1 caixa de chocolate.\n- **Aniversariantes** pagam 1 cento de salgados.'}
              required
            />
          </div>
        )
      ) : (
        <label className="lei-upload">
          <Upload size={26} />
          <b>{arquivo ? arquivo.name : 'Clique para selecionar um arquivo PDF (máx. 10 MB)'}</b>
          <small>
            {arquivo
              ? `${(arquivo.size / 1024 / 1024).toFixed(2)} MB`
              : (lei?.temPdf ? `PDF atual: ${lei.arquivoNome}` : 'Apenas arquivos .pdf')}
          </small>
          <input
            type="file"
            accept="application/pdf,.pdf"
            onChange={(e) => setArquivo(e.target.files?.[0] || null)}
            required
          />
        </label>
      )}

      <div style={{ display: 'flex', gap: 10, justifyContent: 'flex-end' }}>
        <button type="button" className="sec" onClick={onCancelar}>Cancelar</button>
        <button disabled={salvando || (modo === 'Pdf' && !arquivo)}>
          {salvando ? 'Publicando...' : 'Publicar nova versão'}
        </button>
      </div>
    </form>
  )
}
