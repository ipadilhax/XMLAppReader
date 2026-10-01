# Relatório de Garantia de Qualidade (QA)

## 1. Suíte de Casos de Teste (CTs)

| ID       | Título                                                             | Pré-condição                                                                                    | Passos / Entradas                                                                            | Resultado Esperado                                                                                                                      | Resultado Obtido                                                                                          |
| -------- | ------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------- |
| **CT01** | Validação de Série Obrigatória                                     | Aplicação aberta e em execução no Windows.                                                      | 1. Deixar a **Série** vazia.<br>2. Clicar em **Buscar**.                                     | Exibir alerta de validação informando que a série é obrigatória.                                                                        | Pop-up **"Aviso: Digite a série."** exibido corretamente.                                                 |
| **CT02** | Validação de Ausência do Diretório                                 | Diretório `C:\XmlNfce` não existente no sistema.                                                | 1. Preencher **Série 1**.<br>2. Selecionar **Competência**.<br>3. Clicar em **Buscar**.      | Exibir alerta amigável informando que o diretório não foi encontrado.                                                                   | Pop-up **"Aviso: A pasta C:\XmlNfce não foi encontrada."** exibido.                                       |
| **CT03** | Processamento de NFC-e Válida (Consumidor Final)                   | Arquivo `nota_teste.xml` presente em `C:\XmlNfce` — Série 1, Competência 2026-09, sem CPF/CNPJ. | 1. Informar **Série 1**.<br>2. Informar **Competência 09/2026**.<br>3. Clicar em **Buscar**. | Filtrar a nota, exibir resumo do lote e gerar o arquivo `envio.json` em `C:\XmlNfce`.                                                   | Pop-up **"Processamento concluído: Arquivos analisados: 1, Notas que atendem aos critérios: 1"** exibido. |
| **CT04** | Descarte por CPF/CNPJ, Série/Competência Divergente ou Erro no XML | Arquivo XML presente em `C:\XmlNfce` fora do padrão esperado ou com dados de destinatário.      | 1. Informar Série e Competência desejadas.<br>2. Clicar em **Buscar**.                       | Ignorar arquivos com CPF/CNPJ em `<dest>`, datas/séries divergentes ou erros de sintaxe por meio de `try-catch`.                        | A aplicação descartou o XML via log (`Console.WriteLine`) sem interromper a execução nem travar.          |
| **CT05** | Formatação da Chave de Acesso e Geração do JSON                    | Arquivo `envio.json` gerado no diretório `C:\XmlNfce` após processamento.                       | 1. Clicar no botão **Abrir JSON** do alerta de conclusão.                                    | Gerar o JSON removendo o prefixo `"NFe"` do atributo `Id`, mantendo 44 dígitos, formatando a competência (`AAAA-MM`) e o valor decimal. | Estrutura gerada corretamente em formato **camelCase**.                                                   |

---

# 2. Massa de Dados

## XML 1 — Exemplo Válido Processado

**Arquivo:** `nota_teste.xml`

**Localização:** `C:\XmlNfce`

```xml
<?xml version="1.0" encoding="utf-8"?>
<nfeProc xmlns="http://www.portalfiscal.inf.br/nfe">
  <NFe>
    <infNFe Id="NFe35260900000000000000650010000000011000000001">
      <ide>
        <serie>1</serie>
        <dhEmi>2026-09-15T10:00:00-03:00</dhEmi>
      </ide>

      <dest>
        <!-- Sem CPF/CNPJ para passar na regra de Consumidor Final -->
      </dest>

      <total>
        <ICMSTot>
          <vNF>150.00</vNF>
        </ICMSTot>
      </total>
    </infNFe>
  </NFe>
</nfeProc>
```

## Justificativa Técnica

O documento atendeu simultaneamente a todos os critérios da regra de negócio implementada em `RegraXML`:

* Localizado na pasta configurada `C:\XmlNfce`.
* O elemento `<serie>` contém exatamente o valor informado no campo (`1`).
* Os primeiros 7 caracteres do nó `<dhEmi>` correspondem à competência selecionada (`2026-09`).
* O bloco `<dest>` não contém as tags `<CPF>` ou `<CNPJ>`.
* O atributo `Id` de `<infNFe>` possui o prefixo `"NFe"` seguido de 44 dígitos válidos.
* O elemento `<vNF>` contém um valor decimal válido (`150.00`).

---

# 3. Estrutura do Arquivo de Saída

## `envio.json`

Para o cenário **CT03**, o sistema gerou com sucesso o arquivo:

```text
C:\XmlNfce\envio.json
```

O arquivo possui a seguinte estrutura, utilizando o padrão **camelCase**:

```json
[
  {
    "chave": "35260900000000000000650010000000011000000001",
    "competencia": "2026-09",
    "valor": 150.0
  }
]
```

## Mapeamento das Regras Aplicadas ao JSON

### `chave`

Trata o atributo `Id`, removendo o prefixo `"NFe"` por meio de:

```csharp
Substring(3)
```

O resultado mantém exatamente os **44 dígitos numéricos** correspondentes à chave de acesso da nota.

### `competencia`

Extrai os 7 primeiros caracteres de `dhEmi` utilizando:

```csharp
dhEmi.Substring(0, 7)
```

O resultado é apresentado no formato padronizado:

```text
AAAA-MM
```

Exemplo:

```text
2026-09
```

### `valor`

Realiza o parse seguro da tag `<vNF>` utilizando a cultura invariante:

```csharp
CultureInfo.InvariantCulture
```

Isso garante que o valor seja tratado corretamente como um número decimal e exportado dessa forma no arquivo JSON.

---

# 4. Resultados Obtidos e Evidências de Execução

## CT01 — Validação de Série Obrigatória

**Status:** Aprovado

**Descrição:**
Ao tentar iniciar a busca sem informar a série, a aplicação disparou o alerta de validação informando que a série é um campo de preenchimento obrigatório.

**Resultado obtido:**

O sistema exibiu corretamente a mensagem:

```text
Aviso: Digite a série.
```

### Evidência — Imagem 1

> **ANEXAR IMAGEM 1 AQUI — CT01: Alerta de série obrigatória**

**Cole o print abaixo com `Ctrl+V`:**

![Primeira imagem](C:\Users\Gabriel\Downloads\testread\primeira.png)

---

## CT02 — Validação de Ausência do Diretório

**Status:** Aprovado

**Descrição:**
Ao informar uma série válida antes de criar a pasta `C:\XmlNfce`, o sistema validou a ausência do diretório no disco rígido e exibiu uma mensagem amigável ao usuário sem interromper a aplicação.

**Resultado obtido:**

O sistema exibiu corretamente a mensagem:

```text
Aviso: A pasta C:\XmlNfce não foi encontrada.
```

### Evidência — Imagem 2

> **ANEXAR IMAGEM 2 AQUI — CT02: Alerta de diretório não encontrado**

**Cole o print abaixo com `Ctrl+V`:**

![Segunda imagem](C:\Users\Gabriel\Downloads\testread\segunda.png)

---

## CT03 — Processamento de NFC-e Válida e Leitura de Origem

**Status:** Aprovado

**Descrição:**
Após criar o diretório `C:\XmlNfce` com o arquivo `nota_teste.xml`, contendo:

* Série `1`;
* Competência `2026-09`;
* Ausência de CPF/CNPJ;

a aplicação realizou a leitura do lote, aplicou os filtros definidos e confirmou a aprovação da nota.

**Resultado obtido:**

O sistema identificou e processou corretamente o arquivo XML válido.

O processamento apresentou o seguinte resumo:

```text
Processamento concluído:
Arquivos analisados: 1
Notas que atendem aos critérios: 1
```

### Evidência — Imagem 3

> **ANEXAR IMAGEM 3 AQUI — CT03: Arquivo XML de origem `nota_teste.xml`**

**Cole o print abaixo com `Ctrl+V`:**

![Terceira imagem](C:\Users\Gabriel\Downloads\testread\bloco.png)

### Evidência — Imagem 4

> **ANEXAR IMAGEM 4 AQUI — CT03: Pop-up de conclusão do processamento**

**Cole o print abaixo com `Ctrl+V`:**

![Quarta imagem](C:\Users\Gabriel\Downloads\testread\terceira.png)

---

## CT04 — Descarte por CPF/CNPJ, Série/Competência Divergente ou Erro no XML

**Status:** Aprovado

**Descrição:**
A aplicação realiza o descarte de arquivos XML que não atendem aos critérios definidos, como:

* Presença de CPF ou CNPJ em `<dest>`;
* Série divergente;
* Competência divergente;
* XML inválido ou malformado.

Os erros de leitura são tratados por meio de `try-catch`, evitando que um arquivo inválido interrompa o processamento dos demais.

**Resultado obtido:**

A aplicação descartou o XML via log (`Console.WriteLine`) sem interromper a execução ou travar.

### Evidência — Imagem 5

> **ANEXAR IMAGEM 5 AQUI — CT04: Log/resultado do descarte do XML**

**Cole o print abaixo com `Ctrl+V`:**

<!-- COLE A IMAGEM 5 AQUI -->

---

## CT05 — Geração e Formatação do Arquivo `envio.json`

**Status:** Aprovado

**Descrição:**
O sistema exportou a nota aprovada para:

```text
C:\XmlNfce\envio.json
```

O arquivo foi gerado corretamente, garantindo:

* Remoção do prefixo `"NFe"` da chave de acesso;
* Preservação dos **44 dígitos** da chave;
* Formatação correta da competência;
* Conversão correta do valor decimal;
* Utilização do padrão **camelCase** no JSON.

### JSON Gerado

```json
[
  {
    "chave": "35260900000000000000650010000000011000000001",
    "competencia": "2026-09",
    "valor": 150.0
  }
]
```

### Evidência — Imagem 6

> **ANEXAR IMAGEM 6 AQUI — CT05: Arquivo `envio.json` gerado**

**Cole o print abaixo com `Ctrl+V`:**

<!-- COLE A IMAGEM 6 AQUI -->

---

# 5. Resumo dos Resultados

| Caso de Teste | Cenário                                        | Status   |
| ------------- | ---------------------------------------------- | -------- |
| **CT01**      | Validação de série obrigatória                 | Aprovado |
| **CT02**      | Ausência do diretório                          | Aprovado |
| **CT03**      | Processamento de NFC-e válida                  | Aprovado |
| **CT04**      | Descarte de XML inválido ou fora dos critérios | Aprovado |
| **CT05**      | Geração e formatação do `envio.json`           | Aprovado |

---

# 6. Conclusão

Os testes realizados demonstram que a aplicação atende às principais regras de negócio definidas para o processamento das NFC-e.

Foram validados cenários envolvendo:

* Obrigatoriedade da série;
* Ausência do diretório;
* Processamento de NFC-e válida;
* Descarte de documentos inválidos;
* Descarte por CPF/CNPJ;
* Validação de série e competência;
* Tratamento de erros em arquivos XML;
* Geração do arquivo `envio.json`;
* Formatação da chave de acesso;
* Formatação da competência;
* Conversão e exportação do valor da nota.

Todos os casos de teste apresentados foram **aprovados**, e os resultados obtidos foram compatíveis com os resultados esperados para os cenários avaliados.
