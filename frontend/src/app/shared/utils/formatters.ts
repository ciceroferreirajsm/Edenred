export const somenteDigitos = (valor: string | null | undefined): string => (valor ?? '').replace(/\D/g, '');

/** 52998224725 → 529.982.247-25 (aplica progressivamente enquanto o usuário digita). */
export function formatarCpf(valor: string | null | undefined): string {
  const d = somenteDigitos(valor).slice(0, 11);
  if (d.length <= 3) return d;
  if (d.length <= 6) return `${d.slice(0, 3)}.${d.slice(3)}`;
  if (d.length <= 9) return `${d.slice(0, 3)}.${d.slice(3, 6)}.${d.slice(6)}`;
  return `${d.slice(0, 3)}.${d.slice(3, 6)}.${d.slice(6, 9)}-${d.slice(9)}`;
}

/** 11987654321 → (11) 98765-4321 | 1133334444 → (11) 3333-4444 */
export function formatarTelefone(valor: string | null | undefined): string {
  const d = somenteDigitos(valor).slice(0, 11);
  if (d.length === 0) return '';
  if (d.length <= 2) return `(${d}`;
  if (d.length <= 6) return `(${d.slice(0, 2)}) ${d.slice(2)}`;
  if (d.length <= 10) return `(${d.slice(0, 2)}) ${d.slice(2, 6)}-${d.slice(6)}`;
  return `(${d.slice(0, 2)}) ${d.slice(2, 7)}-${d.slice(7)}`;
}

/** Normaliza a placa: maiúsculas, sem separadores, máx. 7 caracteres. */
export function formatarPlaca(valor: string | null | undefined): string {
  return (valor ?? '')
    .toUpperCase()
    .replace(/[^A-Z0-9]/g, '')
    .slice(0, 7);
}

/** yyyy-MM-dd → dd/MM/yyyy (sem conversão de fuso horário). */
export function formatarData(iso: string | null | undefined): string {
  if (!iso) return '';
  const [ano, mes, dia] = iso.slice(0, 10).split('-');
  return ano && mes && dia ? `${dia}/${mes}/${ano}` : iso;
}
