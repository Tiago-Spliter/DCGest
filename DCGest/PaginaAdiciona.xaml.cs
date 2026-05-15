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

        string caminho = BD.CaminhoBD;

        // Listas 
        List<string> listaTurmas = new List<string>();
        List<Curso> listaCursos = new List<Curso>();
        List<Orientador> listaOrientadores = new List<Orientador>();
        List<string> listaAnos = new List<string>();

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
                    listaTurmas.Clear();
                    using (MySqlCommand comando = new MySqlCommand(sql_Turma, conexao))
                    {
                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read()) listaTurmas.Add(leitor["Turma"].ToString());
                        }
                    }
                    cmb_Turma.ItemsSource = null;
                    cmb_Turma.ItemsSource = listaTurmas;
                    if (listaTurmas.Count > 0) cmb_Turma.SelectedIndex = 0;

                    // Curso
                    string sql_Curso = "SELECT Cod_Curso, Nome_Curso FROM cursos ORDER BY Nome_Curso";
                    listaCursos.Clear();
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
                    cmb_Curso.ItemsSource = null;
                    cmb_Curso.ItemsSource = listaCursos;
                    cmb_Curso.DisplayMemberPath = "Nome_Curso";
                    cmb_Curso.SelectedValuePath = "Cod_Curso";
                    if (listaCursos.Count > 0) cmb_Curso.SelectedIndex = 0;

                    // Orientador
                    string sql_Orientador = "SELECT Cod_Orientador, Nome_Orientador FROM orientador ORDER BY Nome_Orientador";
                    listaOrientadores.Clear();
                    listaOrientadores.Add(new Orientador(0, "Sem Orientador"));
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
                    cmb_Orientador.ItemsSource = null;
                    cmb_Orientador.ItemsSource = listaOrientadores;
                    cmb_Orientador.DisplayMemberPath = "Nome_Orientador";
                    cmb_Orientador.SelectedValuePath = "Cod_Orientador";
                    cmb_Orientador.SelectedIndex = 0;

                    // Ano-Letivo
                    string sql_Ano = "SELECT DISTINCT Ano_Letivo FROM aluno ORDER BY Ano_Letivo";
                    listaAnos.Clear();
                    using (MySqlCommand comando = new MySqlCommand(sql_Ano, conexao))
                    {
                        using (MySqlDataReader leitor = comando.ExecuteReader())
                        {
                            while (leitor.Read()) listaAnos.Add(leitor["Ano_Letivo"].ToString());
                        }
                    }
                    cmb_Ano.ItemsSource = null;
                    cmb_Ano.ItemsSource = listaAnos;
                    if (listaAnos.Count > 0) cmb_Ano.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar combos: " + ex.Message);
            }
        }

        private void Btn_Click_Guardar(object sender, RoutedEventArgs e)
        {
            try
            {
                Entidade novaEntidade = null;

                if (cmb_tipo.SelectedIndex == 0) // Aluno
                {
                    if (txtCodAluno.Text == null || txtNomeAluno.Text == null || cmb_Turma.SelectedItem == null || cmb_Curso.SelectedItem == null || cmb_Ano.SelectedItem == null)
                    {
                        MessageBox.Show("Preencha todos os campos obrigatórios do Aluno!");
                        return;
                    }

                    int? codOri = null;
                    if (cmb_Orientador.SelectedItem != null)
                    {
                        var orientadorSelecionado = (Orientador)cmb_Orientador.SelectedItem;
                        if (orientadorSelecionado.Cod_Orientador != 0) codOri = orientadorSelecionado.Cod_Orientador;
                    }

                    novaEntidade = new Aluno(
                        Convert.ToInt32(txtCodAluno.Text),
                        txtNomeAluno.Text.Trim(),
                        cmb_Turma.SelectedValue.ToString(),
                        Convert.ToInt32(cmb_Curso.SelectedValue),
                        "Não Pronto",
                        codOri,
                        cmb_Ano.SelectedValue.ToString()
                    );
                }
                else // Orientador
                {
                    if (txtNomeOri.Text == null)
                    {
                        MessageBox.Show("Preencha o nome do Orientador!");
                        return;
                    }

                    novaEntidade = new Orientador(0, txtNomeOri.Text.Trim());
                }

                if (novaEntidade != null)
                {
                    novaEntidade.InserirNaBD(caminho);
                    MessageBox.Show("Registo guardado com sucesso!");

                    if (novaEntidade.GetType() == typeof(Orientador)) CarregarCombos();
                    
                    LimparCampos();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao guardar dados (Polimorfismo): " + ex.Message);
            }
        }

        private void LimparCampos()
        {
            txtCodAluno.Text = string.Empty;
            txtNomeAluno.Text = string.Empty;
            txtNomeOri.Text = string.Empty;
            cmb_Turma.SelectedIndex = -1;
            cmb_Curso.SelectedIndex = -1;
            cmb_Orientador.SelectedIndex = -1;
            cmb_Ano.SelectedIndex = -1;
        }

        private void Btn_Click_Limpar(object sender, RoutedEventArgs e)
        {
            try
            {
                LimparCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao limpar campos: " + ex.Message);
            }
        }
    }
}
