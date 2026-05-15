using System;

namespace DCGest.Classes
{
    public class Curso
    {
        public int Cod_Curso { get; set; }
        public string Nome_Curso { get; set; } = string.Empty;


        public Curso() 
        {
        
        }

        public Curso(int codCurso, string nomeCurso)
        {
            Cod_Curso = codCurso;
            Nome_Curso = nomeCurso;
        }
    }
}
