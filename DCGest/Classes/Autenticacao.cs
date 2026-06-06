using BCrypt.Net;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace DCGest.Classes
{
    public class Autenticacao
    {
        public int Cod_Aut { get; set; }
        public string Utilizador { get; set; } = string.Empty;
        public string PalavraPasse { get; set; } = string.Empty;


        public Autenticacao()
        {

        }

        public Autenticacao(int codAut, string utilizador, string palavraPasse)
        {
            Cod_Aut = codAut;
            Utilizador = utilizador;
            PalavraPasse = palavraPasse;
        }

        public static DiretorCurso Verificar(string username, string password, out string mensagemErro)
        {
            mensagemErro = null;
            using (var conn = new MySqlConnection(BD.CaminhoBD))
            {
                conn.Open();
                string sql = "SELECT * FROM autenticacao WHERE Utilizador = @user";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@user", username);
                    using (var rAut = cmd.ExecuteReader())
                    {
                        if (rAut.Read())
                        {
                            string hashNaBD = rAut["PalavraPasse"].ToString();
                            int codAut = Convert.ToInt32(rAut["Cod_Aut"]);

                            if (BCrypt.Net.BCrypt.Verify(password, hashNaBD))
                            {
                                rAut.Close();
                                string sqlDC = "SELECT * FROM diretor_curso WHERE Cod_Aut = @codAut";
                                using (var cmdDC = new MySqlCommand(sqlDC, conn))
                                {
                                    cmdDC.Parameters.AddWithValue("@codAut", codAut);
                                    using (var rDC = cmdDC.ExecuteReader())
                                    {
                                        if (rDC.Read())
                                        {
                                            return new DiretorCurso(
                                                Convert.ToInt32(rDC["Cod_DC"]),
                                                rDC["Nome_DC"].ToString(),
                                                Convert.ToInt32(rDC["Cod_Curso"]),
                                                codAut
                                            );
                                        }
                                        else
                                        {
                                            mensagemErro = "Atenção: Utilizador autenticado mas sem perfil de Diretor associado!";
                                            return null;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            mensagemErro = "Credenciais inválidas!";
            return null;
        }

        public static void AtualizarPalavraPasse(int codAut, string novaPasse)
        {
            using (var conn = new MySqlConnection(BD.CaminhoBD))
            {
                conn.Open();
                string hash = BCrypt.Net.BCrypt.HashPassword(novaPasse);
                string sql = "UPDATE autenticacao SET PalavraPasse = @hash WHERE Cod_Aut = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@hash", hash);
                    cmd.Parameters.AddWithValue("@id", codAut);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
