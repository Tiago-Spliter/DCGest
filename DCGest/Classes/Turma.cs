namespace DCGest.Classes
{
    public class Turma
    {
        public int Cod_Turma { get; set; }
        public string Nome { get; set; } = string.Empty;


        public Turma() 
        {
        
        }

        public Turma(int codTurma, string nome)
        {
            Cod_Turma = codTurma;
            Nome = nome;
        }
    }
}
