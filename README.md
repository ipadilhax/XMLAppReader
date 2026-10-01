# 📄 Documentação da Solução XML

**Integrantes:**

* Jéssica Cristina
* Gustavo Henrique
* Ruan Padilha
* Gabriel Freitas
* Victor Rodrigues

**Termo/Curso:**

* 4° Termo A ADS

---

## 📌 Introdução: As dores e suas soluções

A empresa varejista realiza diariamente a emissão de centenas de **Notas Fiscais de Consumidor Eletrônica (NFC-e)**, modelo **65**. Esses documentos fiscais são armazenados em formato XML em um diretório local e precisam ser analisados para atender às exigências de auditoria e possibilitar a integração das informações com o sistema contábil.

Atualmente, o processamento manual desses arquivos pode gerar dificuldades operacionais, como:

* Alto consumo de tempo;
* Possibilidade de erros na conferência dos documentos;
* Risco de selecionar notas que não atendam aos critérios necessários;
* Dificuldade na padronização do processo;
* Maior esforço para garantir a rastreabilidade dos documentos.

Além disso, o envio das informações deve considerar somente as **NFC-e emitidas para consumidores finais não identificados**, ou seja, documentos que **não contenham CPF ou CNPJ do comprador**.

Para atender a essa necessidade, foi desenvolvida uma ferramenta capaz de ler automaticamente os arquivos XML armazenados no diretório definido para o processamento.

A aplicação solicita ao usuário:

* A **série da NFC-e**;
* O **período de competência**, informando o ano e o mês que devem ser considerados no processamento.

Após a leitura dos arquivos, a solução analisa a estrutura de cada XML e aplica os critérios definidos para o processamento:

1. Identificação da série informada;
2. Correspondência do ano e mês da data de emissão;
3. Ausência de CPF ou CNPJ do destinatário;
4. Validação de que o documento pertence ao modelo **65**;
5. Validação da chave de acesso;
6. Validação do valor total da nota.

Ao final, os dados relevantes das notas aprovadas são organizados e armazenados em um arquivo no formato **JSON**, contendo:

* Chave de acesso;
* Competência;
* Valor total de cada documento.

Dessa forma, a solução desenvolvida automatiza a extração das NFC-e válidas para envio, reduz a ocorrência de erros manuais, agiliza o processo de integração contábil e garante maior padronização e rastreabilidade no tratamento dos documentos fiscais.

---

# 🔄 Lógica de Leitura dos XMLs e Aplicação dos Filtros

## 👁️ Visão geral do fluxo

O processamento é disparado pelo botão **Buscar (`ClicarBotao`)** e segue algumas etapas.

### Fluxo de processamento

1. Captura da **série** informada pelo usuário através do campo de texto;
2. Captura da **competência** através do `DatePicker`;
3. Conversão da competência para o formato `yyyy-MM`;
4. Validação das entradas;
5. Verificação da existência da série;
6. Chamada do método `VerificarXMLsAsync`;
7. Leitura dos arquivos XML presentes na pasta `C:\XmlNfce`;
8. Aplicação das regras de validação;
9. Geração do arquivo `envio.json`;
10. Exibição do resumo do processamento;
11. Abertura automática do JSON gerado.

Caso a série não seja informada, o processamento é interrompido e um aviso é apresentado ao usuário.

---

# 📂 Leitura dos Arquivos — `VerificarXMLsAsync`

A leitura dos arquivos começa com a verificação da existência do diretório configurado.

Caso o diretório não exista, um alerta é exibido ao usuário e o método retorna um resultado vazio.

Após essa verificação, são listados todos os arquivos com extensão `.xml` presentes na pasta utilizando:

```csharp
Directory.GetFiles
```

Cada arquivo é carregado utilizando:

```csharp
XDocument.Load
```

A leitura ocorre dentro de um bloco `try/catch`. Dessa forma, caso um arquivo esteja corrompido ou malformado, o erro é registrado no log sem interromper o processamento dos demais arquivos.

Cada documento carregado é enviado ao método:

```csharp
RegraXML
```

O comportamento é:

* Se `RegraXML` retornar um objeto `NotaEnvio`, a nota é adicionada à lista de aprovadas;
* Se retornar `null`, o arquivo é descartado.

Ao final do processamento, é retornada uma tupla contendo:

* Caminho do arquivo JSON;
* Total de arquivos lidos;
* Quantidade de notas válidas.

---

# 🧩 Interpretação do XML

Os arquivos XML seguem o padrão da **NF-e/NFC-e**. Portanto, seus elementos pertencem ao namespace:

```text
https://www.portalfiscal.inf.br/nfe
```

A busca dos nós é realizada utilizando `doc.Descendants`, combinando o namespace com o nome do elemento.

### Elementos utilizados

| Elemento | Utilização                                                     |
| -------- | -------------------------------------------------------------- |
| `infNFe` | Contém o atributo `Id`, utilizado como base da chave de acesso |
| `ide`    | Contém dados de identificação, como `serie` e `dhEmi`          |
| `dest`   | Contém os dados do destinatário, como CPF/CNPJ                 |
| `vNF`    | Contém o valor total da nota                                   |

Caso os elementos `infNFe` ou `ide` não existam, o arquivo é considerado inválido e descartado imediatamente.

---

# 🔎 Filtros Aplicados — `RegraXML`

Os filtros são aplicados de forma sequencial, seguindo o padrão **fail-fast**.

Isso significa que, assim que uma regra não é atendida, a nota é descartada sem que os próximos filtros sejam executados.

## 1. Série

O valor encontrado em:

```text
ide/serie
```

deve ser igual à série informada pelo usuário.

---

## 2. Competência

A data de emissão é obtida através de:

```text
ide/dhEmi
```

Os primeiros 7 caracteres da data devem corresponder à competência selecionada pelo usuário.

### Formato esperado

```text
yyyy-MM
```

Por exemplo:

```text
2026-01
```

Caso `dhEmi` esteja vazio ou possua menos de 7 caracteres, a nota é descartada.

---

## 3. Sem identificação do destinatário

A nota somente é aprovada caso o consumidor **não esteja identificado**.

Portanto, não pode existir:

* CPF;
* CNPJ.

Essas informações são verificadas dentro do elemento:

```text
dest
```

Dessa forma, somente são consideradas notas destinadas a consumidores não identificados.

---

## 4. Chave de acesso válida

O atributo `Id` do elemento `infNFe` deve existir.

Exemplo:

```text
NFe35260112345678901234650010000000011000000019
```

O prefixo:

```text
NFe
```

é removido, restando apenas a chave numérica.

A chave resultante deve possuir exatamente:

```text
44 caracteres
```

Caso contrário, a nota é descartada.

---

## 5. Valor válido

O valor do elemento:

```text
vNF
```

é convertido para `decimal` utilizando:

```csharp
CultureInfo.InvariantCulture
```

Essa configuração garante que o ponto seja utilizado como separador decimal, seguindo o padrão utilizado no XML e evitando problemas relacionados à configuração regional do sistema.

---

# 📦 Notas aprovadas

As notas que passam por todos os filtros são convertidas em objetos `NotaEnvio`.

Cada objeto contém os seguintes campos:

| Campo         | Descrição                |
| ------------- | ------------------------ |
| `Chave`       | Chave de acesso da NFC-e |
| `Competência` | Ano e mês da emissão     |
| `Valor`       | Valor total da nota      |

---

# 📝 Geração do JSON — `GerarJson`

A lista de notas aprovadas é serializada utilizando:

```csharp
System.Text.Json
```

São utilizadas as seguintes configurações:

```csharp
WriteIndented = true
PropertyNamingPolicy = JsonNamingPolicy.CamelCase
```

### `WriteIndented`

Permite que o JSON seja formatado de maneira mais organizada e legível.

### `PropertyNamingPolicy`

Converte os nomes das propriedades para o padrão **camelCase**, resultando em campos como:

```json
chave
competencia
valor
```

O arquivo gerado recebe o nome:

```text
envio.json
```

e é salvo na mesma pasta onde estão armazenados os arquivos XML.

Após sua criação, o arquivo é aberto automaticamente utilizando:

```csharp
Launcher.Default.OpenAsync
```

---

# 📄 Exemplo de saída

```json
[
  {
    "chave": "35260112345678901234650010000000011000000019",
    "competencia": "2026-01",
    "valor": 45.90
  }
]
```

---

# ⚠️ Tratamento de erros

A aplicação possui mecanismos para evitar que problemas individuais interrompam todo o processamento.

### 1. Pasta inexistente

Caso a pasta:

```text
C:\XmlNfce
```

não exista, um alerta é exibido ao usuário e o processamento é encerrado.

### 2. Série não informada

Caso o usuário não informe uma série, um alerta é exibido antes do início da leitura dos arquivos.

### 3. Arquivo XML inválido ou malformado

Caso um XML apresente algum problema durante sua leitura, a exceção é capturada individualmente.

O erro é registrado no console e o processamento continua normalmente com os demais arquivos.

---

# ✅ Resultado

A solução permite automatizar o processamento das NFC-e, evitando a análise manual de centenas de arquivos XML.

O sistema realiza a leitura, validação e filtragem dos documentos de acordo com os critérios definidos, gerando ao final um arquivo `envio.json` contendo apenas as notas que atendem às regras estabelecidas.

Com isso, o processo se torna:

* ⚡ Mais rápido;
* 🎯 Mais preciso;
* 🔄 Padronizado;
* 📊 Rastreável;
* 📁 Mais organizado;
* 🔗 Adequado para integração com o sistema contábil.
