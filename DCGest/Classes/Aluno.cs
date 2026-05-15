using System;

namespace DCGest.Classes
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


        public string Nome_Curso { get; set; } = string.Empty;
        public string Nome_Orientador { get; set; } = string.Empty;


        public Aluno() 
        {
        
        }

        public Aluno(int codAluno, string nomeAluno, string turma, int codCurso, string estadoEstagio, int? codOri, string anoLetivo)
        {
            Cod_Aluno = codAluno;
            Nome_Aluno = nomeAluno;
            Turma = turma;
            Cod_Curso = codCurso;
            Estado_Estagio = estadoEstagio;
            Cod_Ori = codOri;
            Ano_Letivo = anoLetivo;
        }

        public Aluno(int codAluno, string nomeAluno, string turma, int codCurso, string estadoEstagio, int? codOri, string anoLetivo, string nomeCurso, string nomeOrientador)
        {
            Cod_Aluno = codAluno;
            Nome_Aluno = nomeAluno;
            Turma = turma;
            Cod_Curso = codCurso;
            Estado_Estagio = estadoEstagio;
            Cod_Ori = codOri;
            Ano_Letivo = anoLetivo;
            Nome_Curso = nomeCurso;
            Nome_Orientador = nomeOrientador;
        }
    }
}
