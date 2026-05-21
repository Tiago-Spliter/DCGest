using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace DCGest.Classes
{
    public class AnoLetivo
    {
        public int Cod_Letivo { get; set; }
        public string Intervalo { get; set; } = string.Empty;

        public AnoLetivo() { }

        public AnoLetivo(int codLetivo, string intervalo)
        {
            Cod_Letivo = codLetivo;
            Intervalo = intervalo;
        }

        public static List<AnoLetivo> ObterTodos()
        {
            List<AnoLetivo> lista = new List<AnoLetivo>();
            using (MySqlConnection conn = new MySqlConnection(BD.CaminhoBD))
            {
                conn.Open();
                string sql = "SELECT Cod_Letivo, Intervalo FROM anosletivos ORDER BY Intervalo";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                using (MySqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        lista.Add(new AnoLetivo(Convert.ToInt32(r["Cod_Letivo"]), r["Intervalo"].ToString()));
                    }
                }
            }
            return lista;
        }
    }
}
