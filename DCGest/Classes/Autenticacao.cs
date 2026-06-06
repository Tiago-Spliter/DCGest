using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace DCGest.Classes
{
    public class Autenticacao
    {
        public int Cod_Aut { get; set; }
        public string Utilizador { get; set; } = string.Empty;
        public string PalavraPasse { get; set; } = string.Empty;


        public Autenticacao()
        {

        }

        public Autenticacao(int codAut, string utilizador, string palavraPasse)
        {
            Cod_Aut = codAut;
            Utilizador = utilizador;
            PalavraPasse = palavraPasse;
        }
    }
}
