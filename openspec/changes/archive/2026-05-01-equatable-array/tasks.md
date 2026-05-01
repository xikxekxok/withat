## 1. Тип EquatableArray

- [x] 1.1 Добавить `EquatableArray<T>` (`readonly struct`, `IEquatable<EquatableArray<T>>`, фабрики из `ImmutableArray` / `ReadOnlySpan`, безопасное сравнение и хеш по элементам).

## 2. Модели и фабрика

- [x] 2.1 Заменить `ImmutableArray<RecordPropertyModel>` на `EquatableArray<RecordPropertyModel>` в `RecordTypeModel` и `RecordPropertyModel`.
- [x] 2.2 Обновить `RecordTypeModelFactory` для сборки `EquatableArray` из построенных immutable-массивов.

## 3. Потребители и проверка

- [x] 3.1 Обновить код, обращающийся к `.Properties` / `.NestedProperties` (если нужна замена `.AsImmutable()` / `.Length` / индексаторов — через `.ImmutableArray` или индексатор на обёртке).
- [x] 3.2 Прогнать `dotnet test` и исправить регрессии.
