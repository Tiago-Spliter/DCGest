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

        string caminho = "Server=localhost;Database=pap;User=root;Password=rootroot";


        // Tabelas
        DataTable tabela_Ano = new DataTable();
        DataTable tabela_Curso = new DataTable();
        DataTable tabela_Alunos = new DataTable();
        DataTable tabela_Turma = new DataTable();
        DataTable tabela_Orientador = new DataTable();

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
                    tabela_Ano.Clear();

                    using (MySqlCommand comando = new MySqlCommand(sql_Ano, conexao))
                    {
                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            tabela_Ano.Load(leitor);
                        }
                    }
                    

                    DataRow alinha_Ano = tabela_Ano.NewRow();
                    alinha_Ano["Ano_Letivo"] = "Todos";
                    tabela_Ano.Rows.InsertAt(alinha_Ano, 0);

                    cmb_anoletivo.ItemsSource = tabela_Ano.DefaultView;
                    cmb_anoletivo.DisplayMemberPath = "Ano_Letivo";
                    cmb_anoletivo.SelectedValuePath = "Ano_Letivo";
                    cmb_anoletivo.SelectedIndex = 0;

                    
                    // Curso

                    string sql_Curso = "SELECT Cod_Curso, Nome_Curso FROM cursos ORDER BY Nome_Curso";
                    tabela_Curso.Clear();

                    using (MySqlCommand comando = new MySqlCommand(sql_Curso, conexao))
                    {
                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            tabela_Curso.Load(leitor);
                        }
                    }
                    

                    DataRow alinha_Curso = tabela_Curso.NewRow();
                    alinha_Curso["Cod_Curso"] = 0;
                    alinha_Curso["Nome_Curso"] = "Todos";
                    tabela_Curso.Rows.InsertAt(alinha_Curso, 0);

                    cmb_curso.ItemsSource = tabela_Curso.DefaultView;
                    cmb_curso.DisplayMemberPath = "Nome_Curso";
                    cmb_curso.SelectedValuePath = "Cod_Curso";
                    cmb_curso.SelectedIndex = 0;


                    // Turma

                    string sql_Turma = "SELECT DISTINCT Turma FROM aluno ORDER BY Turma";
                    tabela_Turma.Clear();

                    using (MySqlCommand comando = new MySqlCommand(sql_Turma, conexao))
                    {
                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            tabela_Turma.Load(leitor);
                        }
                    }
                    

                    DataRow alinha_Turma = tabela_Turma.NewRow();
                    alinha_Turma["Turma"] = "*";
                    tabela_Turma.Rows.InsertAt(alinha_Turma, 0);

                    cmb_turma.ItemsSource = tabela_Turma.DefaultView;
                    cmb_turma.DisplayMemberPath = "Turma";
                    cmb_turma.SelectedValuePath = "Turma";
                    cmb_turma.SelectedIndex = 0;


                    // Orientador
                    
                    string sql_Orientador = "SELECT Cod_Orientador, Nome_Orientador FROM orientador ORDER BY Nome_Orientador";
                    tabela_Orientador.Clear();

                    using (MySqlCommand comando = new MySqlCommand(sql_Orientador, conexao))
                    {
                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            tabela_Orientador.Load(leitor);
                        }
                    }


                    DataRow rowOrientador = tabela_Orientador.NewRow();
                    rowOrientador["Cod_Orientador"] = 0;
                    rowOrientador["Nome_Orientador"] = "Todos";
                    tabela_Orientador.Rows.InsertAt(rowOrientador, 0);

                    cmb_orientador.ItemsSource = tabela_Orientador.DefaultView;
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

                    // Filtrar Ano Letivo

                    if (cmb_anoletivo.SelectedValue != null && cmb_anoletivo.SelectedValue.ToString() != "Todos")
                    {
                        sql_Alunos.Append(" AND Ano_Letivo = @ano");
                    }

                    // Filtrar Curso

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

                        tabela_Alunos.Clear();

                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            tabela_Alunos.Load(leitor);
                        }

                        dg_alunos.ItemsSource = tabela_Alunos.DefaultView;
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

                    // Filtros

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
                        Filtro.Append(" AND Cod_Orientador = @orientador");
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

                        tabela_Alunos.Clear();

                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            tabela_Alunos.Load(leitor);
                        }

                        dg_alunos.ItemsSource = tabela_Alunos.DefaultView;
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
