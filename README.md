# ICA — Internet Connection Available

Лёгкий монитор состояния интернета для Windows 10/11. Показывает, есть ли у вас подключение, в виде компактного виджета.

![Release](https://img.shields.io/github/v/release/hideopwnz/ica?label=релиз)
![Downloads](https://img.shields.io/github/downloads/hideopwnz/ica/total?label=скачиваний)
![Platform](https://img.shields.io/badge/Windows-10%2F11%20x64-blue)
![License](https://img.shields.io/badge/лицензия-MIT-green)

## Зачем

Чтобы не гадать, работает ли интернет. ICA постоянно проверяет подключение и показывает понятный статус:

- 🟢 **Интернет доступен** — всё работает
- 🔴 **Интернет отсутствует** — связи нет

## Возможности

- Компактный виджет поверх всех окон (справа снизу)
- Иконка в системном трее
- Умная проверка подключения (адаптер → шлюз → HTTPS)
- Защита от «моргания» статуса
- Автозапуск с Windows
- Автоматическое обновление через GitHub Releases
- Self-contained — не требует установки .NET

## Как это работает

ICA выполняет проверки двух уровней.

**Быстрый уровень (каждую секунду):**
- Сетевой адаптер отключён → сразу 🔴
- Шлюз не отвечает (ICMP) 2 раза подряд → 🔴

**Основной уровень (каждые 3 секунды):**
- Параллельные HTTPS-запросы к ya.ru, google.com, cloudflare.com
- Хотя бы один сайт ответил → интернет есть
- Все сайты недоступны 5 раз подряд → 🔴
- Сайты доступны 2 раза подряд → 🟢

Статус 🟢 устанавливается, только когда обе проверки проходят успешно.

## Установка

1. Скачайте **ICA.exe** из [релизов](https://github.com/hideopwnz/ica/releases/latest)
2. Запустите его
3. Готово — конфиг создастся автоматически рядом с программой

## Использование

- Программа запускается свёрнутой в трей
- При смене статуса появляется виджет (зелёный исчезает сам через 5 сек, красный висит до клика)
- Клик по виджету — скрыть
- Клик по иконке в трее — показать / скрыть виджет
- ПКМ по иконке в трее — меню: автозапуск, выход

## Конфигурация

Файл appsettings.json рядом с ICA.exe:

```json
{
  "Sites": [
    "https://ya.ru",
    "https://www.google.com",
    "https://www.cloudflare.com"
  ],
  "AdapterCheckIntervalMs": 1000,
  "HttpsCheckIntervalMs": 3000,
  "FailThreshold": 5,
  "SuccessThreshold": 2
}
```

| Параметр | Описание |
|----------|----------|
| Sites | Сайты для HTTPS-проверки |
| AdapterCheckIntervalMs | Интервал проверки адаптера и шлюза (мс) |
| HttpsCheckIntervalMs | Интервал HTTPS-проверки (мс) |
| FailThreshold | Провалов подряд для статуса «нет интернета» |
| SuccessThreshold | Успехов подряд для статуса «есть интернет» |

## Сборка из исходников

```bash
git clone https://github.com/hideopwnz/ica.git
cd ica
dotnet publish -c Release
```

Результат: ICA/bin/Release/net8.0-windows/win-x64/publish/ICA.exe

## Требования

- Windows 10 / 11 (x64)
- .NET не требуется (встроен в exe)

## Лицензия

[MIT](LICENSE)


