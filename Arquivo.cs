namespace FileHelper
{
    internal class Arquivo
    {
        public string Pasta { get; set; }
        public string Nome { get; set; }
        public long Tamanho { get; set; }
        public bool Ok { get { return Tamanho > 1024; } }
        public bool StLeituraArquivo { get; set; }
    }
}