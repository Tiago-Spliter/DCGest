using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;
using DCGest.Classes;
using BCrypt.Net;

namespace DCGest
{
    public partial class PaginaPerfil : Page
    {
        string caminho = BD.CaminhoBD;

        public PaginaPerfil()
        {
            InitializeComponent();
            CarregarPerfil();
        }

        private void CarregarPerfil()
        {
            try
            {
                if (Sessao.UtilizadorLogado != null)
                {
                    txt_nome.Text = Sessao.UtilizadorLogado.Nome_DC;
                    txt_login.Text = Sessao.Login;

                    using (MySqlConnection conn = new MySqlConnection(caminho))
                    {
                        conn.Open();
                        string sql = "SELECT Nome_Curso FROM cursos WHERE Cod_Curso = @id";
                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", Sessao.UtilizadorLogado.Cod_Curso);
                            object resultado = cmd.ExecuteScalar();
                            if (resultado != null && resultado != DBNull.Value)
                            {
                                txt_curso.Text = resultado.ToString();
                            }
                            else
                            {
                                txt_curso.Text = "N/A";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar perfil: " + ex.Message);
            }
        }

        private void Btn_Click_AtualizarPass(object sender, RoutedEventArgs e)
        {
            try
            {
                string nova = txt_novaPass.Password;
                string confirma = txt_confirmaPass.Password;

                if (string.IsNullOrEmpty(nova) || string.IsNullOrEmpty(confirma))
                {
                    MessageBox.Show("Preencha ambos os campos de palavra-passe!");
                    return;
                }

                if (nova != confirma)
                {
                    MessageBox.Show("As palavras-passe não coincidem!");
                    return;
                }

                MessageBoxResult res = MessageBox.Show("Tem a certeza que deseja alterar a sua palavra-passe?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No)
                {
                    return;
                }

                using (MySqlConnection conn = new MySqlConnection(caminho))
                {
                    conn.Open();

                    string hash = BCrypt.Net.BCrypt.HashPassword(nova);

                    string sql = "UPDATE autenticacao SET PalavraPasse = @hash WHERE Cod_Aut = @id";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@hash", hash);
                        cmd.Parameters.AddWithValue("@id", Sessao.UtilizadorLogado!.Cod_Aut);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Palavra-passe atualizada com sucesso!");
                txt_novaPass.Clear();
                txt_confirmaPass.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao atualizar palavra-passe: " + ex.Message);
            }
        }
    }
}
