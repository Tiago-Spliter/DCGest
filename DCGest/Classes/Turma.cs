using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace DCGest.Classes
{
    public class Turma
    {
        public int Cod_Turma { get; set; }
        public string Nome { get; set; } = string.Empty;

        public Turma() { }

        public Turma(int codTurma, string nome)
        {
            Cod_Turma = codTurma;
            Nome = nome;
        }

        public static List<Turma> ObterTodas()
        {
            List<Turma> lista = new List<Turma>();
            using (MySqlConnection conn = new MySqlConnection(BD.CaminhoBD))
            {
                conn.Open();
                string sql = "SELECT Cod_Turma, Nome FROM turmas ORDER BY Nome";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                using (MySqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        lista.Add(new Turma(Convert.ToInt32(r["Cod_Turma"]), r["Nome"].ToString()));
                    }
                }
            }
            return lista;
        }
    }
}
