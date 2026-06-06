using DCGest.Classes;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace DCGest
{
    public partial class PaginaAlineas : UserControl
    {
        private readonly string _caminho = BD.CaminhoBD;

        private static readonly (int Id, string Regra, string Descricao)[] RegrasSeed =
        {
            (1,  "Ausente",       "Não estiveste presente na avaliação deste módulo."),
            (2,  "Falta",         "A tua classificação foi condicionada por excesso de faltas de presença."),
            (3,  "Dispensado",    "Foste dispensado da realização da avaliação deste módulo."),
            (4,  "Isento",        "Estás isento da avaliação deste módulo, podendo resultar de competências reconhecidas."),
            (5,  "Não Avaliado",  "Ainda não realizaste a avaliação deste módulo."),
            (6,  "Recuperação",   "Tens oportunidade de realizar uma prova de recuperação para este módulo."),
            (7,  "Prova Especial","Realizaste ou vais realizar uma prova especial para concluir este módulo."),
            (8,  "Equivalência",  "A classificação deste módulo foi atribuída por equivalência a formação anterior."),
            (9,  "Transferido",   "O teu historial neste módulo foi transferido de outro estabelecimento de ensino."),
            (10, "Concluido",     "Concluíste com sucesso este módulo."),
            (11, "Anulado",       "A avaliação deste módulo foi anulada por decisão administrativa.")
        };

        public PaginaAlineas()
        {
            InitializeComponent();
            SeedAlineas();
            CarregarAlineas();
        }

        private void SeedAlineas()
        {
            try
            {
                using (var conn = new MySqlConnection(_caminho))
                {
                    conn.Open();
                    foreach (var (id, regra, descricao) in RegrasSeed)
                    {
                        string sql = @"INSERT INTO Alineas (Cod_alinea, Alinea, Regra, Descricao)
                                       VALUES (@Id, '', @R, @D)
                                       ON DUPLICATE KEY UPDATE Regra = @R";
                        using (var cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            cmd.Parameters.AddWithValue("@R", regra);
                            cmd.Parameters.AddWithValue("@D", descricao);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao inicializar tabela de alíneas: " + ex.Message);
            }
        }

        private void CarregarAlineas()
        {
            var lista = new ObservableCollection<Alinea>();
            try
            {
                using (var conn = new MySqlConnection(_caminho))
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar alíneas: " + ex.Message);
            }

            dg_alineas.ItemsSource = lista;
        }

        private void Btn_Guardar_Click(object sender, RoutedEventArgs e)
        {
            var lista = (ObservableCollection<Alinea>)dg_alineas.ItemsSource;

            try
            {
                using (var conn = new MySqlConnection(_caminho))
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

                MessageBox.Show("Alíneas guardadas com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao guardar alíneas: " + ex.Message);
            }
        }
    }
}
