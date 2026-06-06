using iTextSharp.text;
using iTextSharp.text.pdf;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;

namespace DCGest.Classes
{
    public class GeradorPDF
    {
        private readonly string connectionString = BD.CaminhoBD;

        private readonly BaseColor corLaranjaMedio  = new BaseColor(250, 191, 143);
        private readonly BaseColor corVerdeMedio    = new BaseColor(170, 210, 140);
        private readonly BaseColor corVermelhoMedio = new BaseColor(240, 160, 160);

        private readonly BaseColor corLaranjaEscuro  = new BaseColor(228, 108, 10);
        private readonly BaseColor corVerdeEscuro    = new BaseColor(118, 147, 60);
        private readonly BaseColor corVermelhoEscuro = new BaseColor(180, 40, 40);

        private readonly BaseColor corRosaCarregado        = new BaseColor(245, 140, 170);
        private readonly BaseColor corVerdeClaroComponente = new BaseColor(210, 235, 190);
        private readonly BaseColor corRosaMuitoClaro       = new BaseColor(255, 230, 240);
        private readonly BaseColor corFCT          = new BaseColor(183, 222, 232);
        private readonly BaseColor corHeaderGrelha = new BaseColor(211, 211, 211);
        private readonly BaseColor corFinalCurso   = new BaseColor(255, 255,   0);

        public string GerarRelatorioAluno(Aluno aluno)
        {
            Aluno alunoCompleto = Aluno.ObterPorId(aluno.Cod_Aluno) ?? aluno;

            string pasta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Relatorios");
            if (!Directory.Exists(pasta))
            {
                Directory.CreateDirectory(pasta);
            }
            string caminho = Path.Combine(pasta, $"Relatorio_{aluno.Cod_Aluno}.pdf");

            Document doc = new Document(PageSize.A4.Rotate(), 20, 20, 15, 15);
            _ = PdfWriter.GetInstance(doc, new FileStream(caminho, FileMode.Create));
            doc.Open();

            Font fBase  = FontFactory.GetFont(FontFactory.HELVETICA,      7,  BaseColor.BLACK);
            Font fBold  = FontFactory.GetFont(FontFactory.HELVETICA_BOLD,  7,  BaseColor.BLACK);
            Font fTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD,  12, BaseColor.BLACK);

            List<NotaModulo> todasNotas = ObterTodasNotas(aluno.Cod_Aluno);

            doc.Add(CriarCabecalho(alunoCompleto, fBase, fBold, fTitle));
            doc.Add(new Paragraph(" "));

            doc.Add(CriarGrelhaModulos(todasNotas, fBase, fBold, out double notaFCT, out double notaPAP));
            doc.Add(new Paragraph(" "));

            double mediaModulos = CalcularMediaGeral(todasNotas);

            doc.Add(CriarTabelaMediasComponentes(todasNotas, fBase, fBold));
            doc.Add(new Paragraph(" "));

            doc.Add(CriarTabelaMediaFinal(mediaModulos, notaFCT, notaPAP, fBold));
            doc.Add(new Paragraph(" "));

            doc.Add(CriarTabelaAnos(todasNotas, fBase, fBold));

            doc.Close();
            return caminho;
        }

        private PdfPTable CriarCabecalho(Aluno a, Font fBase, Font fBold, Font fTitle)
        {
            PdfPTable tab = new PdfPTable(3);
            tab.WidthPercentage = 100;
            tab.SetWidths(new float[] { 3, 1, 1 });

            PdfPCell titulo = new PdfPCell(new Phrase("REGISTO BIOGRÁFICO DE AVALIAÇÃO", fTitle));
            titulo.Colspan = 3;
            titulo.Border = 0;
            titulo.HorizontalAlignment = Element.ALIGN_CENTER;
            titulo.PaddingBottom = 15;
            tab.AddCell(titulo);

            AdicionarCelula(tab, "ALUNO: "       + a.Nome_Aluno.ToUpper(), fBold, 5);
            AdicionarCelula(tab, "N.º PROC: "    + a.Cod_Aluno,             fBase, 5, Element.ALIGN_CENTER);
            AdicionarCelula(tab, "TURMA: "       + a.Nome_Turma,            fBase, 5, Element.ALIGN_CENTER);
            AdicionarCelula(tab, "CURSO: "       + a.Nome_Curso,            fBase, 5);
            AdicionarCelula(tab, "ANO LETIVO: "  + a.Intervalo_Letivo,      fBase, 5, Element.ALIGN_CENTER);
            AdicionarCelula(tab, "ORIENTADOR: "  + a.Nome_Orientador,       fBase, 5, Element.ALIGN_CENTER);

            return tab;
        }

        private PdfPTable CriarGrelhaModulos(List<NotaModulo> todasNotas, Font fBase, Font fBold, out double notaFCT, out double notaPAP)
        {
            notaFCT = 0;
            notaPAP = 0;

            PdfPTable grid = new PdfPTable(20);
            grid.WidthPercentage = 100;
            float[] larguras = new float[20];
            larguras[0] = 5f;
            for (int i = 1; i < 20; i++) larguras[i] = 1f;
            grid.SetWidths(larguras);

            grid.AddCell(new PdfPCell(new Phrase("DISCIPLINAS", fBold)) { BackgroundColor = corHeaderGrelha, Padding = 2 });
            for (int i = 1; i <= 19; i++)
            {
                grid.AddCell(new PdfPCell(new Phrase("M" + i, fBold)) { BackgroundColor = corHeaderGrelha, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            }

            Dictionary<string, List<NotaModulo>> agrupadas = AgruparPorDisciplina(todasNotas);
            List<string> ordem = OrdenarDisciplinas(agrupadas);

            foreach (string nomeDisc in ordem)
            {
                string tipo = agrupadas[nomeDisc][0].TipoDisciplina;

                if (tipo.ToLower().Contains("final"))
                {
                    double vF = agrupadas[nomeDisc][0].ValorNumerico ?? 0;
                    if (nomeDisc.ToUpper().Contains("FCT")) notaFCT = vF;
                    else if (nomeDisc.ToUpper().Contains("PAP")) notaPAP = vF;
                    continue;
                }

                BaseColor bgComp = BaseColor.WHITE;
                if (tipo.ToLower().Contains("cultural")) bgComp = corRosaCarregado;
                else if (tipo.ToLower().Contains("técnica")) bgComp = corVerdeClaroComponente;

                grid.AddCell(new PdfPCell(new Phrase(nomeDisc, fBase)) { BackgroundColor = bgComp, Padding = 2 });

                NotaModulo[] colunas = new NotaModulo[20];
                foreach (NotaModulo m in agrupadas[nomeDisc])
                {
                    int nMod = ExtrairNumeroModulo(m.NomeModulo);
                    if (nMod >= 1 && nMod <= 19) colunas[nMod] = m;
                }

                for (int i = 1; i <= 19; i++)
                {
                    string v = "";
                    BaseColor bgCell = BaseColor.WHITE;
                    if (colunas[i] != null)
                    {
                        if (colunas[i].Valor == "RC")
                        {
                            v = !string.IsNullOrWhiteSpace(colunas[i].NomeEstado) ? colunas[i].NomeEstado : "RC";
                        }
                        else
                        {
                            v = colunas[i].Valor ?? "";
                        }
                        bgCell = GetCorAno(colunas[i].Ano);
                    }
                    grid.AddCell(new PdfPCell(new Phrase(v, fBase)) { BackgroundColor = bgCell, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
                }
            }

            grid.AddCell(new PdfPCell(new Phrase("FCT", fBase)) { BackgroundColor = corFCT, Padding = 2 });
            grid.AddCell(new PdfPCell(new Phrase(notaFCT.ToString(), fBase)) { BackgroundColor = corFCT, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            grid.AddCell(new PdfPCell(new Phrase("", fBase)) { Border = 0, Colspan = 18 });

            grid.AddCell(new PdfPCell(new Phrase("PAP", fBase)) { BackgroundColor = corRosaCarregado, Padding = 2 });
            grid.AddCell(new PdfPCell(new Phrase(notaPAP.ToString(), fBase)) { BackgroundColor = corRosaCarregado, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            grid.AddCell(new PdfPCell(new Phrase("", fBase)) { Border = 0, Colspan = 18 });

            return grid;
        }

        private PdfPTable CriarTabelaMediasComponentes(List<NotaModulo> todasNotas, Font fBase, Font fBold)
        {
            PdfPTable tab = new PdfPTable(2);
            tab.WidthPercentage = 30;
            tab.HorizontalAlignment = Element.ALIGN_LEFT;

            tab.AddCell(new PdfPCell(new Phrase("Média das Sociocultural", fBase)) { Border = 0 });
            tab.AddCell(new PdfPCell(new Phrase(CalcularMedia(todasNotas, "cultural"),   fBold)) { HorizontalAlignment = Element.ALIGN_CENTER });
            tab.AddCell(new PdfPCell(new Phrase("Média das Científicas",   fBase)) { Border = 0 });
            tab.AddCell(new PdfPCell(new Phrase(CalcularMedia(todasNotas, "científica"), fBold)) { HorizontalAlignment = Element.ALIGN_CENTER });
            tab.AddCell(new PdfPCell(new Phrase("Média das Técnicas",      fBase)) { Border = 0 });
            tab.AddCell(new PdfPCell(new Phrase(CalcularMedia(todasNotas, "técnica"),   fBold)) { HorizontalAlignment = Element.ALIGN_CENTER });

            return tab;
        }

        private PdfPTable CriarTabelaMediaFinal(double mediaModulos, double notaFCT, double notaPAP, Font fBold)
        {
            double mFinal = (mediaModulos * 0.66) + (notaFCT * 0.11) + (notaPAP * 0.23);

            PdfPTable tab = new PdfPTable(2);
            tab.WidthPercentage = 20;
            tab.HorizontalAlignment = Element.ALIGN_LEFT;

            tab.AddCell(new PdfPCell(new Phrase("Média final de curso", fBold)) { BackgroundColor = corFinalCurso, Padding = 4 });
            tab.AddCell(new PdfPCell(new Phrase(mFinal.ToString("N1"), fBold)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });

            return tab;
        }

        private PdfPTable CriarTabelaAnos(List<NotaModulo> todasNotas, Font fBase, Font fBold)
        {
            PdfPTable tab = new PdfPTable(3);
            tab.WidthPercentage = 100;
            tab.SetWidths(new float[] { 1, 1, 1 });

            tab.AddCell(CriarMiniTabelaAno(todasNotas, "1º Ano", corLaranjaEscuro,  fBase, fBold));
            tab.AddCell(CriarMiniTabelaAno(todasNotas, "2º Ano", corVerdeEscuro,    fBase, fBold));
            tab.AddCell(CriarMiniTabelaAno(todasNotas, "3º Ano", corVermelhoEscuro, fBase, fBold));

            return tab;
        }

        private PdfPCell CriarMiniTabelaAno(List<NotaModulo> notas, string ano, BaseColor cHeader, Font fBase, Font fBold)
        {
            PdfPCell cell = new PdfPCell() { Border = 0, Padding = 3 };
            List<NotaModulo> notasAno = new List<NotaModulo>();
            int totalModulos    = 0;
            int tecnicasModulos = 0;
            double somaAno = 0;
            int contaAno   = 0;

            foreach (NotaModulo n in notas)
            {
                if (n.Ano == ano && !n.TipoDisciplina.ToLower().Contains("final"))
                {
                    notasAno.Add(n);
                    totalModulos++;
                    if (n.TipoDisciplina.ToLower().Contains("técnica")) tecnicasModulos++;
                    double? vNum = n.ValorNumerico;
                    if (vNum != null && vNum > 0) { somaAno += vNum.Value; contaAno++; }
                }
            }

            PdfPTable resumo = new PdfPTable(4);
            resumo.WidthPercentage = 100;
            resumo.SetWidths(new float[] { 20, 20, 30, 30 });
            resumo.AddCell(new PdfPCell(new Phrase("Total",    fBase)) { BackgroundColor = cHeader, HorizontalAlignment = Element.ALIGN_CENTER });
            resumo.AddCell(new PdfPCell(new Phrase(totalModulos.ToString(),    fBold)) { BackgroundColor = cHeader, HorizontalAlignment = Element.ALIGN_CENTER });
            resumo.AddCell(new PdfPCell(new Phrase("Técnicas", fBase)) { BackgroundColor = cHeader, HorizontalAlignment = Element.ALIGN_CENTER });
            resumo.AddCell(new PdfPCell(new Phrase(tecnicasModulos.ToString(), fBold)) { BackgroundColor = cHeader, HorizontalAlignment = Element.ALIGN_CENTER });
            cell.AddElement(resumo);

            PdfPTable data = new PdfPTable(3);
            data.WidthPercentage = 100;
            data.SpacingBefore = 2;
            data.AddCell(new PdfPCell(new Phrase(ano.ToUpper(), fBold)) { BackgroundColor = cHeader, Colspan = 3, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            foreach (string h in new[] { "MÉDIA", "MÓD NR", "SIT." })
            {
                data.AddCell(new PdfPCell(new Phrase(h, fBold)) { BackgroundColor = corHeaderGrelha, Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });
            }

            Dictionary<string, List<NotaModulo>> grupos = AgruparPorDisciplina(notasAno);
            List<string> chaves = new List<string>(grupos.Keys);
            for (int i = 0; i < chaves.Count - 1; i++)
            {
                for (int j = i + 1; j < chaves.Count; j++)
                {
                    if (GetPrioridadeTipo(grupos[chaves[i]][0].TipoDisciplina) > GetPrioridadeTipo(grupos[chaves[j]][0].TipoDisciplina))
                    {
                        string tmp = chaves[i]; chaves[i] = chaves[j]; chaves[j] = tmp;
                    }
                }
            }

            foreach (string nome in chaves)
            {
                List<NotaModulo> notasDisc = grupos[nome];
                double soma  = 0;
                int realizado = 0;
                bool sc = false;
                foreach (NotaModulo v in notasDisc)
                {
                    double? vNum = v.ValorNumerico;
                    if (vNum != null && vNum > 0) { soma += vNum.Value; realizado++; }
                    if (string.IsNullOrEmpty(v.Valor) || vNum == null || vNum < 10) sc = true;
                }
                double media = realizado > 0 ? soma / realizado : 0;

                BaseColor bg = BaseColor.WHITE;
                string tp = notasDisc[0].TipoDisciplina.ToLower();
                if (tp.Contains("cultural")) bg = corRosaMuitoClaro;
                else if (tp.Contains("técnica")) bg = corVerdeClaroComponente;

                data.AddCell(new PdfPCell(new Phrase(media.ToString("N1"), fBase)) { BackgroundColor = bg, Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                data.AddCell(new PdfPCell(new Phrase(realizado.ToString(),  fBase)) { BackgroundColor = bg, Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                data.AddCell(new PdfPCell(new Phrase(sc ? "SC" : "C",       fBase)) { BackgroundColor = bg, Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });
            }
            cell.AddElement(data);

            double mAnual = contaAno > 0 ? somaAno / contaAno : 0;
            PdfPTable tMA = new PdfPTable(2);
            tMA.WidthPercentage = 100;
            tMA.SpacingBefore = 5;
            tMA.SetWidths(new float[] { 2, 1 });
            tMA.AddCell(new PdfPCell(new Phrase("MÉDIA DO ANO", fBold)) { BackgroundColor = cHeader, Padding = 3 });
            tMA.AddCell(new PdfPCell(new Phrase(mAnual.ToString("N1"), fBold)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 3 });
            cell.AddElement(tMA);

            return cell;
        }

        private List<NotaModulo> ObterTodasNotas(int codAluno)
        {
            List<NotaModulo> lista = new List<NotaModulo>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string sql =
                    "SELECT n.Cod_NotaMod, d.Ano AS AnoDisciplina, d.Designacao AS Disciplina, d.Tipo, " +
                    "       m.Designacao AS Modulo, m.Cod_Modulo, n.Valor, a.Alinea AS AlineaLetra " +
                    "FROM NotaMod n " +
                    "INNER JOIN Modulos    m ON n.Cod_Modulo = m.Cod_Modulo " +
                    "INNER JOIN Disciplina d ON m.Cod_Disc   = d.Cod_Disc " +
                    "LEFT  JOIN Alineas    a ON n.Cod_Estado  = a.Cod_alinea " +
                    "WHERE n.Cod_Aluno = @Aluno " +
                    "ORDER BY d.Tipo, d.Designacao, m.Cod_Modulo";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Aluno", codAluno);
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            string valor = r["Valor"] != DBNull.Value ? r["Valor"].ToString().Trim() : null;
                            if (valor == string.Empty) valor = null;

                            string sigla = r["AlineaLetra"] != DBNull.Value ? r["AlineaLetra"].ToString().Trim() : string.Empty;
                            string ano   = r["AnoDisciplina"].ToString() + "º Ano";

                            NotaModulo nm = new NotaModulo(
                                Convert.ToInt32(r["Cod_NotaMod"]),
                                codAluno,
                                Convert.ToInt32(r["Cod_Modulo"]),
                                valor, null, ano,
                                r["Modulo"].ToString(),
                                r["Disciplina"].ToString(),
                                r["Tipo"].ToString());
                            nm.NomeEstado = sigla;
                            lista.Add(nm);
                        }
                    }
                }
            }
            return lista;
        }

        private static Dictionary<string, List<NotaModulo>> AgruparPorDisciplina(List<NotaModulo> notas)
        {
            Dictionary<string, List<NotaModulo>> grupos = new Dictionary<string, List<NotaModulo>>();
            foreach (NotaModulo n in notas)
            {
                if (!grupos.ContainsKey(n.NomeDisciplina))
                {
                    grupos.Add(n.NomeDisciplina, new List<NotaModulo>());
                }
                grupos[n.NomeDisciplina].Add(n);
            }
            return grupos;
        }

        private static List<string> OrdenarDisciplinas(Dictionary<string, List<NotaModulo>> agrupadas)
        {
            List<string> socio  = new List<string>();
            List<string> cien   = new List<string>();
            List<string> tec    = new List<string>();
            List<string> outros = new List<string>();

            foreach (string k in agrupadas.Keys)
            {
                string t = agrupadas[k][0].TipoDisciplina.ToLower();
                if      (t.Contains("cultural"))   socio.Add(k);
                else if (t.Contains("científica")) cien.Add(k);
                else if (t.Contains("técnica"))    tec.Add(k);
                else                               outros.Add(k);
            }
            socio.Sort(); cien.Sort(); tec.Sort(); outros.Sort();

            List<string> ordem = new List<string>();
            ordem.AddRange(socio);
            ordem.AddRange(cien);
            ordem.AddRange(tec);
            ordem.AddRange(outros);
            return ordem;
        }

        private static double CalcularMediaGeral(List<NotaModulo> notas)
        {
            double soma = 0;
            int conta   = 0;
            foreach (NotaModulo n in notas)
            {
                double? vNum = n.ValorNumerico;
                if (!n.TipoDisciplina.ToLower().Contains("final") && vNum != null && vNum > 0)
                {
                    soma += vNum.Value;
                    conta++;
                }
            }
            return conta > 0 ? soma / conta : 0;
        }

        private static string CalcularMedia(List<NotaModulo> notas, string tipo)
        {
            double soma = 0;
            int conta   = 0;
            foreach (NotaModulo n in notas)
            {
                double? vNum = n.ValorNumerico;
                if (n.TipoDisciplina.ToLower().Contains(tipo) && vNum != null && vNum > 0)
                {
                    soma += vNum.Value;
                    conta++;
                }
            }
            return conta > 0 ? (soma / conta).ToString("N1") : "0,0";
        }

        private static void AdicionarCelula(PdfPTable tab, string texto, Font font, float padding, int alinhamento = Element.ALIGN_LEFT)
        {
            PdfPCell c = new PdfPCell(new Phrase(texto, font));
            c.Padding = padding;
            c.HorizontalAlignment = alinhamento;
            tab.AddCell(c);
        }

        private static int ExtrairNumeroModulo(string nome)
        {
            string digitos = "";
            foreach (char c in nome)
            {
                if (char.IsDigit(c)) digitos += c;
            }
            return digitos != "" ? Convert.ToInt32(digitos) : 0;
        }

        private BaseColor GetCorAno(string ano)
        {
            if (ano.Contains("1")) return corLaranjaMedio;
            if (ano.Contains("2")) return corVerdeMedio;
            return corVermelhoMedio;
        }

        private static int GetPrioridadeTipo(string tipo)
        {
            string t = tipo.ToLower();
            if (t.Contains("cultural"))   return 1;
            if (t.Contains("científica")) return 2;
            if (t.Contains("técnica"))    return 3;
            return 4;
        }
    }
}
