using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using XMLAppReader.Models;

namespace XMLAppReader
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async Task<(string CaminhoJson, int TotalArquivos, int NotasValidas)> VerificarXMLsAsync(
            string diretorio,
            string serieDesejada,
            string competenciaDesejada)
        {
            if (!Directory.Exists(diretorio))
            {
                await DisplayAlertAsync(
                    "Aviso",
                    $"A pasta {diretorio} não foi encontrada.",
                    "Ok"
                );

                return ("", 0, 0);
            }

            string[] arquivosXml = Directory.GetFiles(diretorio, "*.xml");

            List<NotaEnvio> notasEncontradas = new List<NotaEnvio>();

            foreach (var arquivoXML in arquivosXml)
            {
                string arquivoNome = Path.GetFileName(arquivoXML);

                try
                {
                    XDocument doc = XDocument.Load(arquivoXML);

                    NotaEnvio? nota = RegraXML(
                        doc,
                        serieDesejada,
                        competenciaDesejada
                    );

                    if (nota != null)
                    {
                        notasEncontradas.Add(nota);

                        Console.WriteLine($"XML aprovado: {arquivoNome}");
                    }
                    else
                    {
                        Console.WriteLine($"XML descartado: {arquivoNome}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Erro ao ler {arquivoNome}: {ex.Message}"
                    );
                }
            }

            string caminhoJson = GerarJson(notasEncontradas, diretorio);

            return (caminhoJson, arquivosXml.Length, notasEncontradas.Count);
        }

        private NotaEnvio? RegraXML(
            XDocument doc,
            string serieDesejada,
            string competenciaDesejada)
        {
            XNamespace ns = "http://www.portalfiscal.inf.br/nfe";

            var infNFe = doc.Descendants(ns + "infNFe").FirstOrDefault();
            var ide = doc.Descendants(ns + "ide").FirstOrDefault();
            var dest = doc.Descendants(ns + "dest").FirstOrDefault();

            if (infNFe == null || ide == null)
                return null;

            string serie = ide.Element(ns + "serie")?.Value.Trim();

            if (serie != serieDesejada)
                return null;

            string dhEmi = ide.Element(ns + "dhEmi")?.Value.Trim();

            if (string.IsNullOrEmpty(dhEmi) || dhEmi.Length < 7)
                return null;

            string competencia = dhEmi.Substring(0, 7);

            if (competencia != competenciaDesejada)
                return null;

            string cpf = dest?.Element(ns + "CPF")?.Value.Trim();
            string cnpj = dest?.Element(ns + "CNPJ")?.Value.Trim();

            if (!string.IsNullOrEmpty(cpf) ||
                !string.IsNullOrEmpty(cnpj))
                return null;

            string atributoId = infNFe.Attribute("Id")?.Value;

            if (string.IsNullOrEmpty(atributoId))
                return null;

            string chaveAcesso = atributoId;

            if (chaveAcesso.StartsWith("NFe"))
                chaveAcesso = chaveAcesso.Substring(3);

            if (chaveAcesso.Length != 44)
                return null;

            string valorTexto =
                doc.Descendants(ns + "vNF").FirstOrDefault()?.Value;

            if (!decimal.TryParse(
                valorTexto,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal valor))
            {
                return null;
            }

            return new NotaEnvio
            {
                Chave = chaveAcesso,
                Competencia = competencia,
                Valor = valor
            };
        }

        private string GerarJson(
            List<NotaEnvio> notas,
            string diretorio)
        {
            string json =
                System.Text.Json.JsonSerializer.Serialize(
                    notas,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNamingPolicy =
                            System.Text.Json.JsonNamingPolicy.CamelCase
                    }
                );

            string caminhoJson =
                Path.Combine(diretorio, "envio.json");

            File.WriteAllText(caminhoJson, json);

            Console.WriteLine(
                $"Arquivo gerado em: {caminhoJson}"
            );

            return caminhoJson;
        }

        private async void ClicarBotao(object sender, EventArgs e)
        {
            string serie = EntrySerie.Text?.Trim();
            string competencia = EntryData.Date?.ToString("yyyy-MM") ?? "";

            if (string.IsNullOrEmpty(serie))
            {
                await DisplayAlertAsync(
                    "Aviso",
                    "Digite a série.",
                    "Ok"
                );

                return;
            }

            string caminhoPasta = @"C:\XmlNfce";

            var (caminhoJson, totalArquivos, notasValidas) = await VerificarXMLsAsync(
                caminhoPasta,
                serie,
                competencia
            );

            if (string.IsNullOrEmpty(caminhoJson))
            {
                return;
            }

            await DisplayAlertAsync(
                "Processamento concluído",
                $"Arquivos analisados: {totalArquivos}\n" +
                $"Notas que atendem aos critérios: {notasValidas}\n" +
                $"Arquivo JSON: {caminhoJson}",
                "Abrir JSON"
            );

            await Launcher.Default.OpenAsync(
                new OpenFileRequest
                {
                    Title = "Abrir envio.json",
                    File = new ReadOnlyFile(caminhoJson)
                }
            );
        }
    }
}