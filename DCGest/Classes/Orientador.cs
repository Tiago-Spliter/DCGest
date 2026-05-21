using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace DCGest.Classes
{
    public class Orientador : Entidade
    {
        public int Cod_Orientador { get; set; }
        public string Nome_Orientador { get; set; } = string.Empty;

        public Orientador() { }

        public Orientador(int codOrientador, string nomeOrientador)
        {
            Cod_Orientador = codOrientador;
            Nome_Orientador = nomeOrientador;
        }

        public override void InserirNaBD(string connectionString)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "INSERT INTO orientador (Nome_Orientador) VALUES (@Nome)";
                using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                {
                    comando.Parameters.AddWithValue("@Nome", Nome_Orientador.Trim());
                    comando.ExecuteNonQuery();
                }
            }
        }

        public static List<Orientador> ObterTodos()
        {
            List<Orientador> lista = new List<Orientador>();
            using (MySqlConnection conn = new MySqlConnection(BD.CaminhoBD))
            {
                conn.Open();
                string sql = "SELECT Cod_Orientador, Nome_Orientador FROM orientador ORDER BY Nome_Orientador";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                using (MySqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        lista.Add(new Orientador(Convert.ToInt32(r["Cod_Orientador"]), r["Nome_Orientador"].ToString()));
                    }
                }
            }
            return lista;
        }
    }
}
