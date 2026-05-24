using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Kaleidoscope2.Menu
{
    public static class KaelisMenuLocalizationService
    {
        public const string PlayerPrefsKey = "KAELIS.Menu.Language";

        private static readonly Dictionary<string, string> Russian = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> German = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> Ukrainian = new Dictionary<string, string>();
        private static bool initialized;

        public static event Action LanguageChanged;

        public static KaelisMenuLanguage CurrentLanguage { get; private set; } = KaelisMenuLanguage.English;

        public static void LoadSavedLanguage()
        {
            Initialize();
            string saved = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
            KaelisMenuLanguage language;
            if (!string.IsNullOrWhiteSpace(saved) && Enum.TryParse(saved, out language))
            {
                SetLanguage(language, false);
                return;
            }

            switch (Application.systemLanguage)
            {
                case SystemLanguage.Russian:
                    SetLanguage(KaelisMenuLanguage.Russian, false);
                    break;
                case SystemLanguage.German:
                    SetLanguage(KaelisMenuLanguage.German, false);
                    break;
                case SystemLanguage.Ukrainian:
                    SetLanguage(KaelisMenuLanguage.Ukrainian, false);
                    break;
                default:
                    SetLanguage(KaelisMenuLanguage.English, false);
                    break;
            }
        }

        public static void SetLanguage(KaelisMenuLanguage language)
        {
            SetLanguage(language, true);
        }

        public static void SetLanguage(KaelisMenuLanguage language, bool persist)
        {
            Initialize();
            CurrentLanguage = language;
            if (persist)
            {
                PlayerPrefs.SetString(PlayerPrefsKey, language.ToString());
                PlayerPrefs.Save();
            }

            if (LanguageChanged != null)
            {
                LanguageChanged();
            }

            RefreshAllBoundTexts();
        }

        public static void CycleLanguage()
        {
            KaelisMenuLanguage next;
            switch (CurrentLanguage)
            {
                case KaelisMenuLanguage.English:
                    next = KaelisMenuLanguage.Russian;
                    break;
                case KaelisMenuLanguage.Russian:
                    next = KaelisMenuLanguage.German;
                    break;
                case KaelisMenuLanguage.German:
                    next = KaelisMenuLanguage.Ukrainian;
                    break;
                default:
                    next = KaelisMenuLanguage.English;
                    break;
            }

            SetLanguage(next, true);
        }

        public static string Translate(string source)
        {
            Initialize();
            if (string.IsNullOrEmpty(source) || CurrentLanguage == KaelisMenuLanguage.English)
            {
                return source;
            }

            Dictionary<string, string> table = GetTable(CurrentLanguage);
            string translated;
            return table != null && table.TryGetValue(source, out translated) ? translated : source;
        }

        public static void Bind(TMP_Text text, string source)
        {
            SetText(text, source, false);
        }

        public static void SetText(TMP_Text text, string source)
        {
            SetText(text, source, false);
        }

        public static void SetRawText(TMP_Text text, string value)
        {
            SetText(text, value, true);
        }

        public static string GetLanguageDisplayName(KaelisMenuLanguage language)
        {
            switch (language)
            {
                case KaelisMenuLanguage.Russian:
                    return "Русский";
                case KaelisMenuLanguage.German:
                    return "Deutsch";
                case KaelisMenuLanguage.Ukrainian:
                    return "Українська";
                default:
                    return "English";
            }
        }

        public static string GetCurrentLanguageDisplayName()
        {
            return GetLanguageDisplayName(CurrentLanguage);
        }

        private static void SetText(TMP_Text text, string source, bool raw)
        {
            if (text == null)
            {
                return;
            }

            KaelisMenuLocalizedText localized = text.GetComponent<KaelisMenuLocalizedText>();
            if (localized == null)
            {
                localized = text.gameObject.AddComponent<KaelisMenuLocalizedText>();
            }

            localized.Initialize(text, source, raw);
        }

        private static void RefreshAllBoundTexts()
        {
            KaelisMenuLocalizedText[] texts = Resources.FindObjectsOfTypeAll<KaelisMenuLocalizedText>();
            for (int index = 0; index < texts.Length; index++)
            {
                if (texts[index] != null)
                {
                    texts[index].Refresh();
                }
            }
        }

        private static Dictionary<string, string> GetTable(KaelisMenuLanguage language)
        {
            switch (language)
            {
                case KaelisMenuLanguage.Russian:
                    return Russian;
                case KaelisMenuLanguage.German:
                    return German;
                case KaelisMenuLanguage.Ukrainian:
                    return Ukrainian;
                default:
                    return null;
            }
        }

        private static void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            BuildRussian();
            BuildGerman();
            BuildUkrainian();
        }

        private static void Add(Dictionary<string, string> table, string source, string value)
        {
            table[source] = value;
        }

        private static void BuildRussian()
        {
            AddCommon(Russian, "Ввод / клик", "Ввод / клик для выбора", "Ввод / пробел / клик для переключения", "Ввод / клик для смены языка", "Клик, чтобы посмотреть зарезервированное действие");
            Add(Russian, "ENTER EXPERIENCE", "НАЧАТЬ СЕАНС");
            Add(Russian, "DEMO MODE", "ДЕМО РЕЖИМ");
            Add(Russian, "MODES", "РЕЖИМЫ");
            Add(Russian, "OPTICS", "ОПТИКА");
            Add(Russian, "PRESETS", "ПРЕСЕТЫ");
            Add(Russian, "SETTINGS", "НАСТРОЙКИ");
            Add(Russian, "EXIT", "ВЫХОД");
            Add(Russian, "LIVE PREVIEW", "ЖИВОЙ ПРОСМОТР");
            Add(Russian, "PREVIEW PANEL", "ПАНЕЛЬ ПРОСМОТРА");
            Add(Russian, "RAW IMAGE SURFACE", "ПОВЕРХНОСТЬ ИЗОБРАЖЕНИЯ");
            Add(Russian, "SYSTEM READY     DEMO OFF", "СИСТЕМА ГОТОВА     ДЕМО ВЫКЛ");
            Add(Russian, "SYSTEM READY     DEMO RESERVED", "СИСТЕМА ГОТОВА     ДЕМО ЗАРЕЗЕРВИРОВАНО");
            Add(Russian, "SYSTEM READY", "СИСТЕМА ГОТОВА");
            AddContent(Russian, "ВЫБЕРИТЕ КОНТЕНТ СЕАНСА", "Выберите источники изображений и аудио для этого сеанса.", "ИСТОЧНИК ИЗОБРАЖЕНИЙ", "ИСТОЧНИК МУЗЫКИ", "ВЫБРАТЬ ПАПКУ ИЗОБРАЖЕНИЙ", "ВЫБРАТЬ ПАПКУ МУЗЫКИ", "ОЧИСТИТЬ", "НАЗАД", "СТАРТ СЕАНСА", "Папка изображений не выбрана", "Папка музыки не выбрана", "Папка изображений обязательна. Папка музыки необязательна.");
            AddSections(Russian);
            AddSettings(Russian);
        }

        private static void BuildGerman()
        {
            AddCommon(German, "Enter / Klick", "Enter / Klick zum Auswählen", "Enter / Leertaste / Klick zum Umschalten", "Enter / Klick zum Sprachwechsel", "Klicken, um reservierte Aktion anzusehen");
            Add(German, "ENTER EXPERIENCE", "ERLEBNIS STARTEN");
            Add(German, "DEMO MODE", "DEMO-MODUS");
            Add(German, "MODES", "MODI");
            Add(German, "OPTICS", "OPTIK");
            Add(German, "PRESETS", "PRESETS");
            Add(German, "SETTINGS", "EINSTELLUNGEN");
            Add(German, "EXIT", "BEENDEN");
            Add(German, "LIVE PREVIEW", "LIVE-VORSCHAU");
            Add(German, "PREVIEW PANEL", "VORSCHAU");
            Add(German, "RAW IMAGE SURFACE", "BILDFLÄCHE");
            Add(German, "SYSTEM READY     DEMO OFF", "SYSTEM BEREIT     DEMO AUS");
            Add(German, "SYSTEM READY     DEMO RESERVED", "SYSTEM BEREIT     DEMO RESERVIERT");
            Add(German, "SYSTEM READY", "SYSTEM BEREIT");
            AddContent(German, "ERLEBNISINHALT WÄHLEN", "Wähle Bild- und Audioquellen für diese Sitzung.", "BILDQUELLE", "MUSIKQUELLE", "BILDORDNER WÄHLEN", "MUSIKORDNER WÄHLEN", "LÖSCHEN", "ZURÜCK", "ERLEBNIS STARTEN", "Kein Bildordner gewählt", "Kein Musikordner gewählt", "Ein Bildordner ist erforderlich. Musik ist optional.");
            AddSections(German);
            AddSettings(German);
        }

        private static void BuildUkrainian()
        {
            AddCommon(Ukrainian, "Enter / клік", "Enter / клік для вибору", "Enter / пробіл / клік для перемикання", "Enter / клік для зміни мови", "Клік, щоб переглянути зарезервовану дію");
            Add(Ukrainian, "ENTER EXPERIENCE", "ПОЧАТИ СЕАНС");
            Add(Ukrainian, "DEMO MODE", "ДЕМО РЕЖИМ");
            Add(Ukrainian, "MODES", "РЕЖИМИ");
            Add(Ukrainian, "OPTICS", "ОПТИКА");
            Add(Ukrainian, "PRESETS", "ПРЕСЕТИ");
            Add(Ukrainian, "SETTINGS", "НАЛАШТУВАННЯ");
            Add(Ukrainian, "EXIT", "ВИХІД");
            Add(Ukrainian, "LIVE PREVIEW", "ЖИВИЙ ПЕРЕГЛЯД");
            Add(Ukrainian, "PREVIEW PANEL", "ПАНЕЛЬ ПЕРЕГЛЯДУ");
            Add(Ukrainian, "RAW IMAGE SURFACE", "ПОВЕРХНЯ ЗОБРАЖЕННЯ");
            Add(Ukrainian, "SYSTEM READY     DEMO OFF", "СИСТЕМА ГОТОВА     ДЕМО ВИМК");
            Add(Ukrainian, "SYSTEM READY     DEMO RESERVED", "СИСТЕМА ГОТОВА     ДЕМО ЗАРЕЗЕРВОВАНО");
            Add(Ukrainian, "SYSTEM READY", "СИСТЕМА ГОТОВА");
            AddContent(Ukrainian, "ВИБЕРІТЬ КОНТЕНТ СЕАНСУ", "Оберіть джерела зображень і аудіо для цієї сесії.", "ДЖЕРЕЛО ЗОБРАЖЕНЬ", "ДЖЕРЕЛО МУЗИКИ", "ОБРАТИ ПАПКУ ЗОБРАЖЕНЬ", "ОБРАТИ ПАПКУ МУЗИКИ", "ОЧИСТИТИ", "НАЗАД", "ПОЧАТИ СЕАНС", "Папку зображень не вибрано", "Папку музики не вибрано", "Папка зображень обов'язкова. Музика необов'язкова.");
            AddSections(Ukrainian);
            AddSettings(Ukrainian);
        }

        private static void AddCommon(Dictionary<string, string> table, string action, string select, string toggle, string language, string reserved)
        {
            Add(table, "Enter / Click", action);
            Add(table, "Enter / Click to select", select);
            Add(table, "Enter / Space / Click to toggle", toggle);
            Add(table, "Enter / Click to change language", language);
            Add(table, "Click to view reserved action", reserved);
            Add(table, "Left / Right = small step\nShift + Left / Right = large step\nHome / End = min / max\nR = reset current control",
                table == German
                    ? "Left / Right = kleiner Schritt\nShift + Left / Right = großer Schritt\nHome / End = Min / Max\nR = Regler zurücksetzen"
                    : (table == Russian
                        ? "Left / Right = малый шаг\nShift + Left / Right = большой шаг\nHome / End = мин / макс\nR = сбросить этот регулятор"
                        : "Left / Right = малий крок\nShift + Left / Right = великий крок\nHome / End = мін / макс\nR = скинути цей регулятор"));
            Add(table, "Range", table == German ? "Bereich" : (table == Russian ? "Диапазон" : "Діапазон"));
            Add(table, "Current", table == German ? "Aktuell" : (table == Russian ? "Сейчас" : "Поточне"));
            Add(table, "Keys", table == German ? "Tasten" : (table == Russian ? "Клавиши" : "Клавіші"));
            Add(table, "RESERVED", table == German ? "RESERVIERT" : (table == Russian ? "ЗАРЕЗЕРВИРОВАНО" : "ЗАРЕЗЕРВОВАНО"));
            Add(table, "SAFE COMMAND", table == German ? "SICHERER BEFEHL" : (table == Russian ? "БЕЗОПАСНАЯ КОМАНДА" : "БЕЗПЕЧНА КОМАНДА"));
            Add(table, "COMING SOON", table == German ? "KOMMT BALD" : (table == Russian ? "СКОРО" : "НЕЗАБАРОМ"));
            Add(table, "REAL", table == German ? "AKTIV" : (table == Russian ? "РЕАЛЬНО" : "РЕАЛЬНО"));
            Add(table, "OFF / ON", table == German ? "AUS / EIN" : (table == Russian ? "ВЫКЛ / ВКЛ" : "ВИМК / УВІМК"));
            Add(table, "ON", table == German ? "EIN" : (table == Russian ? "ВКЛ" : "УВІМК"));
            Add(table, "OFF", table == German ? "AUS" : (table == Russian ? "ВЫКЛ" : "ВИМК"));
            Add(table, "Menu action.", table == German ? "Menüaktion." : (table == Russian ? "Действие меню." : "Дія меню."));
            Add(table, "Open the content selection flow for image and audio sources.", table == German ? "Öffnet die Inhaltsauswahl für Bild- und Audioquellen." : (table == Russian ? "Открывает выбор источников изображений и аудио." : "Відкриває вибір джерел зображень і аудіо."));
            Add(table, "Reserved for a dedicated demo playback task. Current click stores only the UI state.", table == German ? "Reserviert für eine eigene Demo-Aufgabe. Der Klick speichert nur den UI-Zustand." : (table == Russian ? "Зарезервировано для отдельной задачи демо. Клик сохраняет только состояние UI." : "Зарезервовано для окремого демо-завдання. Клік зберігає лише стан UI."));
            Add(table, "Open visual route selection cards.", table == German ? "Öffnet Karten zur Auswahl visueller Routen." : (table == Russian ? "Открывает карточки выбора визуальных маршрутов." : "Відкриває картки вибору візуальних маршрутів."));
            Add(table, "Open expressive crystal optics controls.", table == German ? "Öffnet expressive Kristalloptiksteuerung." : (table == Russian ? "Открывает выразительные настройки кристальной оптики." : "Відкриває виразні налаштування кристальної оптики."));
            Add(table, "Open factory look cards and reserved user preset actions.", table == German ? "Öffnet Factory-Looks und reservierte Benutzerpreset-Aktionen." : (table == Russian ? "Открывает фабричные образы и зарезервированные действия пресетов." : "Відкриває фабричні образи й зарезервовані дії пресетів."));
            Add(table, "Open application, audio, controls, system, and diagnostics settings.", table == German ? "Öffnet Einstellungen für App, Audio, Steuerung, System und Diagnose." : (table == Russian ? "Открывает настройки приложения, аудио, управления, системы и диагностики." : "Відкриває налаштування застосунку, аудіо, керування, системи й діагностики."));
            Add(table, "Open the exit confirmation panel. First click never quits immediately.", table == German ? "Öffnet die Beenden-Bestätigung. Der erste Klick beendet nie sofort." : (table == Russian ? "Открывает подтверждение выхода. Первый клик никогда не выходит сразу." : "Відкриває підтвердження виходу. Перший клік ніколи не завершує одразу."));
            Add(table, "Images required; music optional", table == German ? "Bilder erforderlich; Musik optional" : (table == Russian ? "Изображения обязательны; музыка необязательна" : "Зображення обов'язкові; музика необов'язкова"));
            Add(table, "Ready", table == German ? "Bereit" : (table == Russian ? "Готово" : "Готово"));
            Add(table, "Reserved", table == German ? "Reserviert" : (table == Russian ? "Зарезервировано" : "Зарезервовано"));
            Add(table, "Section", table == German ? "Bereich" : (table == Russian ? "Раздел" : "Розділ"));
            Add(table, "Safe", table == German ? "Sicher" : (table == Russian ? "Безопасно" : "Безпечно"));
            Add(table, "Confirm required", table == German ? "Bestätigung erforderlich" : (table == Russian ? "Нужно подтверждение" : "Потрібне підтвердження"));
            Add(table, "Classic / Tunnel / Flight / Reserved", table == German ? "Classic / Tunnel / Flug / reserviert" : (table == Russian ? "Классика / туннель / полёт / резерв" : "Класика / тунель / політ / резерв"));
            Add(table, "Extended creative ranges", table == German ? "Erweiterte Kreativbereiche" : (table == Russian ? "Расширенные творческие диапазоны" : "Розширені творчі діапазони"));
            Add(table, "Factory profiles", table == German ? "Factory-Profile" : (table == Russian ? "Фабричные профили" : "Фабричні профілі"));
            Add(table, "System controls", table == German ? "Systemsteuerung" : (table == Russian ? "Системные настройки" : "Системні налаштування"));
            Add(table, "Status: RESERVED", table == German ? "Status: RESERVIERT" : (table == Russian ? "Статус: ЗАРЕЗЕРВИРОВАНО" : "Статус: ЗАРЕЗЕРВОВАНО"));
            Add(table, "Status: REAL", table == German ? "Status: AKTIV" : (table == Russian ? "Статус: РЕАЛЬНО" : "Статус: РЕАЛЬНО"));
            Add(table, "Status: SAFE COMMAND", table == German ? "Status: SICHERER BEFEHL" : (table == Russian ? "Статус: БЕЗОПАСНАЯ КОМАНДА" : "Статус: БЕЗПЕЧНА КОМАНДА"));
            Add(table, "Status: COMING SOON", table == German ? "Status: KOMMT BALD" : (table == Russian ? "Статус: СКОРО" : "Статус: НЕЗАБАРОМ"));
            Add(table, "Ctrl + Shift + R = Start / Stop recording", table == German ? "Ctrl + Shift + R = Aufnahme starten / stoppen" : (table == Russian ? "Ctrl + Shift + R = старт / стоп записи" : "Ctrl + Shift + R = старт / стоп запису"));
            Add(table, "SECOND DISPLAY READY", table == German ? "ZWEITES DISPLAY BEREIT" : (table == Russian ? "ВТОРОЙ ДИСПЛЕЙ ГОТОВ" : "ДРУГИЙ ДИСПЛЕЙ ГОТОВИЙ"));
            Add(table, "NO SECOND DISPLAY DETECTED", table == German ? "KEIN ZWEITES DISPLAY ERKANNT" : (table == Russian ? "ВТОРОЙ ДИСПЛЕЙ НЕ ОБНАРУЖЕН" : "ДРУГИЙ ДИСПЛЕЙ НЕ ЗНАЙДЕНО"));
            Add(table, "SECOND DISPLAY UNAVAILABLE", table == German ? "ZWEITES DISPLAY NICHT VERFÜGBAR" : (table == Russian ? "ВТОРОЙ ДИСПЛЕЙ НЕДОСТУПЕН" : "ДРУГИЙ ДИСПЛЕЙ НЕДОСТУПНИЙ"));
            Add(table, "SECOND DISPLAY OUTPUT ENABLED", table == German ? "AUSGABE AUF DISPLAY 2 AKTIV" : (table == Russian ? "ВЫВОД НА ВТОРОЙ ДИСПЛЕЙ ВКЛЮЧЁН" : "ВИВІД НА ДРУГИЙ ДИСПЛЕЙ УВІМКНЕНО"));
            Add(table, "SECOND DISPLAY OUTPUT DISABLED", table == German ? "AUSGABE AUF DISPLAY 2 AUS" : (table == Russian ? "ВЫВОД НА ВТОРОЙ ДИСПЛЕЙ ВЫКЛЮЧЕН" : "ВИВІД НА ДРУГИЙ ДИСПЛЕЙ ВИМКНЕНО"));
            Add(table, "SECOND DISPLAY COMMAND UNAVAILABLE", table == German ? "DISPLAY-BEFEHL NICHT VERFÜGBAR" : (table == Russian ? "КОМАНДА ДИСПЛЕЯ НЕДОСТУПНА" : "КОМАНДА ДИСПЛЕЯ НЕДОСТУПНА"));
            Add(table, "RECORDING OFF", table == German ? "AUFNAHME AUS" : (table == Russian ? "ЗАПИСЬ ВЫКЛЮЧЕНА" : "ЗАПИС ВИМКНЕНО"));
            Add(table, "RECORDING READY", table == German ? "AUFNAHME BEREIT" : (table == Russian ? "ЗАПИСЬ ГОТОВА" : "ЗАПИС ГОТОВИЙ"));
            Add(table, "RECORDING RESERVED", table == German ? "AUFNAHME RESERVIERT" : (table == Russian ? "ЗАПИСЬ ЗАРЕЗЕРВИРОВАНА" : "ЗАПИС ЗАРЕЗЕРВОВАНО"));
            Add(table, "RECORDING HOTKEY RESERVED", table == German ? "AUFNAHME-HOTKEY RESERVIERT" : (table == Russian ? "ГОРЯЧАЯ КЛАВИША ЗАПИСИ ЗАРЕЗЕРВИРОВАНА" : "ГАРЯЧУ КЛАВІШУ ЗАПИСУ ЗАРЕЗЕРВОВАНО"));
            Add(table, "RECORDING STARTED", table == German ? "AUFNAHME GESTARTET" : (table == Russian ? "ЗАПИСЬ НАЧАТА" : "ЗАПИС РОЗПОЧАТО"));
            Add(table, "RECORDING STOPPED", table == German ? "AUFNAHME GESTOPPT" : (table == Russian ? "ЗАПИСЬ ОСТАНОВЛЕНА" : "ЗАПИС ЗУПИНЕНО"));
            Add(table, "OUTPUT FOLDER MISSING", table == German ? "AUSGABEORDNER FEHLT" : (table == Russian ? "НЕТ ПАПКИ ВЫВОДА" : "НЕМАЄ ПАПКИ ВИВОДУ"));
            Add(table, "RECORDING OUTPUT FOLDER NOT SELECTED", table == German ? "AUFNAHMEORDNER NICHT GEWÄHLT" : (table == Russian ? "ПАПКА ЗАПИСИ НЕ ВЫБРАНА" : "ПАПКУ ЗАПИСУ НЕ ВИБРАНО"));
            Add(table, "RECORDING OUTPUT FOLDER READY", table == German ? "AUFNAHMEORDNER BEREIT" : (table == Russian ? "ПАПКА ЗАПИСИ ГОТОВА" : "ПАПКА ЗАПИСУ ГОТОВА"));
            Add(table, "RECORDING OUTPUT FOLDER CLEARED", table == German ? "AUFNAHMEORDNER GELÖSCHT" : (table == Russian ? "ПАПКА ЗАПИСИ ОЧИЩЕНА" : "ПАПКУ ЗАПИСУ ОЧИЩЕНО"));
            Add(table, "SESSION STARTED - OUTPUT FOLDER MISSING", table == German ? "SITZUNG GESTARTET - AUSGABEORDNER FEHLT" : (table == Russian ? "СЕАНС ЗАПУЩЕН - НЕТ ПАПКИ ВЫВОДА" : "СЕАНС ЗАПУЩЕНО - НЕМАЄ ПАПКИ ВИВОДУ"));
            Add(table, "SESSION STARTED - RECORDING READY", table == German ? "SITZUNG GESTARTET - AUFNAHME BEREIT" : (table == Russian ? "СЕАНС ЗАПУЩЕН - ЗАПИСЬ ГОТОВА" : "СЕАНС ЗАПУЩЕНО - ЗАПИС ГОТОВИЙ"));
            Add(table, "SESSION STARTED - RECORDING RESERVED", table == German ? "SITZUNG GESTARTET - AUFNAHME RESERVIERT" : (table == Russian ? "СЕАНС ЗАПУЩЕН - ЗАПИСЬ ЗАРЕЗЕРВИРОВАНА" : "СЕАНС ЗАПУЩЕНО - ЗАПИС ЗАРЕЗЕРВОВАНО"));
        }

        private static void AddContent(Dictionary<string, string> table, string title, string subtitle, string imageSource, string musicSource, string selectImage, string selectMusic, string clear, string back, string start, string noImage, string noMusic, string required)
        {
            Add(table, "SELECT EXPERIENCE CONTENT", title);
            Add(table, "Choose image and audio sources for this session.", subtitle);
            Add(table, "IMAGES SOURCE", imageSource);
            Add(table, "MUSIC SOURCE", musicSource);
            Add(table, "SELECT IMAGE FOLDER", selectImage);
            Add(table, "SELECT MUSIC FOLDER", selectMusic);
            Add(table, "CLEAR", clear);
            Add(table, "BACK", back);
            Add(table, "START EXPERIENCE", start);
            Add(table, "No image folder selected", noImage);
            Add(table, "No music folder selected", noMusic);
            Add(table, "Image folder is required. Music folder is optional.", required);
            Add(table, "Supported: .jpg, .jpeg, .png, .bmp, .tga", table == German ? "Unterstützt: .jpg, .jpeg, .png, .bmp, .tga" : (table == Russian ? "Поддерживается: .jpg, .jpeg, .png, .bmp, .tga" : "Підтримується: .jpg, .jpeg, .png, .bmp, .tga"));
            Add(table, "Supported: .mp3, .wav, .ogg, .aiff, .aif. Optional for visual-only sessions.", table == German ? "Unterstützt: .mp3, .wav, .ogg, .aiff, .aif. Optional für reine Bildsitzungen." : (table == Russian ? "Поддерживается: .mp3, .wav, .ogg, .aiff, .aif. Необязательно для визуального сеанса." : "Підтримується: .mp3, .wav, .ogg, .aiff, .aif. Необов'язково для візуального сеансу."));
            Add(table, "Please select an image folder first.", table == German ? "Bitte zuerst einen Bildordner wählen." : (table == Russian ? "Сначала выберите папку изображений." : "Спочатку виберіть папку зображень."));
            Add(table, "Select an image folder to begin. Music is optional.", table == German ? "Wähle zum Start einen Bildordner. Musik ist optional." : (table == Russian ? "Чтобы начать, выберите папку изображений. Музыка необязательна." : "Щоб почати, виберіть папку зображень. Музика необов'язкова."));
            Add(table, "Image folder ready.", table == German ? "Bildordner bereit." : (table == Russian ? "Папка изображений готова." : "Папка зображень готова."));
            Add(table, "Music folder ready. Start Experience will use audio.", table == German ? "Musikordner bereit. Der Start nutzt Audio." : (table == Russian ? "Папка музыки готова. Сеанс будет использовать аудио." : "Папка музики готова. Сеанс використовуватиме аудіо."));
            Add(table, "Selected image folder has no supported image files.", table == German ? "Im Bildordner wurden keine unterstützten Bilddateien gefunden." : (table == Russian ? "В выбранной папке нет поддерживаемых изображений." : "У вибраній папці немає підтримуваних зображень."));
            Add(table, "Selected music folder has no supported audio files. Music is optional.", table == German ? "Im Musikordner wurden keine unterstützten Audiodateien gefunden. Musik ist optional." : (table == Russian ? "В выбранной папке музыки нет поддерживаемых аудиофайлов. Музыка необязательна." : "У вибраній папці музики немає підтримуваних аудіофайлів. Музика необов'язкова."));
            Add(table, "Audio source not selected", table == German ? "Audioquelle nicht gewählt" : (table == Russian ? "Источник аудио не выбран" : "Джерело аудіо не вибрано"));
            Add(table, "Session started. Audio source not selected.", table == German ? "Sitzung gestartet. Audioquelle nicht gewählt." : (table == Russian ? "Сеанс запущен. Источник аудио не выбран." : "Сеанс запущено. Джерело аудіо не вибрано."));
            Add(table, "Session started with image and music sources.", table == German ? "Sitzung mit Bild- und Musikquellen gestartet." : (table == Russian ? "Сеанс запущен с источниками изображений и музыки." : "Сеанс запущено з джерелами зображень і музики."));
            Add(table, "IMAGE FOLDER NOT SELECTED", table == German ? "BILDORDNER NICHT GEWÄHLT" : (table == Russian ? "ПАПКА ИЗОБРАЖЕНИЙ НЕ ВЫБРАНА" : "ПАПКУ ЗОБРАЖЕНЬ НЕ ВИБРАНО"));
            Add(table, "MUSIC FOLDER NOT SELECTED", table == German ? "MUSIKORDNER NICHT GEWÄHLT" : (table == Russian ? "ПАПКА МУЗЫКИ НЕ ВЫБРАНА" : "ПАПКУ МУЗИКИ НЕ ВИБРАНО"));
            Add(table, "IMAGE FOLDER CLEARED", table == German ? "BILDORDNER GELÖSCHT" : (table == Russian ? "ПАПКА ИЗОБРАЖЕНИЙ ОЧИЩЕНА" : "ПАПКУ ЗОБРАЖЕНЬ ОЧИЩЕНО"));
            Add(table, "MUSIC FOLDER CLEARED", table == German ? "MUSIKORDNER GELÖSCHT" : (table == Russian ? "ПАПКА МУЗЫКИ ОЧИЩЕНА" : "ПАПКУ МУЗИКИ ОЧИЩЕНО"));
            Add(table, "IMAGE FOLDER READY", table == German ? "BILDORDNER BEREIT" : (table == Russian ? "ПАПКА ИЗОБРАЖЕНИЙ ГОТОВА" : "ПАПКА ЗОБРАЖЕНЬ ГОТОВА"));
            Add(table, "MUSIC FOLDER READY", table == German ? "MUSIKORDNER BEREIT" : (table == Russian ? "ПАПКА МУЗЫКИ ГОТОВА" : "ПАПКА МУЗИКИ ГОТОВА"));
            Add(table, "IMAGE FOLDER INVALID", table == German ? "BILDORDNER UNGÜLTIG" : (table == Russian ? "ПАПКА ИЗОБРАЖЕНИЙ НЕДЕЙСТВИТЕЛЬНА" : "ПАПКА ЗОБРАЖЕНЬ НЕДІЙСНА"));
            Add(table, "MUSIC FOLDER INVALID", table == German ? "MUSIKORDNER UNGÜLTIG" : (table == Russian ? "ПАПКА МУЗЫКИ НЕДЕЙСТВИТЕЛЬНА" : "ПАПКА МУЗИКИ НЕДІЙСНА"));
            Add(table, "IMAGE FOLDER REQUIRED", table == German ? "BILDORDNER ERFORDERLICH" : (table == Russian ? "НУЖНА ПАПКА ИЗОБРАЖЕНИЙ" : "ПОТРІБНА ПАПКА ЗОБРАЖЕНЬ"));
            Add(table, "IMAGE SOURCE UNAVAILABLE", table == German ? "BILDQUELLE NICHT VERFÜGBAR" : (table == Russian ? "ИСТОЧНИК ИЗОБРАЖЕНИЙ НЕДОСТУПЕН" : "ДЖЕРЕЛО ЗОБРАЖЕНЬ НЕДОСТУПНЕ"));
            Add(table, "SESSION STARTED", table == German ? "SITZUNG GESTARTET" : (table == Russian ? "СЕАНС ЗАПУЩЕН" : "СЕАНС ЗАПУЩЕНО"));
            Add(table, "SESSION STARTED - AUDIO SOURCE NOT SELECTED", table == German ? "SITZUNG GESTARTET - AUDIOQUELLE FEHLT" : (table == Russian ? "СЕАНС ЗАПУЩЕН - АУДИО НЕ ВЫБРАНО" : "СЕАНС ЗАПУЩЕНО - АУДІО НЕ ВИБРАНО"));
        }

        private static void AddSections(Dictionary<string, string> table)
        {
            bool ru = table == Russian;
            bool de = table == German;
            Add(table, "Runtime visual routes", de ? "Visuelle Laufzeitrouten" : (ru ? "Маршруты визуального движка" : "Маршрути візуального рушія"));
            Add(table, "Expressive crystal optics controls", de ? "Ausdrucksstarke Kristalloptik" : (ru ? "Выразительные настройки кристальной оптики" : "Виразні налаштування кристальної оптики"));
            Add(table, "Factory looks and user slots", de ? "Factory-Looks und Benutzerplätze" : (ru ? "Фабричные образы и пользовательские слоты" : "Фабричні образи та користувацькі слоти"));
            Add(table, "Application and system controls", de ? "Anwendungs- und Systemsteuerung" : (ru ? "Настройки приложения и системы" : "Налаштування застосунку й системи"));
            Add(table, "MODES SECTION", de ? "MODI-BEREICH" : (ru ? "РАЗДЕЛ РЕЖИМОВ" : "РОЗДІЛ РЕЖИМІВ"));
            Add(table, "OPTICS SECTION", de ? "OPTIK-BEREICH" : (ru ? "РАЗДЕЛ ОПТИКИ" : "РОЗДІЛ ОПТИКИ"));
            Add(table, "PRESETS SECTION", de ? "PRESET-BEREICH" : (ru ? "РАЗДЕЛ ПРЕСЕТОВ" : "РОЗДІЛ ПРЕСЕТІВ"));
            Add(table, "SETTINGS SECTION", de ? "EINSTELLUNGEN" : (ru ? "РАЗДЕЛ НАСТРОЕК" : "РОЗДІЛ НАЛАШТУВАНЬ"));
            Add(table, "EXIT CONFIRMATION", de ? "BEENDEN BESTÄTIGEN" : (ru ? "ПОДТВЕРЖДЕНИЕ ВЫХОДА" : "ПІДТВЕРДЖЕННЯ ВИХОДУ"));
            Add(table, "SECTION CLOSED", de ? "BEREICH GESCHLOSSEN" : (ru ? "РАЗДЕЛ ЗАКРЫТ" : "РОЗДІЛ ЗАКРИТО"));
            Add(table, "EXIT CANCELLED", de ? "BEENDEN ABGEBROCHEN" : (ru ? "ВЫХОД ОТМЕНЁН" : "ВИХІД СКАСОВАНО"));
            Add(table, "ACTION RESERVED", de ? "AKTION RESERVIERT" : (ru ? "ДЕЙСТВИЕ ЗАРЕЗЕРВИРОВАНО" : "ДІЮ ЗАРЕЗЕРВОВАНО"));
            Add(table, "EXIT KAELIS?", de ? "KAELIS BEENDEN?" : (ru ? "ВЫЙТИ ИЗ KAELIS?" : "ВИЙТИ З KAELIS?"));
            Add(table, "Confirm application exit", de ? "Beenden bestätigen" : (ru ? "Подтвердите выход" : "Підтвердіть вихід"));
            Add(table, "Classic 2D", de ? "Classic 2D" : (ru ? "Классический 2D" : "Класичний 2D"));
            Add(table, "Premium 3D Crystal", de ? "Premium-3D-Kristall" : (ru ? "Премиум 3D кристалл" : "Преміум 3D кристал"));
            Add(table, "4D Tunnel / Funnel", de ? "4D-Tunnel / Trichter" : (ru ? "4D туннель / воронка" : "4D тунель / воронка"));
            Add(table, "5D Endless Flight", de ? "5D-Endlosflug" : (ru ? "5D бесконечный полёт" : "5D нескінченний політ"));
            Add(table, "Experimental / Coming Soon", de ? "Experimentell / bald verfügbar" : (ru ? "Эксперименты / скоро" : "Експерименти / незабаром"));
            Add(table, "Original kaleidoscope surface renderer.", de ? "Originaler Kaleidoskop-Oberflächenrenderer." : (ru ? "Оригинальный рендер поверхности калейдоскопа." : "Оригінальний рендер поверхні калейдоскопа."));
            Add(table, "Crystal-based premium optical scene. Protected until a safe public menu hook exists.", de ? "Premium-Optikszene auf Kristallbasis. Geschützt, bis ein sicherer Menü-Hook verfügbar ist." : (ru ? "Премиальная оптическая сцена на базе кристалла. Защищено до появления безопасного меню-хука." : "Преміальна оптична сцена на базі кристала. Захищено до появи безпечного меню-хука."));
            Add(table, "Depth/funnel mode with curved visual space.", de ? "Tiefen-/Trichtermodus mit gekrümmtem Bildraum." : (ru ? "Режим глубины/воронки с изогнутым визуальным пространством." : "Режим глибини/воронки з викривленим візуальним простором."));
            Add(table, "Continuous movement toward the kaleidoscope center.", de ? "Kontinuierliche Bewegung zum Kaleidoskopzentrum." : (ru ? "Непрерывное движение к центру калейдоскопа." : "Безперервний рух до центру калейдоскопа."));
            Add(table, "Future optical modes remain staged here until promoted.", de ? "Zukünftige optische Modi bleiben hier, bis sie freigegeben werden." : (ru ? "Будущие оптические режимы остаются здесь до продвижения." : "Майбутні оптичні режими залишаються тут до випуску."));
            AddProductionLabels(table);
            AddOpticsLabels(table);
            AddPresetLabels(table);
            Add(table, "CLOSE", de ? "SCHLIESSEN" : (ru ? "ЗАКРЫТЬ" : "ЗАКРИТИ"));
            Add(table, "SAVE SETTINGS AND EXIT", de ? "SPEICHERN UND BEENDEN" : (ru ? "СОХРАНИТЬ И ВЫЙТИ" : "ЗБЕРЕГТИ І ВИЙТИ"));
            Add(table, "EXIT WITHOUT SAVING", de ? "OHNE SPEICHERN BEENDEN" : (ru ? "ВЫЙТИ БЕЗ СОХРАНЕНИЯ" : "ВИЙТИ БЕЗ ЗБЕРЕЖЕННЯ"));
            Add(table, "CANCEL", de ? "ABBRECHEN" : (ru ? "ОТМЕНА" : "СКАСУВАТИ"));
            Add(table, "Close the KAELIS experience shell. Saving settings is reserved until a persistence service exists.", de ? "Schließt die KAELIS-Erlebnishülle. Speichern ist reserviert, bis ein Persistenzdienst existiert." : (ru ? "Закрывает оболочку KAELIS. Сохранение зарезервировано до появления сервиса настроек." : "Закриває оболонку KAELIS. Збереження зарезервовано до появи сервісу налаштувань."));
        }

        private static void AddProductionLabels(Dictionary<string, string> table)
        {
            bool ru = table == Russian;
            bool de = table == German;
            Add(table, "SHOWCASE / RECORDING", de ? "SHOWCASE / AUFNAHME" : (ru ? "ПОКАЗ / ЗАПИСЬ" : "ПОКАЗ / ЗАПИС"));
            Add(table, "Production tools for external display output and video clip capture.", de ? "Produktionswerkzeuge für externe Ausgabe und Videoclip-Aufnahme." : (ru ? "Производственные инструменты для внешнего дисплея и записи видеоклипов." : "Виробничі інструменти для зовнішнього дисплея й запису відеокліпів."));
            Add(table, "Output To Second Display", de ? "Ausgabe auf zweites Display" : (ru ? "Вывод на второй дисплей" : "Вивід на другий дисплей"));
            Add(table, "OUTPUT TO SECOND DISPLAY", de ? "AUSGABE AUF ZWEITES DISPLAY" : (ru ? "ВЫВОД НА ВТОРОЙ ДИСПЛЕЙ" : "ВИВІД НА ДРУГИЙ ДИСПЛЕЙ"));
            Add(table, "First monitor remains the control panel. Kaleidoscope output is sent to the second monitor.", de ? "Der erste Monitor bleibt Bedienfeld. Die Kaleidoskop-Ausgabe geht auf Monitor zwei." : (ru ? "Первый монитор остаётся панелью управления. Калейдоскоп выводится на второй монитор." : "Перший монітор лишається панеллю керування. Калейдоскоп виводиться на другий монітор."));
            Add(table, "Second display: Not detected", de ? "Zweites Display: nicht erkannt" : (ru ? "Второй дисплей: не обнаружен" : "Другий дисплей: не знайдено"));
            Add(table, "Second display: Available", de ? "Zweites Display: verfügbar" : (ru ? "Второй дисплей: доступен" : "Другий дисплей: доступний"));
            Add(table, "Second display: Output enabled", de ? "Zweites Display: Ausgabe aktiv" : (ru ? "Второй дисплей: вывод включён" : "Другий дисплей: вивід увімкнено"));
            Add(table, "TEST DISPLAY", de ? "DISPLAY TESTEN" : (ru ? "ПРОВЕРИТЬ ДИСПЛЕЙ" : "ПЕРЕВІРИТИ ДИСПЛЕЙ"));
            Add(table, "Create Video Clip", de ? "Videoclip erstellen" : (ru ? "Создать видеоклип" : "Створити відеокліп"));
            Add(table, "CREATE VIDEO CLIP", de ? "VIDEOCLIP ERSTELLEN" : (ru ? "СОЗДАТЬ ВИДЕОКЛИП" : "СТВОРИТИ ВІДЕОКЛІП"));
            Add(table, "Records kaleidoscope output to a video file. Auto-starts when experience starts if enabled.", de ? "Zeichnet die Kaleidoskop-Ausgabe als Videodatei auf. Startet bei Sitzungsbeginn automatisch, wenn aktiv." : (ru ? "Записывает вывод калейдоскопа в видеофайл. Если включено, стартует вместе с сеансом." : "Записує вивід калейдоскопа у відеофайл. Якщо увімкнено, стартує разом із сеансом."));
            Add(table, "No output folder selected", de ? "Kein Ausgabeordner gewählt" : (ru ? "Папка вывода не выбрана" : "Папку виводу не вибрано"));
            Add(table, "SELECT OUTPUT FOLDER", de ? "AUSGABEORDNER WÄHLEN" : (ru ? "ВЫБРАТЬ ПАПКУ ВЫВОДА" : "ОБРАТИ ПАПКУ ВИВОДУ"));
            Add(table, "Recording: Disabled", de ? "Aufnahme: aus" : (ru ? "Запись: выключена" : "Запис: вимкнено"));
            Add(table, "Recording: Output folder missing", de ? "Aufnahme: Ausgabeordner fehlt" : (ru ? "Запись: нет папки вывода" : "Запис: немає папки виводу"));
            Add(table, "Recording: Ready", de ? "Aufnahme: bereit" : (ru ? "Запись: готова" : "Запис: готовий"));
            Add(table, "Recording: Reserved", de ? "Aufnahme: reserviert" : (ru ? "Запись: зарезервирована" : "Запис: зарезервовано"));
        }

        private static void AddOpticsLabels(Dictionary<string, string> table)
        {
            bool ru = table == Russian;
            bool de = table == German;
            Add(table, "Brightness", de ? "Helligkeit" : (ru ? "Яркость" : "Яскравість"));
            Add(table, "Contrast", de ? "Kontrast" : (ru ? "Контраст" : "Контраст"));
            Add(table, "Bloom / Glow", de ? "Bloom / Glanz" : (ru ? "Свечение / блум" : "Світіння / bloom"));
            Add(table, "Facet Highlights", de ? "Facettenlichter" : (ru ? "Блики граней" : "Відблиски граней"));
            Add(table, "Refraction Strength", de ? "Brechungsstärke" : (ru ? "Сила преломления" : "Сила заломлення"));
            Add(table, "Reflection Strength", de ? "Reflexionsstärke" : (ru ? "Сила отражения" : "Сила відбиття"));
            Add(table, "Internal Reflections", de ? "Innere Reflexionen" : (ru ? "Внутренние отражения" : "Внутрішні відбиття"));
            Add(table, "Background Distortion", de ? "Hintergrundverzerrung" : (ru ? "Искажение фона" : "Викривлення фону"));
            Add(table, "Direct Transparency", de ? "Direkte Transparenz" : (ru ? "Прямая прозрачность" : "Пряма прозорість"));
            Add(table, "Prism Dispersion", de ? "Prismendispersion" : (ru ? "Призменная дисперсия" : "Призмова дисперсія"));
            Add(table, "Chromatic Aberration", de ? "Chromatische Aberration" : (ru ? "Хроматическая аберрация" : "Хроматична аберація"));
            Add(table, "Rainbow Edge", de ? "Regenbogenkante" : (ru ? "Радужная кромка" : "Райдужна кромка"));
            Add(table, "Spectral Split", de ? "Spektrale Trennung" : (ru ? "Спектральное разделение" : "Спектральний поділ"));
            Add(table, "Crystal Depth", de ? "Kristalltiefe" : (ru ? "Глубина кристалла" : "Глибина кристала"));
            Add(table, "Caustics", de ? "Kaustiken" : (ru ? "Каустика" : "Каустика"));
            Add(table, "Spotlight Shadow", de ? "Spotlight-Schatten" : (ru ? "Тень прожектора" : "Тінь прожектора"));
            Add(table, "Mirror Backdrop", de ? "Spiegelhintergrund" : (ru ? "Зеркальный фон" : "Дзеркальне тло"));
            Add(table, "From dark jewel mood to bright luminous crystal.", de ? "Von dunkler Edelstein-Stimmung bis zu hellem Kristallleuchten." : (ru ? "От тёмного ювелирного настроения до яркого сияющего кристалла." : "Від темного ювелірного настрою до яскравого сяйного кристала."));
            Add(table, "From soft dreamy blending to hard dramatic separation.", de ? "Von weicher Traumwirkung bis zu harter dramatischer Trennung." : (ru ? "От мягкого мечтательного смешения до жёсткого драматичного разделения." : "Від м'якого мрійного змішування до різкого драматичного поділу."));
            Add(table, "Controls radiant gem glow and bloom intensity.", de ? "Steuert Edelsteinleuchten und Bloom-Intensität." : (ru ? "Управляет сиянием самоцвета и интенсивностью bloom." : "Керує сяйвом самоцвіту та інтенсивністю bloom."));
            Add(table, "Strengthens sparkle flashes and facet edge highlights.", de ? "Verstärkt Funkeln und Facettenkanten." : (ru ? "Усиливает вспышки блеска и подсветку граней." : "Підсилює спалахи блиску та підсвітку граней."));
            Add(table, "Changes how strongly the background bends through the crystal.", de ? "Ändert, wie stark sich der Hintergrund durch den Kristall bricht." : (ru ? "Меняет силу изгиба фона через кристалл." : "Змінює силу викривлення тла крізь кристал."));
            Add(table, "Makes facets more mirror-like and polished.", de ? "Macht Facetten spiegelnder und polierter." : (ru ? "Делает грани более зеркальными и полированными." : "Робить грані більш дзеркальними й відполірованими."));
            Add(table, "Adds deeper inner reflection echoes inside the gem.", de ? "Fügt tiefere innere Reflexionsechos im Stein hinzu." : (ru ? "Добавляет более глубокие внутренние отражения внутри самоцвета." : "Додає глибші внутрішні відбиття всередині самоцвіту."));
            Add(table, "Moves from subtle lensing to surreal image bending.", de ? "Von subtiler Linsenwirkung bis zu surrealer Bildbiegung." : (ru ? "От тонкого линзирования до сюрреалистического изгиба изображения." : "Від тонкого лінзування до сюрреалістичного викривлення зображення."));
            Add(table, "Controls how much direct background remains visible through crystal.", de ? "Steuert, wie viel Hintergrund direkt durch den Kristall sichtbar bleibt." : (ru ? "Определяет, сколько прямого фона видно через кристалл." : "Визначає, скільки прямого тла видно крізь кристал."));
            Add(table, "Expands rainbow prism separation on edges and facets.", de ? "Erweitert Regenbogen-Prismentrennung an Kanten und Facetten." : (ru ? "Усиливает радужное призменное разделение на кромках и гранях." : "Підсилює райдужний призматичний поділ на кромках і гранях."));
            Add(table, "Adds RGB edge separation and spectral cinematic color.", de ? "Fügt RGB-Kantentrennung und spektrale Kinofarbe hinzu." : (ru ? "Добавляет RGB-разделение краёв и спектральный кинематичный цвет." : "Додає RGB-поділ країв і спектральний кінематичний колір."));
            Add(table, "Controls colorful glowing edges and spectral highlights.", de ? "Steuert farbige Leuchtkanten und spektrale Highlights." : (ru ? "Управляет цветными светящимися кромками и спектральными бликами." : "Керує кольоровими сяйними кромками та спектральними відблисками."));
            Add(table, "Deepens prismatic energy and color separation.", de ? "Vertieft prismatische Energie und Farbtrennung." : (ru ? "Углубляет призматическую энергию и разделение цвета." : "Поглиблює призматичну енергію та поділ кольорів."));
            Add(table, "Moves from shallow glass to heavy optical mass.", de ? "Von flachem Glas zu schwerer optischer Masse." : (ru ? "От лёгкого стекла до массивной оптической глубины." : "Від легкого скла до масивної оптичної глибини."));
            Add(table, "Adds focused light traces and projected sparkle patterns.", de ? "Fügt fokussierte Lichtspuren und projizierte Funkelmuster hinzu." : (ru ? "Добавляет сфокусированные световые следы и проекции блеска." : "Додає сфокусовані світлові сліди та проекції блиску."));
            Add(table, "Adds a controlled shadow relationship for a spotlight-like premium stage.", de ? "Fügt kontrollierte Schatten für eine premiumartige Spotlight-Bühne hinzu." : (ru ? "Добавляет управляемую тень для премиальной сцены с прожектором." : "Додає керовану тінь для преміальної сцени з прожектором."));
            Add(table, "Adds a reflective backdrop concept for future optical depth.", de ? "Fügt ein reflektierendes Hintergrundkonzept für künftige optische Tiefe hinzu." : (ru ? "Добавляет концепт отражающего фона для будущей оптической глубины." : "Додає концепт відбивного тла для майбутньої оптичної глибини."));
        }

        private static void AddPresetLabels(Dictionary<string, string> table)
        {
            bool ru = table == Russian;
            bool de = table == German;
            Add(table, "Diamond Palace", de ? "Diamantpalast" : (ru ? "Бриллиантовый дворец" : "Діамантовий палац"));
            Add(table, "Blue Ice", de ? "Blaues Eis" : (ru ? "Синий лёд" : "Синя крига"));
            Add(table, "Golden Prism", de ? "Goldenes Prisma" : (ru ? "Золотая призма" : "Золота призма"));
            Add(table, "Ruby Night", de ? "Rubinnacht" : (ru ? "Рубиновая ночь" : "Рубінова ніч"));
            Add(table, "Emerald Depth", de ? "Smaragdtiefe" : (ru ? "Изумрудная глубина" : "Смарагдова глибина"));
            Add(table, "Opal Dream", de ? "Opaltraum" : (ru ? "Опаловая мечта" : "Опалова мрія"));
            Add(table, "Cosmic Glass", de ? "Kosmisches Glas" : (ru ? "Космическое стекло" : "Космічне скло"));
            Add(table, "Dark Luxury", de ? "Dunkler Luxus" : (ru ? "Тёмная роскошь" : "Темна розкіш"));
            Add(table, "APPLY SELECTED", de ? "AUSWAHL ANWENDEN" : (ru ? "ПРИМЕНИТЬ ВЫБОР" : "ЗАСТОСУВАТИ ВИБІР"));
            Add(table, "SAVE CURRENT", de ? "AKTUELLES SPEICHERN" : (ru ? "СОХРАНИТЬ ТЕКУЩЕЕ" : "ЗБЕРЕГТИ ПОТОЧНЕ"));
            Add(table, "RENAME", de ? "UMBENENNEN" : (ru ? "ПЕРЕИМЕНОВАТЬ" : "ПЕРЕЙМЕНУВАТИ"));
            Add(table, "DELETE", de ? "LÖSCHEN" : (ru ? "УДАЛИТЬ" : "ВИДАЛИТИ"));
            Add(table, "RESET FACTORY", de ? "WERK ZURÜCKSETZEN" : (ru ? "СБРОС ФАБРИЧНЫХ" : "СКИНУТИ ФАБРИЧНІ"));
        }

        private static void AddSettings(Dictionary<string, string> table)
        {
            bool ru = table == Russian;
            bool de = table == German;
            Add(table, "DISPLAY", de ? "ANZEIGE" : (ru ? "ЭКРАН" : "ЕКРАН"));
            Add(table, "AUDIO", de ? "AUDIO" : (ru ? "АУДИО" : "АУДІО"));
            Add(table, "CONTROLS", de ? "STEUERUNG" : (ru ? "УПРАВЛЕНИЕ" : "КЕРУВАННЯ"));
            Add(table, "SYSTEM", de ? "SYSTEM" : (ru ? "СИСТЕМА" : "СИСТЕМА"));
            Add(table, "DIAGNOSTICS", de ? "DIAGNOSE" : (ru ? "ДИАГНОСТИКА" : "ДІАГНОСТИКА"));
            Add(table, "Resolution", de ? "Auflösung" : (ru ? "Разрешение" : "Роздільність"));
            Add(table, "Fullscreen", de ? "Vollbild" : (ru ? "Полный экран" : "Повний екран"));
            Add(table, "VSync", de ? "VSync" : (ru ? "VSync" : "VSync"));
            Add(table, "Target FPS", de ? "Ziel-FPS" : (ru ? "Целевой FPS" : "Цільовий FPS"));
            Add(table, "UI Scale", de ? "UI-Skalierung" : (ru ? "Масштаб UI" : "Масштаб UI"));
            Add(table, "Master Volume", de ? "Gesamtlautstärke" : (ru ? "Общая громкость" : "Загальна гучність"));
            Add(table, "Menu Volume", de ? "Menülautstärke" : (ru ? "Громкость меню" : "Гучність меню"));
            Add(table, "Demo Volume", de ? "Demo-Lautstärke" : (ru ? "Громкость демо" : "Гучність демо"));
            Add(table, "Mute", de ? "Stumm" : (ru ? "Без звука" : "Без звуку"));
            Add(table, "Mouse Wheel Crystal Scale", de ? "Kristallskalierung per Mausrad" : (ru ? "Масштаб кристалла колесом мыши" : "Масштаб кристала колесом миші"));
            Add(table, "Mouse Sensitivity", de ? "Mausempfindlichkeit" : (ru ? "Чувствительность мыши" : "Чутливість миші"));
            Add(table, "Crystal Scale Step", de ? "Kristall-Skalenschritt" : (ru ? "Шаг масштаба кристалла" : "Крок масштабу кристала"));
            Add(table, "Invert Zoom", de ? "Zoom umkehren" : (ru ? "Инвертировать зум" : "Інвертувати зум"));
            Add(table, "Hotkeys", de ? "Hotkeys" : (ru ? "Горячие клавиши" : "Гарячі клавіші"));
            Add(table, "Reset Hotkeys", de ? "Hotkeys zurücksetzen" : (ru ? "Сбросить горячие клавиши" : "Скинути гарячі клавіші"));
            Add(table, "LANGUAGE", de ? "SPRACHE" : (ru ? "ЯЗЫК" : "МОВА"));
            Add(table, "Language", de ? "Sprache" : (ru ? "Язык" : "Мова"));
            Add(table, "Start With Menu", de ? "Mit Menü starten" : (ru ? "Запускать с меню" : "Запускати з меню"));
            Add(table, "Auto Save Settings", de ? "Einstellungen automatisch speichern" : (ru ? "Автосохранение настроек" : "Автозбереження налаштувань"));
            Add(table, "Reset Settings", de ? "Einstellungen zurücksetzen" : (ru ? "Сбросить настройки" : "Скинути налаштування"));
            Add(table, "Open Logs Folder", de ? "Logordner öffnen" : (ru ? "Открыть папку логов" : "Відкрити папку логів"));
            Add(table, "Show FPS", de ? "FPS anzeigen" : (ru ? "Показать FPS" : "Показати FPS"));
            Add(table, "Show Diagnostics", de ? "Diagnose anzeigen" : (ru ? "Показать диагностику" : "Показати діагностику"));
            Add(table, "Show Input Overlay", de ? "Eingabe-Overlay anzeigen" : (ru ? "Показать ввод" : "Показати ввід"));
            Add(table, "Show Render Stats", de ? "Renderstatistik anzeigen" : (ru ? "Показать статистику рендера" : "Показати статистику рендера"));
            Add(table, "English / Русский / Deutsch / Українська", de ? "English / Русский / Deutsch / Українська" : "English / Русский / Deutsch / Українська");
            Add(table, "Changes the KAELIS menu language immediately and stores it for the next session.", de ? "Ändert die KAELIS-Menüsprache sofort und speichert sie für die nächste Sitzung." : (ru ? "Сразу меняет язык меню KAELIS и сохраняет его для следующего сеанса." : "Одразу змінює мову меню KAELIS і зберігає її для наступного сеансу."));
        }
    }
}
