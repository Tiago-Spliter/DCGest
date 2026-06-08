/*
 DCGest - Esquema da Base de Dados
 Base de dados: pap
 MySQL 8.0.43
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for alineas
-- ----------------------------
DROP TABLE IF EXISTS `alineas`;
CREATE TABLE `alineas`  (
  `Cod_alinea` int NOT NULL,
  `Alinea` char(2) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `Regra` enum('Ausente','Falta','Dispensado','Isento','Não Avaliado','Recuperação','Prova Especial','Equivalência','Transferido','Concluido','Anulado') CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `Descricao` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`Cod_alinea`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for anosletivos
-- ----------------------------
DROP TABLE IF EXISTS `anosletivos`;
CREATE TABLE `anosletivos`  (
  `Cod_Letivo` int NOT NULL AUTO_INCREMENT,
  `Intervalo` varchar(9) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`Cod_Letivo`) USING BTREE,
  UNIQUE INDEX `Intervalo`(`Intervalo` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 8 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for autenticacao
-- ----------------------------
DROP TABLE IF EXISTS `autenticacao`;
CREATE TABLE `autenticacao`  (
  `Cod_Aut` int NOT NULL AUTO_INCREMENT,
  `Utilizador` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `PalavraPasse` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`Cod_Aut`) USING BTREE,
  UNIQUE INDEX `Utilizador`(`Utilizador` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 6 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for cursos
-- ----------------------------
DROP TABLE IF EXISTS `cursos`;
CREATE TABLE `cursos`  (
  `Cod_Curso` int NOT NULL AUTO_INCREMENT,
  `Nome_Curso` varchar(225) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`Cod_Curso`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for orientador
-- ----------------------------
DROP TABLE IF EXISTS `orientador`;
CREATE TABLE `orientador`  (
  `Cod_Orientador` int NOT NULL AUTO_INCREMENT,
  `Nome_Orientador` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`Cod_Orientador`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 15 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for turmas
-- ----------------------------
DROP TABLE IF EXISTS `turmas`;
CREATE TABLE `turmas`  (
  `Cod_Turma` int NOT NULL AUTO_INCREMENT,
  `Nome` char(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`Cod_Turma`) USING BTREE,
  UNIQUE INDEX `Nome`(`Nome` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 27 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for diretor_curso
-- ----------------------------
DROP TABLE IF EXISTS `diretor_curso`;
CREATE TABLE `diretor_curso`  (
  `Cod_DC` int NOT NULL AUTO_INCREMENT,
  `Nome_DC` varchar(225) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `Cod_Curso` int NULL DEFAULT NULL,
  `Cod_Aut` int NOT NULL,
  PRIMARY KEY (`Cod_DC`) USING BTREE,
  INDEX `Cod_Curso`(`Cod_Curso` ASC) USING BTREE,
  INDEX `diretor_curso_ibfk_2`(`Cod_Aut` ASC) USING BTREE,
  CONSTRAINT `diretor_curso_ibfk_1` FOREIGN KEY (`Cod_Curso`) REFERENCES `cursos` (`Cod_Curso`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `diretor_curso_ibfk_2` FOREIGN KEY (`Cod_Aut`) REFERENCES `autenticacao` (`Cod_Aut`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 4 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for disciplina
-- ----------------------------
DROP TABLE IF EXISTS `disciplina`;
CREATE TABLE `disciplina`  (
  `Cod_Disc` int NOT NULL AUTO_INCREMENT,
  `Designacao` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `Ano` enum('1','2','3') CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `Tipo` enum('Sócio Cultural','Científica','Técnica','Final') CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `Cod_Curso` int NULL DEFAULT NULL,
  PRIMARY KEY (`Cod_Disc`) USING BTREE,
  INDEX `Cod_Curso`(`Cod_Curso` ASC) USING BTREE,
  CONSTRAINT `disciplina_ibfk_1` FOREIGN KEY (`Cod_Curso`) REFERENCES `cursos` (`Cod_Curso`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 39 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for modulos
-- ----------------------------
DROP TABLE IF EXISTS `modulos`;
CREATE TABLE `modulos`  (
  `Cod_Modulo` int NOT NULL AUTO_INCREMENT,
  `Cod_Disc` int NULL DEFAULT NULL,
  `Designacao` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`Cod_Modulo`) USING BTREE,
  INDEX `Cod_Disc`(`Cod_Disc` ASC) USING BTREE,
  CONSTRAINT `modulos_ibfk_1` FOREIGN KEY (`Cod_Disc`) REFERENCES `disciplina` (`Cod_Disc`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 139 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for aluno
-- ----------------------------
DROP TABLE IF EXISTS `aluno`;
CREATE TABLE `aluno`  (
  `Cod_Aluno` int NOT NULL,
  `Nome_Aluno` varchar(225) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `Cod_Turma` int NOT NULL,
  `Cod_Curso` int NOT NULL,
  `Estado_Estagio` enum('Pronto','Não Pronto') CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT 'Não Pronto',
  `Cod_Ori` int NULL DEFAULT NULL,
  `Cod_Letivo` int NOT NULL,
  PRIMARY KEY (`Cod_Aluno`) USING BTREE,
  INDEX `Cod_Curso`(`Cod_Curso` ASC) USING BTREE,
  INDEX `Cod_Ori`(`Cod_Ori` ASC) USING BTREE,
  INDEX `Cod_Turma`(`Cod_Turma` ASC) USING BTREE,
  INDEX `Cod_Letivo`(`Cod_Letivo` ASC) USING BTREE,
  CONSTRAINT `aluno_ibfk_1` FOREIGN KEY (`Cod_Curso`) REFERENCES `cursos` (`Cod_Curso`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `aluno_ibfk_2` FOREIGN KEY (`Cod_Ori`) REFERENCES `orientador` (`Cod_Orientador`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `aluno_ibfk_3` FOREIGN KEY (`Cod_Turma`) REFERENCES `turmas` (`Cod_Turma`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `aluno_ibfk_4` FOREIGN KEY (`Cod_Letivo`) REFERENCES `anosletivos` (`Cod_Letivo`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for notamod
-- ----------------------------
DROP TABLE IF EXISTS `notamod`;
CREATE TABLE `notamod`  (
  `Cod_NotaMod` int NOT NULL AUTO_INCREMENT,
  `Cod_Modulo` int NOT NULL,
  `Cod_Aluno` int NOT NULL,
  `Valor` char(2) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `Cod_Estado` int NULL DEFAULT NULL,
  `Ano` enum('1º Ano','2º Ano','3º Ano') CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `Data_Efetua` date NULL DEFAULT NULL,
  PRIMARY KEY (`Cod_NotaMod`) USING BTREE,
  INDEX `Cod_Modulo`(`Cod_Modulo` ASC) USING BTREE,
  INDEX `Cod_Aluno`(`Cod_Aluno` ASC) USING BTREE,
  INDEX `Estado`(`Cod_Estado` ASC) USING BTREE,
  CONSTRAINT `notamod_ibfk_1` FOREIGN KEY (`Cod_Modulo`) REFERENCES `modulos` (`Cod_Modulo`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `notamod_ibfk_2` FOREIGN KEY (`Cod_Aluno`) REFERENCES `aluno` (`Cod_Aluno`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `notamod_ibfk_3` FOREIGN KEY (`Cod_Estado`) REFERENCES `alineas` (`Cod_alinea`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 3661 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

SET FOREIGN_KEY_CHECKS = 1;
