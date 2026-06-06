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
    public partial class PaginaListaAlunos : Page
    {
        public PaginaListaAlunos()
        {
            InitializeComponent();


            CarregarEntradas();
        }

        string caminho = BD.CaminhoBD;


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
                listaAnos = AnoLetivo.ObterTodos();
                listaAnos.Insert(0, new AnoLetivo(0, "Todos"));
                cmb_anoletivo.ItemsSource = null;
                cmb_anoletivo.ItemsSource = listaAnos;
                cmb_anoletivo.SelectedIndex = 0;

                listaCursos = Curso.ObterTodos();
                listaCursos.Insert(0, new Curso(0, "Todos"));
                cmb_curso.ItemsSource = null;
                cmb_curso.ItemsSource = listaCursos;
                cmb_curso.SelectedIndex = 0;

                listaTurmas = Turma.ObterTodas();
                listaTurmas.Insert(0, new Turma(0, "*"));
                cmb_turma.ItemsSource = null;
                cmb_turma.ItemsSource = listaTurmas;
                cmb_turma.SelectedIndex = 0;

                listaOrientadores = Orientador.ObterTodos();
                listaOrientadores.Insert(0, new Orientador(0, "Todos"));
                cmb_orientador.ItemsSource = null;
                cmb_orientador.ItemsSource = listaOrientadores;
                cmb_orientador.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar opções: " + ex.Message);
            }
        }

        private void CarregarAlunos()
        {
            try
            {
                using (MySqlConnection conexao = new MySqlConnection(caminho))
                {
                    conexao.Open();

                    string sql_Alunos = "SELECT a.*, c.Nome_Curso, o.Nome_Orientador, t.Nome as Nome_Turma, al.Intervalo as Intervalo_Letivo " +
                                       "FROM aluno a " +
                                       "LEFT JOIN cursos c ON a.Cod_Curso = c.Cod_Curso " +
                                       "LEFT JOIN orientador o ON a.Cod_Ori = o.Cod_Orientador " +
                                       "LEFT JOIN turmas t ON a.Cod_Turma = t.Cod_Turma " +
                                       "LEFT JOIN anosletivos al ON a.Cod_Letivo = al.Cod_Letivo " +
                                       "WHERE 1=1";

                    if (cmb_anoletivo.SelectedValue != null)
                    {
                        if ((int)cmb_anoletivo.SelectedValue != 0)
                        {
                            sql_Alunos = sql_Alunos + " AND a.Cod_Letivo = @letivo";
                        }
                    }

                    if (cmb_curso.SelectedValue != null)
                    {
                        if ((int)cmb_curso.SelectedValue != 0)
                        {
                            sql_Alunos = sql_Alunos + " AND a.Cod_Curso = @curso";
                        }
                    }

                    using (MySqlCommand comando = new MySqlCommand(sql_Alunos, conexao))
                    {
                        if (cmb_anoletivo.SelectedValue != null)
                        {
                            if ((int)cmb_anoletivo.SelectedValue != 0)
                            {
                                comando.Parameters.AddWithValue("@letivo", cmb_anoletivo.SelectedValue);
                            }
                        }

                        if (cmb_curso.SelectedValue != null)
                        {
                            if ((int)cmb_curso.SelectedValue != 0)
                            {
                                comando.Parameters.AddWithValue("@curso", cmb_curso.SelectedValue);
                            }
                        }

                        listaAlunos.Clear();

                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read())
                            {
                                int? codOri = null;
                                if (leitor["Cod_Ori"] != DBNull.Value)
                                {
                                    codOri = Convert.ToInt32(leitor["Cod_Ori"]);
                                }

                                string nomeOri = "N/A";
                                if (leitor["Nome_Orientador"] != DBNull.Value)
                                {
                                    nomeOri = leitor["Nome_Orientador"].ToString();
                                }

                                Aluno a = new Aluno(
                                    Convert.ToInt32(leitor["Cod_Aluno"]),
                                    leitor["Nome_Aluno"].ToString(),
                                    Convert.ToInt32(leitor["Cod_Turma"]),
                                    Convert.ToInt32(leitor["Cod_Curso"]),
                                    leitor["Estado_Estagio"].ToString(),
                                    codOri,
                                    Convert.ToInt32(leitor["Cod_Letivo"]),
                                    leitor["Nome_Curso"].ToString(),
                                    nomeOri,
                                    leitor["Nome_Turma"].ToString(),
                                    leitor["Intervalo_Letivo"].ToString()
                                );

                                listaAlunos.Add(a);
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

                    string sql_Filtro = "SELECT a.*, c.Nome_Curso, o.Nome_Orientador, t.Nome as Nome_Turma, al.Intervalo as Intervalo_Letivo " +
                                       "FROM aluno a " +
                                       "LEFT JOIN cursos c ON a.Cod_Curso = c.Cod_Curso " +
                                       "LEFT JOIN orientador o ON a.Cod_Ori = o.Cod_Orientador " +
                                       "LEFT JOIN turmas t ON a.Cod_Turma = t.Cod_Turma " +
                                       "LEFT JOIN anosletivos al ON a.Cod_Letivo = al.Cod_Letivo " +
                                       "WHERE 1=1";

                    if (txt_codigo.Text != "")
                    {
                        sql_Filtro = sql_Filtro + " AND a.Cod_Aluno = @cod";
                    }

                    if (txt_nome.Text != "")
                    {
                        sql_Filtro = sql_Filtro + " AND a.Nome_Aluno LIKE @nome";
                    }

                    if (cmb_turma.SelectedValue != null)
                    {
                        if ((int)cmb_turma.SelectedValue != 0)
                        {
                            sql_Filtro = sql_Filtro + " AND a.Cod_Turma = @turma";
                        }
                    }

                    if (cmb_orientador.SelectedValue != null)
                    {
                        if ((int)cmb_orientador.SelectedValue != 0)
                        {
                            sql_Filtro = sql_Filtro + " AND a.Cod_Ori = @orientador";
                        }
                    }

                    using (MySqlCommand comando = new MySqlCommand(sql_Filtro, conexao))
                    {
                        if (txt_codigo.Text != "")
                        {
                            comando.Parameters.AddWithValue("@cod", txt_codigo.Text);
                        }

                        if (txt_nome.Text != "")
                        {
                            comando.Parameters.AddWithValue("@nome", "%" + txt_nome.Text + "%");
                        }

                        if (cmb_turma.SelectedValue != null)
                        {
                            if ((int)cmb_turma.SelectedValue != 0)
                            {
                                comando.Parameters.AddWithValue("@turma", cmb_turma.SelectedValue);
                            }
                        }

                        if (cmb_orientador.SelectedValue != null)
                        {
                            if ((int)cmb_orientador.SelectedValue != 0)
                            {
                                comando.Parameters.AddWithValue("@orientador", cmb_orientador.SelectedValue);
                            }
                        }

                        listaAlunos.Clear();

                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read())
                            {
                                int? codOri = null;
                                if (leitor["Cod_Ori"] != DBNull.Value)
                                {
                                    codOri = Convert.ToInt32(leitor["Cod_Ori"]);
                                }

                                string nomeOri = "N/A";
                                if (leitor["Nome_Orientador"] != DBNull.Value)
                                {
                                    nomeOri = leitor["Nome_Orientador"].ToString();
                                }

                                Aluno a = new Aluno(
                                    Convert.ToInt32(leitor["Cod_Aluno"]),
                                    leitor["Nome_Aluno"].ToString(),
                                    Convert.ToInt32(leitor["Cod_Turma"]),
                                    Convert.ToInt32(leitor["Cod_Curso"]),
                                    leitor["Estado_Estagio"].ToString(),
                                    codOri,
                                    Convert.ToInt32(leitor["Cod_Letivo"]),
                                    leitor["Nome_Curso"].ToString(),
                                    nomeOri,
                                    leitor["Nome_Turma"].ToString(),
                                    leitor["Intervalo_Letivo"].ToString()
                                );

                                listaAlunos.Add(a);
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

        private void Btn_Click_EditarAluno(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dg_alunos.SelectedItem != null)
                {
                    Aluno selecionado = (Aluno)dg_alunos.SelectedItem;

                    JanelaEditaAluno janela = new JanelaEditaAluno(selecionado.Cod_Aluno);
                    if (janela.ShowDialog() == true)
                    {
                        CarregarAlunos();
                    }
                }
                else
                {
                    MessageBox.Show("Por favor, selecione um aluno na lista para editar.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao abrir edição: " + ex.Message);
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
