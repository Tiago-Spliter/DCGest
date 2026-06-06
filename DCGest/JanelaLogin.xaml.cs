using System;
using System.Windows;
using DCGest.Classes;

namespace DCGest
{
    public partial class JanelaLogin : Window
    {
        public JanelaLogin()
        {
            InitializeComponent();
        }

        private void Btn_Click_Entrar(object sender, RoutedEventArgs e)
        {
            try
            {
                string loginInput = txt_user.Text.Trim();
                string pass = txt_pass.Password;

                if (string.IsNullOrEmpty(loginInput) || string.IsNullOrEmpty(pass))
                {
                    MessageBox.Show("Preencha todos os campos!");
                    return;
                }

                DiretorCurso dc = Autenticacao.Verificar(loginInput, pass, out string erro);
                if (dc != null)
                {
                    Sessao.UtilizadorLogado = dc;
                    Sessao.Login = loginInput;
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(erro);
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
