using iTextSharp.text;
using iTextSharp.text.pdf;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using DCGest.Classes;

namespace DCGest.Classes
{
    public class GeradorPDF
    {
        private string connectionString = BD.CaminhoBD;

        // PALETE DE CORES SINCRONIZADA

        // TONS DE REFERÊNCIA (Para as células M1-M19)
        BaseColor corLaranjaMedio = new BaseColor(250, 191, 143);  // 1º Ano
        BaseColor corVerdeMedio = new BaseColor(170, 210, 140);    // 2º Ano
        BaseColor corVermelhoMedio = new BaseColor(240, 160, 160); // 3º Ano

        // TONS DE CABEÇALHO (Rodapé)
        BaseColor corLaranjaEscuro = new BaseColor(228, 108, 10);
        BaseColor corVerdeEscuro = new BaseColor(118, 147, 60);
        BaseColor corVermelhoEscuro = new BaseColor(180, 40, 40);

        // TONS DE COMPONENTE (Coluna 1)
        BaseColor corRosaCarregado = new BaseColor(245, 140, 170);
        BaseColor corVerdeClaroComponente = new BaseColor(210, 235, 190);

        // CORES ESPECIAIS
        BaseColor corFCT = new BaseColor(183, 222, 232);
        BaseColor corHeaderGrelha = new BaseColor(211, 211, 211);
        BaseColor corFinalCurso = new BaseColor(255, 255, 0);
        BaseColor corRosaMuitoClaro = new BaseColor(255, 230, 240);

        public string GerarRelatorioAluno(Aluno aluno)
        {
            Aluno alunoCompleto = Aluno.ObterPorId(aluno.Cod_Aluno);
            if (alunoCompleto == null)
            {
                alunoCompleto = aluno;
            }

            string pastaTemp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Relatorios");
            if (Directory.Exists(pastaTemp) == false)
            {
                Directory.CreateDirectory(pastaTemp);
            }
            string caminhoPdf = Path.Combine(pastaTemp, $"Relatorio_{aluno.Cod_Aluno}.pdf");

            Document doc = new Document(PageSize.A4.Rotate(), 20, 20, 15, 15);
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(caminhoPdf, FileMode.Create));
            doc.Open();

            Font fBase = FontFactory.GetFont(FontFactory.HELVETICA, 7, BaseColor.BLACK);
            Font fBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7, BaseColor.BLACK);
            Font fTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);

            // 1. CABEÇALHO ESTILO TESTE
            PdfPTable tabHeader = new PdfPTable(3);
            tabHeader.WidthPercentage = 100;
            tabHeader.SetWidths(new float[] { 3, 1, 1 });

            PdfPCell cellTitle = new PdfPCell(new Phrase("REGISTO BIOGRÁFICO DE AVALIAÇÃO", fTitle));
            cellTitle.Colspan = 3; cellTitle.Border = 0; cellTitle.HorizontalAlignment = Element.ALIGN_CENTER; cellTitle.PaddingBottom = 15;
            tabHeader.AddCell(cellTitle);

            PdfPCell cNome = new PdfPCell(new Phrase("ALUNO: " + alunoCompleto.Nome_Aluno.ToUpper(), fBold));
            cNome.Padding = 5; tabHeader.AddCell(cNome);
            PdfPCell cProc = new PdfPCell(new Phrase("N.º PROC: " + alunoCompleto.Cod_Aluno, fBase));
            cProc.Padding = 5; cProc.HorizontalAlignment = Element.ALIGN_CENTER; tabHeader.AddCell(cProc);
            PdfPCell cTurma = new PdfPCell(new Phrase("TURMA: " + alunoCompleto.Nome_Turma, fBase));
            cTurma.Padding = 5; cTurma.HorizontalAlignment = Element.ALIGN_CENTER; tabHeader.AddCell(cTurma);

            PdfPCell cCurso = new PdfPCell(new Phrase("CURSO: " + alunoCompleto.Nome_Curso, fBase));
            cCurso.Padding = 5; tabHeader.AddCell(cCurso);
            PdfPCell cAno = new PdfPCell(new Phrase("ANO LETIVO: " + alunoCompleto.Intervalo_Letivo, fBase));
            cAno.Padding = 5; cAno.HorizontalAlignment = Element.ALIGN_CENTER; tabHeader.AddCell(cAno);
            PdfPCell cOri = new PdfPCell(new Phrase("ORIENTADOR: " + alunoCompleto.Nome_Orientador, fBase));
            cOri.Padding = 5; cOri.HorizontalAlignment = Element.ALIGN_CENTER; tabHeader.AddCell(cOri);

            doc.Add(tabHeader);
            doc.Add(new Paragraph(" "));

            // 2. GRELHA DE MÓDULOS
            PdfPTable gridM = new PdfPTable(20);
            gridM.WidthPercentage = 100;
            float[] wGrid = new float[20];
            wGrid[0] = 5f; for (int i = 1; i < 20; i++) wGrid[i] = 1f;
            gridM.SetWidths(wGrid);

            gridM.AddCell(new PdfPCell(new Phrase("DISCIPLINAS", fBold)) { BackgroundColor = corHeaderGrelha, Padding = 2 });
            for (int i = 1; i <= 19; i++) gridM.AddCell(new PdfPCell(new Phrase("M" + i, fBold)) { BackgroundColor = corHeaderGrelha, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });

            List<NotaModulo> todasNotas = ObterTodasNotas(aluno.Cod_Aluno);
            Dictionary<string, List<NotaModulo>> discAgrupadas = new Dictionary<string, List<NotaModulo>>();
            foreach (NotaModulo n in todasNotas)
            {
                if (discAgrupadas.ContainsKey(n.NomeDisciplina) == false) discAgrupadas.Add(n.NomeDisciplina, new List<NotaModulo>());
                discAgrupadas[n.NomeDisciplina].Add(n);
            }

            List<string> chaves = new List<string>(discAgrupadas.Keys);
            List<string> dSocio = new List<string>(); List<string> dCien = new List<string>(); List<string> dTec = new List<string>();
            foreach (string k in chaves)
            {
                string t = discAgrupadas[k][0].TipoDisciplina.ToLower();
                if (t.Contains("cultural")) dSocio.Add(k);
                else if (t.Contains("científica")) dCien.Add(k);
                else if (t.Contains("técnica")) dTec.Add(k);
            }
            dSocio.Sort(); dCien.Sort(); dTec.Sort();
            List<string> ordemFinal = new List<string>();
            ordemFinal.AddRange(dSocio); ordemFinal.AddRange(dCien); ordemFinal.AddRange(dTec);

            double notaFCT = 0; double notaPAP = 0;

            foreach (string nomeDisc in ordemFinal)
            {
                string tipo = discAgrupadas[nomeDisc][0].TipoDisciplina;
                if (tipo.ToLower().Contains("final"))
                {
                    double vF = 0;
                    if (discAgrupadas[nomeDisc][0].Valor != null) vF = (double)discAgrupadas[nomeDisc][0].Valor;
                    if (nomeDisc.ToUpper().Contains("FCT")) notaFCT = vF;
                    else if (nomeDisc.ToUpper().Contains("PAP")) notaPAP = vF;
                    continue;
                }

                BaseColor bgComp = BaseColor.WHITE;
                if (tipo.ToLower().Contains("cultural")) bgComp = corRosaCarregado;
                else if (tipo.ToLower().Contains("técnica")) bgComp = corVerdeClaroComponente;

                gridM.AddCell(new PdfPCell(new Phrase(nomeDisc, fBase)) { BackgroundColor = bgComp, Padding = 2 });

                NotaModulo[] colunas = new NotaModulo[20];
                foreach (NotaModulo m in discAgrupadas[nomeDisc])
                {
                    int nMod = ExtrairNumeroModulo(m.NomeModulo);
                    if (nMod >= 1 && nMod <= 19) colunas[nMod] = m;
                }

                for (int i = 1; i <= 19; i++)
                {
                    string v = ""; BaseColor bgCell = BaseColor.WHITE;
                    if (colunas[i] != null)
                    {
                        v = colunas[i].Valor != null ? colunas[i].Valor.ToString() : "0";
                        bgCell = GetCorAnoIntensa(colunas[i].Ano);
                    }
                    gridM.AddCell(new PdfPCell(new Phrase(v, fBase)) { BackgroundColor = bgCell, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
                }
            }

            // FCT e PAP (Linhas Manuais no Grid)
            gridM.AddCell(new PdfPCell(new Phrase("FCT", fBase)) { BackgroundColor = corFCT, Padding = 2 });
            gridM.AddCell(new PdfPCell(new Phrase(notaFCT.ToString(), fBase)) { BackgroundColor = corFCT, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            gridM.AddCell(new PdfPCell(new Phrase("", fBase)) { Border = 0, Colspan = 18 });

            gridM.AddCell(new PdfPCell(new Phrase("PAP", fBase)) { BackgroundColor = corRosaCarregado, Padding = 2 });
            gridM.AddCell(new PdfPCell(new Phrase(notaPAP.ToString(), fBase)) { BackgroundColor = corRosaCarregado, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            gridM.AddCell(new PdfPCell(new Phrase("", fBase)) { Border = 0, Colspan = 18 });

            doc.Add(gridM);

            // 3. MÉDIAS
            doc.Add(new Paragraph(" "));
            PdfPTable tabMediasMod = new PdfPTable(2);
            tabMediasMod.WidthPercentage = 30;
            tabMediasMod.HorizontalAlignment = Element.ALIGN_LEFT;

            double somaGeral = 0; int contaGeral = 0;
            foreach (NotaModulo nm in todasNotas)
            {
                if (nm.TipoDisciplina.ToLower().Contains("final") == false && nm.Valor != null && nm.Valor > 0)
                {
                    somaGeral += (double)nm.Valor;
                    contaGeral++;
                }
            }
            double mediaModulos = contaGeral > 0 ? somaGeral / (double)contaGeral : 0;

            tabMediasMod.AddCell(new PdfPCell(new Phrase("Média das Sociocultural", fBase)) { Border = 0 });
            tabMediasMod.AddCell(new PdfPCell(new Phrase(CalcularMedia(todasNotas, "cultural"), fBold)) { HorizontalAlignment = Element.ALIGN_CENTER });
            tabMediasMod.AddCell(new PdfPCell(new Phrase("Média das Científicas", fBase)) { Border = 0 });
            tabMediasMod.AddCell(new PdfPCell(new Phrase(CalcularMedia(todasNotas, "científica"), fBold)) { HorizontalAlignment = Element.ALIGN_CENTER });
            tabMediasMod.AddCell(new PdfPCell(new Phrase("Média das Técnicas", fBase)) { Border = 0 });
            tabMediasMod.AddCell(new PdfPCell(new Phrase(CalcularMedia(todasNotas, "técnica"), fBold)) { HorizontalAlignment = Element.ALIGN_CENTER });
            doc.Add(tabMediasMod);

            doc.Add(new Paragraph(" "));
            PdfPTable tabMF = new PdfPTable(2);
            tabMF.WidthPercentage = 20;
            tabMF.HorizontalAlignment = Element.ALIGN_LEFT;

            // FÓRMULA OFICIAL: Módulos 66%, FCT 11%, PAP 23%
            double mFinal = (mediaModulos * 0.66) + (notaFCT * 0.11) + (notaPAP * 0.23);

            tabMF.AddCell(new PdfPCell(new Phrase("Média final de curso", fBold)) { BackgroundColor = corFinalCurso, Padding = 4 });
            tabMF.AddCell(new PdfPCell(new Phrase(mFinal.ToString("N1"), fBold)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
            doc.Add(tabMF);

            // 4. BLOCOS DE RODAPÉ
            doc.Add(new Paragraph(" "));
            PdfPTable tabAnos = new PdfPTable(3);
            tabAnos.WidthPercentage = 100;
            tabAnos.SetWidths(new float[] { 1, 1, 1 });

            tabAnos.AddCell(CriarMiniTabelaAno(todasNotas, "1º Ano", corLaranjaEscuro, fBase, fBold));
            tabAnos.AddCell(CriarMiniTabelaAno(todasNotas, "2º Ano", corVerdeEscuro, fBase, fBold));
            tabAnos.AddCell(CriarMiniTabelaAno(todasNotas, "3º Ano", corVermelhoEscuro, fBase, fBold));

            doc.Add(tabAnos);
            doc.Close();
            return caminhoPdf;
        }

        private PdfPCell CriarMiniTabelaAno(List<NotaModulo> notas, string ano, BaseColor cHeader, Font fBase, Font fBold)
        {
            PdfPCell cell = new PdfPCell() { Border = 0, Padding = 3 };
            List<NotaModulo> notasAno = new List<NotaModulo>();
            int totalModulos = 0; int tecnicasModulos = 0;
            double somaAno = 0; int contaAno = 0;

            foreach (NotaModulo n in notas)
            {
                if (n.Ano == ano && n.TipoDisciplina.ToLower().Contains("final") == false)
                {
                    notasAno.Add(n);
                    totalModulos++;
                    if (n.TipoDisciplina.ToLower().Contains("técnica")) tecnicasModulos++;
                    if (n.Valor != null && n.Valor > 0) { somaAno += (double)n.Valor; contaAno++; }
                }
            }

            PdfPTable hR = new PdfPTable(4);
            hR.WidthPercentage = 100; hR.SetWidths(new float[] { 20, 20, 30, 30 });
            hR.AddCell(new PdfPCell(new Phrase("Total", fBase)) { BackgroundColor = cHeader, HorizontalAlignment = Element.ALIGN_CENTER });
            hR.AddCell(new PdfPCell(new Phrase(totalModulos.ToString(), fBold)) { BackgroundColor = cHeader, HorizontalAlignment = Element.ALIGN_CENTER });
            hR.AddCell(new PdfPCell(new Phrase("Técnicas", fBase)) { BackgroundColor = cHeader, HorizontalAlignment = Element.ALIGN_CENTER });
            hR.AddCell(new PdfPCell(new Phrase(tecnicasModulos.ToString(), fBold)) { BackgroundColor = cHeader, HorizontalAlignment = Element.ALIGN_CENTER });
            cell.AddElement(hR);

            PdfPTable data = new PdfPTable(4);
            data.WidthPercentage = 100; data.SpacingBefore = 2;
            data.AddCell(new PdfPCell(new Phrase(ano.ToUpper(), fBold)) { BackgroundColor = cHeader, Colspan = 4, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            string[] subH = { "MÉDIA", "MÓD NR", "SIT.", "TEC" };
            foreach (string s in subH) data.AddCell(new PdfPCell(new Phrase(s, fBold)) { BackgroundColor = corHeaderGrelha, Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });

            Dictionary<string, List<NotaModulo>> grupos = new Dictionary<string, List<NotaModulo>>();
            foreach (NotaModulo n in notasAno)
            {
                if (grupos.ContainsKey(n.NomeDisciplina) == false) grupos.Add(n.NomeDisciplina, new List<NotaModulo>());
                grupos[n.NomeDisciplina].Add(n);
            }

            List<string> chaves = new List<string>(grupos.Keys);
            for (int i = 0; i < chaves.Count - 1; i++)
            {
                for (int j = i + 1; j < chaves.Count; j++)
                {
                    if (GetPrioridadeTipo(grupos[chaves[i]][0].TipoDisciplina) > GetPrioridadeTipo(grupos[chaves[j]][0].TipoDisciplina))
                    {
                        string temp = chaves[i]; chaves[i] = chaves[j]; chaves[j] = temp;
                    }
                }
            }

            foreach (string nome in chaves)
            {
                List<NotaModulo> notasDisc = grupos[nome];
                double soma = 0; int realizado = 0; bool sc = false;
                foreach (NotaModulo v in notasDisc)
                {
                    if (v.Valor != null && v.Valor > 0) { soma += (double)v.Valor; realizado++; }
                    if (v.Valor == null || v.Valor < 10) sc = true;
                }
                double m = realizado > 0 ? soma / (double)realizado : 0;
                string s = sc ? "SC" : "C";

                BaseColor bg = BaseColor.WHITE;
                string tp = notasDisc[0].TipoDisciplina.ToLower();
                if (tp.Contains("cultural")) bg = corRosaMuitoClaro;
                else if (tp.Contains("técnica")) bg = corVerdeClaroComponente;

                data.AddCell(new PdfPCell(new Phrase(m.ToString("N1"), fBase)) { BackgroundColor = bg, Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                data.AddCell(new PdfPCell(new Phrase(realizado.ToString(), fBase)) { BackgroundColor = bg, Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                data.AddCell(new PdfPCell(new Phrase(s, fBase)) { BackgroundColor = bg, Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                data.AddCell(new PdfPCell(new Phrase("", fBase)) { BackgroundColor = bg, Padding = 2 });
            }
            cell.AddElement(data);

            double mAnual = contaAno > 0 ? somaAno / (double)contaAno : 0;
            PdfPTable tMA = new PdfPTable(2);
            tMA.WidthPercentage = 100; tMA.SpacingBefore = 5; tMA.SetWidths(new float[] { 2, 1 });
            tMA.AddCell(new PdfPCell(new Phrase("MÉDIA DO ANO", fBold)) { BackgroundColor = cHeader, Padding = 3 });
            tMA.AddCell(new PdfPCell(new Phrase(mAnual.ToString("N1"), fBold)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 3 });
            cell.AddElement(tMA);

            return cell;
        }

        private int ExtrairNumeroModulo(string nome)
        {
            string n = "";
            foreach (char c in nome) { if (char.IsDigit(c)) n += c; }
            return n != "" ? Convert.ToInt32(n) : 0;
        }

        private BaseColor GetCorAnoIntensa(string ano)
        {
            if (ano.Contains("1")) return corLaranjaMedio;
            if (ano.Contains("2")) return corVerdeMedio;
            return corVermelhoMedio;
        }

        private BaseColor GetCorComponente(string tipo)
        {
            if (tipo.ToLower().Contains("técnica")) return corVerdeClaroComponente;
            if (tipo.ToLower().Contains("científica")) return BaseColor.WHITE;
            return corRosaCarregado;
        }

        private int GetPrioridadeTipo(string tipo)
        {
            if (tipo.ToLower().Contains("cultural")) return 1;
            if (tipo.ToLower().Contains("científica")) return 2;
            if (tipo.ToLower().Contains("técnica")) return 3;
            return 4;
        }

        private string CalcularMedia(List<NotaModulo> notas, string tipo)
        {
            double s = 0; int c = 0;
            foreach (NotaModulo n in notas)
            {
                if (n.TipoDisciplina.ToLower().Contains(tipo.ToLower()) && n.Valor != null && n.Valor > 0) { s += (double)n.Valor; c++; }
            }
            return c > 0 ? (s / (double)c).ToString("N1") : "0,0";
        }

        private List<NotaModulo> ObterTodasNotas(int codAluno)
        {
            List<NotaModulo> l = new List<NotaModulo>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT n.Cod_NotaMod, d.Ano as AnoDisciplina, d.Designacao as Disciplina, d.Tipo, m.Designacao as Modulo, m.Cod_Modulo, n.Valor FROM NotaMod n INNER JOIN Modulos m ON n.Cod_Modulo = m.Cod_Modulo INNER JOIN Disciplina d ON m.Cod_Disc = d.Cod_Disc WHERE n.Cod_Aluno = @Aluno ORDER BY d.Tipo, d.Designacao, m.Cod_Modulo";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Aluno", codAluno);
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            int? v = r["Valor"] != DBNull.Value ? (int?)Convert.ToInt32(r["Valor"]) : null;
                            string a = r["AnoDisciplina"].ToString() + "º Ano";
                            l.Add(new NotaModulo(Convert.ToInt32(r["Cod_NotaMod"]), codAluno, Convert.ToInt32(r["Cod_Modulo"]), v, null, a, r["Modulo"].ToString(), r["Disciplina"].ToString(), r["Tipo"].ToString()));
                        }
                    }
                }
            }
            return l;
        }
    }
}
