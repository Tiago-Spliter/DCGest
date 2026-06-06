using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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

        string caminho = BD.CaminhoBD;
        int codAluno;
        List<NotaModulo> listaNotas = new List<NotaModulo>();
        List<NotaModulo> todasAsNotasModulos = new List<NotaModulo>();
        List<MediaDisciplina> listaMedias = new List<MediaDisciplina>();
        NotaModulo notaFCT = null;
        NotaModulo notaPAP = null;
        Dictionary<int, string> _valoresOriginais = new Dictionary<int, string>();
        public List<Alinea> ListaAlineas { get; private set; } = new List<Alinea>();

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
                todasAsNotasModulos.Clear();
                txt_notaFCT.Text = "0";
                txt_notaPAP.Text = "0";
                notaFCT = null;
                notaPAP = null;
                _valoresOriginais.Clear();

                List<NotaModulo> todasAsNotas = new List<NotaModulo>();

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
                                string valorDaNota = null;
                                if (leitor["Valor"] != DBNull.Value)
                                {
                                    valorDaNota = leitor["Valor"].ToString().Trim();
                                    if (valorDaNota == string.Empty)
                                    {
                                        valorDaNota = null;
                                    }
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

                                    string al = string.Empty;
                                    if (leitor["AlineaLetra"] != DBNull.Value)
                                    {
                                        al = leitor["AlineaLetra"].ToString().Trim();
                                    }

                                    string regra = string.Empty;
                                    if (leitor["Regra"] != DBNull.Value)
                                    {
                                        regra = leitor["Regra"].ToString();
                                    }

                                    if (string.IsNullOrEmpty(al))
                                    {
                                        nomeEstado = regra;
                                    }
                                    else
                                    {
                                        nomeEstado = al + " – " + regra;
                                    }
                                }

                                string anoNota = leitor["AnoDisciplina"].ToString() + "º Ano";

                                NotaModulo n = new NotaModulo(
                                    Convert.ToInt32(leitor["Cod_NotaMod"]),
                                    codAluno,
                                    0,
                                    valorDaNota,
                                    dataEfetua,
                                    anoNota,
                                    leitor["Modulo"].ToString(),
                                    leitor["Disciplina"].ToString(),
                                    leitor["Tipo"].ToString()
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
                    else
                    {
                        todasAsNotasModulos.Add(n);
                        if (n.Ano == ano)
                        {
                            listaNotas.Add(n);
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

        private string ValidarENormalizarModulo(string input, out string erro)
        {
            erro = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            string trimmed = input.Trim().ToUpper();

            if (trimmed == LetraChumbo)
            {
                return LetraChumbo;
            }

            string normalizado = trimmed.Replace(",", ".");

            if (!double.TryParse(normalizado, out double valor))
            {
                erro = "'" + input + "' não é uma nota válida. Introduza um valor entre 0 e 20 ou '" + LetraChumbo + "'.";
                return null;
            }

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

        private bool ValidarNotaComponente(string input, string nomeComponente, out double resultado)
        {
            resultado = 0;

            if (string.IsNullOrWhiteSpace(input))
            {
                return true;
            }

            string normalizado = input.Trim().Replace(",", ".");

            if (!double.TryParse(normalizado, out double valor))
            {
                MessageBox.Show("Nota " + nomeComponente + " inválida: '" + input + "' não é um número.", "Nota Inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (valor < 0)
            {
                MessageBox.Show("Nota " + nomeComponente + " não pode ser negativa.", "Nota Inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (valor > 20)
            {
                MessageBox.Show("Nota " + nomeComponente + " não pode ser superior a 20.", "Nota Inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                                double pct = 0;
                                if (total > 0)
                                {
                                    pct = (double)positivos / total;
                                }

                                if (anoStr == "1")
                                {
                                    pct1 = pct;
                                }
                                else if (anoStr == "2")
                                {
                                    pct2 = pct;
                                }
                            }
                        }
                    }

                    bool ano2Desbloqueado = pct1 > LimiteProgressaoAno;
                    bool ano3Desbloqueado = pct2 > LimiteProgressaoAno;

                    btn_ano2.IsEnabled = ano2Desbloqueado;
                    btn_ano3.IsEnabled = ano3Desbloqueado;

                    if (ano2Desbloqueado)
                    {
                        btn_ano2.ToolTip = null;
                    }
                    else
                    {
                        btn_ano2.ToolTip = "Necessário mais de " + LimiteProgressaoAno.ToString("P0") + " dos módulos do 1.º Ano aprovados. Actual: " + pct1.ToString("P0");
                    }

                    if (ano3Desbloqueado)
                    {
                        btn_ano3.ToolTip = null;
                    }
                    else
                    {
                        btn_ano3.ToolTip = "Necessário mais de " + LimiteProgressaoAno.ToString("P0") + " dos módulos do 2.º Ano aprovados. Actual: " + pct2.ToString("P0");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao verificar progressão de anos: " + ex.Message);
            }
        }

        private void CalcularResumos()
        {
            try
            {
                listaMedias.Clear();

                Dictionary<string, List<NotaModulo>> grupos = new Dictionary<string, List<NotaModulo>>();
                foreach (NotaModulo nota in listaNotas)
                {
                    if (!grupos.ContainsKey(nota.NomeDisciplina))
                    {
                        grupos.Add(nota.NomeDisciplina, new List<NotaModulo>());
                    }
                    grupos[nota.NomeDisciplina].Add(nota);
                }

                List<string> chaves = new List<string>(grupos.Keys);

                for (int i = 0; i < chaves.Count - 1; i++)
                {
                    for (int j = i + 1; j < chaves.Count; j++)
                    {
                        int prioridadeI = GetPrioridadeTipo(grupos[chaves[i]][0].TipoDisciplina);
                        int prioridadeJ = GetPrioridadeTipo(grupos[chaves[j]][0].TipoDisciplina);

                        bool trocar = false;
                        if (prioridadeI > prioridadeJ)
                        {
                            trocar = true;
                        }
                        else if (prioridadeI == prioridadeJ && string.Compare(chaves[i], chaves[j]) > 0)
                        {
                            trocar = true;
                        }

                        if (trocar)
                        {
                            string temp = chaves[i];
                            chaves[i] = chaves[j];
                            chaves[j] = temp;
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

                    double mediaCalculada = 0;
                    if (contadorModulosComNota > 0)
                    {
                        mediaCalculada = somaNotas / contadorModulosComNota;
                    }

                    string estado = "SC";
                    if (!reprovouAlgumModulo)
                    {
                        estado = "C";
                    }

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

                if (contGeral > 0)
                {
                    txt_mediaGeral.Text = (somaGeral / contGeral).ToString("N2");
                }
                else
                {
                    txt_mediaGeral.Text = "0,00";
                }

                if (contCien > 0)
                {
                    txt_mediaCientifica.Text = (somaCien / contCien).ToString("N2");
                }
                else
                {
                    txt_mediaCientifica.Text = "0,00";
                }

                if (contTec > 0)
                {
                    txt_mediaTecnica.Text = (somaTec / contTec).ToString("N2");
                }
                else
                {
                    txt_mediaTecnica.Text = "0,00";
                }

                double fct = 0;
                if (!string.IsNullOrWhiteSpace(txt_notaFCT.Text))
                {
                    double.TryParse(txt_notaFCT.Text.Replace(",", "."), out fct);
                }

                double pap = 0;
                if (!string.IsNullOrWhiteSpace(txt_notaPAP.Text))
                {
                    double.TryParse(txt_notaPAP.Text.Replace(",", "."), out pap);
                }

                double somaTodasNotas = 0;
                int contTodasNotas = 0;
                foreach (NotaModulo n in todasAsNotasModulos)
                {
                    double? vNum = n.ValorNumerico;
                    if (vNum != null && vNum > 0)
                    {
                        somaTodasNotas += vNum.Value;
                        contTodasNotas++;
                    }
                }

                double mediaDasNotas = contTodasNotas > 0 ? somaTodasNotas / contTodasNotas : 0;

                double mFinal = (mediaDasNotas * 0.66) + (fct * 0.11) + (pap * 0.23);
                lbl_mediaFinal.Text = mFinal.ToString("N1");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao calcular resumos: " + ex.Message);
            }
        }

        private int GetPrioridadeTipo(string tipo)
        {
            if (tipo.ToLower().Contains("sociocultural")) { return 1; }
            if (tipo.ToLower().Contains("científica")) { return 2; }
            if (tipo.ToLower().Contains("técnica")) { return 3; }
            return 4;
        }

        private void Btn_Click_Editar(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show(
                "Deseja guardar todas as alterações feitas nas notas deste aluno (incluindo FCT/PAP)?",
                "Confirmar Alterações", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (res == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                List<NotaModulo> notasParaSalvar = new List<NotaModulo>();

                foreach (NotaModulo n in listaNotas)
                {
                    if (double.TryParse(n.Valor?.Replace(",", "."), out double testVal) && testVal < 0)
                    {
                        Alinea alineaN = ListaAlineas.Find(a => a.AlineaLetra == "n)");
                        if (alineaN != null)
                        {
                            n.Cod_Estado = alineaN.Cod_Alinea;
                        }
                        n.Valor = null;
                        notasParaSalvar.Add(n);
                        continue;
                    }

                    string normalizado = ValidarENormalizarModulo(n.Valor, out string erroNota);
                    if (!string.IsNullOrEmpty(erroNota))
                    {
                        MessageBox.Show("Nota inválida em '" + n.NomeModulo + "': " + erroNota, "Erro de Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    n.Valor = normalizado;
                    notasParaSalvar.Add(n);
                }

                if (notaFCT != null)
                {
                    if (!ValidarNotaComponente(txt_notaFCT.Text, "FCT", out double fctVal))
                    {
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(txt_notaFCT.Text))
                    {
                        notaFCT.Valor = ((int)Math.Round(fctVal)).ToString();
                    }
                    else
                    {
                        notaFCT.Valor = null;
                    }

                    notasParaSalvar.Add(notaFCT);
                }

                if (notaPAP != null)
                {
                    if (!ValidarNotaComponente(txt_notaPAP.Text, "PAP", out double papVal))
                    {
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(txt_notaPAP.Text))
                    {
                        notaPAP.Valor = ((int)Math.Round(papVal)).ToString();
                    }
                    else
                    {
                        notaPAP.Valor = null;
                    }

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
                                string original = nota.Valor;
                                if (_valoresOriginais.ContainsKey(nota.Cod_NotaMod))
                                {
                                    original = _valoresOriginais[nota.Cod_NotaMod];
                                }

                                if (nota.Valor != original)
                                {
                                    if (nota.Valor != null)
                                    {
                                        nota.Data_Efetua = DateTime.Now;
                                    }
                                    else
                                    {
                                        nota.Data_Efetua = null;
                                    }
                                }

                                object valorParam = DBNull.Value;
                                if (nota.Valor != null)
                                {
                                    valorParam = nota.Valor;
                                }

                                object dataParam = DBNull.Value;
                                if (nota.Data_Efetua != null)
                                {
                                    dataParam = nota.Data_Efetua;
                                }

                                object estadoParam = DBNull.Value;
                                if (nota.Cod_Estado != null && nota.Cod_Estado != 0)
                                {
                                    estadoParam = nota.Cod_Estado;
                                }

                                string sql = "UPDATE NotaMod SET Valor = @Valor, Data_Efetua = @Data, Cod_Estado = @Estado WHERE Cod_NotaMod = @Id";
                                using (MySqlCommand comando = new MySqlCommand(sql, conexao, transacao))
                                {
                                    comando.Parameters.AddWithValue("@Valor", valorParam);
                                    comando.Parameters.AddWithValue("@Data", dataParam);
                                    comando.Parameters.AddWithValue("@Estado", estadoParam);
                                    comando.Parameters.AddWithValue("@Id", nota.Cod_NotaMod);
                                    comando.ExecuteNonQuery();
                                }
                            }

                            transacao.Commit();

                            AtualizarEstadoEstagio();

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

        private void AtualizarEstadoEstagio()
        {
            using (MySqlConnection conexao = new MySqlConnection(caminho))
            {
                conexao.Open();

                string sqlTotal = @"SELECT COUNT(*) FROM NotaMod n
                                    INNER JOIN Modulos m ON n.Cod_Modulo = m.Cod_Modulo
                                    INNER JOIN Disciplina d ON m.Cod_Disc = d.Cod_Disc
                                    WHERE n.Cod_Aluno = @Aluno AND d.Tipo LIKE '%Técnica%'";

                int totalTecnicos = 0;
                using (MySqlCommand cmd = new MySqlCommand(sqlTotal, conexao))
                {
                    cmd.Parameters.AddWithValue("@Aluno", codAluno);
                    totalTecnicos = Convert.ToInt32(cmd.ExecuteScalar());
                }

                string sqlPositivos = @"SELECT COUNT(*) FROM NotaMod n
                                        INNER JOIN Modulos m ON n.Cod_Modulo = m.Cod_Modulo
                                        INNER JOIN Disciplina d ON m.Cod_Disc = d.Cod_Disc
                                        WHERE n.Cod_Aluno = @Aluno AND d.Tipo LIKE '%Técnica%'
                                        AND n.Valor REGEXP '^[0-9]+$' AND n.Valor + 0 >= 10";

                int concluidosTecnicos = 0;
                using (MySqlCommand cmd = new MySqlCommand(sqlPositivos, conexao))
                {
                    cmd.Parameters.AddWithValue("@Aluno", codAluno);
                    concluidosTecnicos = Convert.ToInt32(cmd.ExecuteScalar());
                }

                if (totalTecnicos > 0)
                {
                    double percentagem = (double)concluidosTecnicos / totalTecnicos;
                    if (percentagem > 0.90)
                    {
                        string sqlUpdate = "UPDATE aluno SET Estado_Estagio = 'Pronto' WHERE Cod_Aluno = @Aluno";
                        using (MySqlCommand cmd = new MySqlCommand(sqlUpdate, conexao))
                        {
                            cmd.Parameters.AddWithValue("@Aluno", codAluno);
                            cmd.ExecuteNonQuery();
                        }
                        MessageBox.Show("O aluno atingiu mais de 90% dos módulos técnicos positivos! Estado de Estágio atualizado para 'Pronto'.", "Parabéns", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
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
            Brush corAtivo = (Brush)new BrushConverter().ConvertFrom("#293472");
            Brush corInativo = (Brush)new BrushConverter().ConvertFrom("#EFE6D8");
            Brush textoAtivo = Brushes.White;
            Brush textoInativo = (Brush)new BrushConverter().ConvertFrom("#293472");

            btn_ano1.Background = corInativo;
            btn_ano1.Foreground = textoInativo;
            btn_ano2.Background = corInativo;
            btn_ano2.Foreground = textoInativo;
            btn_ano3.Background = corInativo;
            btn_ano3.Foreground = textoInativo;

            botaoSelecionado.Background = corAtivo;
            botaoSelecionado.Foreground = textoAtivo;
        }
    }
}
