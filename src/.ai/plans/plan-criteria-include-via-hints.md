# Plan: `Include` no `ICriteria` via Operation Hints

## Status: CONCLUÍDO

## Progresso

`████` **100%** - 4 de 4 fases

| Fase | Estado |
|---|---|
| Fase 0 - Decisão de acoplamento + baseline de regressão | Concluida |
| Fase 1 - Hints ambientes no pipeline Criteria/EF (sem mudar `ICriteria`) | Concluida |
| Fase 2 - `ICriteria.UseHints(...)` por consulta (Nível 2) | Concluida |
| Fase 3 - Paridade `Find`/Repository + docs/samples | Concluida |

> **Manutenção deste plano:** ao concluir as tarefas de uma fase, marque cada tarefa com `- [x]`,
> troque o **Estado** da fase para `Concluida` na tabela acima e atualize a barra de progresso
> (um bloco `█` por fase, `%` e `X de 4`). Preencha a seção **Resultado** de cada fase ao finalizá-la.

---

## Escopo e repositórios

Este plano **cruza dois repositórios irmãos** (ambos em `C:\git\RoyalCode\`):

- **`Searches`** (`RoyalCode.SmartSearch.*`) — onde mora o `ICriteria`/pipeline. A maior parte das mudanças.
- **`OperationHint`** (`RoyalCode.OperationHint.*`) — o mecanismo de includes declarativos. Mudanças mínimas ou nenhuma.

O plano vive no repo `OperationHint` por ser onde a feature de includes é definida, mas a execução toca os dois.
Caminhos de arquivo abaixo são absolutos para evitar ambiguidade.

---

## Contexto e achados

### Problema
`ICriteria<TEntity>` ([`Searches\...\RoyalCode.SmartSearch.Abstractions\ICriteria.cs`](../../../../Searches/src/RoyalCode.SmartSearch.Abstractions/ICriteria.cs))
expõe `FilterBy`/`OrderBy`/`Select<TDto>`/`Collect`/`Exists`/`FirstOrDefault`/`Single` — **mas não tem `Include`**.
Logo, não há como carregar o grafo de um agregado (filhos, owned, 1:1) por uma `ICriteria`; quem precisa do grafo
hoje cai para `DbContext.Set<T>().Include(...)` manual, fora da abstração.

### Achado decisivo
O pacote `RoyalCode.SmartSearch.EntityFramework` **já referencia `RoyalCode.OperationHint.Abstractions`**
([csproj linha 21](../../../../Searches/src/RoyalCode.SmartSearch.EntityFramework/RoyalCode.SmartSearch.EntityFramework.csproj))
porém o pipeline (`CriteriaPerformer`/`CriteriaPerformerBase`/`CriteriaQuery`) **nunca chama um `IHintPerformer`**.
Ou seja: **o seam de integração existe, só não foi ligado.** A feature é, em essência, ligar o que já existe.

### Por que NÃO adicionar `Include(Expression<Func<TEntity, object>>)` cru no `ICriteria`
- Acopla a abstração (hoje provider-neutra, estilo Specification/Filter-Specifier) à semântica de navegação do EF.
- Espalha o conhecimento de include pelos call sites — exatamente o que o `OperationHint` **centraliza**.
- Não faz sentido no caminho `Select<TDto>()` nem em backends não-EF.

O include declarativo, centralizado e provider-agnóstico **já existe**: o `Includes<TEntity>`
([`OperationHint\...\EntityFramework\Includes.cs`](../RoyalCode.OperationHint.EntityFramework/Includes.cs))
com `IncludeReference`/`IncludeCollection`, registrado uma vez por `(entidade, hint)` via
`AddIncludesHandler<TEntity,THint>` / `AddIncludesHintHandler<THint>().Add<TEntity>(...)`
([`OperationHintServiceCollectionExtensions.cs`](../RoyalCode.OperationHint.EntityFramework/Extensions/OperationHintServiceCollectionExtensions.cs)).
Portanto: **"Include no `ICriteria`" = fazer a `ICriteria` carregar/disparar Operation Hints.**

### Como o OperationHint aplica includes hoje
Modelo **ambiente/escopado**: `IHintsContainer.AddHint(hint)` acumula hints no escopo; `IHintPerformer.Perform(query)`
itera os hints e, para cada um, busca `IHintHandlerRegistry.GetQueryHandlers<TQuery, THint>()` e chama
`handler.Handle(query, hint)`, que executa `query.Include(...)`
(ver [`DefaultHintPerformer.cs`](../RoyalCode.OperationHint.Abstractions/DefaultHintPerformer.cs),
[`QueryableIncludes.cs`](../RoyalCode.OperationHint.EntityFramework/Internals/QueryableIncludes.cs),
e o uso canônico em [`QueryTestes.cs`](../RoyalCode.OperationHint.Tests/EFCore/QueryTestes.cs)).
O mesmo registro também cobre o caminho pós-carga (`Find`) via handler de entidade
(`registry.Add<TEntity, DbContext, THint>(handler)`).

### Onde o `CriteriaQuery` materializa (seam de aplicação)
[`CriteriaQuery<TEntity>`](../../../../Searches/src/RoyalCode.SmartSearch.EntityFramework/Services/CriteriaQuery.cs)
guarda `IQueryable<TEntity> query` e materializa em `GetQueryableWithSkip()` / `GetQueryableWithSkipAndTake()`
(usados por `FirstOrDefault`/`Single`/`ToList`/`ToResultList`/`ToAsyncListAsync`). `Exists()` usa `Any()`.
`Select<TDto>()` projeta (`query.Select(expr)`) e devolve um **novo** `CriteriaQuery<TDto>` — a partir daí os includes
de `TEntity` deixam de fazer sentido.

---

## Decisões de design

1. **Reusar OperationHint como mecanismo de include** — não criar `Include(Expression)` no contrato. Includes
   continuam declarados uma vez (hint handlers) e referenciados por hint.
2. **Aplicar hints só nos terminais que materializam ENTIDADE**: `Collect`/`ToList`, `FirstOrDefault`, `Single`,
   e o caminho de `AsSearch()` que devolve entidades. **Não** aplicar em `Exists()` (é `Any`, include é desperdício)
   nem após `Select<TDto>()` (projeção; o EF ignora include sob projeção e o tipo já trocou para `TDto`).
3. **Acoplamento (DECIDIDO — opção "visitor / double-dispatch"):** `UseHints` vive no contrato `ICriteria` e os
   hints ficam em `CriteriaOptions` (`Core`), **sem** que `Abstractions` ou `Core` referenciem
   `RoyalCode.OperationHint.Abstractions`. Chave: a assinatura `UseHints<THint>(params THint[]) where THint : Enum`
   só usa `Enum` (BCL); o que acoplaria seria o carrier executar o hint via `IHintHandlerRegistry`. Inverte-se a
   dependência com um **visitor**: o carrier (`Core`) apenas expõe o `THint` capturado (`ICriteriaHint.Accept(visitor)`
   / `ICriteriaHintVisitor.Visit<THint>(hint)`), e o **`.EntityFramework`** — que **já** referencia
   `OperationHint.Abstractions` — implementa o visitor (`RegistryHintVisitor<TQuery>`) trazendo o `IHintHandlerRegistry`
   e o `IQueryable<TEntity>`. `THint` vem do carrier, `TQuery` do visitor; double-dispatch resolve os dois sem reflection.
   - **Resultado:** `Abstractions` = só o método em `ICriteria` (zero ref); `Core` = `CriteriaOptions.Hints`/`AddHint`
     + `ICriteriaHint`/`ICriteriaHintVisitor`/`CriteriaHint<THint>` (zero ref); `.EntityFramework` = `RegistryHintVisitor`
     (ref já existente).
   - **Por que esta e não as outras:** mantém `Abstractions`+`Core` provider-neutros (coerente com o estilo
     Specification/Filter-Specifier já adotado) **e** mantém `UseHints` no contrato `ICriteria`. Supera tanto a opção
     "referenciar `OperationHint.Abstractions` em Abstractions+Core" (acopla os contratos a EF/hints) quanto a
     "acoplamento-zero via _extension method_ no `.EntityFramework`" (tiraria `UseHints` do contrato).
   - **Custo aceito:** uma micro-abstração extra (`ICriteriaHintVisitor`) + a indireção do double-dispatch.

---

## Fase 0 - Decisão de acoplamento + baseline de regressão

**O que/como:** travar a decisão de acoplamento (Decisão 3) e garantir uma rede de testes antes de mexer no pipeline.

**Tarefas:**

- [x] Decidir e registrar o acoplamento: **opção "visitor / double-dispatch"** — `UseHints` no contrato `ICriteria`,
      hints em `CriteriaOptions`, **zero referência** a `OperationHint.Abstractions` em `Abstractions`/`Core`; toda a
      cola com o `IHintHandlerRegistry` isolada no `.EntityFramework` via `RegistryHintVisitor<TQuery>`. Porquê e
      trade-offs registrados na **Decisão de design 3**.
- [x] Confirmar versão/target: `OpHintVer = 1.0.0` confirmado em `Searches\src\Directory.Build.props`;
      `OperationHint.Abstractions` 1.0.0 publica `netstandard2.1/net6/7/8` (build confirmou os 4 TFMs). **Ressalva:**
      `AspTargets` atual = `net8;net9` (não o que a nota original assumia); o target `net9` do Searches resolve o asset
      `net8`/`netstandard2.1` do pacote — **compatível**, sem `net9` necessário no OperationHint.
- [x] Garantir suíte verde de baseline nos dois repos (build + testes atuais) antes de qualquer mudança.

**Critérios de aceite:** decisão de acoplamento registrada; baseline verde nos dois repos. ✅ atendidos.

**Testes:** rodar as suítes existentes (`OperationHint.Tests`, testes do `Searches`).

### Resultado da Fase 0

**Concluída em 2026-06-20.**

- **Acoplamento decidido:** opção "visitor / double-dispatch" — `UseHints` no contrato `ICriteria`, `Abstractions`+`Core`
  sem referência a `OperationHint.Abstractions`, cola isolada no `.EntityFramework`. Ver Decisão de design 3.
- **Versão/TFMs:** `OpHintVer = 1.0.0`; `OperationHint.Abstractions` compila `netstandard2.1/net6/7/8`; Searches
  (`AspTargets = net8;net9`) consome o asset `net8`/`netstandard2.1` no target `net9` — compatível.
- **Baseline (build):** ambas as solutions compilam com **0 erros** (`SmartSearch.sln`: 19 warnings pré-existentes de
  doc XML/NU5104, nada bloqueante; `RoyalCode.OperationHint.sln`: 0 warnings).
- **Baseline (testes):** `RoyalCode.SmartSearch.Tests` **110/110**; `RoyalCode.OperationHint.Tests` **12/12**; 0 falhas.
- **Nota de ambiente:** os projetos de teste são `net8`-only e a máquina tem apenas os runtimes 9.0/10.0 (SDK 10.0.301).
  Foi necessário `DOTNET_ROLL_FORWARD=Major` para rodar o testhost net8 — manter para futuras execuções locais.

---

## Fase 1 - Hints ambientes no pipeline Criteria/EF (Nível 1, sem mudar `ICriteria`)

**O que/como:** ligar o `IHintPerformer` (ambiente) ao caminho EF do Criteria, de forma **opt-in e backward-compatible**
(dependência opcional: ausente ⇒ no-op). Isso já destrava `criteria.Collect()` honrar hints declarados no escopo,
sem tocar a interface.

**Tarefas:**

- [x] Injetar `IHintPerformer?` (opcional) em
      [`CriteriaPerformer<TDbContext,TEntity>`](../../../../Searches/src/RoyalCode.SmartSearch.EntityFramework/Services/CriteriaPerformer.cs)
      (novo parâmetro de ctor, default `null`). Também propagado por `CriteriaPerformerBase`, `SearchManager` e
      `EFSearchesExtensions.Criteria(this DbContext)` (resolução null-safe via provider cru, pois o `db.GetService<T>()`
      do EF lança se ausente).
- [x] Propagar o performer para o `CriteriaQuery<TEntity>` (novo parâmetro de ctor `IHintPerformer? = null`), a partir de
      [`CriteriaPerformerBase.Prepare`](../../../../Searches/src/RoyalCode.SmartSearch.EntityFramework/Services/CriteriaPerformerBase.cs).
- [x] Em `CriteriaQuery<TEntity>`, aplicar os hints **uma vez, lazy** (`GetEntityQuery()` cacheia `hintPerformer.Perform(query)`
      em `hintedQuery`), só nos terminais de entidade via parâmetro `applyHints` em
      `GetQueryableWithSkip()` / `GetQueryableWithSkipAndTake()`. **Não** aplicado em `Exists()`/`ExistsAsync()` (default
      `applyHints: false`), no `Count()`, nem em `CriteriaQuery<TDto>` de `Select` (criado sem performer ⇒ null ⇒ no-op).
- [x] Registro DI: **nenhuma mudança necessária** em `EntityFrameworkSearchesServiceCollectionExtensions`. O parâmetro
      defaultado (`IHintPerformer? = null`) faz o container MS DI injetar quando registrado e usar `null` quando não —
      independente de ordem (resolução adiada ao build do provider). Sem exigir `AddOperationHints`.
- [x] No-op quando `OperationHint` ausente: `hintPerformer` é `null` ⇒ query inalterada. Quando registrado mas sem hints,
      o próprio `DefaultHintPerformer.Perform` já é no-op (`hints is null || registry.IsEmpty`).

**Critérios de aceite:** com `AddOperationHints` + hint handler + `container.AddHint(...)`, `Collect`/`First`/`Single`
trazem as navegações; `Exists` e `Select<TDto>` não; sem OperationHint registrado, comportamento idêntico ao atual.

**Testes (novos, no repo Searches):** em
[`CriteriaOperationHintTests.cs`](../../../../Searches/src/RoyalCode.SmartSearch.Tests/CriteriaOperationHintTests.cs)
(+ model em `OperationHints/CriteriaOperationHintModel.cs`).

- [x] `Collect`/`FirstOrDefault`/`Single` com hint ambiente incluem a navegação registrada.
- [x] `Exists` não dispara include (asserção por SQL/log: `Collect` gera `JOIN`, `Exists` não).
- [x] `Select<TDto>` ignora hints de entidade (projeção intacta; sem `JOIN` no SQL).
- [x] Sem `OperationHint` registrado: queries inalteradas (regressão; navegações `null`).
- [x] Múltiplos hints no container ⇒ união de includes (espelhar `QueryTestes`).

### Resultado da Fase 1

**Concluída em 2026-06-21.**

- **Arquivos tocados (Searches, pacote `.EntityFramework`):** `CriteriaQuery`, `CriteriaPerformerBase`,
  `CriteriaPerformer`, `SearchManager`, `EFSearchesExtensions` — `IHintPerformer?` opcional + aplicação lazy/idempotente
  só nos terminais de entidade.
- **Acoplamento confirmado:** `Abstractions`/`Core` intocados; toda a integração ficou no `.EntityFramework` (que já
  referencia `OperationHint.Abstractions`). Coerente com a Decisão de design 3.
- **DI sem mudança:** o parâmetro de ctor defaultado resolve sozinho com o container MS DI (injeta se registrado, `null`
  caso contrário). Validado pelos testes "com" e "sem" `OperationHint`.
- **Testes:** 7 novos, todos verdes. Suíte Searches **117/117** (era 110). OperationHint não foi tocado (12/12 mantidos).
- **Gotcha de teste registrado:** entidades EF declaradas como **`file`-scoped types** quebram a navigation expansion do
  EF Core 8.0.0 (`IndexOutOfRangeException` em `CreateNavigationExpansionExpression`). Solução: usar classes regulares
  (movidas para `OperationHints/CriteriaOperationHintModel.cs`). Vale para qualquer teste EF futuro neste repo.
- **Nota de ambiente:** testes net8-only ⇒ exigem `DOTNET_ROLL_FORWARD=Major` (runtime 8 ausente; ver Fase 0).

---

## Fase 2 - `ICriteria.UseHints(...)` por consulta (Nível 2)

**O que/como:** dar controle **por consulta** (sem estado ambiente), que é a adição ao `ICriteria` propriamente dita.
Um include por-consulta **não pode** vazar para consultas irmãs do mesmo escopo — por isso aplica via
`IHintHandlerRegistry` direto, **não** via `IHintsContainer`.

**Tarefas:**

- [x] Adicionar ao contrato
      [`ICriteria.cs`](../../../../Searches/src/RoyalCode.SmartSearch.Abstractions/ICriteria.cs):
      `ICriteria<TEntity> UseHints<THint>(params THint[] hints) where THint : Enum;` (zero ref a OperationHint).
- [x] Implementar em
      [`Criteria<TEntity>`](../../../../Searches/src/RoyalCode.SmartSearch.Core/Defaults/Criteria.cs):
      `UseHints<THint>` ⇒ `options.AddHint(new CriteriaHint<THint>(hints))` (espelha `FilterBy`→`AddFilter`).
- [x] Em [`CriteriaOptions`](../../../../Searches/src/RoyalCode.SmartSearch.Core/Defaults/CriteriaOptions.cs):
      adicionado `IReadOnlyList<ICriteriaHint> Hints` + `AddHint(ICriteriaHint)` (espelhando `AddFilter`/`AddSorting`).
- [x] Definir o carrier + visitor (provider-agnóstico; captura o `THint` no ponto de chamada; **sem** referência a
      `OperationHint.Abstractions` — ver Decisão de design 3). Em `Core` (`Hints/ICriteriaHint.cs`, `Hints/CriteriaHint.cs`):
      ```csharp
      // provider-neutro: só Enum, nenhum tipo de OperationHint
      public interface ICriteriaHint
      {
          void Accept(ICriteriaHintVisitor visitor);
      }
      public interface ICriteriaHintVisitor
      {
          void Visit<THint>(THint hint) where THint : Enum;
      }
      internal sealed class CriteriaHint<THint>(THint[] hints) : ICriteriaHint where THint : Enum
      {
          public void Accept(ICriteriaHintVisitor visitor)
          {
              foreach (var h in hints) visitor.Visit(h); // expõe o THint fechado
          }
      }
      ```
- [x] Implementar o visitor no pacote `.EntityFramework` (que **já** referencia `OperationHint.Abstractions`),
      trazendo o `IHintHandlerRegistry` e o `IQueryable<TEntity>` (`Services/RegistryHintVisitor.cs`):
      ```csharp
      internal sealed class RegistryHintVisitor<TQuery>(IHintHandlerRegistry registry, TQuery query)
          : ICriteriaHintVisitor where TQuery : class
      {
          public TQuery Query { get; private set; } = query;
          public void Visit<THint>(THint hint) where THint : Enum
          {
              foreach (var handler in registry.GetQueryHandlers<TQuery, THint>())
                  Query = handler.Handle(Query, hint);
          }
      }
      ```
- [x] No caminho EF, resolver/disponibilizar `IHintHandlerRegistry` ao `CriteriaQuery` (novo param opcional, propagado
      por `CriteriaPerformerBase`/`CriteriaPerformer`/`SearchManager`/`EFSearchesExtensions` igual ao `IHintPerformer`),
      e aplicar `options.Hints` (locais) iterando `hint.Accept(new RegistryHintVisitor<IQueryable<TEntity>>(registry, q))`
      **junto** com o `Perform` ambiente da Fase 1, no `GetEntityQuery()` (cacheado), antes dos terminais de entidade.
      `THint` vem do carrier, `TQuery` do visitor (double-dispatch, sem reflection).
- [x] `UseHints` ignorado de forma graciosa sem registry (`hintRegistry is null` ⇒ hints locais não aplicados);
      registrado mas sem handler para o enum ⇒ `GetQueryHandlers` vazio ⇒ no-op (ambos cobertos por teste).

**Critérios de aceite:** `criteria.UseHints(MyHint.X).FirstOrDefault()` inclui a navegação de `MyHint.X`; hints locais
de uma criteria **não** afetam outra criteria do mesmo escopo; combinar `UseHints` + hint ambiente = união.

**Testes (novos, no repo Searches):**

- [x] `UseHints` inclui a navegação esperada em `Collect`/`First`/`Single` (+ múltiplos valores ⇒ união).
- [x] Isolamento: duas criterias no mesmo escopo, só a que chamou `UseHints` inclui (asserção por SQL/`JOIN`).
- [x] `UseHints` + `container.AddHint` ⇒ união de includes.
- [x] `UseHints` não afeta `Exists`/`Select<TDto>` (sem `JOIN` no SQL).
- [x] Sem registry / sem handler para o enum ⇒ no-op seguro (sem exceção).

### Resultado da Fase 2

**Concluída em 2026-06-21.**

- **Acoplamento (a parte que toca `Abstractions`/`Core`):** `Abstractions` ganhou só `ICriteria.UseHints<THint>`
  (constraint `Enum`, zero ref a OperationHint). `Core` ganhou `Hints/ICriteriaHint.cs` (carrier + `ICriteriaHintVisitor`),
  `Hints/CriteriaHint.cs` (carrier interno), e `CriteriaOptions.Hints`/`AddHint`. **Nenhum** dos dois referencia
  `OperationHint.Abstractions` — exatamente a opção "visitor / double-dispatch" da Decisão 3.
- **EF:** `RegistryHintVisitor<TQuery>` aplica os hints locais via `IHintHandlerRegistry`; `CriteriaQuery.GetEntityQuery()`
  combina ambiente (`Perform`) + locais (registry visitor), cacheado e só nos terminais de entidade.
- **Isolamento garantido:** hints locais vivem em `CriteriaOptions` da própria criteria (nunca no `IHintsContainer`).
  Teste de isolamento (duas criterias, mesmo escopo) confirma por SQL que só a que chamou `UseHints` gera `JOIN`.
- **DI sem mudança:** `IHintHandlerRegistry?` é mais um ctor param defaultado (singleton resolvido quando presente).
- **Testes:** 10 novos (`CriteriaUseHintsTests.cs`), todos verdes. Suíte Searches **132/132**. OperationHint **12/12**
  (intocado — o repo `OperationHint` não foi modificado em nenhuma fase até aqui).

---

## Fase 3 - Paridade `Find`/Repository + docs/samples

**O que/como:** o mesmo hint que dirige o include de query também dirige o caminho pós-carga (`Find`), pois
`AddIncludesHandler` registra **ambos** (`Add<IQueryable<TEntity>,THint>` e `Add<TEntity,DbContext,THint>`).
Fechar a paridade e documentar para consumo por IA.

**Tarefas:**

- [x] Validar/expor que `Find` honra os mesmos hints (ambiente). **Achado:** não há `Repository`/`Find` nos pacotes
      `RoyalCode.SmartSearch.*`; o caminho `Find` (pós-carga) vive no próprio `OperationHint` (`db.Set<T>().Find(id)` +
      `IHintPerformer.Perform(entity, source)`). A paridade já é **estrutural**: `AddIncludesHandler<TEntity,THint>`
      registra os dois handlers (`Add<IQueryable<TEntity>,THint>` para `ICriteria` e `Add<TEntity,DbContext,THint>` para
      `Find`), e já é coberta por [`FindTests.cs`](../RoyalCode.OperationHint.Tests/EFCore/FindTests.cs). Nada a codar.
- [x] (Opcional) `UseHints` por-`Find` simétrico: **deferido** — não há API `Find` nos pacotes Searches para receber
      hints por-consulta, e não houve demanda. O `Find` honra hints ambiente via `Perform(entity, source)`.
- [x] Documentar a feature (orientado a IA): seção "Integração com ICriteria" atualizada em
      [`operation-hint.md`](../.ai/docs/operation-hint.md) (removido o "quando disponível"; +regras de isolamento, paridade
      `Find`, no-op e acoplamento); doc da feature em [`Searches/README.md`](../../../../Searches/README.md); e a nota de
      [`search.md`](../../../../RoyalIdentity/royal-identity/.ai/references/external-libraries/search.md) do repo
      consumidor (RoyalIdentity) reescrita de "não há `Include`" para "Include via Operation Hints".
- [x] Samples: exemplo end-to-end (registro do handler + `UseHints`/ambiente + `Collect`) no `Searches/README.md` e na
      seção End-to-End do `operation-hint.md`; os próprios arquivos de teste são exemplos executáveis.

**Critérios de aceite:** `ICriteria` e `Repository.Find` compartilham a mesma definição de include por hint; docs e
sample publicados; build + suíte verdes nos dois repos.

**Testes:** integração `Find` + hint; build completo das duas solutions.

### Resultado da Fase 3

**Concluída em 2026-06-21.**

- **Paridade `Find`:** validada como estrutural (um único `AddIncludesHandler` cobre query + entidade), já testada no
  `OperationHint` (`FindTests`). Sem código novo nos pacotes Searches (não existe `Repository`/`Find` lá).
- **Docs:** `operation-hint.md` (seção ICriteria atualizada para "implementado" + regras de isolamento/paridade/no-op/
  acoplamento), `Searches/README.md` (guia + sample end-to-end), e a nota do `search.md` do RoyalIdentity reescrita.
- **`UseHints` por-`Find`:** deferido (sem API/demanda).
- **Build + testes (DoD):** verdes nos dois repos — Searches **134/134**, OperationHint **12/12**.

---

## Riscos & mitigação

- **Include duplicado / over-include:** aplicar `Perform`/applicators **uma vez** (flag de idempotência no
  `CriteriaQuery`); `Distinct` natural do EF em includes repetidos, mas evitar reaplicação.
- **Include sob projeção:** garantir que `Select<TDto>` nunca recebe includes (o tipo troca para `TDto`; cobrir por teste).
- **Vazamento ambiente:** hints locais (`UseHints`) via registry direto, nunca via `IHintsContainer` (cobrir por teste de isolamento).
- **Acoplamento de pacote:** se a Decisão 3 for "acoplamento-zero", não tocar `Abstractions`/`Core`; manter `UseHints`
  e storage no pacote `.EntityFramework` via hook genérico.
- **Backward-compat:** dependência de `IHintPerformer`/registry **sempre opcional**; sem OperationHint, zero mudança de comportamento.

## Critério de pronto (Definition of Done)

- Hints ambientes e por-consulta aplicam includes nos terminais de entidade do `ICriteria` (EF).
- `Exists`/`Select<TDto>` intactos; sem OperationHint, comportamento idêntico ao atual.
- `ICriteria` e `Find` compartilham os mesmos hint handlers.
- Suítes verdes nos dois repos; docs/sample orientados a IA atualizados.
