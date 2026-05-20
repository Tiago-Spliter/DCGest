namespace DCGest.Classes
{
    public class MediaDisciplina
    {
        public string NomeDisciplina { get; set; } = string.Empty;
        public string TipoDisciplina { get; set; } = string.Empty;
        public double Media { get; set; }
        public int ModulosRealizados { get; set; }
        public string Situacao { get; set; } = "Em curso";


        public MediaDisciplina() 
        {
        
        }

        public MediaDisciplina(string nomeDisciplina, string tipoDisciplina, double media, int modulosRealizados, string situacao)
        {
            NomeDisciplina = nomeDisciplina;
            TipoDisciplina = tipoDisciplina;
            Media = media;
            ModulosRealizados = modulosRealizados;
            Situacao = situacao;
        }
    }
}
