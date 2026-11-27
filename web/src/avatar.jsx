import { Shuffle } from 'lucide-react'

const BG_COLORS = 'f9c46b,ff9a5c,e5674b,fbe3a0,c8875e'

export const CABELOS_POR_GENERO = {
  F: [
    { id: 'straight01', label: 'Liso longo' },
    { id: 'straight02', label: 'Liso com franja' },
    { id: 'straightAndStrand', label: 'Liso com mecha' },
    { id: 'longButNotTooLong', label: 'Médio liso' },
    { id: 'curvy', label: 'Ondulado longo' },
    { id: 'bob', label: 'Curto (Bob)' },
    { id: 'miaWallace', label: 'Chanel com franja' },
    { id: 'curly', label: 'Cacheado' },
    { id: 'bigHair', label: 'Volumoso' },
    { id: 'bun', label: 'Coque' },
    { id: 'fro', label: 'Afro / Black power' },
    { id: 'froBand', label: 'Afro com faixa' },
    { id: 'dreads', label: 'Dreads longos' },
    { id: 'shavedSides', label: 'Lateral raspada' },
    { id: 'frida', label: 'Com flores' },
    { id: 'winterHat03', label: 'Touca' },
    { id: 'hijab', label: 'Hijab' }
  ],
  M: [
    { id: 'shortFlat', label: 'Curto liso' },
    { id: 'shortWaved', label: 'Ondulado' },
    { id: 'shortCurly', label: 'Cacheado' },
    { id: 'shortRound', label: 'Arredondado' },
    { id: 'theCaesar', label: 'Baixo' },
    { id: 'theCaesarAndSidePart', label: 'Baixo repartido' },
    { id: 'frizzle', label: 'Arrepiado' },
    { id: 'shaggy', label: 'Bagunçado' },
    { id: 'shaggyMullet', label: 'Mullet' },
    { id: 'dreads01', label: 'Dreads curtos' },
    { id: 'dreads02', label: 'Dreads médios' },
    { id: 'sides', label: 'Laterais (calvo no topo)' },
    { id: 'careca', label: 'Careca' },
    { id: 'hat', label: 'Chapéu' },
    { id: 'winterHat1', label: 'Touca' },
    { id: 'turban', label: 'Turbante' }
  ]
}

export const CORES_CABELO = [
  { id: '2c1b18', label: 'Preto' },
  { id: '4a312c', label: 'Castanho escuro' },
  { id: '724133', label: 'Castanho' },
  { id: 'd6b370', label: 'Loiro' },
  { id: 'a55728', label: 'Ruivo' },
  { id: 'e8e1e1', label: 'Grisalho' }
]

export const TONS_PELE = [
  { id: 'ffdbb4', label: 'Clara' },
  { id: 'edb98a', label: 'Bege' },
  { id: 'd08b5b', label: 'Morena' },
  { id: 'ae5d29', label: 'Parda' },
  { id: '614335', label: 'Escura' }
]

export const BARBAS = [
  { id: 'nenhuma', label: 'Sem barba' },
  { id: 'beardLight', label: 'Barba curta' },
  { id: 'beardMedium', label: 'Barba cheia' },
  { id: 'beardMajestic', label: 'Barba longa' },
  { id: 'moustacheFancy', label: 'Bigode fino' },
  { id: 'moustacheMagnum', label: 'Bigode grosso' }
]

export const OCULOS = [
  { id: 'nenhum', label: 'Sem óculos' },
  { id: 'prescription01', label: 'Grau fino' },
  { id: 'prescription02', label: 'Grau clássico' },
  { id: 'round', label: 'Redondo' },
  { id: 'sunglasses', label: 'Escuro' },
  { id: 'wayfarers', label: 'Wayfarer' },
  { id: 'kurt', label: 'Retrô' },
  { id: 'eyepatch', label: 'Tapa-olho' }
]

export const ROUPAS = [
  { id: 'shirtCrewNeck', label: 'Camiseta gola redonda' },
  { id: 'shirtVNeck', label: 'Camiseta gola V' },
  { id: 'shirtScoopNeck', label: 'Blusa gola canoa' },
  { id: 'graphicShirt', label: 'Camiseta estampada' },
  { id: 'hoodie', label: 'Moletom' },
  { id: 'collarAndSweater', label: 'Suéter com gola' },
  { id: 'overall', label: 'Jardineira' },
  { id: 'blazerAndShirt', label: 'Blazer com camisa' },
  { id: 'blazerAndSweater', label: 'Blazer com suéter' }
]

export function avatarPadrao(genero = 'M') {
  const g = genero === 'F' ? 'F' : 'M'
  return {
    genero: g,
    cabelo: CABELOS_POR_GENERO[g][0].id,
    corCabelo: '4a312c',
    pele: 'edb98a',
    barba: 'nenhuma',
    oculos: 'nenhum',
    roupa: 'shirtCrewNeck',
    variacao: 1
  }
}

export function parseAvatar(raw) {
  if (!raw) return null
  if (typeof raw === 'object') return raw
  try {
    const obj = JSON.parse(raw)
    return obj && typeof obj === 'object' ? obj : null
  } catch {
    return null
  }
}

export function serializeAvatar(cfg) {
  if (!cfg) return null
  return JSON.stringify(cfg)
}

function escolher(lista) {
  return lista[Math.floor(Math.random() * lista.length)].id
}

export function sortearAvatar(genero = 'M') {
  const g = genero === 'F' ? 'F' : 'M'
  return {
    genero: g,
    cabelo: escolher(CABELOS_POR_GENERO[g]),
    corCabelo: escolher(CORES_CABELO),
    pele: escolher(TONS_PELE),
    barba: g === 'M' ? escolher(BARBAS) : 'nenhuma',
    oculos: escolher(OCULOS),
    roupa: escolher(ROUPAS),
    variacao: Math.floor(Math.random() * 30) + 1
  }
}

export const TODOS_CABELOS = [...CABELOS_POR_GENERO.F, ...CABELOS_POR_GENERO.M]

export function buildAvatarUrl(nome, avatar) {
  const params = new URLSearchParams()
  const cfg = parseAvatar(avatar)
  // Quando personalizado, usa a variação fixa para não mudar expressão/cores ao digitar o nome
  params.set('seed', cfg ? `v-${cfg.variacao ?? 1}` : (nome || 'u'))
  params.set('backgroundColor', BG_COLORS)
  if (cfg) {
    const cabelosValidos = TODOS_CABELOS.map((c) => c.id)

    if (cfg.cabelo === 'careca') {
      params.set('topProbability', '0')
    } else if (cfg.cabelo && cabelosValidos.includes(cfg.cabelo)) {
      params.set('top', cfg.cabelo)
      params.set('topProbability', '100')
    } else {
      params.set('top', cabelosValidos.filter((c) => c !== 'careca').join(','))
      params.set('topProbability', '100')
    }

    if (cfg.corCabelo) {
      params.set('hairColor', cfg.corCabelo)
      params.set('facialHairColor', cfg.corCabelo)
    }
    if (cfg.pele) params.set('skinColor', cfg.pele)

    if (!cfg.barba || cfg.barba === 'nenhuma') {
      params.set('facialHairProbability', '0')
    } else {
      params.set('facialHair', cfg.barba)
      params.set('facialHairProbability', '100')
    }

    if (!cfg.oculos || cfg.oculos === 'nenhum') {
      params.set('accessoriesProbability', '0')
    } else {
      params.set('accessories', cfg.oculos)
      params.set('accessoriesProbability', '100')
    }

    if (cfg.roupa) params.set('clothing', cfg.roupa)
  }

  return `https://api.dicebear.com/9.x/avataaars/svg?${params.toString()}`
}

export function Avatar({ nome, avatar, grande }) {
  const url = buildAvatarUrl(nome, avatar)
  return (
    <div className={'avatar' + (grande ? ' g' : '')}>
      <img src={url} alt={nome} />
    </div>
  )
}

export function EditorAvatar({ nome, value, onChange }) {
  const cfg = parseAvatar(value) || avatarPadrao('M')

  const atualizar = (patch) => onChange({ ...cfg, ...patch })

  const trocarGenero = (g) => {
    if (g === cfg.genero) return
    const lista = CABELOS_POR_GENERO[g]
    atualizar({
      genero: g,
      cabelo: lista[0].id,
      barba: g === 'F' ? 'nenhuma' : cfg.barba
    })
  }

  return (
    <div className="avatar-editor">
      <div className="avatar-preview">
        <Avatar nome={nome || 'Preview'} avatar={cfg} grande />
        <div className="avatar-preview-info">
          <b>Personalizar avatar</b>
          <div className="avatar-genero">
            <button type="button" className={cfg.genero === 'F' ? 'ativo' : ''} onClick={() => trocarGenero('F')}>
              ♀ Feminino
            </button>
            <button type="button" className={cfg.genero === 'M' ? 'ativo' : ''} onClick={() => trocarGenero('M')}>
              ♂ Masculino
            </button>
          </div>
        </div>
        <button type="button" className="sec peq" title="Sortear características" onClick={() => onChange(sortearAvatar(cfg.genero))}>
          <Shuffle size={14} />Sortear
        </button>
      </div>

      <div className="avatar-slider-box">
        <div className="avatar-slider-cab">
          <span className="avatar-lbl" style={{ margin: 0 }}>Expressão e cores</span>
          <span className="avatar-slider-val">#{cfg.variacao ?? 1}</span>
        </div>
        <input
          type="range"
          className="avatar-slider"
          min={1}
          max={30}
          step={1}
          value={cfg.variacao ?? 1}
          onChange={(e) => atualizar({ variacao: Number(e.target.value) })}
        />
      </div>

      <div className="avatar-cores">
        <div>
          <span className="avatar-lbl">Cor do cabelo</span>
          <div className="cor-lista">
            {CORES_CABELO.map((c) => (
              <button
                type="button"
                key={c.id}
                title={c.label}
                className={'cor-dot' + (cfg.corCabelo === c.id ? ' ativo' : '')}
                style={{ background: `#${c.id}` }}
                onClick={() => atualizar({ corCabelo: c.id })}
              />
            ))}
          </div>
        </div>
        <div>
          <span className="avatar-lbl">Tom de pele</span>
          <div className="cor-lista">
            {TONS_PELE.map((p) => (
              <button
                type="button"
                key={p.id}
                title={p.label}
                className={'cor-dot' + (cfg.pele === p.id ? ' ativo' : '')}
                style={{ background: `#${p.id}` }}
                onClick={() => atualizar({ pele: p.id })}
              />
            ))}
          </div>
        </div>
      </div>

      <div className="avatar-grid">
        <label>
          Cabelo
          <select value={cfg.cabelo} onChange={(e) => atualizar({ cabelo: e.target.value })}>
            <optgroup label="Longos e médios">
              {CABELOS_POR_GENERO.F.map((c) => <option key={c.id} value={c.id}>{c.label}</option>)}
            </optgroup>
            <optgroup label="Curtos e outros">
              {CABELOS_POR_GENERO.M.map((c) => <option key={c.id} value={c.id}>{c.label}</option>)}
            </optgroup>
          </select>
        </label>

        <label>
          Óculos
          <select value={cfg.oculos} onChange={(e) => atualizar({ oculos: e.target.value })}>
            {OCULOS.map((o) => <option key={o.id} value={o.id}>{o.label}</option>)}
          </select>
        </label>

        <label>
          Roupa
          <select value={cfg.roupa} onChange={(e) => atualizar({ roupa: e.target.value })}>
            {ROUPAS.map((r) => <option key={r.id} value={r.id}>{r.label}</option>)}
          </select>
        </label>

        <label>
          Barba
          <select value={cfg.barba || 'nenhuma'} onChange={(e) => atualizar({ barba: e.target.value })}>
            {BARBAS.map((b) => <option key={b.id} value={b.id}>{b.label}</option>)}
          </select>
        </label>
      </div>
    </div>
  )
}
