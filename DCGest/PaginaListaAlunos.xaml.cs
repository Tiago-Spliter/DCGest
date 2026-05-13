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


        // Listas tipadas para as Combos
        List<string> listaAnos = new List<string>();
        List<Curso> listaCursos = new List<Curso>();
        List<string> listaTurmas = new List<string>();
        List<Orientador> listaOrientadores = new List<Orientador>();

        List<Aluno> listaAlunos = new List<Aluno>();

        private void Btn_Click_Seleciona(object sender, RoutedEventArgs e)
        {
            try
            {
                int cod = int.Parse(txt_numero.Text);

                PaginaNotas janela = new PaginaNotas(cod);
                janela.Show();
            }
            catch
            {
                MessageBox.Show("Código inválido!");
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
                    string sql_Ano = "SELECT DISTINCT Ano_Letivo FROM aluno ORDER BY Ano_Letivo";
                    listaAnos.Clear();
                    listaAnos.Add("Todos");

                    using (MySqlCommand comando = new MySqlCommand(sql_Ano, conexao))
                    {
                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read()) listaAnos.Add(leitor["Ano_Letivo"].ToString());
                        }
                    }

                    cmb_anoletivo.ItemsSource = null;
                    cmb_anoletivo.ItemsSource = listaAnos;
                    cmb_anoletivo.SelectedIndex = 0;

                    // Curso
                    string sql_Curso = "SELECT Cod_Curso, Nome_Curso FROM cursos ORDER BY Nome_Curso";
                    listaCursos.Clear();
                    listaCursos.Add(new Curso { Cod_Curso = 0, Nome_Curso = "Todos" });

                    using (MySqlCommand comando = new MySqlCommand(sql_Curso, conexao))
                    {
                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read())
                            {
                                listaCursos.Add(new Curso { 
                                    Cod_Curso = Convert.ToInt32(leitor["Cod_Curso"]), 
                                    Nome_Curso = leitor["Nome_Curso"].ToString() 
                                });
                            }
                        }
                    }

                    cmb_curso.ItemsSource = null;
                    cmb_curso.ItemsSource = listaCursos;
                    cmb_curso.DisplayMemberPath = "Nome_Curso";
                    cmb_curso.SelectedValuePath = "Cod_Curso";
                    cmb_curso.SelectedIndex = 0;

                    // Turma
                    string sql_Turma = "SELECT DISTINCT Turma FROM aluno ORDER BY Turma";
                    listaTurmas.Clear();
                    listaTurmas.Add("*");

                    using (MySqlCommand comando = new MySqlCommand(sql_Turma, conexao))
                    {
                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read()) listaTurmas.Add(leitor["Turma"].ToString());
                        }
                    }

                    cmb_turma.ItemsSource = null;
                    cmb_turma.ItemsSource = listaTurmas;
                    cmb_turma.SelectedIndex = 0;

                    // Orientador
                    string sql_Orientador = "SELECT Cod_Orientador, Nome_Orientador FROM orientador ORDER BY Nome_Orientador";
                    listaOrientadores.Clear();
                    listaOrientadores.Add(new Orientador { Cod_Orientador = 0, Nome_Orientador = "Todos" });

                    using (MySqlCommand comando = new MySqlCommand(sql_Orientador, conexao))
                    {
                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read())
                            {
                                listaOrientadores.Add(new Orientador { 
                                    Cod_Orientador = Convert.ToInt32(leitor["Cod_Orientador"]), 
                                    Nome_Orientador = leitor["Nome_Orientador"].ToString() 
                                });
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

                    StringBuilder sql_Alunos = new StringBuilder("SELECT * FROM aluno WHERE 1=1");

                    if (cmb_anoletivo.SelectedValue != null && cmb_anoletivo.SelectedValue.ToString() != "Todos")
                    {
                        sql_Alunos.Append(" AND Ano_Letivo = @ano");
                    }

                    if (cmb_curso.SelectedValue != null && cmb_curso.SelectedValue.ToString() != "0")
                    {
                        sql_Alunos.Append(" AND Cod_Curso = @curso");
                    }

                    using (MySqlCommand comando = new MySqlCommand(sql_Alunos.ToString(), conexao))
                    {
                        if (cmb_anoletivo.SelectedValue != null && cmb_anoletivo.SelectedValue.ToString() != "Todos")
                        {
                            comando.Parameters.AddWithValue("@ano", cmb_anoletivo.SelectedValue);
                        }

                        if (cmb_curso.SelectedValue != null && cmb_curso.SelectedValue.ToString() != "0")
                        {
                            comando.Parameters.AddWithValue("@curso", cmb_curso.SelectedValue);
                        }

                        listaAlunos.Clear();

                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read())
                            {
                                Aluno a = new Aluno();
                                
                                a.Cod_Aluno = Convert.ToInt32(leitor["Cod_Aluno"]);
                                a.Nome_Aluno = leitor["Nome_Aluno"].ToString();
                                a.Turma = leitor["Turma"].ToString();
                                a.Cod_Curso = Convert.ToInt32(leitor["Cod_Curso"]);
                                a.Estado_Estagio = leitor["Estado_Estagio"].ToString();
                                a.Ano_Letivo = leitor["Ano_Letivo"].ToString();

                                // Verificação do Orientador com IF
                                if (leitor["Cod_Ori"] == DBNull.Value)
                                {
                                    a.Cod_Ori = null;
                                }
                                else
                                {
                                    a.Cod_Ori = Convert.ToInt32(leitor["Cod_Ori"]);
                                }

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

                    StringBuilder Filtro = new StringBuilder("SELECT * FROM aluno WHERE 1=1");

                    if (txt_codigo.Text != string.Empty)
                    {
                        Filtro.Append(" AND Cod_Aluno = @cod");
                    }

                    if (txt_nome.Text != string.Empty)
                    {
                        Filtro.Append(" AND Nome_Aluno LIKE @nome");
                    }

                    if (cmb_turma.SelectedValue != null && cmb_turma.SelectedValue.ToString() != "*")
                    {
                        Filtro.Append(" AND Turma = @turma");
                    }

                    if (cmb_orientador.SelectedValue != null && cmb_orientador.SelectedValue.ToString() != "0")
                    {
                        Filtro.Append(" AND Cod_Ori = @orientador");
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

                        if (cmb_turma.SelectedValue != null && cmb_turma.SelectedValue.ToString() != "*")
                        {
                            comando.Parameters.AddWithValue("@turma", cmb_turma.SelectedValue);
                        }

                        if (cmb_orientador.SelectedValue != null && cmb_orientador.SelectedValue.ToString() != "0")
                        {
                            comando.Parameters.AddWithValue("@orientador", cmb_orientador.SelectedValue);
                        }

                        listaAlunos.Clear();

                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read())
                            {
                                Aluno a = new Aluno();
                                
                                a.Cod_Aluno = Convert.ToInt32(leitor["Cod_Aluno"]);
                                a.Nome_Aluno = leitor["Nome_Aluno"].ToString();
                                a.Turma = leitor["Turma"].ToString();
                                a.Cod_Curso = Convert.ToInt32(leitor["Cod_Curso"]);
                                a.Estado_Estagio = leitor["Estado_Estagio"].ToString();
                                a.Ano_Letivo = leitor["Ano_Letivo"].ToString();

                                if (leitor["Cod_Ori"] == DBNull.Value)
                                {
                                    a.Cod_Ori = null;
                                }
                                else
                                {
                                    a.Cod_Ori = Convert.ToInt32(leitor["Cod_Ori"]);
                                }

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
