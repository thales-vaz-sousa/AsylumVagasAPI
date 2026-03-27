-- ============================================================
-- SEED: Dados de população do banco — Asilos Criciúma/SC
-- Execução: psql -U postgres -d asylum_vagas -f seed.sql
-- ============================================================

BEGIN;

-- ─── ASILOS ──────────────────────────────────────────────────────────────────
INSERT INTO asilos (nome, cnpj, endereco, cidade, telefone) VALUES
    ('Lar dos Idosos São Francisco',       '12.345.678/0001-90', 'Rua Cel. Pedro Benedet, 100',    'Criciúma', '(48) 3433-1000'),
    ('Casa de Repouso Boa Vida',           '98.765.432/0001-10', 'Av. Universitária, 450',          'Criciúma', '(48) 3433-2200'),
    ('Residencial Bem Estar',              '55.444.333/0001-22', 'Rua Henrique Lage, 280',          'Criciúma', '(48) 3455-3300'),
    ('Instituto Esperança de Vida',        '77.888.999/0001-44', 'Rua Marechal Deodoro, 530',       'Criciúma', '(48) 3444-4400'),
    ('Centro de Convivência Raio de Sol',  '66.777.888/0001-55', 'Av. Centenário, 1200',            'Criciúma', '(48) 3422-5500'),
    ('Lar Recanto da Paz',                 '33.222.111/0001-66', 'Rua São José, 88',                'Criciúma', '(48) 3411-6600'),
    ('Residencial Vila dos Avós',          '11.222.333/0001-77', 'Rua Conselheiro João Zanette, 44','Criciúma', '(48) 3400-7700')
ON CONFLICT (cnpj) DO NOTHING;

-- ─── QUARTOS ─────────────────────────────────────────────────────────────────
INSERT INTO quartos (asilo_id, numero, capacidade_total, tipo, preco_base) VALUES
    -- Asilo 1 — São Francisco
    (1, '101', 2, 'Masculino', 2500.00),
    (1, '102', 2, 'Feminino',  2500.00),
    (1, '103', 4, 'Misto',     2000.00),
    (1, '104', 1, 'Feminino',  3200.00),
    (1, '105', 3, 'Masculino', 2200.00),

    -- Asilo 2 — Boa Vida
    (2, '201', 1, 'Masculino', 3500.00),
    (2, '202', 2, 'Feminino',  3000.00),
    (2, '203', 2, 'Misto',     2800.00),
    (2, '204', 4, 'Misto',     2400.00),

    -- Asilo 3 — Bem Estar
    (3, '301', 3, 'Misto',     1800.00),
    (3, '302', 2, 'Feminino',  1900.00),
    (3, '303', 2, 'Masculino', 1900.00),

    -- Asilo 4 — Esperança de Vida
    (4, '401', 1, 'Feminino',  4200.00),
    (4, '402', 2, 'Masculino', 3800.00),
    (4, '403', 2, 'Misto',     3600.00),
    (4, '404', 4, 'Misto',     3000.00),

    -- Asilo 5 — Raio de Sol
    (5, '501', 2, 'Feminino',  2700.00),
    (5, '502', 2, 'Masculino', 2700.00),
    (5, '503', 1, 'Misto',     3100.00),

    -- Asilo 6 — Recanto da Paz
    (6, '601', 3, 'Misto',     1600.00),
    (6, '602', 2, 'Feminino',  1700.00),

    -- Asilo 7 — Vila dos Avós
    (7, '701', 2, 'Masculino', 2900.00),
    (7, '702', 2, 'Feminino',  2900.00),
    (7, '703', 1, 'Misto',     3400.00)
ON CONFLICT (asilo_id, numero) DO NOTHING;

-- ─── RESIDENTES ──────────────────────────────────────────────────────────────
INSERT INTO residentes (quarto_id, nome, cpf, data_entrada, status) VALUES
    -- Quarto 1 (cap 2, Masculino) — 2 ativos → LOTADO
    (1, 'João da Silva',          '123.456.789-00', NOW() - INTERVAL '120 days', 'Ativo'),
    (1, 'Carlos Antônio Pereira', '111.222.333-44', NOW() - INTERVAL '60 days',  'Ativo'),

    -- Quarto 2 (cap 2, Feminino) — 1 ativo → 1 vaga
    (2, 'Maria Aparecida Santos', '987.654.321-00', NOW() - INTERVAL '45 days',  'Ativo'),

    -- Quarto 3 (cap 4, Misto) — 1 ativo (1 é Óbito) → 3 vagas
    (3, 'Pedro Alves Neto',       '555.666.777-88', NOW() - INTERVAL '30 days',  'Ativo'),
    (3, 'Ana Lima Ferreira',       '444.555.666-77', NOW() - INTERVAL '200 days', 'Obito'),

    -- Quarto 4 (cap 1, Feminino) — 1 Alta → 1 vaga (Alta não conta)
    (4, 'Tereza Ramos',            '222.333.444-55', NOW() - INTERVAL '90 days',  'Alta'),

    -- Quarto 5 (cap 3, Masculino) — 2 ativos → 1 vaga
    (5, 'Raimundo Costa',          '888.999.000-11', NOW() - INTERVAL '15 days',  'Ativo'),
    (5, 'Antônio Oliveira',        '777.888.999-22', NOW() - INTERVAL '20 days',  'Ativo'),

    -- Quarto 6 (cap 1, Masculino) — 1 ativo → LOTADO
    (6, 'Manoel Souza',            '666.777.888-33', NOW() - INTERVAL '10 days',  'Ativo'),

    -- Quarto 7 (cap 2, Feminino) — 1 ativo → 1 vaga
    (7, 'Benedita Carvalho',       '333.444.555-66', NOW() - INTERVAL '5 days',   'Ativo'),

    -- Quarto 8 (cap 2, Misto) — 0 ativos → 2 vagas
    -- (vazio, sem residentes)

    -- Quarto 9 (cap 4, Misto) — 2 ativos → 2 vagas
    (9, 'José Rodrigues',          '121.232.343-45', NOW() - INTERVAL '55 days',  'Ativo'),
    (9, 'Francisca Barbosa',       '456.567.678-89', NOW() - INTERVAL '70 days',  'Ativo'),

    -- Quarto 10 (cap 3, Misto) — 3 ativos → LOTADO
    (10, 'Valter Mendes',          '789.890.901-12', NOW() - INTERVAL '80 days',  'Ativo'),
    (10, 'Lurdes Pinto',           '012.123.234-56', NOW() - INTERVAL '40 days',  'Ativo'),
    (10, 'Orlando Teixeira',       '345.456.567-90', NOW() - INTERVAL '25 days',  'Ativo'),

    -- Quarto 11 (cap 2, Feminino) — 1 ativo → 1 vaga
    (11, 'Helena Machado',         '678.789.890-23', NOW() - INTERVAL '12 days',  'Ativo'),

    -- Quarto 12 (cap 2, Masculino) — 0 ativos → 2 vagas
    -- (vazio)

    -- Quarto 13 (cap 1, Feminino) — 1 ativo → LOTADO
    (13, 'Conceição Alves',        '901.012.123-67', NOW() - INTERVAL '100 days', 'Ativo'),

    -- Quarto 14 (cap 2, Masculino) — 1 ativo → 1 vaga
    (14, 'Geraldo Lima',           '234.345.456-01', NOW() - INTERVAL '35 days',  'Ativo'),

    -- Quarto 17 (cap 2, Feminino) — 2 ativos → LOTADO
    (17, 'Rosaria Fernandes',      '567.678.789-34', NOW() - INTERVAL '50 days',  'Ativo'),
    (17, 'Iracema Cunha',          '890.901.012-78', NOW() - INTERVAL '18 days',  'Ativo'),

    -- Quarto 18 (cap 2, Masculino) — 1 ativo → 1 vaga
    (18, 'Severino Gomes',         '123.234.345-89', NOW() - INTERVAL '8 days',   'Ativo'),

    -- Quarto 21 (cap 3, Misto) — 1 ativo → 2 vagas
    (21, 'Augusto Vieira',         '456.567.679-01', NOW() - INTERVAL '22 days',  'Ativo')
ON CONFLICT (cpf) DO NOTHING;

-- ─── SOLICITAÇÕES ─────────────────────────────────────────────────────────────
INSERT INTO solicitacoes_vaga (asilo_id, nome_solicitante, telefone, status, data_solicitacao) VALUES
    (1, 'Fernanda Costa',         '(48) 99901-1234', 'Pendente',  NOW() - INTERVAL '3 days'),
    (1, 'Marcos Oliveira',        '(48) 99902-5678', 'Pendente',  NOW() - INTERVAL '1 day'),
    (1, 'Luciana Borges',         '(48) 99903-9012', 'Aprovada',  NOW() - INTERVAL '10 days'),
    (2, 'Juliana Martins',        '(48) 99904-3456', 'Aprovada',  NOW() - INTERVAL '7 days'),
    (2, 'Roberto Santos',         '(48) 99905-7890', 'Pendente',  NOW() - INTERVAL '2 days'),
    (3, 'Patrícia Duarte',        '(48) 99906-2345', 'Pendente',  NOW()),
    (4, 'Diego Nascimento',       '(48) 99907-6789', 'Pendente',  NOW() - INTERVAL '4 days'),
    (4, 'Simone Freitas',         '(48) 99908-0123', 'Rejeitada', NOW() - INTERVAL '15 days'),
    (5, 'Alexandre Moura',        '(48) 99909-4567', 'Pendente',  NOW() - INTERVAL '6 days'),
    (6, 'Camila Ribeiro',         '(48) 99910-8901', 'Pendente',  NOW() - INTERVAL '1 day'),
    (7, 'Fábio Cardoso',          '(48) 99911-2345', 'Pendente',  NOW() - INTERVAL '9 days'),
    (7, 'Natália Azevedo',        '(48) 99912-6789', 'Aprovada',  NOW() - INTERVAL '20 days');

COMMIT;

-- ─── VERIFICAÇÃO RÁPIDA ───────────────────────────────────────────────────────
SELECT 'asilos'            AS tabela, COUNT(*) AS registros FROM asilos
UNION ALL
SELECT 'quartos',                     COUNT(*) FROM quartos
UNION ALL
SELECT 'residentes',                  COUNT(*) FROM residentes
UNION ALL
SELECT 'solicitacoes_vaga',           COUNT(*) FROM solicitacoes_vaga;
