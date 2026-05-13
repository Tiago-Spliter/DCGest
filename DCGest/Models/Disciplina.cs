namespace DCGest.Models
{
    public class Disciplina
    {
        public int Cod_Disc { get; set; }
        public string Designacao { get; set; } = string.Empty;
        public string Ano { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public int Cod_Curso { get; set; }
    }
}
