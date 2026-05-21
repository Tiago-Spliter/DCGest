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

        // Objetos para guardar as notas especiais (FCT/PAP)
        NotaModulo? notaFCT = null;
        NotaModulo? notaPAP = null;


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
                // Limpar campos visuais das notas finais antes de carregar
                txt_notaFCT.Text = "0";
                txt_notaPAP.Text = "0";
                notaFCT = null;
                notaPAP = null;

                using (MySqlConnection conexao = new MySqlConnection(caminho))
                {
                    conexao.Open();

                    // Query que busca tudo do aluno, incluindo as de tipo 'final'
                    string sql = @"SELECT n.Cod_NotaMod, n.Ano, m.Designacao AS Modulo, d.Designacao AS Disciplina, d.Tipo, n.Valor, n.Data_Efetua 
                                   FROM NotaMod n 
                                   INNER JOIN Modulos m ON n.Cod_Modulo = m.Cod_Modulo 
                                   INNER JOIN Disciplina d ON m.Cod_Disc = d.Cod_Disc 
                                   WHERE n.Cod_Aluno = @Aluno
                                   ORDER BY d.Tipo, d.Designacao, m.Cod_Modulo";

                    using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                    {
                        comando.Parameters.AddWithValue("@Aluno", codAluno);

                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read())
                            {
                                string tipo = leitor["Tipo"].ToString();
                                string discNome = leitor["Disciplina"].ToString();
                                string anoNota = leitor["Ano"].ToString();

                                NotaModulo n = new NotaModulo(
                                    Convert.ToInt32(leitor["Cod_NotaMod"]),
                                    codAluno,
                                    0,
                                    leitor["Valor"] == DBNull.Value ? null : (int?)Convert.ToInt32(leitor["Valor"]),
                                    leitor["Data_Efetua"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(leitor["Data_Efetua"]),
                                    anoNota,
                                    leitor["Modulo"].ToString(),
                                    discNome,
                                    tipo
                                );

                                // Se for do tipo 'final', guardamos nos objetos especiais e não na lista da grid
                                if (tipo.ToLower() == "final")
                                {
                                    if (discNome.ToUpper().Contains("FCT"))
                                    {
                                        notaFCT = n;
                                        txt_notaFCT.Text = n.Valor?.ToString() ?? "0";
                                    }
                                    else if (discNome.ToUpper().Contains("PAP"))
                                    {
                                        notaPAP = n;
                                        txt_notaPAP.Text = n.Valor?.ToString() ?? "0";
                                    }
                                }
                                // Se for uma disciplina normal E do ano selecionado, vai para a grid
                                else if (anoNota == ano)
                                {
                                    listaNotas.Add(n);
                                }
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

        private int GetPrioridadeTipo(string tipo)
        {
            if (tipo.ToLower().Contains("sociocultural")) return 1;
            if (tipo.ToLower().Contains("científica")) return 2;
            if (tipo.ToLower().Contains("técnica")) return 3;
            return 4;
        }

        private void CalcularResumos()
        {
            try
            {
                listaMedias.Clear();

                // 1. Agrupar notas por Disciplina
                Dictionary<string, List<NotaModulo>> grupos = new Dictionary<string, List<NotaModulo>>();

                foreach (NotaModulo nota in listaNotas)
                {
                    if (grupos.ContainsKey(nota.NomeDisciplina) == false)
                    {
                        grupos.Add(nota.NomeDisciplina, new List<NotaModulo>());
                    }
                    grupos[nota.NomeDisciplina].Add(nota);
                }

                // 2. Ordenar grupos pela prioridade do tipo
                var gruposOrdenados = grupos.OrderBy(g => GetPrioridadeTipo(g.Value[0].TipoDisciplina)).ThenBy(g => g.Key);

                // 3. Calcular a média e situação de cada disciplina
                foreach (var par in gruposOrdenados)
                {
                    string nome = par.Key;
                    List<NotaModulo> notasDaDisciplina = par.Value;

                    double somaNotas = 0;
                    int contadorModulosComNota = 0;
                    bool reprovouAlgumModulo = false;
                    string tipo = notasDaDisciplina[0].TipoDisciplina;

                    foreach (NotaModulo n in notasDaDisciplina)
                    {
                        if (n.Valor != null && n.Valor >= 0)
                        {
                            somaNotas = somaNotas + (double)n.Valor;
                            contadorModulosComNota = contadorModulosComNota + 1;
                        }

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

                    string estado = reprovouAlgumModulo ? "SC" : "C";

                    listaMedias.Add(new MediaDisciplina(nome, tipo, mediaCalculada, contadorModulosComNota, estado));
                }

                dg_medias.ItemsSource = null;
                dg_medias.ItemsSource = listaMedias;

                // 4. Calcular Médias por Género para o painel inferior
                double somaGeral = 0, somaCien = 0, somaTec = 0;
                int contGeral = 0, contCien = 0, contTec = 0;

                foreach (NotaModulo n in listaNotas)
                {
                    if (n.Valor != null && n.Valor >= 0)
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

                // Mostrar resultados
                txt_mediaGeral.Text = contGeral > 0 ? (somaGeral / contGeral).ToString("N2") : "0,00";
                txt_mediaCientifica.Text = contCien > 0 ? (somaCien / contCien).ToString("N2") : "0,00";
                txt_mediaTecnica.Text = contTec > 0 ? (somaTec / contTec).ToString("N2") : "0,00";

                // 5. Média Final do Aluno
                double fct = 0;
                if (txt_notaFCT.Text != "") fct = Convert.ToDouble(txt_notaFCT.Text);

                double pap = 0;
                if (txt_notaPAP.Text != "") pap = Convert.ToDouble(txt_notaPAP.Text);

                double mediaDasNotas = contGeral > 0 ? (somaGeral / contGeral) : 0;
                double divisor = 3; // Média + FCT + PAP

                double mFinal = (mediaDasNotas + fct + pap) / divisor;
                lbl_mediaFinal.Text = mFinal.ToString("N1");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro cálculo: " + ex.Message);
            }
        }

        private void Btn_Click_Editar(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Deseja guardar todas as alterações feitas nas notas deste aluno (incluindo FCT/PAP)?", "Confirmar Alterações", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No) return;

            try
            {
                // Lista temporária para agrupar tudo o que deve ser salvo
                List<NotaModulo> notasParaSalvar = new List<NotaModulo>(listaNotas);
                
                // Se temos FCT/PAP, atualizar os valores a partir das caixas de texto e adicionar à lista
                if (notaFCT != null)
                {
                    notaFCT.Valor = string.IsNullOrEmpty(txt_notaFCT.Text) ? null : (int?)Convert.ToInt32(txt_notaFCT.Text);
                    notasParaSalvar.Add(notaFCT);
                }
                if (notaPAP != null)
                {
                    notaPAP.Valor = string.IsNullOrEmpty(txt_notaPAP.Text) ? null : (int?)Convert.ToInt32(txt_notaPAP.Text);
                    notasParaSalvar.Add(notaPAP);
                }

                using (MySqlConnection conexao = new MySqlConnection(caminho))
                {
                    conexao.Open();

                    using (MySqlTransaction transacao = conexao.BeginTransaction())
                    {
                        try
                        {
                            foreach (NotaModulo nota in notasParaSalvar)
                            {
                                // Atualizar data apenas se a nota não for nula
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
                                    comando.Parameters.AddWithValue("@Valor", (object?)nota.Valor ?? DBNull.Value);
                                    comando.Parameters.AddWithValue("@Data", (object?)nota.Data_Efetua ?? DBNull.Value);
                                    comando.Parameters.AddWithValue("@Id", nota.Cod_NotaMod);

                                    comando.ExecuteNonQuery();
                                }
                            }

                            transacao.Commit();
                            dg_alunos.Items.Refresh();
                            CalcularResumos();
                            MessageBox.Show("Todas as notas (Módulos e FCT/PAP) foram guardadas com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
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
