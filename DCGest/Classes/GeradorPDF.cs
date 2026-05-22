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

        // CORES COMPONENTES
        BaseColor corSociocultural = new BaseColor(242, 220, 219); // Rosa
        BaseColor corCientifica = new BaseColor(216, 228, 188);    // Verde
        BaseColor corTecnica = new BaseColor(250, 191, 143);       // Laranja
        BaseColor corFCT = new BaseColor(183, 222, 232);          // Azul
        BaseColor corPAP = new BaseColor(242, 220, 219);          // Rosa
        BaseColor corFinalCurso = new BaseColor(255, 255, 0);     // Amarelo

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

            // 1. CABEÇALHO COMPLETO
            PdfPTable tabHeader = new PdfPTable(2);
            tabHeader.WidthPercentage = 100;
            tabHeader.SetWidths(new float[] { 1, 1 });

            PdfPCell cellTitle = new PdfPCell(new Phrase("REGISTO BIOGRÁFICO DE AVALIAÇÃO", fTitle));
            cellTitle.Colspan = 2; 
            cellTitle.Border = 0; 
            cellTitle.PaddingBottom = 10; 
            cellTitle.HorizontalAlignment = Element.ALIGN_CENTER;
            tabHeader.AddCell(cellTitle);

            PdfPCell cellEsq = new PdfPCell(); 
            cellEsq.Border = 0;
            cellEsq.AddElement(new Phrase("ALUNO: " + alunoCompleto.Nome_Aluno.ToUpper(), fBold));
            cellEsq.AddElement(new Phrase("Nº PROCESSO: " + alunoCompleto.Cod_Aluno, fBase));
            cellEsq.AddElement(new Phrase("CURSO: " + alunoCompleto.Nome_Curso, fBase));
            tabHeader.AddCell(cellEsq);

            PdfPCell cellDir = new PdfPCell(); 
            cellDir.Border = 0; 
            cellDir.HorizontalAlignment = Element.ALIGN_RIGHT;
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
            for (int i = 1; i < 20; i++) 
            {
                wGrid[i] = 1f;
            }
            gridM.SetWidths(wGrid);

            BaseColor cGray = new BaseColor(211, 211, 211);
            gridM.AddCell(new PdfPCell(new Phrase("DISCIPLINAS", fBold)) { BackgroundColor = cGray, Padding = 2 });
            for (int i = 1; i <= 19; i++) 
            {
                gridM.AddCell(new PdfPCell(new Phrase("M" + i, fBold)) { BackgroundColor = cGray, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            }

            List<NotaModulo> todasNotas = ObterTodasNotas(aluno.Cod_Aluno);

            // Agrupar manualmente por Disciplina
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
                // Correção: DB usa "Sócio Cultural"
                if (tipo.Contains("cultural"))
                {
                    discSocioculturais.Add(nomeDisc);
                }
                else if (tipo.Contains("científica"))
                {
                    discCientificas.Add(nomeDisc);
                }
                else if (tipo.Contains("técnica"))
                {
                    discTecnicas.Add(nomeDisc);
                }
            }

            discSocioculturais.Sort();
            discCientificas.Sort();
            discTecnicas.Sort();

            List<string> ordemFinal = new List<string>();
            ordemFinal.AddRange(discSocioculturais);
            ordemFinal.AddRange(discCientificas);
            ordemFinal.AddRange(discTecnicas);

            foreach (string nomeDisc in ordemFinal)
            {
                List<NotaModulo> mods = discAgrupadas[nomeDisc];
                string tipo = mods[0].TipoDisciplina;
                
                gridM.AddCell(new PdfPCell(new Phrase(nomeDisc, fBase)) { BackgroundColor = GetCorComponente(tipo), Padding = 2 });

                for (int i = 0; i < 19; i++)
                {
                    string valorNota = "";
                    BaseColor corFundoCelula = BaseColor.WHITE;
                    
                    if (i < mods.Count)
                    {
                        if (mods[i].Valor != null)
                        {
                            valorNota = mods[i].Valor.ToString();
                        }
                        else
                        {
                            valorNota = "0";
                        }
                        corFundoCelula = GetCorFundoAno(mods[i].Ano);
                    }
                    
                    gridM.AddCell(new PdfPCell(new Phrase(valorNota, fBase)) { BackgroundColor = corFundoCelula, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
                }
            }

            // FCT e PAP
            gridM.AddCell(new PdfPCell(new Phrase("FCT", fBase)) { BackgroundColor = corFCT, Padding = 2 });
            gridM.AddCell(new PdfPCell(new Phrase("0", fBase)) { BackgroundColor = corFCT, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            for (int i = 0; i < 18; i++) 
            {
                gridM.AddCell(new PdfPCell(new Phrase("", fBase)) { Border = 0 });
            }

            gridM.AddCell(new PdfPCell(new Phrase("PAP", fBase)) { BackgroundColor = corPAP, Padding = 2 });
            gridM.AddCell(new PdfPCell(new Phrase("0", fBase)) { BackgroundColor = corPAP, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            for (int i = 0; i < 18; i++) 
            {
                gridM.AddCell(new PdfPCell(new Phrase("", fBase)) { Border = 0 });
            }

            doc.Add(gridM);

            // 3. MÉDIAS (SOCIOCULTURAL, CIENTÍFICA, TÉCNICA)
            doc.Add(new Paragraph(" "));
            PdfPTable tabMediasMod = new PdfPTable(2);
            tabMediasMod.WidthPercentage = 25;
            tabMediasMod.HorizontalAlignment = Element.ALIGN_LEFT;
            
            tabMediasMod.AddCell(new PdfPCell(new Phrase("Média das Sociocultural", fBase)) { Border = 0 });
            tabMediasMod.AddCell(new PdfPCell(new Phrase(CalcularMedia(todasNotas, "cultural"), fBold)) { HorizontalAlignment = Element.ALIGN_CENTER });
            tabMediasMod.AddCell(new PdfPCell(new Phrase("Média das Científicas", fBase)) { Border = 0 });
            tabMediasMod.AddCell(new PdfPCell(new Phrase(CalcularMedia(todasNotas, "científica"), fBold)) { HorizontalAlignment = Element.ALIGN_CENTER });
            tabMediasMod.AddCell(new PdfPCell(new Phrase("Média das Técnicas", fBase)) { Border = 0 });
            tabMediasMod.AddCell(new PdfPCell(new Phrase(CalcularMedia(todasNotas, "técnica"), fBold)) { HorizontalAlignment = Element.ALIGN_CENTER });
            
            doc.Add(tabMediasMod);

            // 4. MÉDIA FINAL (Abaixo das médias)
            doc.Add(new Paragraph(" "));
            PdfPTable tabMF = new PdfPTable(2);
            tabMF.WidthPercentage = 20;
            tabMF.HorizontalAlignment = Element.ALIGN_LEFT;
            tabMF.AddCell(new PdfPCell(new Phrase("Média final de curso", fBold)) { BackgroundColor = corFinalCurso, Padding = 4 });
            tabMF.AddCell(new PdfPCell(new Phrase("---", fBold)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
            doc.Add(tabMF);

            // 5. BLOCOS DE RODAPÉ (Resumo Anual)
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
            
            List<NotaModulo> notasAno = new List<NotaModulo>();
            int totalNR = 0;
            int tecnicasNR = 0;

            foreach (NotaModulo n in notas)
            {
                if (n.Ano == ano)
                {
                    notasAno.Add(n);
                    if (n.Valor != null && n.Valor >= 0)
                    {
                        totalNR = totalNR + 1;
                        if (n.TipoDisciplina.ToLower().Contains("técnica"))
                        {
                            tecnicasNR = tecnicasNR + 1;
                        }
                    }
                }
            }

            // Cabeçalho: Total e Técnicas lado a lado
            PdfPTable head = new PdfPTable(2);
            head.WidthPercentage = 100;
            head.AddCell(new PdfPCell(new Phrase("Total", fBase)) { BackgroundColor = cHeader, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            head.AddCell(new PdfPCell(new Phrase("Técnicas", fBase)) { BackgroundColor = cHeader, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            head.AddCell(new PdfPCell(new Phrase(totalNR.ToString(), fBold)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            head.AddCell(new PdfPCell(new Phrase(tecnicasNR.ToString(), fBold)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            cell.AddElement(head);

            // Dados da tabela anual
            PdfPTable data = new PdfPTable(4);
            data.WidthPercentage = 100;
            data.SpacingBefore = 2;
            data.AddCell(new PdfPCell(new Phrase(ano, fBold)) { BackgroundColor = cHeader, Colspan = 4, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            
            string[] subH = { "MÉDIA", "MÓD NR", "SIT.", "TEC" };
            foreach(string s in subH) 
            {
                data.AddCell(new PdfPCell(new Phrase(s, fBold)) { BackgroundColor = new BaseColor(230, 230, 230), Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });
            }

            // Agrupar e Ordenar
            Dictionary<string, List<NotaModulo>> discAgrupadas = new Dictionary<string, List<NotaModulo>>();
            foreach (NotaModulo n in notasAno)
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

            discSocioculturais.Sort();
            discCientificas.Sort();
            discTecnicas.Sort();

            List<string> ordemFinal = new List<string>();
            ordemFinal.AddRange(discSocioculturais);
            ordemFinal.AddRange(discCientificas);
            ordemFinal.AddRange(discTecnicas);

            foreach (string nomeDisc in ordemFinal)
            {
                List<NotaModulo> notasDaDisciplina = discAgrupadas[nomeDisc];
                
                double somaNotas = 0;
                int countComNota = 0;
                bool chumbouAlgum = false;

                foreach(NotaModulo v in notasDaDisciplina)
                {
                    if (v.Valor != null && v.Valor >= 0)
                    {
                        somaNotas = somaNotas + (double)v.Valor;
                        countComNota = countComNota + 1;
                    }

                    if (v.Valor == null || v.Valor < 10)
                    {
                        chumbouAlgum = true;
                    }
                }

                double mediaCalculada = 0;
                if (countComNota > 0)
                {
                    mediaCalculada = somaNotas / countComNota;
                }
                
                string situacaoFinal = "C";
                if (chumbouAlgum == true)
                {
                    situacaoFinal = "SC";
                }

                BaseColor bgCor = GetCorComponente(notasDaDisciplina[0].TipoDisciplina);

                data.AddCell(new PdfPCell(new Phrase(mediaCalculada.ToString("N1"), fBase)) { BackgroundColor = bgCor, Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                data.AddCell(new PdfPCell(new Phrase(countComNota.ToString(), fBase)) { BackgroundColor = bgCor, Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                data.AddCell(new PdfPCell(new Phrase(situacaoFinal, fBase)) { BackgroundColor = bgCor, Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                data.AddCell(new PdfPCell(new Phrase("", fBase)) { BackgroundColor = bgCor, Padding = 2 });
            }
            
            cell.AddElement(data);
            return cell;
        }

        private BaseColor GetCorComponente(string tipo)
        {
            if (tipo.ToLower().Contains("técnica"))
            {
                return corTecnica;
            }
            if (tipo.ToLower().Contains("científica"))
            {
                return corCientifica;
            }
            return corSociocultural;
        }

        private BaseColor GetCorFundoAno(string ano)
        {
            if (ano == "1º Ano")
            {
                return corFundoAno1;
            }
            if (ano == "2º Ano")
            {
                return corFundoAno2;
            }
            return corFundoAno3;
        }

        private int GetPrioridadeTipo(string tipo)
        {
            if (tipo.ToLower().Contains("cultural"))
            {
                return 1;
            }
            if (tipo.ToLower().Contains("científica"))
            {
                return 2;
            }
            if (tipo.ToLower().Contains("técnica"))
            {
                return 3;
            }
            return 4;
        }

        private string CalcularMedia(List<NotaModulo> notas, string tipo)
        {
            double soma = 0;
            int conta = 0;

            foreach (NotaModulo n in notas)
            {
                if (n.TipoDisciplina.ToLower().Contains(tipo))
                {
                    if (n.Valor != null && n.Valor >= 0)
                    {
                        soma = soma + (double)n.Valor;
                        conta = conta + 1;
                    }
                }
            }

            if (conta > 0)
            {
                return (soma / conta).ToString("N1");
            }
            return "0,0";
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
                            int idNota = Convert.ToInt32(r["Cod_NotaMod"]);
                            int idMod = Convert.ToInt32(r["Cod_Modulo"]);
                            
                            int? valorNota = null;
                            if (r["Valor"] != DBNull.Value)
                            {
                                valorNota = Convert.ToInt32(r["Valor"]);
                            }

                            string anoNota = r["Ano"].ToString();
                            string nomeMod = r["Modulo"].ToString();
                            string nomeDisc = r["Disciplina"].ToString();
                            string tipoDisc = r["Tipo"].ToString();

                            NotaModulo nota = new NotaModulo(idNota, codAluno, idMod, valorNota, null, anoNota, nomeMod, nomeDisc, tipoDisc);
                            lista.Add(nota);
                        }
                    }
                }
            }
            return lista;
        }
    }
}
