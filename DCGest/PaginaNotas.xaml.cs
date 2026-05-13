using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DCGest.Models;

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

                                // Usar IsDBNull para evitar comparação com DBNull.Value
                                if (!leitor.IsDBNull(leitor.GetOrdinal("Valor")))
                                {
                                    n.Valor = Convert.ToInt32(leitor["Valor"]);
                                }
                                else
                                {
                                    n.Valor = null;
                                }

                                if (!leitor.IsDBNull(leitor.GetOrdinal("Data_Efetua")))
                                {
                                    n.Data_Efetua = Convert.ToDateTime(leitor["Data_Efetua"]);
                                }
                                else
                                {
                                    n.Data_Efetua = null;
                                }


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

                    using (MySqlTransaction transacao = conexao.BeginTransaction())
                    {
                        string sql = @"UPDATE NotaMod SET Valor = @Valor, Data_Efetua = @Data WHERE Cod_NotaMod = @Id";

                        using (MySqlCommand comando = new MySqlCommand(sql, conexao, transacao))
                        {
                            // Adicionar os parâmetros uma única vez
                            comando.Parameters.Add("@Valor", MySqlDbType.Int32);
                            comando.Parameters.Add("@Data", MySqlDbType.DateTime);
                            comando.Parameters.Add("@Id", MySqlDbType.Int32);

                            DateTime dataAtual = DateTime.Now;

                            foreach (NotaModulo nota in listaNotas)
                            {
                                // Se a nota tiver valor, atualizamos a data para agora.
                                // Se for null, mantemos ou limpamos a data conforme a regra de negócio.
                                // Aqui, vamos atualizar a data apenas se houver uma nota.
                                if (nota.Valor != null)
                                {
                                    nota.Data_Efetua = dataAtual;
                                }
                                else
                                {
                                    nota.Data_Efetua = null;
                                }

                                // Atualizar os valores dos parâmetros para cada nota
                                comando.Parameters["@Valor"].Value = nota.Valor.ParaDB();
                                comando.Parameters["@Data"].Value = nota.Data_Efetua.ParaDB();
                                comando.Parameters["@Id"].Value = nota.Cod_NotaMod;

                                comando.ExecuteNonQuery();
                            }
                        }

                        transacao.Commit();
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