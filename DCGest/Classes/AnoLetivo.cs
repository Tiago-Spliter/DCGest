namespace DCGest.Classes
{
    public class AnoLetivo
    {
        public int Cod_Letivo { get; set; }
        public string Intervalo { get; set; } = string.Empty;


        public AnoLetivo() 
        {
        
        }

        public AnoLetivo(int codLetivo, string intervalo)
        {
            Cod_Letivo = codLetivo;
            Intervalo = intervalo;
        }
    }
}
