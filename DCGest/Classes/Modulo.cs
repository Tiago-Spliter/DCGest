using System;

namespace DCGest.Classes
{
    public class Modulo
    {
        public int Cod_Modulo { get; set; }
        public string Designacao { get; set; } = string.Empty;
        public int Cod_Disc { get; set; }


        public Modulo()
        {

        }

        public Modulo(int codModulo, string designacao, int codDisc)
        {
            Cod_Modulo = codModulo;
            Designacao = designacao;
            Cod_Disc = codDisc;
        }
    }
}
