namespace FileHelper
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (StringWriter log = new StringWriter())
            {
                string arquivoLog = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\LogConsulta.txt";
                Console.ForegroundColor = ConsoleColor.Green;
                bool stmenu = false;
                bool stPasta = false;
                bool stTipo = false;
                bool stNome = false;
                bool stFiltroCorropido = false;
                int opcao = 0;
                string pasta = string.Empty;
                string tipo = "*.*";
                string nome = string.Empty;
                bool stTamanho = false;
                bool stMesmoNome = false;

                while (!stmenu)
                {
                    Console.Clear();
                    WriteLine("########## Escolha opção ########## ", log);
                    WriteLine("1 - Buscar arquivos corrompidos", log);
                    WriteLine("2 - Buscar arquivo por tipo", log);
                    WriteLine("3 - Buscar arquivo por nome", log);
                    WriteLine("4 - Buscar arquivos mesmo tamanho", log);
                    WriteLine("5 - Buscar arquivos mesmo nome", log);
                    string opcaoEscolhida = ReadLine(log);
                    int.TryParse(opcaoEscolhida, out opcao);
                    stmenu = opcao <= 5 && opcao > 0;
                }
                while (!stPasta)
                {
                    Write("Informe o diretório: ", log);
                    pasta = ReadLine(log);
                    stPasta = !string.IsNullOrEmpty(pasta) && Directory.Exists(pasta);
                    MsgErro(stPasta, "Diretório inválido!", log);

                }

                switch (opcao)
                {
                    case 1:
                        stFiltroCorropido = true;
                        break;

                    case 2:
                        while (!stTipo)
                        {
                            Write("Informe o tipo: ", log);
                            tipo = ReadLine(log);
                            stTipo = !string.IsNullOrEmpty(tipo);
                            MsgErro(stTipo, "Tipo inválido!", log);

                        }
                        break;
                    case 3:
                        while (!stNome)
                        {
                            Write("Informe o nome: ", log);
                            nome = ReadLine(log);
                            stNome = !string.IsNullOrEmpty(nome);
                            MsgErro(stNome, "Nome inválido!", log);
                        }
                        break;
                    case 4:
                        stTamanho = true;
                        break;
                    case 5:
                        stMesmoNome = true;
                        break;

                    default:
                        break;
                }

                List<Arquivo> listaArquivo = new List<Arquivo>();

                List<FileInfo> listaFileInfo = Directory.GetFiles(pasta, tipo, SearchOption.AllDirectories).Select(c => new FileInfo(c)).ToList();
                WriteLine(@$"Total arquivos na pasta --> {listaFileInfo.Count}", log);
                listaFileInfo = !string.IsNullOrEmpty(nome) ? listaFileInfo.Where(c => c.Name.ToLower().Contains(nome.ToLower())).ToList() : listaFileInfo;

                int totalFiltro = listaFileInfo.Count;
                int skip = 0;
                List<ThreadArquivo> listaThreadArquivo = new List<ThreadArquivo>();
                if (stTamanho || stFiltroCorropido)
                {
                    while (totalFiltro > 0)
                    {
                        List<FileInfo> listaSkip = listaFileInfo.Skip(skip).Take(30).ToList();

                        ThreadArquivo threadArquivo = new ThreadArquivo();
                        threadArquivo.ListaFileInfo = listaSkip;

                        listaThreadArquivo.Add(threadArquivo);

                        skip += 100;
                        totalFiltro -= threadArquivo.ListaFileInfo.Count;
                    }
                }
                else
                {
                    ThreadArquivo threadArquivo = new ThreadArquivo();
                    threadArquivo.ListaFileInfo = listaFileInfo;
                    listaThreadArquivo.Add(threadArquivo);
                }

                Parallel.ForEach(listaThreadArquivo, threadArquivo =>
                {
                    foreach (var fileInfo in threadArquivo.ListaFileInfo)
                    {
                        try
                        {

                            Arquivo arquivo = new Arquivo();
                            arquivo.Pasta = fileInfo.Directory.FullName;
                            arquivo.Nome = fileInfo.Name;
                            if (stTamanho || stFiltroCorropido)
                            {
                                arquivo.StLeituraArquivo = true;
                                arquivo.Tamanho = new FileInfo(fileInfo.FullName).Length;

                            }
                            threadArquivo.ListaArquivo.Add(arquivo);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                });
                foreach (var threadArquivo in listaThreadArquivo)
                {
                    listaArquivo.AddRange(threadArquivo.ListaArquivo);
                }

                WriteLine("########## Resultado ##########", log);
                WriteLine("", log);
                listaArquivo = stFiltroCorropido ? listaArquivo.Where(c => !c.Ok).ToList() : listaArquivo;
                if (stTamanho)
                {
                    var listaTamanho = listaArquivo.GroupBy(x => x.Tamanho.ToString()).Where(g => g.Count() > 1).ToList();
                    foreach (var itemMesmoTamamho in listaTamanho)
                        MostrarResultadoAgrupadoArquivo(itemMesmoTamamho, log);

                }
                else if (stMesmoNome)
                {
                    var listaMesmoNome = listaArquivo.GroupBy(x => x.Nome).Where(g => g.Count() > 1).ToList();
                    foreach (var itemMesmoNome in listaMesmoNome)
                        MostrarResultadoAgrupadoArquivo(itemMesmoNome, log);

                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    WriteLine(@$"Total Arquivos ->{listaArquivo.Count}", log);
                    Console.ForegroundColor = ConsoleColor.Green;
                    foreach (var item in listaArquivo)
                        MostrarResultadoArquivo(item, log);
                }
                Console.ForegroundColor = ConsoleColor.Yellow;
                WriteLine("******FIM*********", log);
                Console.ForegroundColor = ConsoleColor.Green;
                File.WriteAllText(arquivoLog, log.ToString());
            }

        }

        private static string ReadLine(StringWriter log)
        {
            string input = Console.ReadLine();
            log.WriteLine(input);
            return input;
        }

        private static void Write(string text, StringWriter log)
        {
            Console.Write(text);
            log.Write(text);
        }

        private static void WriteLine(string text, StringWriter log)
        {
            Console.WriteLine(text);
            log.WriteLine(text);
        }

        private static void MsgErro(bool stValidacao, string msgErro, StringWriter log)
        {
            if (!stValidacao)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                WriteLine(msgErro, log);
                Console.ForegroundColor = ConsoleColor.Green;
            }
        }

        private static void MostrarResultadoAgrupadoArquivo(IGrouping<string, Arquivo>? itemMesmoNome, StringWriter log)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            WriteLine(@$"{itemMesmoNome.Key} Total ->{itemMesmoNome.Count()}", log);
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (var item in itemMesmoNome)
            {
                MostrarResultadoArquivo(item, log);
            }
        }

        private static void MostrarResultadoArquivo(Arquivo item, StringWriter log)
        {
            WriteLine("***************", log);
            WriteLine($@"Nome: {item.Nome}", log);
            WriteLine($@"Pasta: {item.Pasta}", log);

            if (item.StLeituraArquivo)
            {
                WriteLine($@"Tamanho: {item.Tamanho}", log);
                Console.ForegroundColor = item.Ok ? ConsoleColor.Green : ConsoleColor.Red;
                string status = item.Ok ? "OK" : "CORROMPIDO";
                WriteLine($@"Status: {status}", log);
                Console.ForegroundColor = ConsoleColor.Green;
            }

            WriteLine("", log);

        }
    }
}