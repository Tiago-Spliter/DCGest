using System;

namespace DCGest.Classes
{
    public class Disciplina
    {
        public int Cod_Disc { get; set; }
        public string Designacao { get; set; } = string.Empty;
        public string Ano { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public int Cod_Curso { get; set; }


        public Disciplina() 
        {
        
        }

        public Disciplina(int codDisc, string designacao, string ano, string tipo, int codCurso)
        {
            Cod_Disc = codDisc;
            Designacao = designacao;
            Ano = ano;
            Tipo = tipo;
            Cod_Curso = codCurso;
        }
    }
}
