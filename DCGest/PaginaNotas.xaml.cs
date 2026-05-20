using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
        List<MediaDisciplina> listaMedias = new List<MediaDisciplina>();


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

                    string sql = @"SELECT n.Cod_NotaMod, m.Designacao AS Modulo, d.Designacao AS Disciplina, d.Tipo, n.Valor, n.Data_Efetua 
                                   FROM NotaMod n 
                                   INNER JOIN Modulos m ON n.Cod_Modulo = m.Cod_Modulo 
                                   INNER JOIN Disciplina d ON m.Cod_Disc = d.Cod_Disc 
                                   WHERE n.Ano = @Ano AND n.Cod_Aluno = @Aluno";

                    using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                    {
                        comando.Parameters.AddWithValue("@Ano", ano);
                        comando.Parameters.AddWithValue("@Aluno", codAluno);

                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read())
                            {
                                listaNotas.Add(new NotaModulo(
                                    Convert.ToInt32(leitor["Cod_NotaMod"]),
                                    codAluno,
                                    0,
                                    leitor["Valor"] as int?,
                                    leitor["Data_Efetua"] as DateTime?,
                                    ano,
                                    leitor["Modulo"].ToString(),
                                    leitor["Disciplina"].ToString(),
                                    leitor["Tipo"].ToString()
                                ));
                            }
                        }
                    }
                }

                dg_alunos.ItemsSource = null;
                dg_alunos.ItemsSource = listaNotas;

                CalcularResumos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar as notas: " + ex.Message);
            }
        }

        private void CalcularResumos()
        {
            try
            {
                listaMedias.Clear();

                // 1. Agrupar notas por Disciplina manualmente para a tabela da direita
                Dictionary<string, List<NotaModulo>> grupos = new Dictionary<string, List<NotaModulo>>();

                foreach (NotaModulo nota in listaNotas)
                {
                    if (grupos.ContainsKey(nota.NomeDisciplina) == false)
                    {
                        grupos.Add(nota.NomeDisciplina, new List<NotaModulo>());
                    }
                    grupos[nota.NomeDisciplina].Add(nota);
                }

                // 2. Calcular a média e situação de cada disciplina
                foreach (var par in grupos)
                {
                    string nome = par.Key;
                    List<NotaModulo> notasDaDisciplina = par.Value;

                    double somaNotas = 0;
                    int contadorModulosComNota = 0;
                    bool reprovouAlgumModulo = false;
                    string tipo = notasDaDisciplina[0].TipoDisciplina;

                    foreach (NotaModulo n in notasDaDisciplina)
                    {
                        // Se tem nota lançada (> 0)
                        if (n.Valor != null && n.Valor > 0)
                        {
                            somaNotas = somaNotas + (double)n.Valor;
                            contadorModulosComNota = contadorModulosComNota + 1;
                        }

                        // Verifica se falta alguma nota ou se é negativa para a situação
                        if (n.Valor == null || n.Valor < 10)
                        {
                            reprovouAlgumModulo = true;
                        }
                    }

                    double mediaCalculada = 0;
                    if (contadorModulosComNota > 0)
                    {
                        mediaCalculada = somaNotas / contadorModulosComNota;
                    }

                    string estado = "Em curso";
                    if (reprovouAlgumModulo == false)
                    {
                        estado = "Concluído";
                    }

                    listaMedias.Add(new MediaDisciplina(nome, tipo, mediaCalculada, contadorModulosComNota, estado));
                }

                dg_medias.ItemsSource = null;
                dg_medias.ItemsSource = listaMedias;

                // 3. Calcular Médias por Género para o painel inferior
                double somaGeral = 0, somaCien = 0, somaTec = 0;
                int contGeral = 0, contCien = 0, contTec = 0;

                foreach (NotaModulo n in listaNotas)
                {
                    if (n.Valor != null && n.Valor > 0)
                    {
                        double valorNota = (double)n.Valor;
                        
                        somaGeral = somaGeral + valorNota;
                        contGeral = contGeral + 1;

                        if (n.TipoDisciplina.ToLower().Contains("científica"))
                        {
                            somaCien = somaCien + valorNota;
                            contCien = contCien + 1;
                        }

                        if (n.TipoDisciplina.ToLower().Contains("técnica"))
                        {
                            somaTec = somaTec + valorNota;
                            contTec = contTec + 1;
                        }
                    }
                }

                // Mostrar resultados nas caixas de texto
                if (contGeral > 0) txt_mediaGeral.Text = (somaGeral / contGeral).ToString("N2");
                else txt_mediaGeral.Text = "---";

                if (contCien > 0) txt_mediaCientifica.Text = (somaCien / contCien).ToString("N2");
                else txt_mediaCientifica.Text = "---";

                if (contTec > 0) txt_mediaTecnica.Text = (somaTec / contTec).ToString("N2");
                else txt_mediaTecnica.Text = "---";

                // 4. Média Final do Aluno (Visual)
                double fct = 0, pap = 0;
                if (txt_notaFCT.Text != "") fct = Convert.ToDouble(txt_notaFCT.Text);
                if (txt_notaPAP.Text != "") pap = Convert.ToDouble(txt_notaPAP.Text);

                if (contGeral > 0)
                {
                    double mediaDasNotas = somaGeral / contGeral;
                    double divisor = 1;
                    if (fct > 0 && pap > 0) divisor = 3;

                    double mFinal = (mediaDasNotas + fct + pap) / divisor;
                    lbl_mediaFinal.Text = mFinal.ToString("N1");
                }
            }
            catch (Exception ex)
            {
                // Erro apenas para debug se necessário
                System.Diagnostics.Debug.WriteLine("Erro cálculo: " + ex.Message);
            }
        }

        private void Btn_Click_Editar(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Deseja guardar todas as alterações feitas nas notas deste aluno?", "Confirmar Alterações", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No) return;

            try
            {
                using (MySqlConnection conexao = new MySqlConnection(caminho))
                {
                    conexao.Open();

                    using (MySqlTransaction transacao = conexao.BeginTransaction())
                    {
                        try
                        {
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

                                using (MySqlCommand comando = new MySqlCommand(sql, conexao, transacao))
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

                            transacao.Commit();
                            dg_alunos.Items.Refresh();
                            CalcularResumos(); // Recalcula após guardar
                            MessageBox.Show("Todas as notas foram guardadas com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            transacao.Rollback();
                            MessageBox.Show("Erro ao guardar notas: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro de conexão ao guardar notas: " + ex.Message);
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
