using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace DCGest.Classes
{
    public class Aluno : Entidade
    {
        public int Cod_Aluno { get; set; }
        public string Nome_Aluno { get; set; } = string.Empty;
        public string Turma { get; set; } = string.Empty;
        public int Cod_Curso { get; set; }
        public string Estado_Estagio { get; set; } = string.Empty;
        public int? Cod_Ori { get; set; }
        public string Ano_Letivo { get; set; } = string.Empty;


        public string Nome_Curso { get; set; } = string.Empty;
        public string Nome_Orientador { get; set; } = string.Empty;


        public Aluno() 
        {
        
        }

        public Aluno(int codAluno, string nomeAluno, string turma, int codCurso, string estadoEstagio, int? codOri, string anoLetivo)
        {
            Cod_Aluno = codAluno;
            Nome_Aluno = nomeAluno;
            Turma = turma;
            Cod_Curso = codCurso;
            Estado_Estagio = estadoEstagio;
            Cod_Ori = codOri;
            Ano_Letivo = anoLetivo;
        }

        public Aluno(int codAluno, string nomeAluno, string turma, int codCurso, string estadoEstagio, int? codOri, string anoLetivo, string nomeCurso, string nomeOrientador)
        {
            Cod_Aluno = codAluno;
            Nome_Aluno = nomeAluno;
            Turma = turma;
            Cod_Curso = codCurso;
            Estado_Estagio = estadoEstagio;
            Cod_Ori = codOri;
            Ano_Letivo = anoLetivo;
            Nome_Curso = nomeCurso;
            Nome_Orientador = nomeOrientador;
        }

        public override void InserirNaBD(string connectionString)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();

                using (MySqlTransaction transacao = conexao.BeginTransaction())
                {
                    try
                    {
                        // 1. Inserir o Aluno
                        string sqlAluno = @"INSERT INTO aluno (Cod_Aluno, Nome_Aluno, Turma, Cod_Curso, Estado_Estagio, Cod_Ori, Ano_Letivo) 
                                            VALUES (@Cod, @Nome, @Turma, @Curso, @Estado, @Ori, @Ano)";

                        using (MySqlCommand cmdAluno = new MySqlCommand(sqlAluno, conexao, transacao))
                        {
                            cmdAluno.Parameters.AddWithValue("@Cod", Cod_Aluno);
                            cmdAluno.Parameters.AddWithValue("@Nome", Nome_Aluno);
                            cmdAluno.Parameters.AddWithValue("@Turma", Turma);
                            cmdAluno.Parameters.AddWithValue("@Curso", Cod_Curso);
                            cmdAluno.Parameters.AddWithValue("@Estado", Estado_Estagio);
                            cmdAluno.Parameters.AddWithValue("@Ori", (object)Cod_Ori ?? DBNull.Value);
                            cmdAluno.Parameters.AddWithValue("@Ano", Ano_Letivo);
                            cmdAluno.ExecuteNonQuery();
                        }

                        // 2. Obter Módulos do Curso
                        string sqlModulos = @"SELECT m.Cod_Modulo, d.Ano 
                                               FROM Modulos m 
                                               INNER JOIN Disciplina d ON m.Cod_Disc = d.Cod_Disc 
                                               WHERE d.Cod_Curso = @Curso";

                        List<Tuple<int, string>> listaModulos = new List<Tuple<int, string>>();
                        using (MySqlCommand cmdMod = new MySqlCommand(sqlModulos, conexao, transacao))
                        {
                            cmdMod.Parameters.AddWithValue("@Curso", Cod_Curso);
                            using (MySqlDataReader leitor = cmdMod.ExecuteReader())
                            {
                                while (leitor.Read())
                                {
                                    listaModulos.Add(new Tuple<int, string>(Convert.ToInt32(leitor["Cod_Modulo"]), leitor["Ano"].ToString()));
                                }
                            }
                        }

                        // 3. Inserir Registos de Notas
                        string sqlNota = "INSERT INTO NotaMod (Cod_Aluno, Cod_Modulo, Ano) VALUES (@Aluno, @Modulo, @Ano)";
                        foreach (var mod in listaModulos)
                        {
                            using (MySqlCommand cmdNota = new MySqlCommand(sqlNota, conexao, transacao))
                            {
                                cmdNota.Parameters.AddWithValue("@Aluno", Cod_Aluno);
                                cmdNota.Parameters.AddWithValue("@Modulo", mod.Item1);
                                cmdNota.Parameters.AddWithValue("@Ano", mod.Item2);
                                cmdNota.ExecuteNonQuery();
                            }
                        }

                        transacao.Commit();
                    }
                    catch (Exception ex)
                    {
                        transacao.Rollback();
                        throw new Exception("Erro ao processar transação de inserção de aluno: " + ex.Message);
                    }
                }
            }
        }
    }
}
