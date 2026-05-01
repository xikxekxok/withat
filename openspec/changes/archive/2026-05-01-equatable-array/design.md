## Context

Модели `RecordTypeModel` и `RecordPropertyModel` передаются через `IncrementalValuesProvider` в incremental source generator. Сейчас коллекции свойств представлены как `ImmutableArray<T>`. Документация Roslyn ([incremental-generators.cookbook.md](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md)) и практика community ([Andrew Lock](https://andrewlock.net/creating-a-source-generator-part-9-avoiding-performance-pitfalls-in-incremental-generators/), паттерн Community Toolkit `EquatableArray`) единогласны: для кеша нужна **структурная** эквивалентность коллекций; у `ImmutableArray<T>` сравнение по умолчанию не гарантирует это для содержимого.

## Goals / Non-Goals

**Goals:**

- Внутренние модели генератора сравнимы по значению так, что при неизменной семантике входных данных downstream-шаги pipeline могут переиспользовать кеш.
- Минимальный объём кода: один переиспользуемый тип-обёртка и замена типов полей в моделях + фабрике.

**Non-Goals:**

- Изменение публичного API пакета Withat для потребителей или формата генерируемого кода.
- Замена всех использований `ImmutableArray` в тестовых фикстурах семантики API (кроме сборки моделей генератора, где тип полей меняется).

## Decisions

1. **Тип `EquatableArray<T>`** — `readonly struct`, хранит `ImmutableArray<T>`, реализует `IEquatable<EquatableArray<T>>`: `Equals` и `GetHashCode` по элементам через `EqualityComparer<T>.Default` (как в Roslyn cookbook / Community Toolkit). Для пустых массивов — стабильное поведение.

2. **Почему не только `record` с массивом** — `record` генерирует равенство для свойств, но вложенный `ImmutableArray` всё равно сравнивается по ссылке на буфер; нужна явная обёртка.

3. **Альтернатива** — кастомный `IEqualityComparer<RecordTypeModel>` на этапе `Select`/`WithComparer`: возможно, но дублирует знание о всех полях и легко забыть при добавлении свойств; обёртка на коллекциях локализует проблему.

## Risks / Trade-offs

- [Дублирование с Community Toolkit] → Не тянем зависимость в source-only проект ради одного типа; копируем проверенный минимальный паттерн (MIT-совместимый стиль).

- [Производительность GetHashCode на больших списках свойств] → Приемлемо для размеров типичной модели record; при необходимости позже можно сузить хеш (не требуется в первой итерации).

## Migration Plan

Изменения только в сборке генератора; потребители не мигрируют. Откат — revert коммита.

## Open Questions

Нет.
