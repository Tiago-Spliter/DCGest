# DCGest

Aplicação de gestão de alunos e notas para Diretores de Curso do ensino profissional, desenvolvida em WPF com .NET 8.0 e base de dados MySQL.

---

## Índice

- [Funcionalidades](#funcionalidades)
- [Tecnologias e Dependências](#tecnologias-e-dependências)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Pré-requisitos](#pré-requisitos)
- [Configuração](#configuração)
- [Instalação e Execução](#instalação-e-execução)
- [Arquitetura](#arquitetura)
- [Perfis de Utilizador](#perfis-de-utilizador)
- [Segurança](#segurança)

---

## Funcionalidades

- **Autenticação** — login com credenciais protegidas por BCrypt; sessão gerida durante toda a utilização
- **Gestão de Alunos** — listagem, pesquisa por nome/código/turma/orientador, edição de dados e filtragem por ano letivo e curso
- **Gestão de Notas** — visualização e edição de notas por módulo, organizadas por ano curricular (1.º, 2.º e 3.º ano), com desbloqueio progressivo por ano mediante percentagem de aprovações
- **Componentes finais** — registo de nota FCT e nota PAP com cálculo automático da média final ponderada (módulos × 66% + FCT × 11% + PAP × 23%)
- **Resumo por disciplina** — cálculo automático de médias por disciplina e por tipo (Científica / Técnica / Sociocultural), com indicação do estado de conclusão
- **Alíneas** — gestão das alíneas de estado das notas (ex: anulação, falta de comparência), com legenda consultável
- **Estado de Estágio** — atualização automática para "Pronto" quando o aluno supera 90% de módulos técnicos positivos
- **Geração de PDF** — relatório individual do aluno com todas as notas, médias e componentes finais, gerado em A4 paisagem com código de cores por situação
- **Registo de novos dados** — formulário único para adicionar Alunos, Orientadores e Diretores de Curso
- **Perfil** — consulta de dados da conta e alteração de palavra-passe

---

## Tecnologias e Dependências

| Componente | Versão |
|---|---|
| .NET | 8.0 (net8.0-windows) |
| WPF (Windows Presentation Foundation) | .NET 8.0 |
| MySql.Data | 9.6.0 |
| BCrypt.Net-Next | 4.2.0 |
| iTextSharp | 5.x (DLL local) |
| MySQL Server | 8.x recomendado |

> A aplicação utiliza exclusivamente WPF e tem como alvo `net8.0-windows`, pelo que **só é compatível com Windows**.

---

## Estrutura do Projeto

```
DCGest/
├── Classes/                    # Lógica de negócio e acesso a dados
│   ├── Entidade.cs             # Classe base abstrata com InserirNaBD
│   ├── Aluno.cs                # Modelo + queries de alunos
│   ├── Turma.cs                # Modelo + listagem de turmas
│   ├── Curso.cs                # Modelo + listagem de cursos
│   ├── AnoLetivo.cs            # Modelo + listagem de anos letivos
│   ├── Modulo.cs               # Modelo de módulo
│   ├── Disciplina.cs           # Modelo de disciplina
│   ├── NotaModulo.cs           # Modelo + validação + queries de notas
│   ├── MediaDisciplina.cs      # Modelo de média por disciplina
│   ├── Alinea.cs               # Modelo + CRUD de alíneas
│   ├── Orientador.cs           # Modelo + CRUD de orientadores
│   ├── DiretorCurso.cs         # Modelo + inserção de diretor
│   ├── Autenticacao.cs         # Login e atualização de password (BCrypt)
│   ├── Sessao.cs               # Estado global da sessão do utilizador
│   ├── BD.cs                   # String de ligação à base de dados
│   └── GeradorPDF.cs           # Geração de relatórios PDF com iTextSharp
│
├── Bibliotecas/
│   └── itextsharp.dll          # Biblioteca de geração de PDF
│
├── Logo/
│   └── DCGest.png
│
├── MainWindow.xaml(.cs)        # Janela principal com navegação lateral
├── InformacaoInicial.xaml(.cs) # Página inicial após login
├── PaginaListaAlunos.xaml(.cs) # Listagem e filtro de alunos
├── PaginaNotas.xaml(.cs)       # Gestão de notas por aluno
├── PaginaAlineas.xaml(.cs)     # Gestão de alíneas
├── PaginaAdiciona.xaml(.cs)    # Adição de alunos, orientadores e diretores
├── PaginaPerfil.xaml(.cs)      # Perfil e alteração de password
├── JanelaLogin.xaml(.cs)       # Ecrã de autenticação
├── JanelaEditaAluno.xaml(.cs)  # Edição de dados de um aluno
├── JanelaPreviewPDF.xaml(.cs)  # Pré-visualização do relatório PDF
├── JanelaLegendaAlineas.xaml(.cs) # Legenda das alíneas
│
├── BD.cs                       # Configuração da ligação à base de dados
├── App.xaml(.cs)
└── DCGest.csproj
```

---

## Pré-requisitos

- **Windows 10 ou superior** (64 bits)
- **.NET 8.0 Runtime** para Windows — [download](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- **MySQL Server** (versão 8.x recomendada) com a base de dados `pap` criada e populada
- **SDK .NET 8.0** (apenas necessário para compilar a partir do código-fonte)

---

## Configuração

A string de ligação à base de dados está definida em `DCGest/BD.cs`:

```csharp
public static readonly string CaminhoBD =
    "Server=localhost;Database=pap;User=root;Password=rootroot";
```

Altere os campos `Server`, `User` e `Password` conforme o ambiente onde o MySQL está instalado antes de compilar.

---

## Instalação e Execução

**1. Clonar o repositório**

```bash
git clone https://github.com/Tiago-Spliter/DCGest.git
cd DCGest
```

**2. Configurar a base de dados**

Crie a base de dados `pap` no MySQL e importe o script de estrutura e dados iniciais, se disponível.

**3. Ajustar a string de ligação**

Edite `DCGest/BD.cs` com as credenciais do seu servidor MySQL.

**4. Compilar e executar**

```bash
dotnet build DCGest/DCGest.csproj
dotnet run --project DCGest/DCGest.csproj
```

Ou abra a solução no Visual Studio e execute com **F5**.

O executável compilado encontra-se em:
```
DCGest/bin/Debug/net8.0-windows/DCGest.exe
```

---

## Arquitetura

A aplicação segue uma separação clara entre camadas:

- **Camada de dados** (`Classes/`) — cada classe de entidade contém os seus próprios métodos de acesso à base de dados (queries parametrizadas, inserções em transação, atualizações). Não existe ORM; o acesso é feito diretamente via `MySql.Data`.

- **Camada de apresentação** (`Pagina*.xaml` / `Janela*.xaml`) — as páginas e janelas são responsáveis exclusivamente pela interface gráfica. Invocam métodos das classes e fazem binding dos resultados aos controlos WPF.

- **Sessão** (`Sessao.cs`) — classe estática que mantém o estado do utilizador autenticado disponível globalmente durante a sessão, sem necessidade de passar o objeto entre janelas.

O fluxo de arranque obriga sempre à autenticação antes de qualquer outra janela ser inicializada. Se o login for cancelado, a aplicação encerra imediatamente.

---

## Perfis de Utilizador

A aplicação define um único perfil de utilizador autenticado: **Diretor de Curso**.

Cada Diretor de Curso tem:
- Credenciais próprias (utilizador + password armazenada com BCrypt)
- Associação a um curso específico (`Cod_Curso`)
- Acesso a todas as funcionalidades da aplicação dentro do contexto da sua sessão

Não existe acesso anónimo nem outros níveis de permissão.

---

## Segurança

- **Passwords com BCrypt** — as palavras-passe nunca são guardadas em texto simples; o BCrypt aplica hashing com sal aleatório a cada registo
- **Queries parametrizadas** — toda a comunicação com o MySQL usa parâmetros nomeados (`@param`), eliminando a possibilidade de SQL Injection
- **Transações com rollback** — operações que afetam múltiplas tabelas são executadas em transação; em caso de erro, todas as alterações são revertidas automaticamente
- **Gestão de recursos com `using`** — ligações, comandos e leitores de base de dados são sempre libertados após utilização, mesmo em caso de excepção
- **Validação de entradas** — regex e verificações de intervalo aplicadas nos formulários antes de qualquer dado ser enviado à base de dados
