using iTextSharp.text;
using iTextSharp.text.pdf;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DCGest.Classes;

namespace DCGest.Classes
{
    public class GeradorPDF
    {
        private string connectionString = BD.CaminhoBD;

        // CORES COMPONENTES
        BaseColor corSociocultural = new BaseColor(242, 220, 219); // Rosa
        BaseColor corCientifica = new BaseColor(216, 228, 188);    // Verde
        BaseColor corTecnica = new BaseColor(250, 191, 143);       // Laranja
        BaseColor corFCT = new BaseColor(183, 222, 232);          // Azul
        BaseColor corPAP = new BaseColor(242, 220, 219);          // Rosa

        // CORES ANOS
        BaseColor corFundoAno1 = new BaseColor(252, 213, 180);    
        BaseColor corFundoAno2 = new BaseColor(216, 228, 188);    
        BaseColor corFundoAno3 = new BaseColor(230, 184, 183);    

        // CORES CABEÇALHOS ANOS
        BaseColor corHeaderAno1 = new BaseColor(228, 108, 10);    
        BaseColor corHeaderAno2 = new BaseColor(118, 147, 60);    
        BaseColor corHeaderAno3 = new BaseColor(148, 54, 52);     

        public string GerarRelatorioAluno(Aluno aluno)
        {
            string pastaTemp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Relatorios");
            if (!Directory.Exists(pastaTemp)) Directory.CreateDirectory(pastaTemp);
            string caminhoPdf = Path.Combine(pastaTemp, $"Relatorio_{aluno.Cod_Aluno}.pdf");

            // Margens menores para caber tudo na mesma página
            Document doc = new Document(PageSize.A4.Rotate(), 20, 20, 15, 15);
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(caminhoPdf, FileMode.Create));
            doc.Open();

            Font fBase = FontFactory.GetFont(FontFactory.HELVETICA, 7, BaseColor.BLACK);
            Font fBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7, BaseColor.BLACK);
            Font fTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);

            // 1. CABEÇALHO COMPLETO
            PdfPTable tabHeader = new PdfPTable(2);
            tabHeader.WidthPercentage = 100;
            tabHeader.SetWidths(new float[] { 1, 1 });

            PdfPCell cellTitle = new PdfPCell(new Phrase("REGISTO BIOGRÁFICO DE AVALIAÇÃO", fTitle));
            cellTitle.Colspan = 2; cellTitle.Border = 0; cellTitle.PaddingBottom = 10; cellTitle.HorizontalAlignment = Element.ALIGN_CENTER;
            tabHeader.AddCell(cellTitle);

            PdfPCell cellEsq = new PdfPCell(); cellEsq.Border = 0;
            cellEsq.AddElement(new Phrase("ALUNO: " + aluno.Nome_Aluno.ToUpper(), fBold));
            cellEsq.AddElement(new Phrase("Nº PROCESSO: " + aluno.Cod_Aluno, fBase));
            cellEsq.AddElement(new Phrase("CURSO: " + aluno.Nome_Curso, fBase));
            tabHeader.AddCell(cellEsq);

            PdfPCell cellDir = new PdfPCell(); cellDir.Border = 0; cellDir.HorizontalAlignment = Element.ALIGN_RIGHT;
            Paragraph pDir = new Paragraph();
            pDir.Alignment = Element.ALIGN_RIGHT;
            pDir.Add(new Phrase("TURMA: " + aluno.Turma + "\n", fBase));
            pDir.Add(new Phrase("ANO LETIVO: " + aluno.Ano_Letivo + "\n", fBase));
            pDir.Add(new Phrase("ORIENTADOR: " + aluno.Nome_Orientador, fBase));
            cellDir.AddElement(pDir);
            tabHeader.AddCell(cellDir);

            doc.Add(tabHeader);

            // 2. GRELHA DE MÓDULOS (Sem ID, 20 Colunas)
            PdfPTable gridM = new PdfPTable(20);
            gridM.WidthPercentage = 100;
            float[] wGrid = new float[20];
            wGrid[0] = 5f; // Disciplinas
            for (int i = 1; i < 20; i++) wGrid[i] = 1f;
            gridM.SetWidths(wGrid);

            BaseColor cGray = new BaseColor(211, 211, 211);
            gridM.AddCell(new PdfPCell(new Phrase("DISCIPLINAS", fBold)) { BackgroundColor = cGray, Padding = 2 });
            for (int i = 1; i <= 19; i++) gridM.AddCell(new PdfPCell(new Phrase("M" + i, fBold)) { BackgroundColor = cGray, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });

            var todasNotas = ObterTodasNotas(aluno.Cod_Aluno);

            // ORDEM: SOCIOCULTURAL (1), CIENTIFICA (2), TECNICA (3)
            var gruposComponentes = todasNotas.GroupBy(n => n.NomeDisciplina).OrderBy(g => GetPrioridadeTipo(g.First().TipoDisciplina)).ThenBy(g => g.Key);

            foreach (var disc in gruposComponentes)
            {
                gridM.AddCell(new PdfPCell(new Phrase(disc.Key, fBase)) { BackgroundColor = GetCorComponente(disc.First().TipoDisciplina), Padding = 2 });

                var mods = disc.OrderBy(m => m.Cod_Modulo).ToList();
                for (int i = 0; i < 19; i++)
                {
                    string v = "";
                    BaseColor bgCell = BaseColor.WHITE;
                    if (i < mods.Count)
                    {
                        v = mods[i].Valor?.ToString() ?? "0";
                        bgCell = GetCorFundoAno(mods[i].Ano);
                    }
                    gridM.AddCell(new PdfPCell(new Phrase(v, fBase)) { BackgroundColor = bgCell, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
                }
            }

            // FCT e PAP
            gridM.AddCell(new PdfPCell(new Phrase("FCT", fBase)) { BackgroundColor = corFCT, Padding = 2 });
            gridM.AddCell(new PdfPCell(new Phrase("0", fBase)) { BackgroundColor = corFCT, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            for (int i = 0; i < 18; i++) gridM.AddCell(new PdfPCell(new Phrase("", fBase)) { Border = 0 });

            gridM.AddCell(new PdfPCell(new Phrase("PAP", fBase)) { BackgroundColor = corPAP, Padding = 2 });
            gridM.AddCell(new PdfPCell(new Phrase("0", fBase)) { BackgroundColor = corPAP, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            for (int i = 0; i < 18; i++) gridM.AddCell(new PdfPCell(new Phrase("", fBase)) { Border = 0 });

            doc.Add(gridM);

            // 3. MÉDIAS E FINAL (Compacto)
            PdfPTable rowMedias = new PdfPTable(2);
            rowMedias.WidthPercentage = 100;
            rowMedias.SetWidths(new float[] { 1, 3 });

            // Bloco Médias Modalidades
            PdfPTable tabMediasMod = new PdfPTable(2);
            tabMediasMod.AddCell(new PdfPCell(new Phrase("Média das Sociocultural", fBase)) { Border = 0 });
            tabMediasMod.AddCell(new PdfPCell(new Phrase(CalcularMedia(todasNotas, "sociocultural"), fBold)) { HorizontalAlignment = Element.ALIGN_CENTER });
            tabMediasMod.AddCell(new PdfPCell(new Phrase("Média das Científicas", fBase)) { Border = 0 });
            tabMediasMod.AddCell(new PdfPCell(new Phrase(CalcularMedia(todasNotas, "científica"), fBold)) { HorizontalAlignment = Element.ALIGN_CENTER });
            tabMediasMod.AddCell(new PdfPCell(new Phrase("Média das Técnicas", fBase)) { Border = 0 });
            tabMediasMod.AddCell(new PdfPCell(new Phrase(CalcularMedia(todasNotas, "técnica"), fBold)) { HorizontalAlignment = Element.ALIGN_CENTER });

            PdfPCell cellMedias = new PdfPCell(tabMediasMod) { Border = 0, VerticalAlignment = Element.ALIGN_BOTTOM };
            rowMedias.AddCell(cellMedias);

            // Bloco Média Final
            PdfPTable tabMF = new PdfPTable(2);
            tabMF.WidthPercentage = 50;
            tabMF.HorizontalAlignment = Element.ALIGN_RIGHT;
            tabMF.AddCell(new PdfPCell(new Phrase("Média final de curso", fBold)) { BackgroundColor = new BaseColor(255, 255, 0), Padding = 3 });
            tabMF.AddCell(new PdfPCell(new Phrase("---", fBold)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 3 });
            
            PdfPCell cellMF = new PdfPCell(tabMF) { Border = 0, VerticalAlignment = Element.ALIGN_BOTTOM, HorizontalAlignment = Element.ALIGN_RIGHT };
            rowMedias.AddCell(cellMF);

            doc.Add(new Paragraph(" "));
            doc.Add(rowMedias);

            // 4. BLOCOS DE RODAPÉ
            doc.Add(new Paragraph(" "));
            PdfPTable tabAnos = new PdfPTable(3);
            tabAnos.WidthPercentage = 100;
            tabAnos.SetWidths(new float[] { 1, 1, 1 });

            tabAnos.AddCell(CriarMiniTabelaAno(todasNotas, "1º Ano", corHeaderAno1, fBase, fBold));
            tabAnos.AddCell(CriarMiniTabelaAno(todasNotas, "2º Ano", corHeaderAno2, fBase, fBold));
            tabAnos.AddCell(CriarMiniTabelaAno(todasNotas, "3º Ano", corHeaderAno3, fBase, fBold));
            
            doc.Add(tabAnos);
            doc.Close();
            return caminhoPdf;
        }

        private PdfPCell CriarMiniTabelaAno(List<NotaModulo> notas, string ano, BaseColor cHeader, Font fBase, Font fBold)
        {
            PdfPCell cell = new PdfPCell() { Border = 0, Padding = 3 };
            var notasAno = notas.Where(n => n.Ano == ano).ToList();

            PdfPTable head = new PdfPTable(2);
            head.AddCell(new PdfPCell(new Phrase("Total", fBase)) { BackgroundColor = corSociocultural, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            head.AddCell(new PdfPCell(new Phrase("Técnicas", fBase)) { BackgroundColor = corSociocultural, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            head.AddCell(new PdfPCell(new Phrase(notasAno.Count(n => n.Valor > 0).ToString(), fBold)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            head.AddCell(new PdfPCell(new Phrase(notasAno.Count(n => n.TipoDisciplina.ToLower().Contains("técnica") && n.Valor > 0).ToString(), fBold)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            cell.AddElement(head);

            PdfPTable data = new PdfPTable(4);
            data.WidthPercentage = 100;
            data.AddCell(new PdfPCell(new Phrase(ano, fBold)) { BackgroundColor = cHeader, Colspan = 4, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            
            string[] subH = { "MÉDIA", "MÓD NR", "SIT.", "TEC" };
            foreach(var s in subH) data.AddCell(new PdfPCell(new Phrase(s, fBold)) { BackgroundColor = new BaseColor(230, 230, 230), Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });

            var grupos = notasAno.GroupBy(n => n.NomeDisciplina).OrderBy(g => GetPrioridadeTipo(g.First().TipoDisciplina));
            foreach (var g in grupos)
            {
                var val = g.Where(v => v.Valor > 0).ToList();
                double m = val.Any() ? val.Average(v => (double)v.Valor!) : 0;
                string sit = g.All(v => v.Valor >= 10) ? "C" : "SC";
                BaseColor bg = GetCorComponente(g.First().TipoDisciplina);

                data.AddCell(new PdfPCell(new Phrase(m.ToString("N1"), fBase)) { BackgroundColor = bg, Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                data.AddCell(new PdfPCell(new Phrase(val.Count.ToString(), fBase)) { BackgroundColor = bg, Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                data.AddCell(new PdfPCell(new Phrase(sit, fBase)) { BackgroundColor = bg, Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                data.AddCell(new PdfPCell(new Phrase("", fBase)) { BackgroundColor = bg, Padding = 2 });
            }
            cell.AddElement(data);
            return cell;
        }

        private BaseColor GetCorComponente(string tipo)
        {
            if (tipo.ToLower().Contains("técnica")) return corTecnica;
            if (tipo.ToLower().Contains("científica")) return corCientifica;
            return corSociocultural;
        }

        private BaseColor GetCorFundoAno(string ano)
        {
            if (ano == "1º Ano") return corFundoAno1;
            if (ano == "2º Ano") return corFundoAno2;
            return corFundoAno3;
        }

        private int GetPrioridadeTipo(string tipo)
        {
            if (tipo.ToLower().Contains("sociocultural")) return 1;
            if (tipo.ToLower().Contains("científica")) return 2;
            if (tipo.ToLower().Contains("técnica")) return 3;
            return 4;
        }

        private string CalcularMedia(List<NotaModulo> notas, string tipo)
        {
            var f = notas.Where(n => n.TipoDisciplina.ToLower().Contains(tipo) && n.Valor != null && n.Valor >= 0).ToList();
            return f.Any() ? f.Average(n => (double)n.Valor!).ToString("N1") : "0,0";
        }

        private List<NotaModulo> ObterTodasNotas(int codAluno)
        {
            List<NotaModulo> lista = new List<NotaModulo>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT n.Cod_NotaMod, n.Ano, d.Designacao as Disciplina, d.Tipo, m.Designacao as Modulo, m.Cod_Modulo, n.Valor
                    FROM NotaMod n
                    INNER JOIN Modulos m ON n.Cod_Modulo = m.Cod_Modulo
                    INNER JOIN Disciplina d ON m.Cod_Disc = d.Cod_Disc
                    WHERE n.Cod_Aluno = @Aluno
                    ORDER BY d.Tipo, d.Designacao, m.Cod_Modulo";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Aluno", codAluno);
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            lista.Add(new NotaModulo(
                                Convert.ToInt32(r["Cod_NotaMod"]),
                                codAluno,
                                Convert.ToInt32(r["Cod_Modulo"]),
                                r["Valor"] == DBNull.Value ? null : (int?)Convert.ToInt32(r["Valor"]),
                                null,
                                r["Ano"].ToString(),
                                r["Modulo"].ToString(),
                                r["Disciplina"].ToString(),
                                r["Tipo"].ToString()
                            ));
                        }
                    }
                }
            }
            return lista;
        }
    }
}
