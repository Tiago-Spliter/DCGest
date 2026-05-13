namespace DCGest
{
    /// <summary>
    /// Classe central para configurações da Base de Dados.
    /// </summary>
    public static class BD
    {
        /// <summary>
        /// Caminho de ligação à base de dados MySQL.
        /// </summary>
        public static readonly string CaminhoBD = "Server=localhost;Database=pap;User=root;Password=rootroot";

        /// <summary>
        /// Converte um valor para o formato aceite pelo MySQL (trata null como DBNull).
        /// </summary>
        public static object ParaDB(this object valor)
        {
            return valor ?? System.DBNull.Value;
        }
    }
}
