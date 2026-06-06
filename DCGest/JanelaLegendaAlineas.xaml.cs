using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using DCGest.Classes;

namespace DCGest
{
    public partial class JanelaLegendaAlineas : Window
    {
        public JanelaLegendaAlineas()
        {
            InitializeComponent();
            CarregarAlineas();
        }

        private void CarregarAlineas()
        {
            try
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

                dg_alineas.ItemsSource = lista;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar alíneas: " + ex.Message);
            }
        }

        private void Btn_Fechar_Click(object sender, RoutedEventArgs e) => Close();
    }
}
