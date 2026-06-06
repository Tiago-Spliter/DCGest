using System;
using System.Collections.Generic;
using System.Windows;
using DCGest.Classes;

namespace DCGest
{
    public partial class JanelaEditaAluno : Window
    {
        int idAluno;

        List<Turma> listaTurmas = new List<Turma>();
        List<Curso> listaCursos = new List<Curso>();
        List<Orientador> listaOrientadores = new List<Orientador>();
        List<AnoLetivo> listaAnos = new List<AnoLetivo>();

        public JanelaEditaAluno(int cod)
        {
            InitializeComponent();
            idAluno = cod;
            CarregarDados();
        }

        private void CarregarDados()
        {
            try
            {
                listaTurmas = Turma.ObterTodas();
                cmb_turma.ItemsSource = listaTurmas;
                cmb_turma.DisplayMemberPath = "Nome";
                cmb_turma.SelectedValuePath = "Cod_Turma";

                listaCursos = Curso.ObterTodos();
                cmb_curso.ItemsSource = listaCursos;
                cmb_curso.DisplayMemberPath = "Nome_Curso";
                cmb_curso.SelectedValuePath = "Cod_Curso";

                listaOrientadores = Orientador.ObterTodos();
                listaOrientadores.Insert(0, new Orientador(0, "Sem Orientador"));
                cmb_orientador.ItemsSource = listaOrientadores;
                cmb_orientador.DisplayMemberPath = "Nome_Orientador";
                cmb_orientador.SelectedValuePath = "Cod_Orientador";

                listaAnos = AnoLetivo.ObterTodos();
                cmb_anoletivo.ItemsSource = listaAnos;
                cmb_anoletivo.DisplayMemberPath = "Intervalo";
                cmb_anoletivo.SelectedValuePath = "Cod_Letivo";

                Aluno a = Aluno.ObterPorId(idAluno);
                if (a != null)
                {
                    txt_nome.Text = a.Nome_Aluno;
                    cmb_turma.SelectedValue = a.Cod_Turma;
                    cmb_curso.SelectedValue = a.Cod_Curso;
                    cmb_anoletivo.SelectedValue = a.Cod_Letivo;
                    cmb_orientador.SelectedValue = a.Cod_Ori ?? 0;
                    foreach (System.Windows.Controls.ComboBoxItem item in cmb_estado.Items)
                    {
                        if (item.Content.ToString() == a.Estado_Estagio)
                        {
                            cmb_estado.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar dados do aluno: " + ex.Message);
            }
        }

        private void Btn_Click_Salvar(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txt_nome.Text) || cmb_turma.SelectedValue == null || cmb_curso.SelectedValue == null || cmb_anoletivo.SelectedValue == null)
                {
                    MessageBox.Show("Preencha todos os campos obrigatórios!");
                    return;
                }

                int codOri = Convert.ToInt32(cmb_orientador.SelectedValue);
                var aluno = new Aluno
                {
                    Cod_Aluno      = idAluno,
                    Nome_Aluno     = txt_nome.Text.Trim(),
                    Cod_Turma      = Convert.ToInt32(cmb_turma.SelectedValue),
                    Cod_Curso      = Convert.ToInt32(cmb_curso.SelectedValue),
                    Estado_Estagio = ((System.Windows.Controls.ComboBoxItem)cmb_estado.SelectedItem).Content.ToString(),
                    Cod_Ori        = codOri == 0 ? (int?)null : codOri,
                    Cod_Letivo     = Convert.ToInt32(cmb_anoletivo.SelectedValue)
                };

                aluno.AtualizarNaBD();
                MessageBox.Show("Dados do aluno atualizados com sucesso!");
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar alterações: " + ex.Message);
            }
        }

        private void Btn_Click_Cancelar(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
