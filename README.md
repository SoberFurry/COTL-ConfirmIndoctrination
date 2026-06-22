# Sam ConfirmIndoctrination

BepInEx-плагин для **Cult of the Lamb**: предотвращает случайный запуск принятия нового последователя
у круга посвящения. Перед открытием экрана имени/формы показывается подтверждение.

- Версия: **1.1.0**
- GUID: `com.sam.cultofthelamb.confirmindoctrination`
- Собрано под: Unity 2022.3.62f2, build 22885603
- Зависимость: BepInEx 5.4.x (пак для COTL). COTL_API не нужен.

## Демонстрация
![Круговое подтверждение: Принять / Отмена](docs/images/confirm-wheel.gif)

## Установка (для игроков)
```
<Папка игры>\BepInEx\plugins\SamMods\ConfirmIndoctrination\
```
Конфиг: `BepInEx\config\com.sam.cultofthelamb.confirmindoctrination.cfg`.

## Поведение
При нажатии взаимодействия на **нового ожидающего** последователя открывается **круговое меню**
(нативное колесо игры) с двумя секторами: **«Принять»** и **«Отмена»**.

- Управляется как обычное колесо — стиком/мышью, **удобно с геймпада**.
- **Отмена встроена**: Escape на клавиатуре и кнопка отмены на геймпаде закрывают как отказ.
- **Принять** → штатный экран имени/формы открывается ровно один раз (одноразовый bypass по ID).
- **Отмена** → последователь остаётся ждать, ничего не меняется.
- **Повторное посвящение** уже принятого культиста **не** перехватывается (цепляемся только к
  `FollowerRecruit.OnInteract/OnSecondaryInteract`, а не к общему `ShowIndoctrinationMenu`).
- Если нативное колесо недоступно — автоматически подстрахует простое окно подтверждения.

## Конфигурация
| Параметр | По умолчанию | Описание |
|---|---|---|
| `Mode` | `BeforeScreen` | `BeforeScreen` / `InsideScreen` / `Both` / `Disabled` |
| `DefaultSelection` | `Cancel` | Безопасный выбор по умолчанию |
| `InputDebounceMs` | `200` | Защита от мгновенного подтверждения |
| `AllowCancelForTutorial` | `false` | Не вмешиваться в обучающего/первого |
| `AllowCancelForIntegrations` | `false` | Не вмешиваться в Twitch-последователей |
| `VerboseLogging` | `false` | Диагностический журнал |

`InsideScreen`/`Both` — экспериментальная кнопка отмены **внутри** экрана посвящения (без удаления
ванильных listeners), выключена по умолчанию. `BeforeScreen` — основной безопасный режим.

## Совместимость
Не трогает listeners кнопки принятия (дружит с Namify и т.п.). При несовпадении сигнатур после
обновления игры — **fail-open**: не блокирует ванильное действие и пишет причину в лог.

## Сборка из исходников
```
dotnet build src/Sam.ConfirmIndoctrination/Sam.ConfirmIndoctrination.csproj -c Release -p:GamePath="X:\путь\к\Cult of the Lamb"
```
Путь к игре также можно задать переменной `COTL_GAMEPATH`. Игровые DLL не входят в репозиторий.

## Удаление
Удалить папку `BepInEx\plugins\SamMods\ConfirmIndoctrination\`.

## Лицензия
MIT. Мод не содержит файлов игры. «Cult of the Lamb» — торговая марка Massive Monster / Devolver Digital.
