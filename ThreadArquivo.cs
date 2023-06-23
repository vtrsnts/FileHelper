namespace FileHelper
{
    internal class ThreadArquivo
    {
        public ThreadArquivo()
        {
            ListaArquivo = new List<Arquivo>();
            ListaFileInfo = new List<FileInfo>();
        }
        public List<FileInfo> ListaFileInfo { get; set; }
        public List<Arquivo> ListaArquivo { get; set; }
    }
}