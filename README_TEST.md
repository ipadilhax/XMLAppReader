# Relatório de QA e Testes

Documentação dos testes funcionais realizados no sistema **XMLAppReader**, com o objetivo de validar o comportamento da aplicação diante dos principais cenários de uso.

## Casos de Teste

| ID       | Cenário               | Ação                                                                                 | Resultado Esperado                                                      |  Status  |
| -------- | --------------------- | ------------------------------------------------------------------------------------ | ----------------------------------------------------------------------- | :------: |
| **CT01** | Entrada obrigatória   | Clicar em **Buscar** com a Série vazia.                                              | Exibir alerta solicitando o preenchimento da série.                     | Aprovado |
| **CT02** | Diretório inexistente | Informar a Série sem possuir a pasta `C:\XmlNfce`.                                   | Exibir alerta informando que o diretório não foi encontrado.            | Aprovado |
| **CT03** | NFC-e válida          | Processar um XML que atende a todos os critérios.                                    | Aprovar a nota e exibir o resumo do processamento.                      | Aprovado |
| **CT04** | Documento inválido    | Processar XMLs com CPF/CNPJ, série ou competência divergente, ou arquivo corrompido. | Descartar o documento sem interromper o processamento dos demais.       | Aprovado |
| **CT05** | Geração do JSON       | Processar as notas válidas e verificar o arquivo `envio.json`.                       | Gerar o arquivo com chave, competência e valor corretamente formatados. | Aprovado |

## Validações realizadas

### Validação de entrada

A aplicação verifica se os campos obrigatórios foram preenchidos antes de iniciar o processamento.

### Validação do diretório

O sistema verifica a existência do diretório `C:\XmlNfce` antes de realizar a leitura dos arquivos.

### Processamento dos XMLs

Os arquivos são analisados individualmente, permitindo identificar documentos válidos e inválidos de acordo com as regras definidas.

### Tratamento de erros

Arquivos XML inválidos ou corrompidos são tratados individualmente por meio de `try-catch`, evitando que um erro interrompa o processamento dos demais arquivos.

### Validação das regras fiscais

São verificadas:

* Série da NFC-e;
* Competência da emissão;
* Ausência de CPF ou CNPJ do destinatário;
* Modelo do documento;
* Chave de acesso com 44 caracteres;
* Valor total da nota.

### Geração do arquivo JSON

As notas aprovadas são armazenadas no arquivo `envio.json`, contendo:

```json
{
  "chave": "chave de acesso",
  "competencia": "YYYY-MM",
  "valor": 0.00
}
```

## Resultado dos Testes

Todos os casos de teste definidos foram **aprovados**, validando as principais funcionalidades do sistema, incluindo entrada de dados, leitura dos XMLs, aplicação dos filtros, tratamento de erros e geração do arquivo JSON.
