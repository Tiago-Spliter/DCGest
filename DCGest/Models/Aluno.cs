using System;

namespace DCGest.Models
{
    public class Aluno
    {
        public int Cod_Aluno { get; set; }
        public string Nome_Aluno { get; set; } = string.Empty;
        public string Turma { get; set; } = string.Empty;
        public int Cod_Curso { get; set; }
        public string Estado_Estagio { get; set; } = string.Empty;
        public int? Cod_Ori { get; set; }
        public string Ano_Letivo { get; set; } = string.Empty;
    }
}
