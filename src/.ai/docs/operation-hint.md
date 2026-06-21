# Operation Hint - Documentacao Orientada a IA

**Versao:** 1.0.0+
**Pacotes:** `RoyalCode.OperationHint.Abstractions` + `RoyalCode.OperationHint.EntityFramework`
**Proposito:** aplicar comportamentos declarativos em queries e entidades a partir de hints enum acumulados no escopo da operacao.

## Visao Geral

Operation Hint e um mecanismo scoped e enum-based. O caso de uso principal e carregar navegacoes no Entity Framework de forma centralizada, sem espalhar `.Include(...)` pelos call sites.

Sem Operation Hint:

```csharp
var query = db.Set<Order>()
    .Include(o => o.Customer)
    .Include(o => o.Items);
```

Com Operation Hint:

```csharp
container.AddHint(OrderHints.WithCustomer);
var orders = performer.Perform(db.Set<Order>()).ToList();
```

O pacote `Abstractions` nao depende de EF. A integracao com EF fica no pacote `EntityFramework`.

## Conceitos

### Hint

Um hint e um enum que identifica um comportamento.

```csharp
[Flags]
public enum OrderHints
{
    None = 0,
    WithCustomer = 1,
    WithItems = 2,
    Full = WithCustomer | WithItems
}
```

Use enums tipados, nao strings. As APIs usam `where THint : Enum`.

### Handler de Query

`IHintQueryHandler<TQuery, THint>` transforma uma query.

```csharp
public interface IHintQueryHandler<TQuery, THint>
    where TQuery : class
    where THint : Enum
{
    TQuery Handle(TQuery query, THint hint);
}
```

### Handler de Entidade

`IHintEntityHandler<TEntity, TSource, THint>` transforma uma entidade ja carregada, usando uma fonte como `DbContext`.

```csharp
public interface IHintEntityHandler<TEntity, TSource, THint>
    where TEntity : class
    where TSource : class
    where THint : Enum
{
    void Handle(TEntity entity, TSource source, THint hint);
    Task HandleAsync(TEntity entity, TSource source, THint hint);
}
```

### Registry

`IHintHandlerRegistry` guarda handlers registrados no startup e resolve os handlers pelo par `(tipo alvo, tipo do hint)`.

```csharp
registry.Add<IQueryable<Order>, OrderHints>(handler);
registry.Add<Order, DbContext, OrderHints>(handler);
```

### Performer e Container

`IHintsContainer` acumula hints no escopo. `IHintPerformer` aplica os hints acumulados em uma query ou entidade.

```csharp
var container = scope.ServiceProvider.GetRequiredService<IHintsContainer>();
var performer = scope.ServiceProvider.GetRequiredService<IHintPerformer>();

container.AddHint(OrderHints.Full);

var query = performer.Perform(db.Set<Order>());
var order = db.Set<Order>().Find(id);
if (order is not null)
    performer.Perform(order, db);
```

`DefaultHintPerformer` implementa as duas interfaces e e registrado como scoped.

## Setup

```csharp
services.AddOperationHints();
```

Com configuracao de handlers:

```csharp
services.ConfigureOperationHints(registry =>
{
    registry.Add<IQueryable<Order>, OrderHints>(new OrderQueryHintHandler());
    registry.Add<Order, DbContext, OrderHints>(new OrderEntityHintHandler());
});
```

Servicos registrados:

- `IHintPerformer` scoped.
- `IHintsContainer` scoped, mesma instancia do performer padrao.
- `IHintHandlerRegistry` singleton.

## Entity Framework

O pacote `RoyalCode.OperationHint.EntityFramework` fornece `Includes<TEntity>` e helpers para registrar includes declarativos.

```csharp
services.ConfigureOperationHints(registry =>
    registry.AddIncludesHandler<Order, OrderHints>((hint, includes) =>
    {
        if ((hint & OrderHints.WithCustomer) != 0)
            includes.IncludeReference(o => o.Customer);

        if ((hint & OrderHints.WithItems) != 0)
            includes.IncludeCollection(o => o.Items);
    }));
```

`AddIncludesHandler<TEntity, THint>` registra dois caminhos:

- Query: `IQueryable<TEntity>` + `THint`, para aplicar `Include` antes da materializacao.
- Entidade: `TEntity` + `DbContext` + `THint`, para carregar navegacoes depois de um `Find`.

## Uso em Query

```csharp
public IReadOnlyList<Order> ListOrders(
    IHintsContainer container,
    IHintPerformer performer,
    AppDbContext db)
{
    container.AddHint(OrderHints.WithCustomer);

    return performer
        .Perform(db.Set<Order>().Where(o => o.Status == OrderStatus.Open))
        .ToList();
}
```

Sem hints no container ou sem handler registrado, `Perform(query)` retorna a query original.

## Uso em Find / Pos-Carga

```csharp
public Order? FindOrder(
    int id,
    IHintsContainer container,
    IHintPerformer performer,
    AppDbContext db)
{
    container.AddHint(OrderHints.Full);

    var order = db.Set<Order>().Find(id);
    if (order is not null)
        performer.Perform(order, db);

    return order;
}
```

Para fluxo async:

```csharp
var order = await db.Set<Order>().FindAsync(id);
if (order is not null)
    await performer.PerformAsync(order, db);
```

## Escopo e Composicao

- Hints vivem no escopo atual do DI.
- Hints de um escopo nao vazam para outro.
- Multiples `AddHint(...)` acumulam comportamentos.
- Flags enum podem representar combinacoes como `OrderHints.Full`.
- `Perform(...)` e lazy em relacao ao LINQ: a query so executa quando materializada.

## Boas Praticas

- Registre handlers no startup, em um modulo central de infraestrutura.
- Prefira `AddIncludesHandler<TEntity, THint>` para includes EF, porque ele cobre query e entidade pos-carga.
- Use enums separados quando os contextos de carga forem diferentes.
- Mantenha handlers pequenos e declarativos.
- Teste query e pos-carga quando o mesmo hint deve funcionar nos dois caminhos.

## Troubleshooting

### Hints nao aplicam

Verifique:

- `AddOperationHints()` ou `ConfigureOperationHints(...)` foi chamado.
- O hint foi adicionado ao `IHintsContainer` do mesmo escopo.
- O handler foi registrado com o mesmo tipo de query ou entidade usado em runtime.
- Para EF includes, prefira `AddIncludesHandler<TEntity, THint>`.

### Handler nao e encontrado

O registry resolve por tipo exato. Registrar `IQueryable<Order>` nao atende uma chamada `Perform<List<Order>>(...)`.

### Pos-carga nao carrega navegacoes

Confira se existe handler de entidade. `AddIncludesHandler<TEntity, THint>` ja registra esse handler; registrar apenas `Add<IQueryable<TEntity>, THint>(...)` cobre somente query.

## Referencias de Codigo

- `DefaultHintPerformer`: aplica hints acumulados.
- `IHintHandlerRegistry`: registra e resolve handlers.
- `OperationHintServiceCollectionExtensions`: registra DI e helpers EF.
- `FindTests`: exemplos de pos-carga com `Find`.
- `QueryTestes`: exemplos de query com includes.
