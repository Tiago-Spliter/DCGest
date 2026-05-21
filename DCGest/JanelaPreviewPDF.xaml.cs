using System;
using System.Windows;

namespace DCGest
{
    public partial class JanelaPreviewPDF : Window
    {
        public JanelaPreviewPDF(string caminhoPdf)
        {
            InitializeComponent();

            // O WebBrowser do WPF precisa de um URI absoluto
            pdfViewer.Navigate(new Uri(caminhoPdf));
        }
    }
}
