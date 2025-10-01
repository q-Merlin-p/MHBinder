🧺 Зависимости:
	- .NET 9.0	


📂 Архитектура проекта (устарела)

```md
MHbinder/
│   Program.cs              # Точка входа
│   Renderer.cs              # Логика ImGui overlay
│   HotkeyListener.cs       # Ловим RightShift и открываем меню
│   ConsoleSplash.cs        # ASCII art + инфо в консоли
│
├── Core/
│   BinderManager.cs        # Логика биндов
│   ConfigManager.cs        # Чтение/запись .ini конфигов
│   GrammarClient.cs        # Клиент для общения с сервером (или локальным API)
│   Validator.cs            # Форматирование/валидация текста (для стакеров)
│
└── MHBConfigs/             # Папка с локальными .ini конфигами
```
