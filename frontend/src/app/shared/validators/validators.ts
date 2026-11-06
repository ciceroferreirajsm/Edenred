import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { somenteDigitos } from '../utils/formatters';

/** Mesmo algoritmo de dígitos verificadores usado no backend (Cadastro.Domain.Validation.Cpf). */
export function cpfValido(valor: string | null | undefined): boolean {
  const d = somenteDigitos(valor);
  if (d.length !== 11 || /^(\d)\1{10}$/.test(d)) return false;

  const numeros = [...d].map(Number);
  const digito = (qtd: number) => {
    const soma = numeros.slice(0, qtd).reduce((acc, n, i) => acc + n * (qtd + 1 - i), 0);
    const resto = soma % 11;
    return resto < 2 ? 0 : 11 - resto;
  };

  return numeros[9] === digito(9) && numeros[10] === digito(10);
}

/** Placa no padrão antigo (ABC1234) ou Mercosul (ABC1D23). */
export function placaValida(valor: string | null | undefined): boolean {
  const placa = (valor ?? '').toUpperCase().replace(/[^A-Z0-9]/g, '');
  return /^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$/.test(placa);
}

export const EMAIL_REGEX = /^[^@\s]+@[^@\s]+\.[^@\s.]{2,}$/;

export class AppValidators {
  static cpf: ValidatorFn = (c: AbstractControl): ValidationErrors | null =>
    !c.value || cpfValido(c.value) ? null : { cpf: true };

  static placa: ValidatorFn = (c: AbstractControl): ValidationErrors | null =>
    !c.value || placaValida(c.value) ? null : { placa: true };

  static email: ValidatorFn = (c: AbstractControl): ValidationErrors | null =>
    !c.value || EMAIL_REGEX.test(String(c.value).trim()) ? null : { email: true };

  /** Telefone opcional; quando informado deve ter DDD + número (10 ou 11 dígitos). */
  static telefone: ValidatorFn = (c: AbstractControl): ValidationErrors | null => {
    const d = somenteDigitos(c.value);
    return d.length === 0 || d.length === 10 || d.length === 11 ? null : { telefone: true };
  };

  /** Data (yyyy-MM-dd) não pode ser posterior a hoje. */
  static dataNaoFutura(hoje: () => Date = () => new Date()): ValidatorFn {
    return (c: AbstractControl): ValidationErrors | null => {
      if (!c.value) return null;
      const h = hoje();
      const hojeIso = `${h.getFullYear()}-${String(h.getMonth() + 1).padStart(2, '0')}-${String(h.getDate()).padStart(2, '0')}`;
      return String(c.value) > hojeIso ? { dataFutura: true } : null;
    };
  }

  /** Ano de fabricação não pode ser maior que o ano atual. */
  static anoMaximoAtual(anoAtual: () => number = () => new Date().getFullYear()): ValidatorFn {
    return (c: AbstractControl): ValidationErrors | null => {
      if (c.value === null || c.value === '') return null;
      const max = anoAtual();
      return Number(c.value) > max ? { anoFuturo: { max } } : null;
    };
  }
}
