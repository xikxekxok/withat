## Why

Incremental source generator в Roslyn кеширует шаги pipeline, сравнивая значения моделей. `ImmutableArray<T>.Equals` сравнивает **ссылки** на внутренний буфер, а не содержимое, из‑за чего семантически неизменённые модели считаются разными и кеш не срабатывает — генератор лишний раз перестраивает вывод.

## What Changes

- Вводится тип-обёртка `EquatableArray<T>` (или аналог) с сравнением по содержимому для элементов, участвующих в incremental pipeline.
- Поля `ImmutableArray<RecordPropertyModel>` в моделях генератора (`RecordTypeModel`, `RecordPropertyModel`) заменяются на эту обёртку; фабрика моделей заполняет её из `ImmutableArray`/`ToImmutableArray()`.
- Поведение генерируемого кода для потребителей **не** меняется (изменения только во внутренних моделях source generator).

## Capabilities

### New Capabilities

- `equatable-generator-models`: Внутренние модели incremental generator (`RecordTypeModel`, `RecordPropertyModel`) используют коллекции с value-based equality, совместимые с кешированием `IncrementalValuesProvider` / `IncrementalValueProvider`.

### Modified Capabilities

- (пусто — требования к сгенерированному API и семантике `With` не меняются, только внутренняя модель генератора)

## Impact

- Проект `Withat` (source generator): новый тип, правки `Models/*`, возможные точечные правки в коде, проходящем по `Properties` / `NestedProperties`.
- Тесты: при необходимости обновить конструкцию моделей в тестах.
