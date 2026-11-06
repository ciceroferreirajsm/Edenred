import { FormControl } from '@angular/forms';
import { AppValidators, cpfValido, placaValida } from './validators';

describe('cpfValido', () => {
  it.each(['529.982.247-25', '52998224725', '111.444.777-35'])('aceita CPF válido %s', (cpf) => {
    expect(cpfValido(cpf)).toBe(true);
  });

  it.each(['', '123', '529.982.247-26', '000.000.000-00', '11111111111', null])('rejeita CPF inválido %s', (cpf) => {
    expect(cpfValido(cpf)).toBe(false);
  });
});

describe('placaValida', () => {
  it.each(['ABC1234', 'abc-1234', 'ABC1D23', 'bra2e19'])('aceita placa %s', (placa) => {
    expect(placaValida(placa)).toBe(true);
  });

  it.each(['', 'AB12345', 'ABCD123', 'ABC1DD3', '1BC1234'])('rejeita placa %s', (placa) => {
    expect(placaValida(placa)).toBe(false);
  });
});

describe('AppValidators', () => {
  it('cpf: não valida campo vazio (responsabilidade do required)', () => {
    expect(AppValidators.cpf(new FormControl(''))).toBeNull();
    expect(AppValidators.cpf(new FormControl('123'))).toEqual({ cpf: true });
  });

  it.each([
    ['maria@email.com', null],
    ['maria.silva+tag@empresa.com.br', null],
    ['maria', { email: true }],
    ['maria@email', { email: true }],
    ['maria @email.com', { email: true }],
  ])('email %s', (valor, esperado) => {
    expect(AppValidators.email(new FormControl(valor))).toEqual(esperado);
  });

  it.each([
    ['', null],
    ['(11) 98765-4321', null],
    ['(11) 3333-4444', null],
    ['1234', { telefone: true }],
  ])('telefone %s', (valor, esperado) => {
    expect(AppValidators.telefone(new FormControl(valor))).toEqual(esperado);
  });

  it('dataNaoFutura rejeita datas posteriores a hoje', () => {
    const validator = AppValidators.dataNaoFutura(() => new Date(2025, 5, 15));
    expect(validator(new FormControl('2025-06-15'))).toBeNull();
    expect(validator(new FormControl('2025-06-16'))).toEqual({ dataFutura: true });
  });

  it('anoMaximoAtual rejeita ano maior que o atual', () => {
    const validator = AppValidators.anoMaximoAtual(() => 2025);
    expect(validator(new FormControl(2025))).toBeNull();
    expect(validator(new FormControl(2026))).toEqual({ anoFuturo: { max: 2025 } });
  });
});
