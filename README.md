# 📄 Leitor e Processador de NFC-e (XMLAppReader)

> **Solução automatizada em .NET MAUI para leitura, filtragem e exportação em lote de Notas Fiscais de Consumidor Eletrônica (Modelo 65).**

---

## 👥 Equipa de Desenvolvimento

* Jéssica Cristina
* Gustavo
* Ruan
* Gabriel
* Victor

---

## 📌 Contexto e Solução de Negócio

Uma empresa varejista realiza diariamente a emissão de centenas de Notas Fiscais de Consumidor Eletrônica (NFC-e, modelo 65). Esses documentos fiscais são salvos em formato XML em um diretório local e precisam ser analisados para atender a exigências de auditoria e permitir a integração contábil.

O processamento manual desses arquivos apresenta desafios operacionais, como o alto consumo de tempo, a ocorrência de erros na conferência individual e o risco de selecionar documentos fora do escopo. Além disso, a integração exige o envio exclusivo de notas emitidas para consumidores finais não identificados (sem CPF ou CNPJ).

O **XMLAppReader** automatiza o ciclo completo de leitura dos XMLs locais, aplica filtros de negócio no padrão *fail-fast* e gera um arquivo de saída estruturado em JSON contendo a chave de acesso (44 dígitos), a competência e o valor total de cada nota aprovada.

---

## ⚙️ Regras de Negócio e Filtros de Seleção

O método `RegraXML` aplica validações sequenciais aos ficheiros XML:

1. **Validação de Série**: O valor da tag `<ide><serie>` deve corresponder exatamente à série informada pelo utilizador.
2. **Validação de Competência**: Os 7 primeiros caracteres da data de emissão (`<dhEmi>`, no formato `YYYY-MM`) devem ser iguais ao período selecionado.
3. **Consumidor Não Identificado**: O bloco `<dest>` não pode conter as tags `<CPF>` nem `<CNPJ>`.
4. **Chave de Acesso Válida**: O atributo `Id` do bloco `<infNFe>` tem o prefixo `"NFe"` removido e a chave resultante deve conter exatamente 44 dígitos.
5. **Conversão de Valor**: A tag `<vNF>` é convertida para decimal utilizando `CultureInfo.InvariantCulture`.

---

## 🔄 Lógica de Funcionamento

[ Entrada de Série e Competência ] ──► [ Validação de Campos ]
│
▼
[ Leitura de Diretório C:\XmlNfce ] ──► [ XDocument.Load() por XML ]
│
▼
[ Aplicação de RegraXML ] ───────────► [ Geração de C:\XmlNfce\envio.json ]


---

## 🛠️ Tecnologias Utilizadas

* **Linguagem**: C# (.NET 10.0)
* **Framework GUI**: .NET MAUI (Target: `net10.0-windows10.0.19041.0`)
* **Manipulação de XML**: `System.Xml.Linq` (`XDocument`)
* **Serialização JSON**: `System.Text.Json`

---

## 📤 Formato do Ficheiro de Saída (`envio.json`)

```json
[
  {
    "chave": "35260112345678901234650010000000011000000019",
    "competencia": "2026-01",
    "valor": 45.90
  }
]
## 💻 Como Executar o Projeto

1. **Clone o repositório para o seu ambiente local:**
   ```bash
   git clone [https://github.com/seu-usuario/XMLAppReader.git](https://github.com/seu-usuario/XMLAppReader.git)
Abra a solução no Visual Studio:

Certifique-se de ter o Visual Studio 2022 com a carga de trabalho de desenvolvimento .NET MAUI instalada.

Prepare o ambiente de teste:

Crie o diretório C:\XmlNfce\ no seu sistema operacional.

Adicione os ficheiros .xml das notas fiscais dentro dessa pasta.

Execute a aplicação:

Selecione o alvo de execução para Windows Machine no painel superior.

Pressione F5 para compilar e iniciar o projeto.
