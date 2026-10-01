# Solução XML — NFC-e

Aplicação desenvolvida para automatizar a leitura e filtragem de arquivos XML de **NFC-e (modelo 65)**, facilitando a seleção dos documentos que serão enviados para integração contábil.

## Equipe

Jéssica Cristina · Gustavo · Ruan · Gabriel · Victor

## Objetivo

A aplicação processa os XMLs armazenados em `C:\XmlNfce` e identifica automaticamente as notas que atendem aos critérios definidos pelo usuário.

O usuário informa:

* **Série da NFC-e**
* **Competência (ano e mês)**

A aplicação então realiza a leitura dos arquivos e aplica os filtros necessários.

## Critérios de validação

Uma NFC-e é aprovada quando:

* A série corresponde à informada pelo usuário;
* A competência corresponde ao período selecionado;
* Não possui CPF ou CNPJ do destinatário;
* Possui uma chave de acesso válida com 44 caracteres;
* O valor total da nota é válido.

Documentos que não atendem a algum dos critérios são descartados.

## Processamento

```text
XMLs
 ↓
Leitura
 ↓
Validação
 ↓
Aplicação dos filtros
 ↓
Notas aprovadas
 ↓
envio.json
```

Arquivos XML inválidos ou corrompidos são tratados individualmente, permitindo que os demais documentos continuem sendo processados.

## Saída

As notas aprovadas são armazenadas em `envio.json`:

```json
[
  {
    "chave": "35260112345678901234650010000000011000000019",
    "competencia": "2026-01",
    "valor": 45.90
  }
]
```

## Tecnologias

* C#
* LINQ to XML
* System.Text.Json
* XML / JSON
* Async/Await

## Resultado

A solução reduz o processamento manual dos documentos fiscais, minimiza erros de conferência e padroniza a geração das informações necessárias para integração contábil.
