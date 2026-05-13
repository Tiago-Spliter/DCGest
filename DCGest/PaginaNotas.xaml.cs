using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DCGest.Classes;

namespace DCGest
{
    public partial class PaginaNotas : Window
    {

        public PaginaNotas(int cod)
        {
            InitializeComponent();

            codAluno = cod;

            try
            {
                CarregarNomeAluno();
                CarregarNotas("1º Ano");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao inicializar a página: " + ex.Message);
                this.Close();
            }
        }

        string caminho = BD.CaminhoBD;

        int codAluno;

        List<NotaModulo> listaNotas = new List<NotaModulo>();


        private void CarregarNomeAluno()
        {
            try
            {
                using (MySqlConnection conexao = new MySqlConnection(caminho))
                {
                    conexao.Open();

                    string sql_Nome = "SELECT Nome_Aluno FROM Aluno WHERE Cod_Aluno = @Cod";

                    using (MySqlCommand comando = new MySqlCommand(sql_Nome, conexao))
                    {
                        comando.Parameters.AddWithValue("@Cod", codAluno);

                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            if (leitor.Read())
                            {
                                txtNomeAluno.Text = leitor["Nome_Aluno"].ToString();
                            }
                            else
                            {
                                MessageBox.Show("Aluno não encontrado!");
                                this.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar o nome do aluno: " + ex.Message);
            }
        }

        private void CarregarNotas(string ano)
        {
            try
            {
                listaNotas.Clear();

                using (MySqlConnection conexao = new MySqlConnection(caminho))
                {
                    conexao.Open();

                    string sql = @"SELECT n.Cod_NotaMod, m.Designacao AS Modulo, d.Designacao AS Disciplina, n.Valor, n.Data_Efetua 
                                   FROM NotaMod n INNER JOIN Modulos m ON n.Cod_Modulo = m.Cod_Modulo INNER JOIN Disciplina d ON m.Cod_Disc = d.Cod_Disc WHERE n.Ano = @Ano AND n.Cod_Aluno = @Aluno";

                    using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                    {
                        comando.Parameters.AddWithValue("@Ano", ano);
                        comando.Parameters.AddWithValue("@Aluno", codAluno);

                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read())
                            {
                                NotaModulo n = new NotaModulo();
                                
                                n.Cod_NotaMod = Convert.ToInt32(leitor["Cod_NotaMod"]);
                                n.NomeModulo = leitor["Modulo"].ToString();
                                n.NomeDisciplina = leitor["Disciplina"].ToString();


                                n.Valor = (int?)leitor["Valor"];
                                n.Data_Efetua = (DateTime?)leitor["Data_Efetua"];

                                listaNotas.Add(n);
                            }
                        }
                    }
                }

                dg_alunos.ItemsSource = null;
                dg_alunos.ItemsSource = listaNotas;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar as notas: " + ex.Message);
            }
        }

        private void Btn_Click_Editar(object sender, RoutedEventArgs e)
        {
            try
            {
                using (MySqlConnection conexao = new MySqlConnection(caminho))
                {
                    conexao.Open();

                    foreach (NotaModulo nota in listaNotas)
                    {
                        if (nota.Valor != null)
                        {
                            nota.Data_Efetua = DateTime.Now;
                        }
                        else
                        {
                            nota.Data_Efetua = null;
                        }

                        string sql = "UPDATE NotaMod SET Valor = @Valor, Data_Efetua = @Data WHERE Cod_NotaMod = @Id";

                        using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                        {

                            if (nota.Valor == null)
                            {
                                comando.Parameters.AddWithValue("@Valor", DBNull.Value);
                            }
                            else
                            {
                                comando.Parameters.AddWithValue("@Valor", nota.Valor);
                            }

                            if (nota.Data_Efetua == null)
                            {
                                comando.Parameters.AddWithValue("@Data", DBNull.Value);
                            }
                            else
                            {
                                comando.Parameters.AddWithValue("@Data", nota.Data_Efetua);
                            }

                            comando.Parameters.AddWithValue("@Id", nota.Cod_NotaMod);

                            comando.ExecuteNonQuery();
                        }
                    }
                }

                dg_alunos.Items.Refresh();
                MessageBox.Show("Notas guardadas com sucesso!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao guardar notas: " + ex.Message);
            }
        }

        private void Btn_Click_Voltar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Btn_Click_Ano1(object sender, RoutedEventArgs e)
        {
            AtualizarBotoes(btn_ano1);
            CarregarNotas("1º Ano");
        }

        private void Btn_Click_Ano2(object sender, RoutedEventArgs e)
        {
            AtualizarBotoes(btn_ano2);
            CarregarNotas("2º Ano");
        }

        private void Btn_Click_Ano3(object sender, RoutedEventArgs e)
        {
            AtualizarBotoes(btn_ano3);
            CarregarNotas("3º Ano");
        }

        private void AtualizarBotoes(Button botaoSelecionado)
        {
            var CorAtivo = (Brush)new BrushConverter().ConvertFrom("#293472");
            var CorInativo = (Brush)new BrushConverter().ConvertFrom("#EFE6D8");

            var TextoAtivo = Brushes.White;
            var TextoInativo = (Brush)new BrushConverter().ConvertFrom("#293472");

            btn_ano1.Background = CorInativo;
            btn_ano1.Foreground = TextoInativo;

            btn_ano2.Background = CorInativo;
            btn_ano2.Foreground = TextoInativo;

            btn_ano3.Background = CorInativo;
            btn_ano3.Foreground = TextoInativo;

            botaoSelecionado.Background = CorAtivo;
            botaoSelecionado.Foreground = TextoAtivo;
        }
    }
}