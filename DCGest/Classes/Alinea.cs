namespace DCGest.Classes
{
    public class Alinea
    {
        public int Cod_Alinea { get; set; }
        public string AlineaLetra { get; set; } = string.Empty;
        public string Regra { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;

        public Alinea() { }

        public Alinea(int codAlinea, string alineaLetra, string regra, string descricao)
        {
            Cod_Alinea = codAlinea;
            AlineaLetra = alineaLetra;
            Regra = regra;
            Descricao = descricao;
        }
    }
}
