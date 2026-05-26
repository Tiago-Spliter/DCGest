using System.Text;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            JanelaLogin login = new JanelaLogin();
            if (login.ShowDialog() == true)
            {
                InitializeComponent();

                // Mostrar nome do utilizador logado no cabeçalho (pt-PT)
                if (Sessao.UtilizadorLogado != null)
                {
                    lbl_UserNome.Text = Sessao.UtilizadorLogado.Nome_DC;
                }

                frm.Content = new InformacaoInicial();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private void Btn_Click_PaginaInicial(object sender, RoutedEventArgs e)
        {
            InformacaoInicial InformacaoInicial = new InformacaoInicial();
            frm.Content = InformacaoInicial;
        }

        private void Btn_Click_PaginaListaAlunos(object sender, RoutedEventArgs e)
        {
            PaginaListaAlunos paginaListaAlunos = new PaginaListaAlunos();
            frm.Content = paginaListaAlunos;
        }

        private void Btn_Click_PaginaAdiciona(object sender, RoutedEventArgs e)
        {
            PaginaAdiciona paginaAdiciona = new PaginaAdiciona();
            frm.Content = paginaAdiciona;
        }

        private void Btn_Click_Perfil(object sender, RoutedEventArgs e)
        {
            PaginaPerfil paginaPerfil = new PaginaPerfil();
            frm.Content = paginaPerfil;
        }

        private void Btn_Click_Logout(object sender, RoutedEventArgs e)
        {
            MessageBoxResult resultado = MessageBox.Show("Tem a certeza que deseja terminar a sessão?", "Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                // Limpar dados da sessão
                Sessao.UtilizadorLogado = null;
                Sessao.Login = string.Empty;

                // Esconder a janela principal
                this.Hide();

                // Abrir novamente a janela de login
                JanelaLogin janelaLogin = new JanelaLogin();

                if (janelaLogin.ShowDialog() == true)
                {
                    // Se o login for bem-sucedido, atualizamos a interface e mostramos a janela
                    if (Sessao.UtilizadorLogado != null)
                    {
                        lbl_UserNome.Text = Sessao.UtilizadorLogado.Nome_DC;
                    }

                    frm.Content = new InformacaoInicial();
                    this.Show();
                }
                else
                {
                    // Se cancelar o login, fecha a aplicação
                    Application.Current.Shutdown();
                }
            }
        }
    }
}