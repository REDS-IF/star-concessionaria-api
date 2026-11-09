-- =============================================================================
-- BANCO DE DADOS STAR CONCESSIONÁRIA (COMPATÍVEL POSTGRESQL 16+)
-- Fonte de verdade derivada dos requisitos e schema oficial (docs/database/star.sql)
-- =============================================================================

CREATE TABLE Loja (
    id_loj SERIAL PRIMARY KEY,
    nome_loj VARCHAR(300) NOT NULL,
    telefone_loj VARCHAR(30),
    cep_loj VARCHAR(15),
    endereco_loj VARCHAR(300),
    numero_loj VARCHAR(30),
    complemento_loj VARCHAR(100),
    bairro_loj VARCHAR(100),
    cidade_loj VARCHAR(100),
    estado_loj CHAR(2),
    latitude_loj DECIMAL(10,7),
    longitude_loj DECIMAL(10,7)
);

CREATE TABLE UsuarioSistema (
    id_usu SERIAL PRIMARY KEY,
    nome_usu VARCHAR(300) NOT NULL,
    email_usu VARCHAR(300) NOT NULL UNIQUE,
    senha_usu VARCHAR(300) NOT NULL,
    perfil_usu VARCHAR(100) NOT NULL
);

CREATE TABLE Cliente (
    id_cli SERIAL PRIMARY KEY,
    nome_cli VARCHAR(300) NOT NULL,
    email_cli VARCHAR(300) NOT NULL,
    telefone_cli VARCHAR(30) NOT NULL
);

CREATE TABLE FormaPagamento (
    id_fp SERIAL PRIMARY KEY,
    nome_fp VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE Veiculo (
    id_vei SERIAL PRIMARY KEY,
    id_loj_fk INT NOT NULL REFERENCES Loja(id_loj) ON DELETE RESTRICT,
    tipo_vei VARCHAR(30) NOT NULL,
    marca_vei VARCHAR(100) NOT NULL,
    modelo_vei VARCHAR(100) NOT NULL,
    ano_vei INT NOT NULL,
    preco_vei DECIMAL(12,2) NOT NULL,
    quilometragem_vei INT NOT NULL,
    cor_vei VARCHAR(50) NOT NULL,
    descricao_vei TEXT,
    situacao_vei VARCHAR(30) NOT NULL DEFAULT 'Disponivel'
);

CREATE TABLE FotoVeiculo (
    id_fot SERIAL PRIMARY KEY,
    id_vei_fk INT NOT NULL REFERENCES Veiculo(id_vei) ON DELETE CASCADE,
    url_fot VARCHAR(500) NOT NULL
);

CREATE TABLE AlteracaoVeiculo (
    id_alt SERIAL PRIMARY KEY,
    id_vei_fk INT NOT NULL REFERENCES Veiculo(id_vei) ON DELETE CASCADE,
    descricao_alt TEXT NOT NULL
);

CREATE TABLE Contato (
    id_con SERIAL PRIMARY KEY,
    id_cli_fk INT REFERENCES Cliente(id_cli) ON DELETE SET NULL,
    id_vei_fk INT REFERENCES Veiculo(id_vei) ON DELETE SET NULL,
    id_loj_fk INT REFERENCES Loja(id_loj) ON DELETE SET NULL,
    id_usu_fk INT REFERENCES UsuarioSistema(id_usu) ON DELETE SET NULL,
    tipo_con VARCHAR(100) NOT NULL,
    mensagem_con TEXT NOT NULL,
    data_con TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE Reserva (
    id_res SERIAL PRIMARY KEY,
    id_cli_fk INT NOT NULL REFERENCES Cliente(id_cli) ON DELETE RESTRICT,
    id_vei_fk INT NOT NULL REFERENCES Veiculo(id_vei) ON DELETE RESTRICT,
    data_solicitacao_res TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    situacao_res VARCHAR(100) NOT NULL DEFAULT 'Pendente'
);

CREATE TABLE Agendamento (
    id_age SERIAL PRIMARY KEY,
    id_cli_fk INT NOT NULL REFERENCES Cliente(id_cli) ON DELETE RESTRICT,
    id_loj_fk INT NOT NULL REFERENCES Loja(id_loj) ON DELETE RESTRICT,
    id_vei_fk INT REFERENCES Veiculo(id_vei) ON DELETE SET NULL,
    tipo_age VARCHAR(100) NOT NULL,
    data_hora_inicio_age TIMESTAMP WITH TIME ZONE NOT NULL,
    data_hora_fim_age TIMESTAMP WITH TIME ZONE NOT NULL,
    situacao_age VARCHAR(100) NOT NULL DEFAULT 'Solicitado'
);

CREATE TABLE Peca (
    id_pec SERIAL PRIMARY KEY,
    nome_pec VARCHAR(300) NOT NULL,
    descricao_pec TEXT,
    preco_pec DECIMAL(10,2) NOT NULL,
    quantidade_estoque_pec INT NOT NULL DEFAULT 0
);

CREATE TABLE VendaVeiculo (
    id_vv SERIAL PRIMARY KEY,
    id_vei_fk INT NOT NULL UNIQUE REFERENCES Veiculo(id_vei) ON DELETE RESTRICT,
    id_cli_fk INT NOT NULL REFERENCES Cliente(id_cli) ON DELETE RESTRICT,
    id_vendedor_fk INT NOT NULL REFERENCES UsuarioSistema(id_usu) ON DELETE RESTRICT,
    id_loj_fk INT NOT NULL REFERENCES Loja(id_loj) ON DELETE RESTRICT,
    id_fp_fk INT NOT NULL REFERENCES FormaPagamento(id_fp) ON DELETE RESTRICT,
    data_venda_vv TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    valor_final_vv DECIMAL(12,2) NOT NULL
);

CREATE TABLE VendaPeca (
    id_vp SERIAL PRIMARY KEY,
    id_pec_fk INT NOT NULL REFERENCES Peca(id_pec) ON DELETE RESTRICT,
    id_cli_fk INT REFERENCES Cliente(id_cli) ON DELETE SET NULL,
    quantidade_vp INT NOT NULL,
    valor_unitario_vp DECIMAL(10,2) NOT NULL,
    data_venda_vp TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE DuvidaFrequente (
    id_df SERIAL PRIMARY KEY,
    pergunta_df TEXT NOT NULL,
    resposta_df TEXT NOT NULL
);

-- ÍNDICES DE PERFORMANCE E INTEGRIDADE
CREATE INDEX IX_Veiculo_Filtros ON Veiculo (situacao_vei, marca_vei, modelo_vei, ano_vei, preco_vei);
CREATE INDEX IX_Veiculo_Loja ON Veiculo (id_loj_fk);
CREATE INDEX IX_Reserva_Veiculo ON Reserva (id_vei_fk, situacao_res);
CREATE INDEX IX_Agendamento_Veiculo_Horario ON Agendamento (id_vei_fk, data_hora_inicio_age, data_hora_fim_age);
