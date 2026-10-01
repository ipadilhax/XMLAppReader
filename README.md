Entendido perfeitamente. O erro na primeira imagem acontece porque esqueceste-te de fechar o bloco de código do JSON com ``` na linha anterior ao título. Quando não se fecha esse bloco, o GitHub considera que todo o texto seguinte (incluindo o título e o passo a passo) faz parte do próprio código e fica tudo dentro da caixa preta.   Para ficar com o mesmo separador e linha horizontal das secções anteriores (como em "Regras de Negócio", "Tecnologias Utilizadas", etc.), adicionamos a linha --- antes do título.   Aqui está o ficheiro README.md completo e corrigido. Podes copiar o código integralmente do bloco abaixo (substituindo todo o conteúdo atual no GitHub):Markdown# 📄 Leitor e Processador de NFC-e (XMLAppReader)

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

[ Entrada de Série e Competência ] ──► [ Validação de Campos ]│▼[ Leitura de Diretório C:\XmlNfce ] ──► [ XDocument.Load() por XML ]│▼[ Aplicação de RegraXML ] ───────────► [ Geração de C:\XmlNfce\envio.json ]
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

