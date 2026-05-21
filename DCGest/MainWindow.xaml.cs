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
    }
}