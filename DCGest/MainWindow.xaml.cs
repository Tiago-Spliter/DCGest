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

namespace DCGest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            InformacaoInicial InformacaoInicial = new InformacaoInicial();
            frm.Content = InformacaoInicial;
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
    }
}