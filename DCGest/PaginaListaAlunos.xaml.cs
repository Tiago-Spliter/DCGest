using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
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
                int? codLetivo = cmb_anoletivo.SelectedValue != null && (int)cmb_anoletivo.SelectedValue != 0
                    ? (int?)cmb_anoletivo.SelectedValue : null;
                int? codCurso = cmb_curso.SelectedValue != null && (int)cmb_curso.SelectedValue != 0
                    ? (int?)cmb_curso.SelectedValue : null;

                listaAlunos = Aluno.ObterComFiltros(codLetivo: codLetivo, codCurso: codCurso);
                dg_alunos.ItemsSource = null;
                dg_alunos.ItemsSource = listaAlunos;
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
                int? codAluno = int.TryParse(txt_codigo.Text, out int cod) ? (int?)cod : null;
                string nomeAluno = !string.IsNullOrEmpty(txt_nome.Text) ? txt_nome.Text : null;
                int? codTurma = cmb_turma.SelectedValue != null && (int)cmb_turma.SelectedValue != 0
                    ? (int?)cmb_turma.SelectedValue : null;
                int? codOrientador = cmb_orientador.SelectedValue != null && (int)cmb_orientador.SelectedValue != 0
                    ? (int?)cmb_orientador.SelectedValue : null;

                listaAlunos = Aluno.ObterComFiltros(codTurma: codTurma, codOrientador: codOrientador, codAluno: codAluno, nomeAluno: nomeAluno);
                dg_alunos.ItemsSource = null;
                dg_alunos.ItemsSource = listaAlunos;
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
