<div align="center">

# ✅ ConfirmIndoctrination

### Подтверждение перед принятием нового последователя · *Cult of the Lamb*

![version](https://img.shields.io/badge/версия-1.0.0-8A2BE2?style=for-the-badge)
![BepInEx](https://img.shields.io/badge/BepInEx-5.4.x-2ea44f?style=for-the-badge)
![game](https://img.shields.io/badge/Cult%20of%20the%20Lamb-c0392b?style=for-the-badge)
![license](https://img.shields.io/badge/лицензия-только%20использование-e67e22?style=for-the-badge)

Автор — **[SoberFurry](https://github.com/SoberFurry)**

<img src="docs/images/confirm-wheel.gif" width="560" alt="Круговое подтверждение: Принять / Отмена" />

</div>

---

## ✨ Возможности

- 🛑 Случайное нажатие больше **не запускает** посвящение мгновенно
- 🎡 **Круговое меню «Принять / Отмена»** — удобно с геймпада
- 🎮 Escape и кнопка отмены на геймпаде = **отказ**
- 🔁 Повторное посвящение уже принятого культиста **не перехватывается**
- 🧷 Безопасно: при отказе последователь не меняется

## 📦 Установка

1. Установите **BepInEx** (пак для Cult of the Lamb).
2. Скачайте архив из **[Releases](../../releases)** и распакуйте в:

```
<Папка игры>\BepInEx\plugins\SoberFurryMods\ConfirmIndoctrination\
```

## 🎮 Поведение

При нажатии на нового ожидающего последователя открывается круговое меню:

| Выбор | Результат |
|---|---|
| ✅ **Принять** | штатный экран имени/формы открывается один раз |
| ❌ **Отмена** | последователь остаётся ждать, ничего не меняется |

## ⚙️ Конфиг

`BepInEx\config\com.soberfurry.cultofthelamb.confirmindoctrination.cfg` —
`Mode` (`BeforeScreen`), `DefaultSelection` (`Cancel`), `InputDebounceMs` (`200`),
`AllowCancelForTutorial` / `AllowCancelForIntegrations` (`false`), `VerboseLogging` (`false`).

## 🛠️ Сборка из исходников

```bash
dotnet build src/SoberFurry.ConfirmIndoctrination/SoberFurry.ConfirmIndoctrination.csproj -c Release -p:GamePath="X:\путь\к\Cult of the Lamb"
```

## 🧩 Другие мои моды

- 🧿 **[MultiNecklaces](https://github.com/SoberFurry/COTL-MultiNecklaces)** — несколько ожерелий на последователе
- ✅ **[ConfirmIndoctrination](https://github.com/SoberFurry/COTL-ConfirmIndoctrination)** — подтверждение при принятии последователя

## 📄 Лицензия

Автор: **[SoberFurry](https://github.com/SoberFurry)**. Разрешено только **скачивание и использование**;
изменение, переиздание и коммерческое использование — без разрешения автора **запрещены**
(см. [LICENSE](LICENSE)). Мод не содержит файлов игры.
