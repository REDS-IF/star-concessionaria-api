CREATE DATABASE bd_star_concessionaria;
USE bd_star_concessionaria;

CREATE TABLE Loja (
    id_loj INT PRIMARY KEY AUTO_INCREMENT,
    nome_loj VARCHAR(300),
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
) ENGINE=InnoDB;

CREATE TABLE UsuarioSistema (
    id_usu INT PRIMARY KEY AUTO_INCREMENT,
    nome_usu VARCHAR(300),
    email_usu VARCHAR(300),
    senha_usu VARCHAR(300),
    perfil_usu VARCHAR(100)
) ENGINE=InnoDB;

CREATE TABLE Cliente (
    id_cli INT PRIMARY KEY AUTO_INCREMENT,
    nome_cli VARCHAR(300),
    email_cli VARCHAR(300),
    telefone_cli VARCHAR(30)
) ENGINE=InnoDB;

CREATE TABLE FormaPagamento (
    id_fp INT PRIMARY KEY AUTO_INCREMENT,
    nome_fp VARCHAR(100)
) ENGINE=InnoDB;

CREATE TABLE Veiculo (
    id_vei INT PRIMARY KEY AUTO_INCREMENT,
    id_loj_fk INT,
    tipo_vei VARCHAR(30),
    marca_vei VARCHAR(100),
    modelo_vei VARCHAR(100),
    ano_vei INT,
    preco_vei DECIMAL(12,2),
    quilometragem_vei INT,
    cor_vei VARCHAR(50),
    descricao_vei TEXT,
    situacao_vei VARCHAR(30),
    FOREIGN KEY (id_loj_fk) REFERENCES Loja(id_loj)
) ENGINE=InnoDB;

CREATE TABLE FotoVeiculo (
    id_fot INT PRIMARY KEY AUTO_INCREMENT,
    id_vei_fk INT,
    url_fot VARCHAR(500),
    FOREIGN KEY (id_vei_fk) REFERENCES Veiculo(id_vei)
) ENGINE=InnoDB;

CREATE TABLE AlteracaoVeiculo (
    id_alt INT PRIMARY KEY AUTO_INCREMENT,
    id_vei_fk INT,
    descricao_alt TEXT,
    FOREIGN KEY (id_vei_fk) REFERENCES Veiculo(id_vei)
) ENGINE=InnoDB;

CREATE TABLE Contato (
    id_con INT PRIMARY KEY AUTO_INCREMENT,
    id_cli_fk INT,
    id_vei_fk INT,
    id_loj_fk INT,
    id_usu_fk INT,
    tipo_con VARCHAR(100),
    mensagem_con TEXT,
    data_con DATETIME,
    FOREIGN KEY (id_cli_fk) REFERENCES Cliente(id_cli),
    FOREIGN KEY (id_vei_fk) REFERENCES Veiculo(id_vei),
    FOREIGN KEY (id_loj_fk) REFERENCES Loja(id_loj),
    FOREIGN KEY (id_usu_fk) REFERENCES UsuarioSistema(id_usu)
) ENGINE=InnoDB;

CREATE TABLE Reserva (
    id_res INT PRIMARY KEY AUTO_INCREMENT,
    id_cli_fk INT,
    id_vei_fk INT,
    data_solicitacao_res DATETIME,
    situacao_res VARCHAR(100),
    FOREIGN KEY (id_cli_fk) REFERENCES Cliente(id_cli),
    FOREIGN KEY (id_vei_fk) REFERENCES Veiculo(id_vei)
) ENGINE=InnoDB;

CREATE TABLE Agendamento (
    id_age INT PRIMARY KEY AUTO_INCREMENT,
    id_cli_fk INT,
    id_loj_fk INT,
    id_vei_fk INT,
    tipo_age VARCHAR(100),
    data_hora_inicio_age DATETIME,
    data_hora_fim_age DATETIME,
    situacao_age VARCHAR(100),
    FOREIGN KEY (id_cli_fk) REFERENCES Cliente(id_cli),
    FOREIGN KEY (id_loj_fk) REFERENCES Loja(id_loj),
    FOREIGN KEY (id_vei_fk) REFERENCES Veiculo(id_vei)
) ENGINE=InnoDB;

CREATE TABLE Peca (
    id_pec INT PRIMARY KEY AUTO_INCREMENT,
    nome_pec VARCHAR(300),
    descricao_pec TEXT,
    preco_pec DECIMAL(10,2),
    quantidade_estoque_pec INT
) ENGINE=InnoDB;

CREATE TABLE VendaVeiculo (
    id_vv INT PRIMARY KEY AUTO_INCREMENT,
    id_vei_fk INT,
    id_cli_fk INT,
    id_vendedor_fk INT,
    id_loj_fk INT,
    id_fp_fk INT,
    data_venda_vv DATETIME,
    valor_final_vv DECIMAL(12,2),
    FOREIGN KEY (id_vei_fk) REFERENCES Veiculo(id_vei),
    FOREIGN KEY (id_cli_fk) REFERENCES Cliente(id_cli),
    FOREIGN KEY (id_vendedor_fk) REFERENCES UsuarioSistema(id_usu),
    FOREIGN KEY (id_loj_fk) REFERENCES Loja(id_loj),
    FOREIGN KEY (id_fp_fk) REFERENCES FormaPagamento(id_fp)
) ENGINE=InnoDB;

CREATE TABLE VendaPeca (
    id_vp INT PRIMARY KEY AUTO_INCREMENT,
    id_pec_fk INT,
    id_cli_fk INT,
    quantidade_vp INT,
    valor_unitario_vp DECIMAL(10,2),
    data_venda_vp DATETIME,
    FOREIGN KEY (id_pec_fk) REFERENCES Peca(id_pec),
    FOREIGN KEY (id_cli_fk) REFERENCES Cliente(id_cli)
) ENGINE=InnoDB;

CREATE TABLE DuvidaFrequente (
    id_df INT PRIMARY KEY AUTO_INCREMENT,
    pergunta_df TEXT,
    resposta_df TEXT
) ENGINE=InnoDB;