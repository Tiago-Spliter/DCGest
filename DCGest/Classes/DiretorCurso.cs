using BCrypt.Net;
using MySql.Data.MySqlClient;
using System;

namespace DCGest.Classes
{
    public class DiretorCurso
    {
        public int Cod_DC { get; set; }
        public string Nome_DC { get; set; } = string.Empty;
        public int Cod_Curso { get; set; }
        public int Cod_Aut { get; set; }


        public DiretorCurso()
        {

        }

        public DiretorCurso(int codDC, string nomeDC, int codCurso, int codAut)
        {
            Cod_DC = codDC;
            Nome_DC = nomeDC;
            Cod_Curso = codCurso;
            Cod_Aut = codAut;
        }

        public void InserirNaBD(string username, string password, string connectionString)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        string passHash = BCrypt.Net.BCrypt.HashPassword(password);
                        string sqlAut = "INSERT INTO autenticacao (Utilizador, PalavraPasse) VALUES (@user, @pass); SELECT LAST_INSERT_ID();";
                        int codAut;
                        using (var cmdAut = new MySqlCommand(sqlAut, conn, trans))
                        {
                            cmdAut.Parameters.AddWithValue("@user", username);
                            cmdAut.Parameters.AddWithValue("@pass", passHash);
                            codAut = Convert.ToInt32(cmdAut.ExecuteScalar());
                        }

                        string sqlDC = "INSERT INTO diretor_curso (Nome_DC, Cod_Curso, Cod_Aut) VALUES (@nome, @curso, @aut)";
                        using (var cmdDC = new MySqlCommand(sqlDC, conn, trans))
                        {
                            cmdDC.Parameters.AddWithValue("@nome", Nome_DC);
                            cmdDC.Parameters.AddWithValue("@curso", Cod_Curso);
                            cmdDC.Parameters.AddWithValue("@aut", codAut);
                            cmdDC.ExecuteNonQuery();
                        }

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
