-- ============================================================
-- Gestão de Vagas de Asilos — Criciúma/SC
-- Script de criação do banco PostgreSQL
-- ============================================================

-- Extensão para UUIDs (opcional, ID serial usado por padrão)
-- CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ─── ASILOS ──────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS asilos (
    id        SERIAL PRIMARY KEY,
    nome      VARCHAR(200) NOT NULL,
    cnpj      VARCHAR(18)  NOT NULL UNIQUE,
    endereco  VARCHAR(300) NOT NULL,
    cidade    VARCHAR(100) NOT NULL DEFAULT 'Criciúma',
    telefone  VARCHAR(20)  NOT NULL
);

-- ─── QUARTOS ─────────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS quartos (
    id               SERIAL PRIMARY KEY,
    asilo_id         INTEGER      NOT NULL REFERENCES asilos(id) ON DELETE CASCADE,
    numero           VARCHAR(20)  NOT NULL,
    capacidade_total INTEGER      NOT NULL CHECK (capacidade_total > 0),
    tipo             VARCHAR(20)  NOT NULL CHECK (tipo IN ('Masculino', 'Feminino', 'Misto')),
    preco_base       NUMERIC(10,2) NOT NULL CHECK (preco_base >= 0),
    UNIQUE (asilo_id, numero)
);

-- ─── RESIDENTES ──────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS residentes (
    id           SERIAL PRIMARY KEY,
    quarto_id    INTEGER     NOT NULL REFERENCES quartos(id) ON DELETE RESTRICT,
    nome         VARCHAR(200) NOT NULL,
    cpf          VARCHAR(14)  NOT NULL UNIQUE,
    data_entrada TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    status       VARCHAR(20)  NOT NULL DEFAULT 'Ativo'
                 CHECK (status IN ('Ativo', 'Obito', 'Alta'))
);

-- ─── SOLICITAÇÕES DE VAGA ─────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS solicitacoes_vaga (
    id                SERIAL PRIMARY KEY,
    asilo_id          INTEGER      NOT NULL REFERENCES asilos(id) ON DELETE CASCADE,
    nome_solicitante  VARCHAR(200) NOT NULL,
    telefone          VARCHAR(20)  NOT NULL,
    status            VARCHAR(20)  NOT NULL DEFAULT 'Pendente'
                      CHECK (status IN ('Pendente', 'Aprovada', 'Rejeitada')),
    data_solicitacao  TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- ─── ÍNDICES ──────────────────────────────────────────────────────────────────
CREATE INDEX IF NOT EXISTS idx_quartos_asilo_id       ON quartos(asilo_id);
CREATE INDEX IF NOT EXISTS idx_residentes_quarto_id   ON residentes(quarto_id);
CREATE INDEX IF NOT EXISTS idx_residentes_status      ON residentes(status);
CREATE INDEX IF NOT EXISTS idx_solicitacoes_asilo_id  ON solicitacoes_vaga(asilo_id);
CREATE INDEX IF NOT EXISTS idx_solicitacoes_status    ON solicitacoes_vaga(status);

-- ─── SEED: DADOS DE EXEMPLO ──────────────────────────────────────────────────
INSERT INTO asilos (nome, cnpj, endereco, cidade, telefone) VALUES
    ('Lar dos Idosos São Francisco',  '12.345.678/0001-90', 'Rua Cel. Pedro Benedet, 100', 'Criciúma', '(48) 3433-1000'),
    ('Casa de Repouso Boa Vida',      '98.765.432/0001-10', 'Av. Universitária, 450',       'Criciúma', '(48) 3433-2200'),
    ('Residencial Bem Estar',         '55.444.333/0001-22', 'Rua Henrique Lage, 280',       'Criciúma', '(48) 3455-3300')
ON CONFLICT DO NOTHING;

INSERT INTO quartos (asilo_id, numero, capacidade_total, tipo, preco_base) VALUES
    (1, '101', 2, 'Masculino', 2500.00),
    (1, '102', 2, 'Feminino',  2500.00),
    (1, '103', 4, 'Misto',     2000.00),
    (2, '201', 1, 'Masculino', 3500.00),
    (2, '202', 2, 'Feminino',  3000.00),
    (3, '301', 3, 'Misto',     1800.00)
ON CONFLICT DO NOTHING;

INSERT INTO residentes (quarto_id, nome, cpf, data_entrada, status) VALUES
    (1, 'João da Silva',     '123.456.789-00', NOW() - INTERVAL '30 days', 'Ativo'),
    (1, 'Carlos Pereira',    '111.222.333-44', NOW() - INTERVAL '60 days', 'Ativo'),
    (2, 'Maria Aparecida',   '987.654.321-00', NOW() - INTERVAL '15 days', 'Ativo'),
    (3, 'Pedro Alves',       '555.666.777-88', NOW() - INTERVAL '5 days',  'Ativo'),
    (3, 'Ana Lima',          '444.555.666-77', NOW() - INTERVAL '90 days', 'Obito'),  -- não conta como ativo
    (4, 'Antônio Souza',     '333.444.555-66', NOW() - INTERVAL '20 days', 'Ativo')
ON CONFLICT DO NOTHING;

INSERT INTO solicitacoes_vaga (asilo_id, nome_solicitante, telefone, status, data_solicitacao) VALUES
    (1, 'Fernanda Costa',    '(48) 99901-1234', 'Pendente',  NOW() - INTERVAL '2 days'),
    (1, 'Marcos Oliveira',   '(48) 99902-5678', 'Pendente',  NOW() - INTERVAL '1 day'),
    (2, 'Juliana Martins',   '(48) 99903-9012', 'Aprovada',  NOW() - INTERVAL '5 days'),
    (3, 'Roberto Santos',    '(48) 99904-3456', 'Pendente',  NOW())
ON CONFLICT DO NOTHING;

-- ─── VIEW ÚTIL (opcional) ────────────────────────────────────────────────────
-- Útil para consultas manuais / dashboards administrativos
CREATE OR REPLACE VIEW vw_vagas_disponiveis AS
SELECT
    a.id            AS asilo_id,
    a.nome          AS asilo,
    a.cidade,
    q.id            AS quarto_id,
    q.numero,
    q.tipo,
    q.preco_base,
    q.capacidade_total,
    COUNT(r.id) FILTER (WHERE r.status = 'Ativo')                              AS residentes_ativos,
    q.capacidade_total - COUNT(r.id) FILTER (WHERE r.status = 'Ativo')        AS vagas_disponiveis
FROM asilos a
INNER JOIN quartos q ON q.asilo_id = a.id
LEFT JOIN residentes r ON r.quarto_id = q.id
GROUP BY a.id, a.nome, a.cidade, q.id, q.numero, q.tipo, q.preco_base, q.capacidade_total
HAVING (q.capacidade_total - COUNT(r.id) FILTER (WHERE r.status = 'Ativo')) > 0
ORDER BY a.nome, q.numero;
