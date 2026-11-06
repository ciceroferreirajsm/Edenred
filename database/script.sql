-- Script de criação do banco (SQLite) - Cadastro de Pessoas e Veículos
-- Gerado a partir das migrations do EF Core (dotnet ef migrations script).
-- Uso: sqlite3 cadastro.db < database/script.sql
-- Obs.: não é necessário executá-lo manualmente; a API aplica as migrations ao iniciar.

CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "Pessoas" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Pessoas" PRIMARY KEY,
    "Nome" TEXT NOT NULL,
    "Cpf" TEXT NOT NULL,
    "DataNascimento" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "Telefone" TEXT NULL,
    "DataCadastro" TEXT NOT NULL,
    "DataAtualizacao" TEXT NULL
);

CREATE TABLE "Veiculos" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Veiculos" PRIMARY KEY,
    "Placa" TEXT NOT NULL,
    "AnoFabricacao" INTEGER NOT NULL,
    "Marca" TEXT NOT NULL,
    "Modelo" TEXT NOT NULL,
    "PessoaId" TEXT NOT NULL,
    "DataCadastro" TEXT NOT NULL,
    "DataAtualizacao" TEXT NULL,
    CONSTRAINT "FK_Veiculos_Pessoas_PessoaId" FOREIGN KEY ("PessoaId") REFERENCES "Pessoas" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_Pessoas_Nome" ON "Pessoas" ("Nome");

CREATE UNIQUE INDEX "UX_Pessoas_Cpf" ON "Pessoas" ("Cpf");

CREATE INDEX "IX_Veiculos_PessoaId" ON "Veiculos" ("PessoaId");

CREATE UNIQUE INDEX "UX_Veiculos_Placa" ON "Veiculos" ("Placa");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261104212150_InitialCreate', '10.0.14');

COMMIT;

