using MySql.Data.MySqlClient;
using System;

namespace DCGest.Classes
{
    public class Orientador : Entidade
    {
        public int Cod_Orientador { get; set; }
        public string Nome_Orientador { get; set; } = string.Empty;


        public Orientador() 
        {
        
        }

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
    }
}
