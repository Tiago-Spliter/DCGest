using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace DCGest.Classes
{
    public class Aluno : Entidade
    {
        public int Cod_Aluno { get; set; }
        public string Nome_Aluno { get; set; } = string.Empty;
        public int Cod_Turma { get; set; }
        public int Cod_Curso { get; set; }
        public string Estado_Estagio { get; set; } = string.Empty;
        public int? Cod_Ori { get; set; }
        public int Cod_Letivo { get; set; }


        public string Nome_Curso { get; set; } = string.Empty;
        public string Nome_Orientador { get; set; } = string.Empty;
        public string Nome_Turma { get; set; } = string.Empty;
        public string Intervalo_Letivo { get; set; } = string.Empty;


        public Aluno()
        {

        }

        public Aluno(int codAluno, string nomeAluno, int codTurma, int codCurso, string estadoEstagio, int? codOri, int codLetivo)
        {
            Cod_Aluno = codAluno;
            Nome_Aluno = nomeAluno;
            Cod_Turma = codTurma;
            Cod_Curso = codCurso;
            Estado_Estagio = estadoEstagio;
            Cod_Ori = codOri;
            Cod_Letivo = codLetivo;
        }

        public Aluno(int codAluno, string nomeAluno, int codTurma, int codCurso, string estadoEstagio, int? codOri, int codLetivo, string nomeCurso, string nomeOrientador, string nomeTurma, string intervaloLetivo)
        {
            Cod_Aluno = codAluno;
            Nome_Aluno = nomeAluno;
            Cod_Turma = codTurma;
            Cod_Curso = codCurso;
            Estado_Estagio = estadoEstagio;
            Cod_Ori = codOri;
            Cod_Letivo = codLetivo;
            Nome_Curso = nomeCurso;
            Nome_Orientador = nomeOrientador;
            Nome_Turma = nomeTurma;
            Intervalo_Letivo = intervaloLetivo;
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
                        string sqlAluno = @"INSERT INTO aluno (Cod_Aluno, Nome_Aluno, Cod_Turma, Cod_Curso, Estado_Estagio, Cod_Ori, Cod_Letivo)
                                            VALUES (@Cod, @Nome, @Turma, @Curso, @Estado, @Ori, @Letivo)";

                        using (MySqlCommand cmdAluno = new MySqlCommand(sqlAluno, conexao, transacao))
                        {
                            cmdAluno.Parameters.AddWithValue("@Cod", Cod_Aluno);
                            cmdAluno.Parameters.AddWithValue("@Nome", Nome_Aluno);
                            cmdAluno.Parameters.AddWithValue("@Turma", Cod_Turma);
                            cmdAluno.Parameters.AddWithValue("@Curso", Cod_Curso);
                            cmdAluno.Parameters.AddWithValue("@Estado", Estado_Estagio);
                            cmdAluno.Parameters.AddWithValue("@Ori", (object)Cod_Ori ?? DBNull.Value);
                            cmdAluno.Parameters.AddWithValue("@Letivo", Cod_Letivo);
                            cmdAluno.ExecuteNonQuery();
                        }

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

        public static Aluno ObterPorId(int id)
        {
            using (MySqlConnection conn = new MySqlConnection(BD.CaminhoBD))
            {
                conn.Open();
                string sql = @"
                    SELECT a.*, c.Nome_Curso, o.Nome_Orientador, t.Nome as Nome_Turma, al.Intervalo as Intervalo_Letivo
                    FROM aluno a
                    LEFT JOIN cursos c ON a.Cod_Curso = c.Cod_Curso
                    LEFT JOIN orientador o ON a.Cod_Ori = o.Cod_Orientador
                    LEFT JOIN turmas t ON a.Cod_Turma = t.Cod_Turma
                    LEFT JOIN anosletivos al ON a.Cod_Letivo = al.Cod_Letivo
                    WHERE a.Cod_Aluno = @Cod";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Cod", id);
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            int? orientadorId = null;
                            if (r["Cod_Ori"] != DBNull.Value)
                            {
                                orientadorId = Convert.ToInt32(r["Cod_Ori"]);
                            }

                            string nomeOri = "N/A";
                            if (r["Nome_Orientador"] != DBNull.Value)
                            {
                                nomeOri = r["Nome_Orientador"].ToString();
                            }

                            return new Aluno(
                                Convert.ToInt32(r["Cod_Aluno"]),
                                r["Nome_Aluno"].ToString(),
                                Convert.ToInt32(r["Cod_Turma"]),
                                Convert.ToInt32(r["Cod_Curso"]),
                                r["Estado_Estagio"].ToString(),
                                orientadorId,
                                Convert.ToInt32(r["Cod_Letivo"]),
                                r["Nome_Curso"].ToString(),
                                nomeOri,
                                r["Nome_Turma"].ToString(),
                                r["Intervalo_Letivo"].ToString()
                            );
                        }
                    }
                }
            }
            return null;
        }
    }
}
