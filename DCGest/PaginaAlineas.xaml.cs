using DCGest.Classes;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace DCGest
{
    public partial class PaginaAlineas : UserControl
    {
        private string _caminho = BD.CaminhoBD;

        public PaginaAlineas()
        {
            InitializeComponent();
            CarregarAlineas();
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
