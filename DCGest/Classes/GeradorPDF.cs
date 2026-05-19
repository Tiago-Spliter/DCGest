using iTextSharp.text;
using iTextSharp.text.pdf;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DCGest.Classes
{
    public class GeradorPDF
    {
        private string connectionString = BD.CaminhoBD;

        public string GerarRelatorioAluno(Aluno aluno)
        {
            string pastaTemp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Relatorios");
            if (!Directory.Exists(pastaTemp)) Directory.CreateDirectory(pastaTemp);

            string caminhoPdf = Path.Combine(pastaTemp, $"Relatorio_{aluno.Cod_Aluno}.pdf");

            Document doc = new Document(PageSize.A4.Rotate(), 30, 30, 30, 30);
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(caminhoPdf, FileMode.Create));

            doc.Open();

            // Fontes
            Font fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 22, BaseColor.BLACK);
            Font fontSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.DARK_GRAY);
            Font fontCorpo = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);
            Font fontHeaderTabela = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
            Font fontCelula = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);

            // Cabeçalho do Relatório
            Paragraph pTitulo = new Paragraph("RELATÓRIO DE AVALIAÇÃO ACADÉMICA", fontTitulo);
            pTitulo.Alignment = Element.ALIGN_CENTER;
            pTitulo.SpacingAfter = 20;
            doc.Add(pTitulo);

            // Info Aluno
            doc.Add(new Paragraph($"Nome do Aluno: {aluno.Nome_Aluno}", fontCorpo));
            doc.Add(new Paragraph($"Turma: {aluno.Turma} | Ano Letivo: {aluno.Ano_Letivo}", fontCorpo));
            doc.Add(new Paragraph($"Curso: {aluno.Nome_Curso}", fontCorpo));
            doc.Add(new Paragraph($"Orientador: {aluno.Nome_Orientador}", fontCorpo));
            doc.Add(new Paragraph(" ", fontCorpo)); // Espaço

            // Obter Dados da Base de Dados
            var dados = ObterDadosAvaliacao(aluno.Cod_Aluno);

            // Agrupar por Tipo de Disciplina
            var grupos = dados.GroupBy(d => d.TipoDisciplina).OrderBy(g => g.Key);

            foreach (var grupo in grupos)
            {
                doc.Add(new Paragraph($"Componente de Formação: {grupo.Key}", fontSubtitulo));
                doc.Add(new Paragraph(" ", fontCorpo));

                // Definir Cor Base pelo Tipo
                BaseColor corFundo = BaseColor.LIGHT_GRAY;
                if (grupo.Key.ToLower().Contains("técnica")) corFundo = new BaseColor(215, 225, 255); // Azul claro
                else if (grupo.Key.ToLower().Contains("científica")) corFundo = new BaseColor(215, 255, 215); // Verde claro

                // Criar Tabela
                // Vamos assumir um máximo de 15 módulos por disciplina para a estrutura da tabela
                PdfPTable tabela = new PdfPTable(16); // 1 para Disciplina + 15 para Módulos
                tabela.WidthPercentage = 100;
                float[] widths = new float[16];
                widths[0] = 3f; // Disciplina mais larga
                for (int i = 1; i < 16; i++) widths[i] = 1f;
                tabela.SetWidths(widths);

                // Header da Tabela
                PdfPCell cellHeader = new PdfPCell(new Phrase("Disciplina", fontHeaderTabela));
                cellHeader.BackgroundColor = new BaseColor(41, 52, 114); // Azul Escuro do projeto
                cellHeader.HorizontalAlignment = Element.ALIGN_CENTER;
                cellHeader.Padding = 5;
                tabela.AddCell(cellHeader);

                for (int i = 1; i <= 15; i++)
                {
                    PdfPCell h = new PdfPCell(new Phrase($"M{i}", fontHeaderTabela));
                    h.BackgroundColor = new BaseColor(41, 52, 114);
                    h.HorizontalAlignment = Element.ALIGN_CENTER;
                    h.Padding = 5;
                    tabela.AddCell(h);
                }

                // Linhas (Disciplinas)
                var disciplinas = grupo.GroupBy(d => d.NomeDisciplina);
                foreach (var disc in disciplinas)
                {
                    PdfPCell cDisc = new PdfPCell(new Phrase(disc.Key, fontCelula));
                    cDisc.BackgroundColor = corFundo;
                    cDisc.Padding = 5;
                    tabela.AddCell(cDisc);

                    // Módulos e Notas
                    var modulos = disc.OrderBy(m => m.CodModulo).ToList();
                    for (int i = 0; i < 15; i++)
                    {
                        string notaStr = "";
                        if (i < modulos.Count)
                        {
                            notaStr = modulos[i].Valor?.ToString() ?? "-";
                        }
                        
                        PdfPCell cNota = new PdfPCell(new Phrase(notaStr, fontCelula));
                        cNota.HorizontalAlignment = Element.ALIGN_CENTER;
                        cNota.VerticalAlignment = Element.ALIGN_MIDDLE;
                        cNota.BackgroundColor = (i < modulos.Count) ? BaseColor.WHITE : new BaseColor(245, 245, 245);
                        tabela.AddCell(cNota);
                    }
                }

                doc.Add(tabela);
                doc.Add(new Paragraph(" ", fontCorpo));
            }

            // Nota Final (Visual Placeholder)
            doc.Add(new Paragraph(" ", fontCorpo));
            PdfPTable tabFinal = new PdfPTable(3);
            tabFinal.WidthPercentage = 50;
            tabFinal.HorizontalAlignment = Element.ALIGN_RIGHT;

            tabFinal.AddCell(new PdfPCell(new Phrase("Média Final do Aluno", fontHeaderTabela)) { BackgroundColor = new BaseColor(41, 52, 114), Padding = 8, Colspan = 2 });
            tabFinal.AddCell(new PdfPCell(new Phrase("---", fontSubtitulo)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 8 });

            doc.Add(tabFinal);

            doc.Close();

            return caminhoPdf;
        }

        private List<InfoNotaPDF> ObterDadosAvaliacao(int codAluno)
        {
            List<InfoNotaPDF> lista = new List<InfoNotaPDF>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT d.Designacao as Disciplina, d.Tipo, m.Designacao as Modulo, m.Cod_Modulo, n.Valor
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
                            lista.Add(new InfoNotaPDF
                            {
                                NomeDisciplina = r["Disciplina"].ToString(),
                                TipoDisciplina = r["Tipo"].ToString(),
                                NomeModulo = r["Modulo"].ToString(),
                                CodModulo = Convert.ToInt32(r["Cod_Modulo"]),
                                Valor = r["Valor"] == DBNull.Value ? null : (int?)Convert.ToInt32(r["Valor"])
                            });
                        }
                    }
                }
            }
            return lista;
        }

        private class InfoNotaPDF
        {
            public string NomeDisciplina { get; set; }
            public string TipoDisciplina { get; set; }
            public string NomeModulo { get; set; }
            public int CodModulo { get; set; }
            public int? Valor { get; set; }
        }
    }
}
