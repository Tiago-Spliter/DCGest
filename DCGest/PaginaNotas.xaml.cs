using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DCGest.Classes;

namespace DCGest
{
    public partial class PaginaNotas : Window
    {
        private const double LimiteProgressaoAno = 0.75;

        private const string LetraChumbo = "RC";

        public PaginaNotas(int cod)
        {
            InitializeComponent();
            DataContext = this;
            codAluno = cod;
            try
            {
                CarregarAlineas();
                CarregarNomeAluno();
                CarregarNotas("1º Ano");
                VerificarProgressaoAnos();
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
        NotaModulo notaFCT = null;
        NotaModulo notaPAP = null;

        Dictionary<int, string> _valoresOriginais = new Dictionary<int, string>();

        public List<Alinea> ListaAlineas { get; private set; } = new List<Alinea>();


        private void CarregarAlineas()
        {
            ListaAlineas.Clear();
            ListaAlineas.Add(new Alinea { Cod_Alinea = 0, AlineaLetra = string.Empty, Regra = "(sem estado)" });

            using (MySqlConnection conn = new MySqlConnection(caminho))
            {
                conn.Open();
                string sql = "SELECT Cod_alinea, Alinea, Regra, Descricao FROM Alineas ORDER BY Cod_alinea";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                using (MySqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        ListaAlineas.Add(new Alinea
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

        private void CarregarNomeAluno()
        {
            try
            {
                Aluno a = Aluno.ObterPorId(codAluno);
                if (a != null)
                {
                    txtNomeAluno.Text = a.Nome_Aluno;
                }
                else
                {
                    MessageBox.Show("Aluno não encontrado!");
                    this.Close();
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
                txt_notaFCT.Text = "0";
                txt_notaPAP.Text = "0";
                notaFCT = null;
                notaPAP = null;

                List<NotaModulo> todasAsNotas = new List<NotaModulo>();
                _valoresOriginais.Clear();

                using (MySqlConnection conexao = new MySqlConnection(caminho))
                {
                    conexao.Open();

                    string sql = @"SELECT n.Cod_NotaMod, d.Ano AS AnoDisciplina, m.Designacao AS Modulo,
                                          d.Designacao AS Disciplina, d.Tipo, n.Valor, n.Data_Efetua,
                                          n.Cod_Estado, a.Alinea AS AlineaLetra, a.Regra
                                   FROM NotaMod n
                                   INNER JOIN Modulos m ON n.Cod_Modulo = m.Cod_Modulo
                                   INNER JOIN Disciplina d ON m.Cod_Disc = d.Cod_Disc
                                   LEFT JOIN Alineas a ON n.Cod_Estado = a.Cod_alinea
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
                                string anoNota = leitor["AnoDisciplina"].ToString() + "º Ano";

                                string valorDaNota = null;
                                if (leitor["Valor"] != DBNull.Value)
                                {
                                    valorDaNota = leitor["Valor"].ToString().Trim();
                                    if (valorDaNota == string.Empty) valorDaNota = null;
                                }

                                DateTime? dataEfetua = null;
                                if (leitor["Data_Efetua"] != DBNull.Value)
                                {
                                    dataEfetua = Convert.ToDateTime(leitor["Data_Efetua"]);
                                }

                                int? codEstado = null;
                                string nomeEstado = string.Empty;
                                if (leitor["Cod_Estado"] != DBNull.Value)
                                {
                                    codEstado = Convert.ToInt32(leitor["Cod_Estado"]);
                                    string al = leitor["AlineaLetra"] != DBNull.Value ? leitor["AlineaLetra"].ToString().Trim() : string.Empty;
                                    string regra = leitor["Regra"] != DBNull.Value ? leitor["Regra"].ToString() : string.Empty;
                                    nomeEstado = string.IsNullOrEmpty(al) ? regra : $"{al} – {regra}";
                                }

                                NotaModulo n = new NotaModulo(
                                    Convert.ToInt32(leitor["Cod_NotaMod"]),
                                    codAluno,
                                    0,
                                    valorDaNota,
                                    dataEfetua,
                                    anoNota,
                                    leitor["Modulo"].ToString(),
                                    discNome,
                                    tipo
                                );
                                n.Cod_Estado = codEstado;
                                n.NomeEstado = nomeEstado;

                                todasAsNotas.Add(n);
                                _valoresOriginais[n.Cod_NotaMod] = n.Valor;
                            }
                        }
                    }
                }

                foreach (NotaModulo n in todasAsNotas)
                {
                    if (n.TipoDisciplina.ToLower() == "final")
                    {
                        if (n.NomeDisciplina.ToUpper().Contains("FCT"))
                        {
                            notaFCT = n;
                            txt_notaFCT.Text = n.Valor ?? "0";
                        }
                        else if (n.NomeDisciplina.ToUpper().Contains("PAP"))
                        {
                            notaPAP = n;
                            txt_notaPAP.Text = n.Valor ?? "0";
                        }
                    }
                    else if (n.Ano == ano)
                    {
                        listaNotas.Add(n);
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

        private string ValidarENormalizarModulo(string input, out string erro)
        {
            erro = null;
            if (string.IsNullOrWhiteSpace(input)) return null;

            string trimmed = input.Trim().ToUpper();

            if (trimmed == LetraChumbo) return LetraChumbo;

            string normalizado = trimmed.Replace(",", ".");
            if (double.TryParse(normalizado, NumberStyles.Any, CultureInfo.InvariantCulture, out double valor))
            {
                if (valor < 0)
                {
                    erro = "A nota não pode ser negativa.";
                    return null;
                }
                if (valor > 20)
                {
                    erro = "A nota não pode ser superior a 20.";
                    return null;
                }
                if (valor < 9.5)
                {
                    return LetraChumbo;
                }
                return ((int)Math.Round(valor)).ToString();
            }

            erro = $"'{input}' não é uma nota válida. Introduza um valor entre 0 e 20 ou '{LetraChumbo}'.";
            return null;
        }

        private bool ValidarNotaComponente(string input, string nomeComponente, out double resultado)
        {
            resultado = 0;
            if (string.IsNullOrWhiteSpace(input)) return true;

            string normalizado = input.Trim().Replace(",", ".");
            if (!double.TryParse(normalizado, NumberStyles.Any, CultureInfo.InvariantCulture, out double valor))
            {
                MessageBox.Show($"Nota {nomeComponente} inválida: '{input}' não é um número.", "Nota Inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (valor < 0)
            {
                MessageBox.Show($"Nota {nomeComponente} não pode ser negativa.", "Nota Inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (valor > 20)
            {
                MessageBox.Show($"Nota {nomeComponente} não pode ser superior a 20.", "Nota Inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            resultado = valor;
            return true;
        }

        private void VerificarProgressaoAnos()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(caminho))
                {
                    conn.Open();

                    string sql = @"SELECT d.Ano,
                                          COUNT(*) AS Total,
                                          SUM(CASE WHEN n.Valor REGEXP '^[0-9]+$' AND n.Valor + 0 >= 10 THEN 1 ELSE 0 END) AS Positivos
                                   FROM NotaMod n
                                   INNER JOIN Modulos m ON n.Cod_Modulo = m.Cod_Modulo
                                   INNER JOIN Disciplina d ON m.Cod_Disc = d.Cod_Disc
                                   WHERE n.Cod_Aluno = @Aluno AND d.Tipo NOT LIKE '%final%'
                                   GROUP BY d.Ano";

                    double pct1 = 0, pct2 = 0;

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Aluno", codAluno);
                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                string anoStr = r["Ano"].ToString();
                                int total = Convert.ToInt32(r["Total"]);
                                int positivos = Convert.ToInt32(r["Positivos"]);
                                double pct = total > 0 ? (double)positivos / total : 0;

                                if (anoStr == "1") pct1 = pct;
                                else if (anoStr == "2") pct2 = pct;
                            }
                        }
                    }

                    bool ano2Desbloqueado = pct1 > LimiteProgressaoAno;
                    bool ano3Desbloqueado = pct2 > LimiteProgressaoAno;

                    btn_ano2.IsEnabled = ano2Desbloqueado;
                    btn_ano3.IsEnabled = ano3Desbloqueado;

                    btn_ano2.ToolTip = ano2Desbloqueado
                        ? null
                        : $"Necessário mais de {LimiteProgressaoAno:P0} dos módulos do 1.º Ano aprovados. Actual: {pct1:P0}";

                    btn_ano3.ToolTip = ano3Desbloqueado
                        ? null
                        : $"Necessário mais de {LimiteProgressaoAno:P0} dos módulos do 2.º Ano aprovados. Actual: {pct2:P0}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao verificar progressão de anos: " + ex.Message);
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

                Dictionary<string, List<NotaModulo>> grupos = new Dictionary<string, List<NotaModulo>>();
                foreach (NotaModulo nota in listaNotas)
                {
                    if (grupos.ContainsKey(nota.NomeDisciplina) == false)
                        grupos.Add(nota.NomeDisciplina, new List<NotaModulo>());
                    grupos[nota.NomeDisciplina].Add(nota);
                }

                List<string> chaves = new List<string>(grupos.Keys);

                for (int i = 0; i < chaves.Count - 1; i++)
                {
                    for (int j = i + 1; j < chaves.Count; j++)
                    {
                        int prioridadeI = GetPrioridadeTipo(grupos[chaves[i]][0].TipoDisciplina);
                        int prioridadeJ = GetPrioridadeTipo(grupos[chaves[j]][0].TipoDisciplina);

                        if (prioridadeI > prioridadeJ)
                        {
                            string temp = chaves[i]; chaves[i] = chaves[j]; chaves[j] = temp;
                        }
                        else if (prioridadeI == prioridadeJ && string.Compare(chaves[i], chaves[j]) > 0)
                        {
                            string temp = chaves[i]; chaves[i] = chaves[j]; chaves[j] = temp;
                        }
                    }
                }

                foreach (string nome in chaves)
                {
                    List<NotaModulo> notasDaDisciplina = grupos[nome];

                    double somaNotas = 0;
                    int contadorModulosComNota = 0;
                    bool reprovouAlgumModulo = false;
                    string tipo = notasDaDisciplina[0].TipoDisciplina;

                    foreach (NotaModulo n in notasDaDisciplina)
                    {
                        double? vNum = n.ValorNumerico;

                        if (vNum != null && vNum >= 0)
                        {
                            somaNotas += vNum.Value;
                            contadorModulosComNota++;
                        }

                        if (string.IsNullOrEmpty(n.Valor) || vNum == null || vNum < 10)
                        {
                            reprovouAlgumModulo = true;
                        }
                    }

                    double mediaCalculada = contadorModulosComNota > 0 ? somaNotas / contadorModulosComNota : 0;
                    string estado = reprovouAlgumModulo ? "SC" : "C";

                    listaMedias.Add(new MediaDisciplina(nome, tipo, mediaCalculada, contadorModulosComNota, estado));
                }

                dg_medias.ItemsSource = null;
                dg_medias.ItemsSource = listaMedias;

                double somaGeral = 0, somaCien = 0, somaTec = 0;
                int contGeral = 0, contCien = 0, contTec = 0;

                foreach (NotaModulo n in listaNotas)
                {
                    double? vNum = n.ValorNumerico;
                    if (vNum != null && vNum >= 0)
                    {
                        somaGeral += vNum.Value;
                        contGeral++;

                        if (n.TipoDisciplina.ToLower().Contains("científica"))
                        {
                            somaCien += vNum.Value;
                            contCien++;
                        }
                        if (n.TipoDisciplina.ToLower().Contains("técnica"))
                        {
                            somaTec += vNum.Value;
                            contTec++;
                        }
                    }
                }

                txt_mediaGeral.Text = contGeral > 0 ? (somaGeral / contGeral).ToString("N2") : "0,00";
                txt_mediaCientifica.Text = contCien > 0 ? (somaCien / contCien).ToString("N2") : "0,00";
                txt_mediaTecnica.Text = contTec > 0 ? (somaTec / contTec).ToString("N2") : "0,00";

                double fct = 0;
                if (!string.IsNullOrWhiteSpace(txt_notaFCT.Text))
                    double.TryParse(txt_notaFCT.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out fct);

                double pap = 0;
                if (!string.IsNullOrWhiteSpace(txt_notaPAP.Text))
                    double.TryParse(txt_notaPAP.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out pap);

                double mediaDasNotas = contGeral > 0 ? somaGeral / contGeral : 0;
                double mFinal = (mediaDasNotas * 0.66) + (fct * 0.11) + (pap * 0.23);
                lbl_mediaFinal.Text = mFinal.ToString("N1");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro cálculo: " + ex.Message);
            }
        }

        private void Btn_Click_Editar(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show(
                "Deseja guardar todas as alterações feitas nas notas deste aluno (incluindo FCT/PAP)?",
                "Confirmar Alterações", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No) return;

            try
            {
                List<NotaModulo> notasParaSalvar = new List<NotaModulo>();
                foreach (NotaModulo n in listaNotas)
                {
                    string normalizado = ValidarENormalizarModulo(n.Valor, out string erroNota);
                    if (!string.IsNullOrEmpty(erroNota))
                    {
                        MessageBox.Show($"Nota inválida em '{n.NomeModulo}': {erroNota}",
                            "Erro de Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    n.Valor = normalizado;
                    notasParaSalvar.Add(n);
                }

                if (notaFCT != null)
                {
                    if (!ValidarNotaComponente(txt_notaFCT.Text, "FCT", out double fctVal)) return;
                    notaFCT.Valor = !string.IsNullOrWhiteSpace(txt_notaFCT.Text)
                        ? ((int)Math.Round(fctVal)).ToString()
                        : null;
                    notasParaSalvar.Add(notaFCT);
                }

                if (notaPAP != null)
                {
                    if (!ValidarNotaComponente(txt_notaPAP.Text, "PAP", out double papVal)) return;
                    notaPAP.Valor = !string.IsNullOrWhiteSpace(txt_notaPAP.Text)
                        ? ((int)Math.Round(papVal)).ToString()
                        : null;
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
                                string original = _valoresOriginais.ContainsKey(nota.Cod_NotaMod)
                                    ? _valoresOriginais[nota.Cod_NotaMod]
                                    : nota.Valor;
                                if (nota.Valor != original)
                                {
                                    nota.Data_Efetua = nota.Valor != null ? DateTime.Now : (DateTime?)null;
                                }

                                string sql = "UPDATE NotaMod SET Valor = @Valor, Data_Efetua = @Data, Cod_Estado = @Estado WHERE Cod_NotaMod = @Id";
                                using (MySqlCommand comando = new MySqlCommand(sql, conexao, transacao))
                                {
                                    comando.Parameters.AddWithValue("@Valor", (object)nota.Valor ?? DBNull.Value);
                                    comando.Parameters.AddWithValue("@Data", (object)nota.Data_Efetua ?? DBNull.Value);
                                    object estadoParam = (nota.Cod_Estado == null || nota.Cod_Estado == 0)
                                        ? (object)DBNull.Value
                                        : nota.Cod_Estado;
                                    comando.Parameters.AddWithValue("@Estado", estadoParam);
                                    comando.Parameters.AddWithValue("@Id", nota.Cod_NotaMod);
                                    comando.ExecuteNonQuery();
                                }
                            }

                            transacao.Commit();

                            using (MySqlConnection conexaoStatus = new MySqlConnection(caminho))
                            {
                                conexaoStatus.Open();

                                string sqlTotal = "SELECT COUNT(*) FROM NotaMod n " +
                                                  "INNER JOIN Modulos m ON n.Cod_Modulo = m.Cod_Modulo " +
                                                  "INNER JOIN Disciplina d ON m.Cod_Disc = d.Cod_Disc " +
                                                  "WHERE n.Cod_Aluno = @Aluno AND d.Tipo LIKE '%Técnica%'";

                                int totalTecnicos = 0;
                                using (MySqlCommand cmdTotal = new MySqlCommand(sqlTotal, conexaoStatus))
                                {
                                    cmdTotal.Parameters.AddWithValue("@Aluno", codAluno);
                                    totalTecnicos = Convert.ToInt32(cmdTotal.ExecuteScalar());
                                }

                                string sqlPositivos = "SELECT COUNT(*) FROM NotaMod n " +
                                                      "INNER JOIN Modulos m ON n.Cod_Modulo = m.Cod_Modulo " +
                                                      "INNER JOIN Disciplina d ON m.Cod_Disc = d.Cod_Disc " +
                                                      "WHERE n.Cod_Aluno = @Aluno AND d.Tipo LIKE '%Técnica%' " +
                                                      "AND n.Valor REGEXP '^[0-9]+$' AND n.Valor + 0 >= 10";

                                int concluidosTecnicos = 0;
                                using (MySqlCommand cmdPos = new MySqlCommand(sqlPositivos, conexaoStatus))
                                {
                                    cmdPos.Parameters.AddWithValue("@Aluno", codAluno);
                                    concluidosTecnicos = Convert.ToInt32(cmdPos.ExecuteScalar());
                                }

                                if (totalTecnicos > 0)
                                {
                                    double percentagem = (double)concluidosTecnicos / totalTecnicos;
                                    if (percentagem > 0.90)
                                    {
                                        string sqlUpdate = "UPDATE aluno SET Estado_Estagio = 'Pronto' WHERE Cod_Aluno = @Aluno";
                                        using (MySqlCommand cmdUpd = new MySqlCommand(sqlUpdate, conexaoStatus))
                                        {
                                            cmdUpd.Parameters.AddWithValue("@Aluno", codAluno);
                                            cmdUpd.ExecuteNonQuery();
                                        }
                                        MessageBox.Show(
                                            "O aluno atingiu mais de 90% dos módulos técnicos positivos! Estado de Estágio atualizado para 'Pronto'.",
                                            "Parabéns", MessageBoxButton.OK, MessageBoxImage.Information);
                                    }
                                }
                            }

                            dg_alunos.Items.Refresh();
                            CalcularResumos();
                            VerificarProgressaoAnos();
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

        private void Btn_Click_Legenda(object sender, RoutedEventArgs e)
        {
            new JanelaLegendaAlineas { Owner = this }.ShowDialog();
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
            Brush CorAtivo = (Brush)new BrushConverter().ConvertFrom("#293472");
            Brush CorInativo = (Brush)new BrushConverter().ConvertFrom("#EFE6D8");
            Brush TextoAtivo = Brushes.White;
            Brush TextoInativo = (Brush)new BrushConverter().ConvertFrom("#293472");

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
