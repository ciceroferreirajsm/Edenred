import { formatarCpf, formatarData, formatarPlaca, formatarTelefone, somenteDigitos } from './formatters';

describe('formatters', () => {
  it('somenteDigitos remove caracteres não numéricos', () => {
    expect(somenteDigitos('529.982.247-25')).toBe('52998224725');
    expect(somenteDigitos(null)).toBe('');
  });

  it.each([
    ['529', '529'],
    ['5299', '529.9'],
    ['5299822', '529.982.2'],
    ['52998224725', '529.982.247-25'],
    ['529982247259999', '529.982.247-25'],
  ])('formatarCpf(%s) = %s', (entrada, esperado) => {
    expect(formatarCpf(entrada)).toBe(esperado);
  });

  it.each([
    ['', ''],
    ['11', '(11'],
    ['1133334444', '(11) 3333-4444'],
    ['11987654321', '(11) 98765-4321'],
  ])('formatarTelefone(%s) = %s', (entrada, esperado) => {
    expect(formatarTelefone(entrada)).toBe(esperado);
  });

  it('formatarPlaca normaliza e limita a 7 caracteres', () => {
    expect(formatarPlaca('abc-1d23x')).toBe('ABC1D23');
  });

  it('formatarData converte ISO para dd/MM/yyyy sem fuso', () => {
    expect(formatarData('1990-05-20')).toBe('20/05/1990');
    expect(formatarData(null)).toBe('');
  });
});
