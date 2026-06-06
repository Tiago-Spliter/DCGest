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

        public static List<Aluno> ObterComFiltros(
            int? codLetivo = null, int? codCurso = null, int? codTurma = null,
            int? codOrientador = null, int? codAluno = null, string nomeAluno = null)
        {
            var lista = new List<Aluno>();
            using (var conn = new MySqlConnection(BD.CaminhoBD))
            {
                conn.Open();
                string sql = "SELECT a.*, c.Nome_Curso, o.Nome_Orientador, t.Nome as Nome_Turma, al.Intervalo as Intervalo_Letivo " +
                             "FROM aluno a " +
                             "LEFT JOIN cursos c ON a.Cod_Curso = c.Cod_Curso " +
                             "LEFT JOIN orientador o ON a.Cod_Ori = o.Cod_Orientador " +
                             "LEFT JOIN turmas t ON a.Cod_Turma = t.Cod_Turma " +
                             "LEFT JOIN anosletivos al ON a.Cod_Letivo = al.Cod_Letivo " +
                             "WHERE 1=1";

                if (codAluno.HasValue)      sql += " AND a.Cod_Aluno = @cod";
                if (!string.IsNullOrEmpty(nomeAluno)) sql += " AND a.Nome_Aluno LIKE @nome";
                if (codLetivo.HasValue)     sql += " AND a.Cod_Letivo = @letivo";
                if (codCurso.HasValue)      sql += " AND a.Cod_Curso = @curso";
                if (codTurma.HasValue)      sql += " AND a.Cod_Turma = @turma";
                if (codOrientador.HasValue) sql += " AND a.Cod_Ori = @orientador";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    if (codAluno.HasValue)      cmd.Parameters.AddWithValue("@cod", codAluno.Value);
                    if (!string.IsNullOrEmpty(nomeAluno)) cmd.Parameters.AddWithValue("@nome", "%" + nomeAluno + "%");
                    if (codLetivo.HasValue)     cmd.Parameters.AddWithValue("@letivo", codLetivo.Value);
                    if (codCurso.HasValue)      cmd.Parameters.AddWithValue("@curso", codCurso.Value);
                    if (codTurma.HasValue)      cmd.Parameters.AddWithValue("@turma", codTurma.Value);
                    if (codOrientador.HasValue) cmd.Parameters.AddWithValue("@orientador", codOrientador.Value);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            int? ori = r["Cod_Ori"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["Cod_Ori"]);
                            string nomeOri = r["Nome_Orientador"] == DBNull.Value ? "N/A" : r["Nome_Orientador"].ToString();

                            lista.Add(new Aluno(
                                Convert.ToInt32(r["Cod_Aluno"]),
                                r["Nome_Aluno"].ToString(),
                                Convert.ToInt32(r["Cod_Turma"]),
                                Convert.ToInt32(r["Cod_Curso"]),
                                r["Estado_Estagio"].ToString(),
                                ori,
                                Convert.ToInt32(r["Cod_Letivo"]),
                                r["Nome_Curso"].ToString(),
                                nomeOri,
                                r["Nome_Turma"].ToString(),
                                r["Intervalo_Letivo"].ToString()
                            ));
                        }
                    }
                }
            }
            return lista;
        }

        public void AtualizarNaBD()
        {
            using (var conn = new MySqlConnection(BD.CaminhoBD))
            {
                conn.Open();
                string sql = @"UPDATE aluno SET
                               Nome_Aluno = @nome,
                               Cod_Turma = @turma,
                               Cod_Curso = @curso,
                               Estado_Estagio = @estado,
                               Cod_Ori = @ori,
                               Cod_Letivo = @letivo
                               WHERE Cod_Aluno = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@nome", Nome_Aluno);
                    cmd.Parameters.AddWithValue("@turma", Cod_Turma);
                    cmd.Parameters.AddWithValue("@curso", Cod_Curso);
                    cmd.Parameters.AddWithValue("@estado", Estado_Estagio);
                    cmd.Parameters.AddWithValue("@ori", (object)Cod_Ori ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@letivo", Cod_Letivo);
                    cmd.Parameters.AddWithValue("@id", Cod_Aluno);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static bool AtualizarEstadoEstagio(int codAluno)
        {
            using (var conn = new MySqlConnection(BD.CaminhoBD))
            {
                conn.Open();

                string sqlTotal = @"SELECT COUNT(*) FROM NotaMod n
                                    INNER JOIN Modulos m ON n.Cod_Modulo = m.Cod_Modulo
                                    INNER JOIN Disciplina d ON m.Cod_Disc = d.Cod_Disc
                                    WHERE n.Cod_Aluno = @Aluno AND d.Tipo LIKE '%Técnica%'";
                int totalTecnicos;
                using (var cmd = new MySqlCommand(sqlTotal, conn))
                {
                    cmd.Parameters.AddWithValue("@Aluno", codAluno);
                    totalTecnicos = Convert.ToInt32(cmd.ExecuteScalar());
                }

                string sqlPositivos = @"SELECT COUNT(*) FROM NotaMod n
                                        INNER JOIN Modulos m ON n.Cod_Modulo = m.Cod_Modulo
                                        INNER JOIN Disciplina d ON m.Cod_Disc = d.Cod_Disc
                                        WHERE n.Cod_Aluno = @Aluno AND d.Tipo LIKE '%Técnica%'
                                        AND n.Valor REGEXP '^[0-9]+$' AND n.Valor + 0 >= 10";
                int concluidosTecnicos;
                using (var cmd = new MySqlCommand(sqlPositivos, conn))
                {
                    cmd.Parameters.AddWithValue("@Aluno", codAluno);
                    concluidosTecnicos = Convert.ToInt32(cmd.ExecuteScalar());
                }

                if (totalTecnicos > 0 && (double)concluidosTecnicos / totalTecnicos > 0.90)
                {
                    string sqlUpdate = "UPDATE aluno SET Estado_Estagio = 'Pronto' WHERE Cod_Aluno = @Aluno";
                    using (var cmd = new MySqlCommand(sqlUpdate, conn))
                    {
                        cmd.Parameters.AddWithValue("@Aluno", codAluno);
                        cmd.ExecuteNonQuery();
                    }
                    return true;
                }
            }
            return false;
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
