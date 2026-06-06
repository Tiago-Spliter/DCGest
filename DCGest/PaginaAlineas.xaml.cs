using DCGest.Classes;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace DCGest
{
    public partial class PaginaAlineas : UserControl
    {
        public PaginaAlineas()
        {
            InitializeComponent();
            CarregarAlineas();
        }

        private void CarregarAlineas()
        {
            try
            {
                dg_alineas.ItemsSource = new ObservableCollection<Alinea>(Alinea.ObterTodas());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar alíneas: " + ex.Message);
            }
        }

        private void Btn_Guardar_Click(object sender, RoutedEventArgs e)
        {
            var lista = (ObservableCollection<Alinea>)dg_alineas.ItemsSource;
            try
            {
                Alinea.GuardarTodas(lista);
                MessageBox.Show("Alíneas guardadas com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao guardar alíneas: " + ex.Message);
            }
        }
    }
}
