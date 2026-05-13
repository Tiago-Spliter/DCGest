using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        string caminho = "Server=localhost;Database=pap;User=root;Password=rootroot";

        int codAluno;

        DataTable tabelaNotas = new DataTable();


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
                tabelaNotas.Clear();
                tabelaNotas.Columns.Clear();
                tabelaNotas.Columns.Add("Cod_NotaMod");
                tabelaNotas.Columns.Add("Modulo");
                tabelaNotas.Columns.Add("Disciplina");
                tabelaNotas.Columns.Add("Valor");
                tabelaNotas.Columns.Add("Data_Efetua");

                using (MySqlConnection conexao = new MySqlConnection(caminho))
                {
                    conexao.Open();

                    string sql = "SELECT n.Cod_NotaMod, m.Designacao AS Modulo, d.Designacao AS Disciplina, n.Valor, n.Data_Efetua FROM NotaMod n INNER JOIN Modulos m ON n.Cod_Modulo = m.Cod_Modulo INNER JOIN Disciplina d ON m.Cod_Disc = d.Cod_Disc WHERE n.Ano = @Ano AND n.Cod_Aluno = @Aluno";

                    using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                    {
                        comando.Parameters.AddWithValue("@Ano", ano);
                        comando.Parameters.AddWithValue("@Aluno", codAluno);

                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read())
                            {
                                tabelaNotas.Rows.Add(
                                    leitor["Cod_NotaMod"],
                                    leitor["Modulo"],
                                    leitor["Disciplina"],
                                    leitor["Valor"] == DBNull.Value ? "" : leitor["Valor"],
                                    leitor["Data_Efetua"] == DBNull.Value ? "" : leitor["Data_Efetua"]
                                );
                            }
                        }
                    }
                }

                dg_alunos.ItemsSource = tabelaNotas.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar as notas: " + ex.Message);
            }
        }

        private void Btn_Click_Editar(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection conexao = new MySqlConnection(caminho))
            {
                conexao.Open();

                foreach (DataRow linha in tabelaNotas.Rows)
                {
                    if (linha["Cod_NotaMod"] != DBNull.Value)
                    {
                        string valorString = linha["Valor"].ToString();

                        if (valorString != string.Empty)
                        {
                            int valor = 0;
                            bool atualizarData = false;

                            try
                            {
                                valor = int.Parse(valorString);
                                atualizarData = true;
                            }
                            catch
                            {
                                MessageBox.Show($"Valor inválido no módulo {linha["Modulo"]}. Será considerado vazio.");
                                valor = 0;
                                atualizarData = true;
                            }

                            string sql_Notas = @"UPDATE NotaMod SET Valor = @Valor, Data_Efetua = @Data WHERE Cod_NotaMod = @Id";

                            using (MySqlCommand comando = new MySqlCommand(sql_Notas, conexao))
                            {
                                comando.Parameters.AddWithValue("@Valor", valorString == "" ? DBNull.Value : (object)valor);
                                comando.Parameters.AddWithValue("@Data", atualizarData ? DateTime.Now : DBNull.Value);
                                comando.Parameters.AddWithValue("@Id", linha["Cod_NotaMod"]);

                                comando.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }

            MessageBox.Show("Notas guardadas com sucesso!");
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