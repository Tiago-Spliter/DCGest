using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace DCGest.Classes
{
    public class Alinea
    {
        public int Cod_Alinea { get; set; }
        public string AlineaLetra { get; set; } = string.Empty;
        public string Regra { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;

        public Alinea() { }

        public Alinea(int codAlinea, string alineaLetra, string regra, string descricao)
        {
            Cod_Alinea = codAlinea;
            AlineaLetra = alineaLetra;
            Regra = regra;
            Descricao = descricao;
        }

        public static List<Alinea> ObterTodas()
        {
            var lista = new List<Alinea>();
            using (var conn = new MySqlConnection(BD.CaminhoBD))
            {
                conn.Open();
                string sql = "SELECT Cod_alinea, Alinea, Regra, Descricao FROM Alineas ORDER BY Cod_alinea";
                using (var cmd = new MySqlCommand(sql, conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        lista.Add(new Alinea
                        {
                            Cod_Alinea  = Convert.ToInt32(r["Cod_alinea"]),
                            AlineaLetra = r["Alinea"].ToString().Trim(),
                            Regra       = r["Regra"].ToString(),
                            Descricao   = r["Descricao"].ToString()
                        });
                    }
                }
            }
            return lista;
        }

        public static void GuardarTodas(IEnumerable<Alinea> lista)
        {
            using (var conn = new MySqlConnection(BD.CaminhoBD))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    string sql = "UPDATE Alineas SET Alinea = @A, Descricao = @D WHERE Cod_alinea = @Id";
                    foreach (var a in lista)
                    {
                        using (var cmd = new MySqlCommand(sql, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@A", a.AlineaLetra.Trim());
                            cmd.Parameters.AddWithValue("@D", a.Descricao.Trim());
                            cmd.Parameters.AddWithValue("@Id", a.Cod_Alinea);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
            }
        }
    }
}
