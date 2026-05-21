using DCGest.Classes;

namespace DCGest.Classes
{
    public static class Sessao
    {
        // Guarda os dados do Diretor que fez login
        public static DiretorCurso? UtilizadorLogado { get; set; }
        
        // Guarda o nome de utilizador (Login) para display
        public static string Login { get; set; } = string.Empty;
    }
}
