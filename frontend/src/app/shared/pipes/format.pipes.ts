import { Pipe, PipeTransform } from '@angular/core';
import { formatarCpf, formatarData, formatarTelefone } from '../utils/formatters';

@Pipe({ name: 'cpf' })
export class CpfPipe implements PipeTransform {
  transform(valor: string | null | undefined): string {
    return formatarCpf(valor);
  }
}

@Pipe({ name: 'telefone' })
export class TelefonePipe implements PipeTransform {
  transform(valor: string | null | undefined): string {
    return formatarTelefone(valor) || '—';
  }
}

@Pipe({ name: 'dataBr' })
export class DataBrPipe implements PipeTransform {
  transform(valor: string | null | undefined): string {
    return formatarData(valor);
  }
}
