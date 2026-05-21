using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DCGest.Classes;

namespace DCGest
{
    /// <summary>
    /// Interação lógica para PaginaListaAlunos.xam
    /// </summary>
    public partial class PaginaListaAlunos : Page
    {
        public PaginaListaAlunos()
        {
            InitializeComponent();


            CarregarEntradas();
        }

        string caminho = BD.CaminhoBD;


        // Listas
        List<AnoLetivo> listaAnos = new List<AnoLetivo>();
        List<Curso> listaCursos = new List<Curso>();
        List<Turma> listaTurmas = new List<Turma>();
        List<Orientador> listaOrientadores = new List<Orientador>();

        List<Aluno> listaAlunos = new List<Aluno>();

        private void Btn_Click_Seleciona(object sender, RoutedEventArgs e)
        {
            try
            {
                int cod;

                if (dg_alunos.SelectedItem != null)
                {
                    Aluno selecionado = (Aluno)dg_alunos.SelectedItem;
                    cod = selecionado.Cod_Aluno;
                }
                else
                {
                    MessageBox.Show("Por favor, selecione um aluno na lista.");
                    return;
                }

                PaginaNotas janela = new PaginaNotas(cod);
                janela.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu um erro ao tentar selecionar o aluno: " + ex.Message);
                return;
            }
        }

        private void CarregarEntradas()
        {
            try
            {
                using (MySqlConnection conexao = new MySqlConnection(caminho))
                {
                    conexao.Open();

                    // Ano-Letivo
                    string sql_Ano = "SELECT Cod_Letivo, Intervalo FROM anosletivos ORDER BY Intervalo";
                    listaAnos.Clear();
                    listaAnos.Add(new AnoLetivo(0, "Todos"));

                    using (MySqlCommand comando = new MySqlCommand(sql_Ano, conexao))
                    {
                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read())
                            {
                                listaAnos.Add(new AnoLetivo(
                                    Convert.ToInt32(leitor["Cod_Letivo"]),
                                    leitor["Intervalo"].ToString()
                                ));
                            }
                        }
                    }

                    cmb_anoletivo.ItemsSource = null;
                    cmb_anoletivo.ItemsSource = listaAnos;
                    cmb_anoletivo.DisplayMemberPath = "Intervalo";
                    cmb_anoletivo.SelectedValuePath = "Cod_Letivo";
                    cmb_anoletivo.SelectedIndex = 0;

                    // Curso
                    string sql_Curso = "SELECT Cod_Curso, Nome_Curso FROM cursos ORDER BY Nome_Curso";
                    listaCursos.Clear();
                    listaCursos.Add(new Curso(0, "Todos"));

                    using (MySqlCommand comando = new MySqlCommand(sql_Curso, conexao))
                    {
                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read())
                            {
                                listaCursos.Add(new Curso(
                                    Convert.ToInt32(leitor["Cod_Curso"]), 
                                    leitor["Nome_Curso"].ToString() 
                                ));
                            }
                        }
                    }

                    cmb_curso.ItemsSource = null;
                    cmb_curso.ItemsSource = listaCursos;
                    cmb_curso.DisplayMemberPath = "Nome_Curso";
                    cmb_curso.SelectedValuePath = "Cod_Curso";
                    cmb_curso.SelectedIndex = 0;

                    // Turma
                    string sql_Turma = "SELECT Cod_Turma, Nome FROM turmas ORDER BY Nome";
                    listaTurmas.Clear();
                    listaTurmas.Add(new Turma(0, "*"));

                    using (MySqlCommand comando = new MySqlCommand(sql_Turma, conexao))
                    {
                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read())
                            {
                                listaTurmas.Add(new Turma(
                                    Convert.ToInt32(leitor["Cod_Turma"]),
                                    leitor["Nome"].ToString()
                                ));
                            }
                        }
                    }

                    cmb_turma.ItemsSource = null;
                    cmb_turma.ItemsSource = listaTurmas;
                    cmb_turma.DisplayMemberPath = "Nome";
                    cmb_turma.SelectedValuePath = "Cod_Turma";
                    cmb_turma.SelectedIndex = 0;

                    // Orientador
                    string sql_Orientador = "SELECT Cod_Orientador, Nome_Orientador FROM orientador ORDER BY Nome_Orientador";
                    listaOrientadores.Clear();
                    listaOrientadores.Add(new Orientador(0, "Todos"));

                    using (MySqlCommand comando = new MySqlCommand(sql_Orientador, conexao))
                    {
                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read())
                            {
                                listaOrientadores.Add(new Orientador(
                                    Convert.ToInt32(leitor["Cod_Orientador"]), 
                                    leitor["Nome_Orientador"].ToString() 
                                ));
                            }
                        }
                    }

                    cmb_orientador.ItemsSource = null;
                    cmb_orientador.ItemsSource = listaOrientadores;
                    cmb_orientador.DisplayMemberPath = "Nome_Orientador";
                    cmb_orientador.SelectedValuePath = "Cod_Orientador";
                    cmb_orientador.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar combos: " + ex.Message);
            }
        }

        private void CarregarAlunos()
        {
            try
            {
                using (MySqlConnection conexao = new MySqlConnection(caminho))
                {
                    conexao.Open();

                    StringBuilder sql_Alunos = new StringBuilder(@"
                        SELECT a.*, c.Nome_Curso, o.Nome_Orientador, t.Nome as Nome_Turma, al.Intervalo as Intervalo_Letivo
                        FROM aluno a
                        LEFT JOIN cursos c ON a.Cod_Curso = c.Cod_Curso
                        LEFT JOIN orientador o ON a.Cod_Ori = o.Cod_Orientador
                        LEFT JOIN turmas t ON a.Cod_Turma = t.Cod_Turma
                        LEFT JOIN anosletivos al ON a.Cod_Letivo = al.Cod_Letivo
                        WHERE 1=1");

                    if (cmb_anoletivo.SelectedValue != null && (int)cmb_anoletivo.SelectedValue != 0)
                    {
                        sql_Alunos.Append(" AND a.Cod_Letivo = @letivo");
                    }

                    if (cmb_curso.SelectedValue != null && (int)cmb_curso.SelectedValue != 0)
                    {
                        sql_Alunos.Append(" AND a.Cod_Curso = @curso");
                    }

                    using (MySqlCommand comando = new MySqlCommand(sql_Alunos.ToString(), conexao))
                    {
                        if (cmb_anoletivo.SelectedValue != null && (int)cmb_anoletivo.SelectedValue != 0)
                        {
                            comando.Parameters.AddWithValue("@letivo", cmb_anoletivo.SelectedValue);
                        }

                        if (cmb_curso.SelectedValue != null && (int)cmb_curso.SelectedValue != 0)
                        {
                            comando.Parameters.AddWithValue("@curso", cmb_curso.SelectedValue);
                        }

                        listaAlunos.Clear();

                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read())
                            {
                                int? codOri = leitor["Cod_Ori"] == DBNull.Value ? null : (int?)Convert.ToInt32(leitor["Cod_Ori"]);

                                listaAlunos.Add(new Aluno(
                                    Convert.ToInt32(leitor["Cod_Aluno"]),
                                    leitor["Nome_Aluno"].ToString(),
                                    Convert.ToInt32(leitor["Cod_Turma"]),
                                    Convert.ToInt32(leitor["Cod_Curso"]),
                                    leitor["Estado_Estagio"].ToString(),
                                    codOri,
                                    Convert.ToInt32(leitor["Cod_Letivo"]),
                                    leitor["Nome_Curso"].ToString(),
                                    leitor["Nome_Orientador"]?.ToString() ?? "N/A",
                                    leitor["Nome_Turma"].ToString(),
                                    leitor["Intervalo_Letivo"].ToString()
                                ));
                            }
                        }

                        dg_alunos.ItemsSource = null;
                        dg_alunos.ItemsSource = listaAlunos;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar alunos: " + ex.Message);
            }
        }

        private void FiltrarAlunos()
        {
            try
            {
                using (MySqlConnection conexao = new MySqlConnection(caminho))
                {
                    conexao.Open();

                    StringBuilder Filtro = new StringBuilder(@"
                        SELECT a.*, c.Nome_Curso, o.Nome_Orientador, t.Nome as Nome_Turma, al.Intervalo as Intervalo_Letivo
                        FROM aluno a
                        LEFT JOIN cursos c ON a.Cod_Curso = c.Cod_Curso
                        LEFT JOIN orientador o ON a.Cod_Ori = o.Cod_Orientador
                        LEFT JOIN turmas t ON a.Cod_Turma = t.Cod_Turma
                        LEFT JOIN anosletivos al ON a.Cod_Letivo = al.Cod_Letivo
                        WHERE 1=1");

                    if (txt_codigo.Text != string.Empty)
                    {
                        Filtro.Append(" AND a.Cod_Aluno = @cod");
                    }

                    if (txt_nome.Text != string.Empty)
                    {
                        Filtro.Append(" AND a.Nome_Aluno LIKE @nome");
                    }

                    if (cmb_turma.SelectedValue != null && (int)cmb_turma.SelectedValue != 0)
                    {
                        Filtro.Append(" AND a.Cod_Turma = @turma");
                    }

                    if (cmb_orientador.SelectedValue != null && (int)cmb_orientador.SelectedValue != 0)
                    {
                        Filtro.Append(" AND a.Cod_Ori = @orientador");
                    }

                    using (MySqlCommand comando = new MySqlCommand(Filtro.ToString(), conexao))
                    {
                        if (txt_codigo.Text != string.Empty)
                        {
                            comando.Parameters.AddWithValue("@cod", txt_codigo.Text);
                        }

                        if (txt_nome.Text != string.Empty)
                        {
                            comando.Parameters.AddWithValue("@nome", "%" + txt_nome.Text + "%");
                        }

                        if (cmb_turma.SelectedValue != null && (int)cmb_turma.SelectedValue != 0)
                        {
                            comando.Parameters.AddWithValue("@turma", cmb_turma.SelectedValue);
                        }

                        if (cmb_orientador.SelectedValue != null && (int)cmb_orientador.SelectedValue != 0)
                        {
                            comando.Parameters.AddWithValue("@orientador", cmb_orientador.SelectedValue);
                        }

                        listaAlunos.Clear();

                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read())
                            {
                                int? codOri = leitor["Cod_Ori"] == DBNull.Value ? null : (int?)Convert.ToInt32(leitor["Cod_Ori"]);

                                listaAlunos.Add(new Aluno(
                                    Convert.ToInt32(leitor["Cod_Aluno"]),
                                    leitor["Nome_Aluno"].ToString(),
                                    Convert.ToInt32(leitor["Cod_Turma"]),
                                    Convert.ToInt32(leitor["Cod_Curso"]),
                                    leitor["Estado_Estagio"].ToString(),
                                    codOri,
                                    Convert.ToInt32(leitor["Cod_Letivo"]),
                                    leitor["Nome_Curso"].ToString(),
                                    leitor["Nome_Orientador"]?.ToString() ?? "N/A",
                                    leitor["Nome_Turma"].ToString(),
                                    leitor["Intervalo_Letivo"].ToString()
                                ));
                            }
                        }

                        dg_alunos.ItemsSource = null;
                        dg_alunos.ItemsSource = listaAlunos;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao filtrar alunos: " + ex.Message);
            }
        }



        private void Btn_Click_GerarPDF(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dg_alunos.SelectedItem != null)
                {
                    Aluno selecionado = (Aluno)dg_alunos.SelectedItem;

                    GeradorPDF gerador = new GeradorPDF();
                    string caminhoPdf = gerador.GerarRelatorioAluno(selecionado);

                    JanelaPreviewPDF preview = new JanelaPreviewPDF(caminhoPdf);
                    preview.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Por favor, selecione um aluno na lista para gerar o relatório.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao gerar PDF: " + ex.Message);
            }
        }

        private void Btn_Click_Filtrar(object sender, RoutedEventArgs e)
        {
            FiltrarAlunos();
        }

        private void Btn_Click_LimparFiltro(object sender, RoutedEventArgs e)
        {
            txt_codigo.Text = string.Empty;
            txt_nome.Text = string.Empty;
            cmb_turma.SelectedIndex = 0;
            cmb_orientador.SelectedIndex = 0;
        }

        private void Btn_Click_Continuar(object sender, RoutedEventArgs e)
        {
            Overlay.Visibility = Visibility.Collapsed;

            CarregarAlunos();
        }

        private void Btn_Click_AbrirSeleciona(object sender, RoutedEventArgs e)
        {
            Overlay.Visibility = Visibility.Visible;
        }
    }
}
