export interface PagedResult<T> {
  itens: T[];
  pagina: number;
  tamanhoPagina: number;
  totalItens: number;
  totalPaginas: number;
}

export interface Pessoa {
  id: string;
  nome: string;
  cpf: string;
  /** yyyy-MM-dd */
  dataNascimento: string;
  email: string;
  telefone: string | null;
  quantidadeVeiculos: number;
  dataCadastro: string;
  dataAtualizacao: string | null;
}

export interface PessoaRequest {
  nome: string;
  cpf: string;
  dataNascimento: string;
  email: string;
  telefone: string | null;
}

export interface Veiculo {
  id: string;
  placa: string;
  anoFabricacao: number;
  marca: string;
  modelo: string;
  pessoaId: string;
  pessoaNome: string | null;
  dataCadastro: string;
  dataAtualizacao: string | null;
}

export interface VeiculoRequest {
  placa: string;
  anoFabricacao: number;
  marca: string;
  modelo: string;
  pessoaId: string;
}
