namespace DCGest.Classes
{
    public class Alinea
    {
        public int Cod_Alinea { get; set; }
        public string AlineaLetra { get; set; } = string.Empty;
        public string Regra { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;

        public string Display => string.IsNullOrWhiteSpace(AlineaLetra)
            ? Regra
            : $"{AlineaLetra} – {Regra}";
    }
}
