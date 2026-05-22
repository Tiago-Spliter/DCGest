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

        // PALETE DE CORES SINCRONIZADA (Laranja, Verde, Vermelho)
        
        // TONS DE REFERÊNCIA (Para as células M1-M19 - Médios para não precisar de branco)
        BaseColor corLaranjaMedio = new BaseColor(250, 191, 143);  // 1º Ano
        BaseColor corVerdeMedio = new BaseColor(170, 210, 140);    // 2º Ano
        BaseColor corVermelhoMedio = new BaseColor(230, 150, 150);  // 3º Ano (Vermelho suave)

        // TONS DE CABEÇALHO (Rodapé - Escuros)
        BaseColor corLaranjaEscuro = new BaseColor(228, 108, 10);  
        BaseColor corVerdeEscuro = new BaseColor(118, 147, 60);    
        BaseColor corVermelhoEscuro = new BaseColor(148, 54, 52);  // Vermelho escuro

        // TONS DE COMPONENTE (Coluna 1)
        BaseColor corRosaCarregado = new BaseColor(245, 140, 170);  // Sociocultural
        BaseColor corVerdeClaroComponente = new BaseColor(210, 235, 190); // Técnica

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

            // 1. CABEÇALHO ESTILO TESTE DE AVALIAÇÃO (Alinhado e Boxed)
            PdfPTable tabHeader = new PdfPTable(3);
            tabHeader.WidthPercentage = 100;
            tabHeader.SetWidths(new float[] { 3, 1, 1 }); // Nome ganha mais espaço

            // Título Principal
            PdfPCell cellTitle = new PdfPCell(new Phrase("REGISTO BIOGRÁFICO DE AVALIAÇÃO", fTitle));
            cellTitle.Colspan = 3; 
            cellTitle.Border = 0; 
            cellTitle.HorizontalAlignment = Element.ALIGN_CENTER; 
            cellTitle.PaddingBottom = 15;
            tabHeader.AddCell(cellTitle);

            // Linha 1: Nome, N.º e Turma
            PdfPCell cNome = new PdfPCell(new Phrase("ALUNO: " + alunoCompleto.Nome_Aluno.ToUpper(), fBold));
            cNome.Padding = 5; 
            tabHeader.AddCell(cNome);

            PdfPCell cProc = new PdfPCell(new Phrase("N.º PROC: " + alunoCompleto.Cod_Aluno, fBase));
            cProc.Padding = 5; 
            cProc.HorizontalAlignment = Element.ALIGN_CENTER;
            tabHeader.AddCell(cProc);

            PdfPCell cTurma = new PdfPCell(new Phrase("TURMA: " + alunoCompleto.Nome_Turma, fBase));
            cTurma.Padding = 5; 
            cTurma.HorizontalAlignment = Element.ALIGN_CENTER;
            tabHeader.AddCell(cTurma);

            // Linha 2: Curso, Ano Letivo e Orientador
            PdfPCell cCurso = new PdfPCell(new Phrase("CURSO: " + alunoCompleto.Nome_Curso, fBase));
            cCurso.Padding = 5; 
            tabHeader.AddCell(cCurso);

            PdfPCell cAno = new PdfPCell(new Phrase("ANO LETIVO: " + alunoCompleto.Intervalo_Letivo, fBase));
            cAno.Padding = 5; 
            cAno.HorizontalAlignment = Element.ALIGN_CENTER;
            tabHeader.AddCell(cAno);

            PdfPCell cOri = new PdfPCell(new Phrase("ORIENTADOR: " + alunoCompleto.Nome_Orientador, fBase));
            cOri.Padding = 5; 
            cOri.HorizontalAlignment = Element.ALIGN_CENTER;
            tabHeader.AddCell(cOri);

            doc.Add(tabHeader);
            doc.Add(new Paragraph(" ")); // Espaçamento após o cabeçalho

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

            gridM.AddCell(new PdfPCell(new Phrase("DISCIPLINAS", fBold)) { BackgroundColor = corHeaderGrelha, Padding = 2 });
            for (int i = 1; i <= 19; i++) 
            {
                gridM.AddCell(new PdfPCell(new Phrase("M" + i, fBold)) { BackgroundColor = corHeaderGrelha, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            }

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
                string tipo = discAgrupadas[nomeDisc][0].TipoDisciplina;
                BaseColor bgComp = BaseColor.WHITE;
                if (tipo.ToLower().Contains("cultural"))
                {
                    bgComp = corRosaCarregado;
                }
                else if (tipo.ToLower().Contains("técnica"))
                {
                    bgComp = corVerdeClaroComponente;
                }

                gridM.AddCell(new PdfPCell(new Phrase(nomeDisc, fBase)) { BackgroundColor = bgComp, Padding = 2 });

                NotaModulo[] colunas = new NotaModulo[20];
                foreach (NotaModulo m in discAgrupadas[nomeDisc])
                {
                    int nMod = ExtrairNumeroModulo(m.NomeModulo);
                    if (nMod >= 1 && nMod <= 19)
                    {
                        colunas[nMod] = m;
                    }
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

            // FCT e PAP
            gridM.AddCell(new PdfPCell(new Phrase("FCT", fBase)) { BackgroundColor = corFCT, Padding = 2 });
            gridM.AddCell(new PdfPCell(new Phrase("0", fBase)) { BackgroundColor = corFCT, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
            gridM.AddCell(new PdfPCell(new Phrase("", fBase)) { Border = 0, Colspan = 18 });

            gridM.AddCell(new PdfPCell(new Phrase("PAP", fBase)) { BackgroundColor = corRosaCarregado, Padding = 2 });
            gridM.AddCell(new PdfPCell(new Phrase("0", fBase)) { BackgroundColor = corRosaCarregado, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 2 });
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
            tabAnos.AddCell(CriarMiniTabelaAno(todasNotas, "3º Ano", corVermelhoEscuro, fBase, fBold));
            
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

            double somaAno = 0;
            int contaAno = 0;

            foreach (NotaModulo n in notas)
            {
                if (n.Ano == ano)
                {
                    if (n.TipoDisciplina.ToLower().Contains("final") == false)
                    {
                        notasAno.Add(n);
                        totalModulos = totalModulos + 1;
                        if (n.TipoDisciplina.ToLower().Contains("técnica"))
                        {
                            tecnicasModulos = tecnicasModulos + 1;
                        }

                        if (n.Valor != null && n.Valor > 0)
                        {
                            somaAno = somaAno + (double)n.Valor;
                            contaAno = contaAno + 1;
                        }
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
            foreach(string s in subH) 
            {
                data.AddCell(new PdfPCell(new Phrase(s, fBold)) { BackgroundColor = corHeaderGrelha, Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });
            }

            Dictionary<string, List<NotaModulo>> grupos = new Dictionary<string, List<NotaModulo>>();
            foreach (NotaModulo n in notasAno)
            {
                if (grupos.ContainsKey(n.NomeDisciplina) == false)
                {
                    grupos.Add(n.NomeDisciplina, new List<NotaModulo>());
                }
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
                foreach(NotaModulo v in notasDisc)
                {
                    if (v.Valor != null && v.Valor > 0) { soma = soma + (double)v.Valor; countRealizados = countRealizados + 1; }
                    if (v.Valor == null || v.Valor < 10) incompleto = true;
                }
                double media = countRealizados > 0 ? soma / (double)countRealizados : 0;
                string situacao = incompleto ? "SC" : "C";

                BaseColor bgCor = BaseColor.WHITE;
                string tipo = notasDisc[0].TipoDisciplina.ToLower();
                if (tipo.Contains("cultural"))
                {
                    bgCor = corRosaMuitoClaro;
                }
                else if (tipo.Contains("técnica"))
                {
                    bgCor = corVerdeClaroComponente;
                }

                data.AddCell(new PdfPCell(new Phrase(media.ToString("N1"), fBase)) { BackgroundColor = bgCor, Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                data.AddCell(new PdfPCell(new Phrase(countRealizados.ToString(), fBase)) { BackgroundColor = bgCor, Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                data.AddCell(new PdfPCell(new Phrase(situacao, fBase)) { BackgroundColor = bgCor, Padding = 2, HorizontalAlignment = Element.ALIGN_CENTER });
                data.AddCell(new PdfPCell(new Phrase("", fBase)) { BackgroundColor = bgCor, Padding = 2 });
            }
            cell.AddElement(data);

            // MÉDIA DO ANO
            double mediaAno = 0;
            if (contaAno > 0)
            {
                mediaAno = somaAno / (double)contaAno;
            }

            PdfPTable tabMediaAnual = new PdfPTable(2);
            tabMediaAnual.WidthPercentage = 100;
            tabMediaAnual.SpacingBefore = 5;
            tabMediaAnual.SetWidths(new float[] { 2, 1 });

            tabMediaAnual.AddCell(new PdfPCell(new Phrase("MÉDIA DO ANO", fBold)) { BackgroundColor = cHeader, Padding = 3 });
            tabMediaAnual.AddCell(new PdfPCell(new Phrase(mediaAno.ToString("N1"), fBold)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 3 });

            cell.AddElement(tabMediaAnual);

            return cell;
        }

        private int ExtrairNumeroModulo(string nome)
        {
            string num = "";
            foreach (char c in nome) { if (char.IsDigit(c)) num = num + c; }
            return num != "" ? Convert.ToInt32(num) : 0;
        }

        private BaseColor GetCorAnoIntensa(string ano)
        {
            if (ano.Contains("1"))
            {
                return corLaranjaMedio;
            }
            if (ano.Contains("2"))
            {
                return corVerdeMedio;
            }
            return corVermelhoMedio;
        }

        private BaseColor GetCorComponente(string tipo)
        {
            if (tipo.ToLower().Contains("técnica"))
            {
                return corVerdeClaroComponente;
            }
            if (tipo.ToLower().Contains("científica"))
            {
                return BaseColor.WHITE;
            }
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
            double soma = 0; int conta = 0;
            foreach (NotaModulo n in notas)
            {
                if (n.TipoDisciplina.ToLower().Contains(tipo.ToLower()))
                {
                    if (n.Valor != null && n.Valor > 0) { soma = soma + (double)n.Valor; conta = conta + 1; }
                }
            }
            if (conta > 0)
            {
                return (soma / (double)conta).ToString("N1");
            }
            return "0,0";
        }

        private List<NotaModulo> ObterTodasNotas(int codAluno)
        {
            List<NotaModulo> lista = new List<NotaModulo>();
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
                            int? val = r["Valor"] != DBNull.Value ? (int?)Convert.ToInt32(r["Valor"]) : null;
                            string anoCalc = r["AnoDisciplina"].ToString() + "º Ano";
                            lista.Add(new NotaModulo(Convert.ToInt32(r["Cod_NotaMod"]), codAluno, Convert.ToInt32(r["Cod_Modulo"]), val, null, anoCalc, r["Modulo"].ToString(), r["Disciplina"].ToString(), r["Tipo"].ToString()));
                        }
                    }
                }
            }
            return lista;
        }
    }
}
