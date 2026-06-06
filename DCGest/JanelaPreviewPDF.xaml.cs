using System;
using System.Windows;

namespace DCGest
{
    public partial class JanelaPreviewPDF : Window
    {
        public JanelaPreviewPDF(string caminhoPdf)
        {
            InitializeComponent();

            pdfViewer.Navigate(new Uri(caminhoPdf));
        }
    }
}
