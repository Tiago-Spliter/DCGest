using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace DCGest.Classes
{
    public class NotaModulo
    {
        public const string LetraChumbo = "RC";
        public int Cod_NotaMod { get; set; }
        public int Cod_Aluno { get; set; }
        public int Cod_Modulo { get; set; }
        public string Valor { get; set; }
        public DateTime? Data_Efetua { get; set; }
        public string Ano { get; set; } = string.Empty;

        public int? Cod_Estado { get; set; }
        public string NomeEstado { get; set; } = string.Empty;

        public string NomeModulo { get; set; } = string.Empty;
        public string NomeDisciplina { get; set; } = string.Empty;
        public string TipoDisciplina { get; set; } = string.Empty;

        public double? ValorNumerico
        {
            get
            {
                if (double.TryParse(Valor, out double v))
                {
                    return v;
                }

                return null;
            }
        }

        public NotaModulo() { }

        public NotaModulo(int codNotaMod, int codAluno, int codModulo, string valor, DateTime? dataEfetua, string ano)
        {
            Cod_NotaMod = codNotaMod;
            Cod_Aluno = codAluno;
            Cod_Modulo = codModulo;
            Valor = valor;
            Data_Efetua = dataEfetua;
            Ano = ano;
        }

        public NotaModulo(int codNotaMod, int codAluno, int codModulo, string valor, DateTime? dataEfetua, string ano, string nomeModulo, string nomeDisciplina, string tipoDisciplina)
        {
            Cod_NotaMod = codNotaMod;
            Cod_Aluno = codAluno;
            Cod_Modulo = codModulo;
            Valor = valor;
            Data_Efetua = dataEfetua;
            Ano = ano;
            NomeModulo = nomeModulo;
            NomeDisciplina = nomeDisciplina;
            TipoDisciplina = tipoDisciplina;
        }

        public static string ValidarENormalizar(string input, out string erro)
        {
            erro = null;

            if (string.IsNullOrWhiteSpace(input))
                return null;

            string trimmed = input.Trim().ToUpper();

            if (trimmed == LetraChumbo)
                return LetraChumbo;

            string normalizado = trimmed.Replace(",", ".");

            if (!double.TryParse(normalizado, out double valor))
            {
                erro = "'" + input + "' não é uma nota válida. Introduza um valor entre 0 e 20 ou '" + LetraChumbo + "'.";
                return null;
            }

            if (valor < 0) { erro = "A nota não pode ser negativa."; return null; }
            if (valor > 20) { erro = "A nota não pode ser superior a 20."; return null; }
            if (valor < 9.5) return LetraChumbo;

            return ((int)Math.Round(valor)).ToString();
        }

        public static List<NotaModulo> ObterPorAluno(int codAluno)
        {
            var lista = new List<NotaModulo>();
            using (var conn = new MySqlConnection(BD.CaminhoBD))
            {
                conn.Open();
                string sql = @"SELECT n.Cod_NotaMod, d.Ano AS AnoDisciplina, m.Designacao AS Modulo,
                                      d.Designacao AS Disciplina, d.Tipo, n.Valor, n.Data_Efetua,
                                      n.Cod_Estado, a.Alinea AS AlineaLetra, a.Regra
                               FROM NotaMod n
                               INNER JOIN Modulos m ON n.Cod_Modulo = m.Cod_Modulo
                               INNER JOIN Disciplina d ON m.Cod_Disc = d.Cod_Disc
                               LEFT JOIN Alineas a ON n.Cod_Estado = a.Cod_alinea
                               WHERE n.Cod_Aluno = @Aluno
                               ORDER BY d.Tipo, d.Designacao, m.Cod_Modulo";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Aluno", codAluno);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            string valorDaNota = null;
                            if (r["Valor"] != DBNull.Value)
                            {
                                valorDaNota = r["Valor"].ToString().Trim();
                                if (valorDaNota == string.Empty) valorDaNota = null;
                            }

                            DateTime? dataEfetua = r["Data_Efetua"] != DBNull.Value
                                ? (DateTime?)Convert.ToDateTime(r["Data_Efetua"]) : null;

                            int? codEstado = null;
                            string nomeEstado = string.Empty;
                            if (r["Cod_Estado"] != DBNull.Value)
                            {
                                codEstado = Convert.ToInt32(r["Cod_Estado"]);
                                string al = r["AlineaLetra"] != DBNull.Value ? r["AlineaLetra"].ToString().Trim() : string.Empty;
                                string regra = r["Regra"] != DBNull.Value ? r["Regra"].ToString() : string.Empty;
                                nomeEstado = string.IsNullOrEmpty(al) ? regra : al + " – " + regra;
                            }

                            var n = new NotaModulo(
                                Convert.ToInt32(r["Cod_NotaMod"]),
                                codAluno,
                                0,
                                valorDaNota,
                                dataEfetua,
                                r["AnoDisciplina"].ToString() + "º Ano",
                                r["Modulo"].ToString(),
                                r["Disciplina"].ToString(),
                                r["Tipo"].ToString()
                            );
                            n.Cod_Estado = codEstado;
                            n.NomeEstado = nomeEstado;
                            lista.Add(n);
                        }
                    }
                }
            }
            return lista;
        }
    }
}
