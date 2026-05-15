using System;

namespace DCGest.Classes
{
    public class Orientador
    {
        public int Cod_Orientador { get; set; }
        public string Nome_Orientador { get; set; } = string.Empty;


        public Orientador() 
        {
        
        }

        public Orientador(int codOrientador, string nomeOrientador)
        {
            Cod_Orientador = codOrientador;
            Nome_Orientador = nomeOrientador;
        }
    }
}
