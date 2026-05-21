using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace DCGest.Classes
{
    public class Curso
    {
        public int Cod_Curso { get; set; }
        public string Nome_Curso { get; set; } = string.Empty;

        public Curso() { }

        public Curso(int codCurso, string nomeCurso)
        {
            Cod_Curso = codCurso;
            Nome_Curso = nomeCurso;
        }

        public static List<Curso> ObterTodos()
        {
            List<Curso> lista = new List<Curso>();
            using (MySqlConnection conn = new MySqlConnection(BD.CaminhoBD))
            {
                conn.Open();
                string sql = "SELECT Cod_Curso, Nome_Curso FROM cursos ORDER BY Nome_Curso";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                using (MySqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        lista.Add(new Curso(Convert.ToInt32(r["Cod_Curso"]), r["Nome_Curso"].ToString()));
                    }
                }
            }
            return lista;
        }
    }
}
