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

        // PALETE DE CORES SINCRONIZADA (3 níveis de intensidade)

        // TEMA LARANJA (Ano 1 / Técnica)
        BaseColor corLaranjaClaro = new BaseColor(255, 235, 210);  // Células Grid
        BaseColor corLaranjaMedio = new BaseColor(250, 191, 143);  // Nomes Disciplinas / Linhas Resumo
        BaseColor corLaranjaEscuro = new BaseColor(228, 108, 10);  // Header Rodapé

        // TEMA VERDE (Ano 2 / Científica)
        BaseColor corVerdeClaro = new BaseColor(230, 255, 230);    // Células Grid
        BaseColor corVerdeMedio = new BaseColor(200, 230, 180);    // Nomes Disciplinas / Linhas Resumo
        BaseColor corVerdeEscuro = new BaseColor(118, 147, 60);    // Header Rodapé

        // TEMA ROSA (Ano 3 / Sociocultural)
        BaseColor corRosaClaro = new BaseColor(255, 235, 235);     // Células Grid
        BaseColor corRosaMedio = new BaseColor(242, 220, 219);     // Nomes Disciplinas / Linhas Resumo
        BaseColor corRosaEscuro = new BaseColor(148, 54, 52);      // Header Rodapé

        // CORES ESPECIAIS
        BaseColor corFCT = new BaseColor(183, 222, 232);          // Azul
        BaseColor corHeaderGrelha = new BaseColor(211, 211, 211); // Cinza
        BaseColor corFinalCurso = new BaseColor(255, 255, 0);     // Amarelo

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

            // 1. CABEÇALHO
            PdfPTable tabHeader = new PdfPTable(2);
            tabHeader.WidthPercentage = 100;
            tabHeader.SetWidths(new float[] { 1, 1 });

            PdfPCell cellTitle = new PdfPCell(new Phrase("REGISTO BIOGRÁFICO DE AVALIAÇÃO", fTitle));
            cellTitle.Colspan = 2; cellTitle.Border = 0; cellTitle.PaddingBottom = 10; cellTitle.HorizontalAlignment = Element.ALIGN_CENTER;
            tabHeader.AddCell(cellTitle);

            PdfPCell cellEsq = new PdfPCell(); cellEsq.Border = 0;
            cellEsq.AddElement(new Phrase("ALUNO: " + alunoCompleto.Nome_Aluno.ToUpper(), fBold));
            cellEsq.AddElement(new Phrase("Nº PROCESSO: " + alunoCompleto.Cod_Aluno, fBase));
            cellEsq.AddElement(new Phrase("CURSO: " + alunoCompleto.Nome_Curso, fBase));
            tabHeader.AddCell(cellEsq);

            PdfPCell cellDir = new PdfPCell(); cellDir.Border = 0; cellDir.HorizontalAlignment = Element.ALIGN_RIGHT;
            Paragraph pDir = new Paragraph();
            pDir.Alignment = Element.ALIGN_RIGHT;
            pDir.Add(new Phrase("TURMA: " + alunoCompleto.Nome_Turma + "\n", fBase));
            pDir.Add(new Phrase("ANO LETIVO: " + alunoCompleto.Intervalo_Letivo + "\n", fBase));
            pDir.Add(new Phrase("ORIENTADOR: " + alunoCompleto.Nome_Orientador, fBase));
            cellDir.AddElement(pDir);
            tabHeader.AddCell(cellDir);

            doc.Add(tabHeader);

            // 2. GRELHA DE MÓDULOS
            PdfPTable gridM = new PdfPTable(20);
            gridM.WidthPercentage = 100;
            float[] wGrid = new float[20];
            wGrid[0] = 5f;
            for (int i = 1; i < 20; i++) wGrid[i] = 1f;
            gridM.SetWidths(wGrid);

            gridM.AddCell(new PdfPCell(new Phrase("DISCIPLINAS", fBold)) { BackgroundColor = corHeaderGrelha, Padding = 2 });
            for (int i = 1; i <= 19; i++) gridM.AddCell(new PdfPCell(new Phrase("M" + i, fBold)) { BackgroundColor = corHeaderGrelha, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });

            List<NotaModulo> todasNotas = ObterTodasNotas(aluno.Cod_Aluno);

            Dictionary<string, List<NotaModulo>> discAgrupadas = new Dictionary<string, List<NotaModulo>>();
            foreach (NotaModulo n in todasNotas)
            {
                if (discAgrupadas.ContainsKey(n.NomeDisciplina) == false)
                {
                    discAgrupadas.Add(n.NomeDisciplina, new List<NotaModulo>());
                }
                discAgrupadas[n.NomeDisciplina].Add(n);
            }

            List<string> nomesDisciplinas = new List<string>(discAgrupadas.Keys);
            List<string> discSocioculturais = new List<string>();
            List<string> discCientificas = new List<string>();
            List<string> discTecnicas = new List<string>();

            foreach (string nomeDisc in nomesDisciplinas)
            {
                string tipo = discAgrupadas[nomeDisc][0].TipoDisciplina.ToLower();
                if (tipo.Contains("cultural")) discSocioculturais.Add(nomeDisc);
                else if (tipo.Contains("científica")) discCientificas.Add(nomeDisc);
                else if (tipo.Contains("técnica")) discTecnicas.Add(nomeDisc);
            }

            discSocioculturais.Sort(); discCientificas.Sort(); discTecnicas.Sort();
            List<string> ordemFinal = new List<string>();
            ordemFinal.AddRange(discSocioculturais); ordemFinal.AddRange(discCientificas); ordemFinal.AddRange(discTecnicas);

            foreach (string nomeDisc in ordemFinal)
            {
                string tipo = discAgrupadas[nomeDisc][0].TipoDisciplina;
                gridM.AddCell(new PdfPCell(new Phrase(nomeDisc, fBase)) { BackgroundColor = GetCorComponente(tipo, "medio"), Padding = 2 });

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
                        bgCell = GetCorAnoIntensa(colunas[i].Ano, "claro");
                    }
                    gridM.AddCell(new PdfPCell(new Phrase(v, fBase)) { BackgroundColor = bgCell, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
                }
            }

            // FCT e PAP
            gridM.AddCell(new PdfPCell(new Phrase("FCT", fBase)) { BackgroundColor = corFCT, Padding = 2 });
            gridM.AddCell(new PdfPCell(new Phrase("0", fBase)) { BackgroundColor = corFCT, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            gridM.AddCell(new PdfPCell(new Phrase("", fBase)) { Border = 0, Colspan = 18 });

            gridM.AddCell(new PdfPCell(new Phrase("PAP", fBase)) { BackgroundColor = corRosaMedio, Padding = 2 });
            gridM.AddCell(new PdfPCell(new Phrase("0", fBase)) { BackgroundColor = corRosaMedio, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            gridM.AddCell(new PdfPCell(new Phrase("", fBase)) { Border = 0, Colspan = 18 });

            doc.Add(gridM);

            // 3. MÉDIAS
            doc.Add(new Paragraph(" "));
            PdfPTable tabMediasMod = new PdfPTable(2);
            tabMediasMod.WidthPercentage = 30;
            tabMediasMod.HorizontalAlignment = Element.ALIGN_LEFT;
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
            tabMF.AddCell(new PdfPCell(new Phrase("Média final de curso", fBold)) { BackgroundColor = corFinalCurso, Padding = 4 });
            tabMF.AddCell(new PdfPCell(new Phrase("---", fBold)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
            doc.Add(tabMF);

            // 4. BLOCOS DE RODAPÉ
            doc.Add(new Paragraph(" "));
            PdfPTable tabAnos = new PdfPTable(3);
            tabAnos.WidthPercentage = 100;
            tabAnos.SetWidths(new float[] { 1, 1, 1 });

            tabAnos.AddCell(CriarMiniTabelaAno(todasNotas, "1º Ano", corLaranjaEscuro, fBase, fBold));
            tabAnos.AddCell(CriarMiniTabelaAno(todasNotas, "2º Ano", corVerdeEscuro, fBase, fBold));
            tabAnos.AddCell(CriarMiniTabelaAno(todasNotas, "3º Ano", corRosaEscuro, fBase, fBold));

            doc.Add(tabAnos);
            doc.Close();
            return caminhoPdf;
        }

        private PdfPCell CriarMiniTabelaAno(List<NotaModulo> notas, string ano, BaseColor cHeader, Font fBase, Font fBold)
        {
            PdfPCell cell = new PdfPCell() { Border = 0, Padding = 3 };
            List<NotaModulo> notasAno = new List<NotaModulo>();
            int totalModulos = 0;
            int tecnicasModulos = 0;

            foreach (NotaModulo n in notas)
            {
                if (n.Ano == ano)
                {
                    if (n.TipoDisciplina.ToLower().Contains("final") == false)
                    {
                        notasAno.Add(n);
                        totalModulos = totalModulos + 1;
                        if (n.TipoDisciplina.ToLower().Contains("técnica")) tecnicasModulos = tecnicasModulos + 1;
                    }
                }
            }

            PdfPTable headRow = new PdfPTable(4);
            headRow.WidthPercentage = 100;
            headRow.SetWidths(new float[] { 20, 20, 30, 30 });
            headRow.AddCell(new PdfPCell(new Phrase("Total", fBase)) { BackgroundColor = cHeader, HorizontalAlignment = Element.ALIGN_CENTER });
            headRow.AddCell(new PdfPCell(new Phrase(totalModulos.ToString(), fBold)) { BackgroundColor = cHeader, HorizontalAlignment = Element.ALIGN_CENTER });
            headRow.AddCell(new PdfPCell(new Phrase("Técnicas", fBase)) { BackgroundColor = cHeader, HorizontalAlignment = Element.ALIGN_CENTER });
            headRow.AddCell(new PdfPCell(new Phrase(tecnicasModulos.ToString(), fBold)) { BackgroundColor = cHeader, HorizontalAlignment = Element.ALIGN_CENTER });
            cell.AddElement(headRow);

            PdfPTable data = new PdfPTable(4);
            data.WidthPercentage = 100;
            data.SpacingBefore = 2;
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
                double soma = 0; int countRealizados = 0; bool incompleto = false;
                foreach (NotaModulo v in notasDisc)
                {
                    if (v.Valor != null && v.Valor > 0) { soma = soma + (double)v.Valor; countRealizados = countRealizados + 1; }
                    if (v.Valor == null || v.Valor < 10) incompleto = true;
                }
                double media = countRealizados > 0 ? soma / (double)countRealizados : 0;
                string situacao = incompleto ? "SC" : "C";

                BaseColor bgCor = GetCorComponente(notasDisc[0].TipoDisciplina, "medio");
                data.AddCell(new PdfPCell(new Phrase(media.ToString("N1"), fBase)) { BackgroundColor = bgCor, Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                data.AddCell(new PdfPCell(new Phrase(countRealizados.ToString(), fBase)) { BackgroundColor = bgCor, Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                data.AddCell(new PdfPCell(new Phrase(situacao, fBase)) { BackgroundColor = bgCor, Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                data.AddCell(new PdfPCell(new Phrase("", fBase)) { BackgroundColor = bgCor, Padding = 2 });
            }
            cell.AddElement(data);
            return cell;
        }

        private int ExtrairNumeroModulo(string nome)
        {
            string num = "";
            foreach (char c in nome) { if (char.IsDigit(c)) num = num + c; }
            return num != "" ? Convert.ToInt32(num) : 0;
        }

        private BaseColor GetCorComponente(string tipo, string intensidade)
        {
            if (tipo.ToLower().Contains("técnica"))
            {
                if (intensidade == "claro") return corLaranjaClaro;
                return corLaranjaEscuro;
            }
            if (tipo.ToLower().Contains("científica"))
            {
                if (intensidade == "claro") return corVerdeClaro;
                return corVerdeEscuro;
            }
            if (intensidade == "claro") return corRosaClaro;
            return corRosaEscuro;
        }

        private BaseColor GetCorAnoIntensa(string ano, string intensidade)
        {
            if (ano.Contains("1"))
            {
                if (intensidade == "claro") return corLaranjaClaro;
                return corLaranjaEscuro;
            }
            if (ano.Contains("2"))
            {
                if (intensidade == "claro") return corVerdeClaro;
                return corVerdeEscuro;
            }
            if (intensidade == "claro") return corRosaClaro;
            return corRosaEscuro;
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
            double soma = 0; int conta = 0;
            foreach (NotaModulo n in notas)
            {
                if (n.TipoDisciplina.ToLower().Contains(tipo.ToLower()))
                {
                    if (n.Valor != null && n.Valor > 0) { soma = soma + (double)n.Valor; conta = conta + 1; }
                }
            }
            return conta > 0 ? (soma / (double)conta).ToString("N1") : "0,0";
        }

        private List<NotaModulo> ObterTodasNotas(int codAluno)
        {
            List<NotaModulo> lista = new List<NotaModulo>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT n.Cod_NotaMod, n.Ano, d.Designacao as Disciplina, d.Tipo, m.Designacao as Modulo, m.Cod_Modulo, n.Valor FROM NotaMod n INNER JOIN Modulos m ON n.Cod_Modulo = m.Cod_Modulo INNER JOIN Disciplina d ON m.Cod_Disc = d.Cod_Disc WHERE n.Cod_Aluno = @Aluno ORDER BY d.Tipo, d.Designacao, m.Cod_Modulo";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Aluno", codAluno);
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            int? val = r["Valor"] != DBNull.Value ? (int?)Convert.ToInt32(r["Valor"]) : null;
                            lista.Add(new NotaModulo(Convert.ToInt32(r["Cod_NotaMod"]), codAluno, Convert.ToInt32(r["Cod_Modulo"]), val, null, r["Ano"].ToString(), r["Modulo"].ToString(), r["Disciplina"].ToString(), r["Tipo"].ToString()));
                        }
                    }
                }
            }
            return lista;
        }
    }
}
