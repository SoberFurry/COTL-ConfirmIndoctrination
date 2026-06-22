# Sam ConfirmIndoctrination

BepInEx-мод для **Cult of the Lamb**: перед принятием нового последователя показывается подтверждение,
чтобы случайным нажатием не запустить посвящение.

**Версия:** 1.0.0 · **Зависимость:** BepInEx 5.4.x · **Автор:** [SoberFurry](https://github.com/SoberFurry)

## Демонстрация
![Круговое подтверждение: Принять / Отмена](docs/images/confirm-wheel.gif)

## Установка
Скачай из [Releases](../../releases) и распакуй в:
```
<Папка игры>\BepInEx\plugins\SamMods\ConfirmIndoctrination\
```

## Поведение
При нажатии на нового ожидающего последователя открывается **круговое меню «Принять / Отмена»**:
- удобно с геймпада; Escape и кнопка отмены на геймпаде = отказ;
- **Принять** → штатный экран имени/формы открывается один раз;
- **Отмена** → последователь остаётся ждать, ничего не меняется;
- повторное посвящение уже принятого культиста не перехватывается.

## Конфиг (`BepInEx\config\com.sam.cultofthelamb.confirmindoctrination.cfg`)
`Mode` (по умолчанию `BeforeScreen`), `DefaultSelection` (`Cancel`), `InputDebounceMs` (`200`),
`AllowCancelForTutorial`/`AllowCancelForIntegrations` (`false`), `VerboseLogging` (`false`).

## Сборка из исходников
```
dotnet build src/Sam.ConfirmIndoctrination/Sam.ConfirmIndoctrination.csproj -c Release -p:GamePath="X:\путь\к\Cult of the Lamb"
```

## Другие мои моды для Cult of the Lamb
- [MultiNecklaces](https://github.com/SoberFurry/COTL-MultiNecklaces) — несколько ожерелий на последователе
- [ConfirmIndoctrination](https://github.com/SoberFurry/COTL-ConfirmIndoctrination) — подтверждение при принятии последователя
- Все моды автора: https://github.com/SoberFurry?tab=repositories

## Лицензия
Автор: **SoberFurry**. Разрешено только **скачивание и использование** — изменение, переиздание и
коммерческое использование запрещены без разрешения автора (см. [LICENSE](LICENSE)).
Мод не содержит файлов игры.
