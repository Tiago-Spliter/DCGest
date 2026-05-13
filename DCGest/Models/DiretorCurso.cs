namespace DCGest.Models
{
    public class DiretorCurso
    {
        public int Cod_DC { get; set; }
        public string Nome_DC { get; set; } = string.Empty;
        public int Cod_Curso { get; set; }
        public int Cod_Aut { get; set; }
    }
}
