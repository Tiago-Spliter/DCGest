using MySql.Data.MySqlClient;
using System;
using System.Windows;
using BCrypt.Net;
using DCGest.Classes;

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
                string loginInput = txt_user.Text.Trim();
                string pass = txt_pass.Password;

                if (string.IsNullOrEmpty(loginInput) || string.IsNullOrEmpty(pass))
                {
                    MessageBox.Show("Preencha todos os campos!");
                    return;
                }

                using (MySqlConnection conn = new MySqlConnection(caminho))
                {
                    conn.Open();

                    // 1. Procurar na tabela Autenticacao
                    string sql = "SELECT * FROM autenticacao WHERE Utilizador = @user";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", loginInput);

                        using (MySqlDataReader rAut = cmd.ExecuteReader())
                        {
                            if (rAut.Read())
                            {
                                string hashNaBD = rAut["PalavraPasse"].ToString();
                                int codAut = Convert.ToInt32(rAut["Cod_Aut"]);

                                // VERIFICAÇÃO BCRYPT
                                if (BCrypt.Net.BCrypt.Verify(pass, hashNaBD))
                                {
                                    rAut.Close(); // Fechar para poder fazer nova consulta

                                    // 2. Buscar dados do Diretor de Curso associado
                                    string sqlDC = "SELECT * FROM diretor_curso WHERE Cod_Aut = @codAut";
                                    using (MySqlCommand cmdDC = new MySqlCommand(sqlDC, conn))
                                    {
                                        cmdDC.Parameters.AddWithValue("@codAut", codAut);
                                        using (MySqlDataReader rDC = cmdDC.ExecuteReader())
                                        {
                                            if (rDC.Read())
                                            {
                                                // GUARDAR NA SESSÃO (pt-PT: UtilizadorLogado)
                                                Sessao.UtilizadorLogado = new DiretorCurso(
                                                    Convert.ToInt32(rDC["Cod_DC"]),
                                                    rDC["Nome_DC"].ToString(),
                                                    Convert.ToInt32(rDC["Cod_Curso"]),
                                                    codAut
                                                );
                                                Sessao.Login = loginInput;

                                                this.DialogResult = true;
                                                this.Close();
                                            }
                                            else
                                            {
                                                MessageBox.Show("Atenção: Utilizador autenticado mas sem perfil de Diretor associado!");
                                            }
                                        }
                                    }
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
