namespace DCGest.Models
{
    public class Autenticacao
    {
        public int Cod_Aut { get; set; }
        public string Utilizador { get; set; } = string.Empty;
        public string PalavraPasse { get; set; } = string.Empty;
    }
}
