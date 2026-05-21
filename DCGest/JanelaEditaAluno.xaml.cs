using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using DCGest.Classes;

namespace DCGest
{
    public partial class JanelaEditaAluno : Window
    {
        string caminho = BD.CaminhoBD;
        int idAluno;

        // Listas 
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
                // 1. Carregar Combos usando métodos estáticos
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

                using (MySqlConnection conn = new MySqlConnection(caminho))
                {
                    conn.Open();

                    // 2. Buscar dados do aluno para preencher os campos
                    string sql_Aluno = "SELECT * FROM aluno WHERE Cod_Aluno = @id";
                    using (MySqlCommand cmd = new MySqlCommand(sql_Aluno, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", idAluno);
                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                txt_nome.Text = r["Nome_Aluno"].ToString();
                                cmb_turma.SelectedValue = Convert.ToInt32(r["Cod_Turma"]);
                                cmb_curso.SelectedValue = Convert.ToInt32(r["Cod_Curso"]);
                                cmb_anoletivo.SelectedValue = Convert.ToInt32(r["Cod_Letivo"]);
                                cmb_orientador.SelectedValue = r["Cod_Ori"] == DBNull.Value ? 0 : Convert.ToInt32(r["Cod_Ori"]);
                                
                                string estado = r["Estado_Estagio"].ToString();
                                foreach (System.Windows.Controls.ComboBoxItem item in cmb_estado.Items)
                                {
                                    if (item.Content.ToString() == estado)
                                    {
                                        cmb_estado.SelectedItem = item;
                                        break;
                                    }
                                }
                            }
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

                using (MySqlConnection conn = new MySqlConnection(caminho))
                {
                    conn.Open();
                    string sql = @"UPDATE aluno SET 
                                   Nome_Aluno = @nome, 
                                   Cod_Turma = @turma, 
                                   Cod_Curso = @curso, 
                                   Estado_Estagio = @estado, 
                                   Cod_Ori = @ori, 
                                   Cod_Letivo = @letivo 
                                   WHERE Cod_Aluno = @id";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@nome", txt_nome.Text.Trim());
                        cmd.Parameters.AddWithValue("@turma", cmb_turma.SelectedValue);
                        cmd.Parameters.AddWithValue("@curso", cmb_curso.SelectedValue);
                        cmd.Parameters.AddWithValue("@estado", ((System.Windows.Controls.ComboBoxItem)cmb_estado.SelectedItem).Content.ToString());
                        
                        int codOri = Convert.ToInt32(cmb_orientador.SelectedValue);
                        cmd.Parameters.AddWithValue("@ori", codOri == 0 ? (object)DBNull.Value : codOri);
                        
                        cmd.Parameters.AddWithValue("@letivo", cmb_anoletivo.SelectedValue);
                        cmd.Parameters.AddWithValue("@id", idAluno);

                        cmd.ExecuteNonQuery();
                    }
                }

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
