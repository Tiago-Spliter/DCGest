using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using DCGest.Classes;

namespace DCGest
{
    public partial class PaginaAdiciona : Page
    {
        public PaginaAdiciona()
        {
            InitializeComponent();


            cmb_tipo.SelectedIndex = 0;
            CarregarCombos();
        }

        string caminho = BD.CaminhoBD;

        List<Turma> listaTurmas = new List<Turma>();
        List<Curso> listaCursos = new List<Curso>();
        List<Orientador> listaOrientadores = new List<Orientador>();
        List<AnoLetivo> listaAnos = new List<AnoLetivo>();

        private void Cmb_Tipo_Seleciona(object sender, SelectionChangedEventArgs e)
        {
            if (painelAluno == null || painelOrientador == null || painelDC == null)
            {
                return;
            }

            painelAluno.Visibility = Visibility.Collapsed;
            painelOrientador.Visibility = Visibility.Collapsed;
            painelDC.Visibility = Visibility.Collapsed;

            if (cmb_tipo.SelectedIndex == 0)
            {
                painelAluno.Visibility = Visibility.Visible;
            }
            else if (cmb_tipo.SelectedIndex == 1)
            {
                painelOrientador.Visibility = Visibility.Visible;
            }
            else if (cmb_tipo.SelectedIndex == 2)
            {
                painelDC.Visibility = Visibility.Visible;
            }
        }

        private void CarregarCombos()
        {
            try
            {
                listaTurmas = Turma.ObterTodas();
                cmb_Turma.ItemsSource = null;
                cmb_Turma.ItemsSource = listaTurmas;
                if (listaTurmas.Count > 0)
                {
                    cmb_Turma.SelectedIndex = 0;
                }

                listaCursos = Curso.ObterTodos();
                cmb_Curso.ItemsSource = null;
                cmb_Curso.ItemsSource = listaCursos;
                if (listaCursos.Count > 0)
                {
                    cmb_Curso.SelectedIndex = 0;
                }

                cmb_CursoDC.ItemsSource = null;
                cmb_CursoDC.ItemsSource = listaCursos;
                if (listaCursos.Count > 0)
                {
                    cmb_CursoDC.SelectedIndex = 0;
                }

                listaOrientadores = Orientador.ObterTodos();
                listaOrientadores.Insert(0, new Orientador(0, "Sem Orientador"));
                cmb_Orientador.ItemsSource = null;
                cmb_Orientador.ItemsSource = listaOrientadores;
                cmb_Orientador.SelectedIndex = 0;

                listaAnos = AnoLetivo.ObterTodos();
                cmb_Ano.ItemsSource = null;
                cmb_Ano.ItemsSource = listaAnos;
                if (listaAnos.Count > 0)
                {
                    cmb_Ano.SelectedIndex = 0;
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
                if (cmb_tipo.SelectedIndex == 0)
                {
                    if (string.IsNullOrEmpty(txtCodAluno.Text) || string.IsNullOrEmpty(txtNomeAluno.Text) || cmb_Turma.SelectedItem == null || cmb_Curso.SelectedItem == null || cmb_Ano.SelectedItem == null)
                    {
                        MessageBox.Show("Preencha todos os campos obrigatórios do Aluno!");
                        return;
                    }

                    if (Regex.IsMatch(txtCodAluno.Text, @"\D"))
                    {
                        MessageBox.Show("O código do aluno deve conter apenas números!");
                        return;
                    }

                    if (!Regex.IsMatch(txtNomeAluno.Text, @"^[\p{L}\s]+$"))
                    {
                        MessageBox.Show("O nome do aluno deve conter apenas letras!");
                        return;
                    }

                    int? codOri = null;
                    if (cmb_Orientador.SelectedItem != null)
                    {
                        Orientador orientadorSelecionado = (Orientador)cmb_Orientador.SelectedItem;
                        if (orientadorSelecionado.Cod_Orientador != 0)
                        {
                            codOri = orientadorSelecionado.Cod_Orientador;
                        }
                    }
                    Aluno novoAluno = new Aluno(
                        Convert.ToInt32(txtCodAluno.Text),
                        txtNomeAluno.Text.Trim(),
                        Convert.ToInt32(cmb_Turma.SelectedValue),
                        Convert.ToInt32(cmb_Curso.SelectedValue),
                        "Não Pronto",
                        codOri,
                        Convert.ToInt32(cmb_Ano.SelectedValue)
                    );

                    MessageBoxResult resAluno = MessageBox.Show("Deseja confirmar a gravação deste Aluno?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (resAluno == MessageBoxResult.Yes)
                    {
                        novoAluno.InserirNaBD(caminho);
                        MessageBox.Show("Aluno guardado com sucesso!");
                        LimparCampos();
                    }
                }
                else if (cmb_tipo.SelectedIndex == 1)
                {
                    if (string.IsNullOrEmpty(txtNomeOri.Text))
                    {
                        MessageBox.Show("Preencha o nome do Orientador!");
                        return;
                    }

                    if (!Regex.IsMatch(txtNomeOri.Text, @"^[\p{L}\s]+$"))
                    {
                        MessageBox.Show("O nome do orientador deve conter apenas letras!");
                        return;
                    }

                    Orientador novoOri = new Orientador(0, txtNomeOri.Text.Trim());

                    MessageBoxResult resOri = MessageBox.Show("Deseja confirmar a gravação deste Orientador?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (resOri == MessageBoxResult.Yes)
                    {
                        novoOri.InserirNaBD(caminho);
                        MessageBox.Show("Orientador guardado com sucesso!");
                        CarregarCombos();
                        LimparCampos();
                    }
                }
                else if (cmb_tipo.SelectedIndex == 2)
                {
                    if (string.IsNullOrEmpty(txtNomeDC.Text) || string.IsNullOrEmpty(txtUserDC.Text) || string.IsNullOrEmpty(txtPassDC.Password) || cmb_CursoDC.SelectedItem == null)
                    {
                        MessageBox.Show("Preencha todos os campos obrigatórios do Diretor!");
                        return;
                    }

                    MessageBoxResult resDC = MessageBox.Show("Deseja confirmar a gravação deste Diretor de Curso?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (resDC == MessageBoxResult.Yes)
                    {
                        InserirDiretorCurso();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao guardar dados: " + ex.Message);
            }
        }

        private void InserirDiretorCurso()
        {
            try
            {
                var dc = new DiretorCurso(0, txtNomeDC.Text.Trim(), Convert.ToInt32(cmb_CursoDC.SelectedValue), 0);
                dc.InserirNaBD(txtUserDC.Text.Trim(), txtPassDC.Password, caminho);
                MessageBox.Show("Diretor de Curso e Credenciais criados com sucesso!");
                LimparCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao criar Diretor: " + ex.Message);
            }
        }

        private void LimparCampos()
        {
            txtCodAluno.Text = string.Empty;
            txtNomeAluno.Text = string.Empty;
            txtNomeOri.Text = string.Empty;
            txtNomeDC.Text = string.Empty;
            txtUserDC.Text = string.Empty;
            txtPassDC.Clear();
            cmb_Turma.SelectedIndex = -1;
            cmb_Curso.SelectedIndex = -1;
            cmb_CursoDC.SelectedIndex = -1;
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
