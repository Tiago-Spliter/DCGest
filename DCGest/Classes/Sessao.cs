using DCGest.Classes;

namespace DCGest.Classes
{
    public static class Sessao
    {
        public static DiretorCurso? UtilizadorLogado { get; set; }

        public static string Login { get; set; } = string.Empty;
    }
}
