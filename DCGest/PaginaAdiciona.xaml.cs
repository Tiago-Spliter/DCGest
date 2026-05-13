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
    /// Interação lógica para PaginaAdiciona.xam
    /// </summary>
    public partial class PaginaAdiciona : Page
    {
        public PaginaAdiciona()
        {
            InitializeComponent();

            
            cmb_tipo.SelectedIndex = 0;
            CarregarCombos();
        }

        string caminho = "Server=localhost;Database=pap;User=root;Password=rootroot";

        // Tabelas
        DataTable tabela_Turma = new DataTable();
        DataTable tabela_Curso = new DataTable();
        DataTable tabela_Orientador = new DataTable();
        DataTable tabela_Ano = new DataTable();


        private void Cmb_Tipo_Seleciona(object sender, SelectionChangedEventArgs e)
        {
            if (painelAluno == null || painelOrientador == null) return;

            if (cmb_tipo.SelectedIndex == 0)
            {
                painelAluno.Visibility = Visibility.Visible;
                painelOrientador.Visibility = Visibility.Collapsed;
            }
            else
            {
                painelAluno.Visibility = Visibility.Collapsed;
                painelOrientador.Visibility = Visibility.Visible;
            }
        }

        private void CarregarCombos()
        {
            try
            {
                using (MySqlConnection conexao = new MySqlConnection(caminho))
                {
                    conexao.Open();


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

                    cmb_Turma.ItemsSource = tabela_Turma.DefaultView;
                    cmb_Turma.DisplayMemberPath = "Turma";
                    cmb_Turma.SelectedValuePath = "Turma";
                    cmb_Turma.SelectedIndex = 0;


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

                    cmb_Curso.ItemsSource = tabela_Curso.DefaultView;
                    cmb_Curso.DisplayMemberPath = "Nome_Curso";
                    cmb_Curso.SelectedValuePath = "Cod_Curso";
                    cmb_Curso.SelectedIndex = 0;


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

                    cmb_Orientador.ItemsSource = tabela_Orientador.DefaultView;
                    cmb_Orientador.DisplayMemberPath = "Nome_Orientador";
                    cmb_Orientador.SelectedValuePath = "Cod_Orientador";
                    cmb_Orientador.SelectedIndex = 0;


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

                    cmb_Ano.ItemsSource = tabela_Ano.DefaultView;
                    cmb_Ano.DisplayMemberPath = "Ano_Letivo";
                    cmb_Ano.SelectedValuePath = "Ano_Letivo";
                    cmb_Ano.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar combos: " + ex.Message);
            }
        }


        private void InserirAluno(string codAluno, string nomeAluno, string turma, int curso, int? orientadorId, string anoLetivo)
        {
            try
            {
                using (MySqlConnection conexao = new MySqlConnection(caminho))
                {
                    conexao.Open();

                    string InserirA = "INSERT INTO aluno (Cod_Aluno, Nome_Aluno, Turma, Cod_Curso, Estado_Estagio, Cod_Ori, Ano_Letivo) VALUES (@Cod, @Nome, @Turma, @Curso, @Estado_Estagio, @Orientador, @Ano)";

                    using (MySqlCommand comando = new MySqlCommand(InserirA, conexao))
                    {
                        comando.Parameters.AddWithValue("@Cod", codAluno);
                        comando.Parameters.AddWithValue("@Nome", nomeAluno);
                        comando.Parameters.AddWithValue("@Turma", turma);
                        comando.Parameters.AddWithValue("@Curso", curso);
                        comando.Parameters.AddWithValue("@Estado_Estagio", "Não Pronto");
                        comando.Parameters.AddWithValue("@Orientador", orientadorId.HasValue ? (object)orientadorId.Value : DBNull.Value);
                        comando.Parameters.AddWithValue("@Ano", anoLetivo);

                        comando.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Aluno inserido com sucesso!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao inserir aluno: " + ex.Message);
            }
        }


        private void InserirOrientador(string nomeOrientador)
        {
            if (nomeOrientador == string.Empty)
            {
                MessageBox.Show("Preencha o nome do Orientador!");
                return;
            }

            try
            {
                using (MySqlConnection conexao = new MySqlConnection(caminho))
                {
                    conexao.Open();

                    string sql = "INSERT INTO orientador (Nome_Orientador) VALUES (@Nome)";

                    using (MySqlCommand comando = new MySqlCommand(sql, conexao))
                    {
                        comando.Parameters.AddWithValue("@Nome", nomeOrientador.Trim());

                        comando.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Orientador inserido com sucesso!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao inserir orientador: " + ex.Message);
            }
        }



        private void Btn_Click_Guardar(object sender, RoutedEventArgs e)
        {
            if (cmb_tipo.SelectedIndex == 0) // Aluno
            {
                if (txtCodAluno.Text == string.Empty ||
                    txtNomeAluno.Text == string.Empty ||
                    cmb_Turma.SelectedItem == null ||
                    cmb_Curso.SelectedItem == null ||
                    cmb_Ano.SelectedItem == null)
                {
                    MessageBox.Show("Preencha todos os campos obrigatórios do Aluno!");
                    return;
                }

                string codAluno = txtCodAluno.Text.Trim();
                string nomeAluno = txtNomeAluno.Text.Trim();
                string turma = cmb_Turma.SelectedValue.ToString();
                int curso = Convert.ToInt32(cmb_Curso.SelectedValue);
                string ano = cmb_Ano.SelectedValue.ToString();

                int? orientadorId = null;

                if (cmb_Orientador.SelectedValue != null)
                {
                    int id = Convert.ToInt32(((DataRowView)cmb_Orientador.SelectedItem)["Cod_Orientador"]);
                    if (id != 0) orientadorId = id;
                }

                InserirAluno(codAluno, nomeAluno, turma, curso, orientadorId, ano);
            }
            else // Orientador
            {
                string nomeOri = txtNomeOri.Text.Trim();

                if (string.IsNullOrWhiteSpace(nomeOri))
                {
                    MessageBox.Show("Preencha o nome do Orientador!");
                    return;
                }

                InserirOrientador(nomeOri);
            }
        }

        private void Btn_Click_Limpar(object sender, RoutedEventArgs e)
        {
            txtCodAluno.Text = string.Empty;
            txtNomeAluno.Text = string.Empty;
            txtNomeOri.Text = string.Empty;
            cmb_Turma.SelectedIndex = -1;
            cmb_Curso.SelectedIndex = -1;
            cmb_Orientador.SelectedIndex = -1; 
            cmb_Ano.SelectedIndex = -1;
        }
    }
}
