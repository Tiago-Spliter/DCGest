using System;

namespace DCGest.Classes
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
        public string TipoDisciplina { get; set; } = string.Empty;


        public NotaModulo() 
        {
        
        }

        public NotaModulo(int codNotaMod, int codAluno, int codModulo, int? valor, DateTime? dataEfetua, string ano)
        {
            Cod_NotaMod = codNotaMod;
            Cod_Aluno = codAluno;
            Cod_Modulo = codModulo;
            Valor = valor;
            Data_Efetua = dataEfetua;
            Ano = ano;
        }

        public NotaModulo(int codNotaMod, int codAluno, int codModulo, int? valor, DateTime? dataEfetua, string ano, string nomeModulo, string nomeDisciplina, string tipoDisciplina)
        {
            Cod_NotaMod = codNotaMod;
            Cod_Aluno = codAluno;
            Cod_Modulo = codModulo;
            Valor = valor;
            Data_Efetua = dataEfetua;
            Ano = ano;
            NomeModulo = nomeModulo;
            NomeDisciplina = nomeDisciplina;
            TipoDisciplina = tipoDisciplina;
        }
    }
}
