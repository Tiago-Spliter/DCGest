using System;

namespace DCGest.Classes
{
    public class DiretorCurso
    {
        public int Cod_DC { get; set; }
        public string Nome_DC { get; set; } = string.Empty;
        public int Cod_Curso { get; set; }
        public int Cod_Aut { get; set; }


        public DiretorCurso() 
        {
        
        }

        public DiretorCurso(int codDC, string nomeDC, int codCurso, int codAut)
        {
            Cod_DC = codDC;
            Nome_DC = nomeDC;
            Cod_Curso = codCurso;
            Cod_Aut = codAut;
        }
    }
}
