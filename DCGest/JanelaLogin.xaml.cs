using MySql.Data.MySqlClient;
using System;
using System.Windows;
using BCrypt.Net;

namespace DCGest
{
    public partial class JanelaLogin : Window
    {
        string caminho = BD.CaminhoBD;

        public JanelaLogin()
        {
            InitializeComponent();
        }

        private void Btn_Click_Entrar(object sender, RoutedEventArgs e)
        {
            try
            {
                string user = txt_user.Text.Trim();
                string pass = txt_pass.Password;

                if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                {
                    MessageBox.Show("Preencha todos os campos!");
                    return;
                }

                using (MySqlConnection conexao = new MySqlConnection(caminho))
                {
                    conexao.Open();
                    string sql = "SELECT PalavraPasse FROM autenticacao WHERE Utilizador = @user";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conexao))
                    {
                        cmd.Parameters.AddWithValue("@user", user);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            string hashNaBD = result.ToString();

                            if (BCrypt.Net.BCrypt.Verify(pass, hashNaBD))
                            {
                                this.DialogResult = true;
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Credenciais inválidas!");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Credenciais inválidas!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao fazer login: " + ex.Message);
            }
        }

        private void Btn_Click_Sair(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
