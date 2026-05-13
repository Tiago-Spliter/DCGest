using System;

namespace DCGest.Models
{
    public class NotaModulo
    {
        public int Cod_NotaMod { get; set; }
        public int Cod_Aluno { get; set; }
        public int Cod_Modulo { get; set; }
        public int? Valor { get; set; }
        public DateTime? Data_Efetua { get; set; }
        public string Ano { get; set; } = string.Empty;
        public string NomeModulo { get; set; } = string.Empty;
        public string NomeDisciplina { get; set; } = string.Empty;
    }
}
