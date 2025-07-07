using System;
using System.Collections.Generic;
using Modding;


namespace ShowHPOnDeath
{
    [Serializable]
    public class GlobalSettings
    {
        public bool EnabledMod = false;
    }

    public class ShowHPOnDeath : Mod, IMenuMod, IGlobalSettings<GlobalSettings>
    {
        // ==== Подготовкаа ====
        public static GlobalSettings GS { get; private set; } = new();
        public void OnLoadGlobal(GlobalSettings s) => GS = s;
        public GlobalSettings OnSaveGlobal() => GS;

        public ShowHPOnDeath() : base("ShowHPOnDeath") { }
        public override string GetVersion() => "1.1.0";

        private static List<(string Name, int HP)> CurrentBosses = new List<(string, int)>();

        public override void Initialize()
        {
            On.HealthManager.OnEnable += OnHealthManagerEnable;
            ModHooks.BeforeSceneLoadHook += BeforeSceneLoad;
            CreateUI();
        }

        // ==== Реализация меню (без satchel btw) ====

        bool IMenuMod.ToggleButtonInsideMenu => true;

        public bool ToggleButtonInsideMenu => true;

        List<IMenuMod.MenuEntry> IMenuMod.GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
        {
            return new List<IMenuMod.MenuEntry>
            {
                new IMenuMod.MenuEntry
                {
                    Name = "Global Switch",
                    Description = "Turn mod On/Off",
                    Values = new string[] {
                        "Off",
                        "On",
                    },
                    Saver = opt => ChangeGlobalSwitchState(opt == 1),
                    Loader = () => GS.EnabledMod ? 1 : 0
                }
            };
        }

        private void ChangeGlobalSwitchState(bool state)
        {
            GS.EnabledMod = state;
        }


        // ==== Логика добавления босса после того как сдох ====
        private string BeforeSceneLoad(string newSceneName)
        {
            if (!GS.EnabledMod)
            {
                CurrentBosses.Clear();
                UpdateDisplay(""); // Очистить дисплей, если мод выключен
                return newSceneName;
            }

            CurrentBosses.Clear();

            foreach (var boss in UnityEngine.Object.FindObjectsOfType<HealthManager>())
            {
                if (boss != null && boss.gameObject.activeInHierarchy && boss.hp > 0)
                {
                    if (BossNames.TryGetValue(boss.gameObject.name, out string displayName))
                    {
                        CurrentBosses.Add((displayName, boss.hp));
                    }
                }
            }

            // ==== Логика дислея, например добавление порядкового номера ====
            string displayText = "";
            for (int i = 0; i < CurrentBosses.Count; i++)
            {
                var (name, hp) = CurrentBosses[i];

                int count = 0;
                for (int j = 0; j < i; j++)
                {
                    if (CurrentBosses[j].Name == name) count++;
                }

                string finalName = count > 0 ? $"{name} ({count + 1})" : name;

                displayText += $"[{finalName}]\nHP: {hp}\n";
            }

            if (!string.IsNullOrEmpty(displayText))
            {
                UpdateDisplay(displayText.Trim());
            }

            return newSceneName;
        }



        // ==== Ну тут понятно получение хп ====
        private void OnHealthManagerEnable(On.HealthManager.orig_OnEnable orig, HealthManager self)
        {
            if (!GS.EnabledMod) return;

            if (BossNames.TryGetValue(self.gameObject.name, out string displayName))
            {
                Log($"[ShowHPOnDeath] Обнаружен босс: {self.gameObject.name} → {displayName}");
                CurrentBosses.Add((displayName, self.hp));
            }

            orig(self);
        }
        // ==== Отображение информации ====
        private void CreateUI()
        {
            if (ModDisplay.Instance == null)
            {
                ModDisplay.Instance = new ModDisplay();
            }
            else
            {
                ModDisplay.Instance.Destroy();
                ModDisplay.Instance = new ModDisplay();
            }
        }

        private void UpdateDisplay(string text)
        {
            if (ModDisplay.Instance != null)
            {
                ModDisplay.Instance.Display(text);
            }
        }

        // ==== Словарь названий боссов в хоге, слева название самого моба, справа как его зовут в игре думаю это очевидно ====
        public static Dictionary<string, string> BossNames = new Dictionary<string, string>()
        {
            // Hall Of Gods
            { "Giant Fly", "Gruz Mother" },
            { "Giant Buzzer Col", "Vengerfly King" },
            { "Giant Buzzer Col (1)", "Vengerfly King" }, // дубликаты тоже можно обрабатывать
            { "Mawlek Body", "Brooding Mawlek" },
            { "False Knight New", "False Knight" },
            { "False Knight Dream", "Failed Champion" },
            { "Hornet Boss 1", "Hornet Protector" },
            { "Hornet Boss 2", "Hornet Sentinel" },
            { "Mega Moss Charger", "Massive Moss Charger" },
            { "Fluke Mother", "Flukemarm" },
            { "Mantis Lord", "Mantis Lord Phase 1" },
            { "Mantis Lord S1", "Mantis Lord Phase 2" },
            { "Mantis Lord S2", "Mantis Lord Phase 2" },
            { "Mantis Lord S3", "Sisters Of Battle" },
            { "Mega Fat Bee", "Oblobbles" },
            { "Mega Fat Bee (1)", "Oblobbles" },
            { "Hive Knight", "Hive Knight" },
            { "Infected Knight", "Broken Vessel" },
            { "Lost Kin", "Lost Kin" },
            { "Mimic Spider", "Nosk" },
            { "Hornet Nosk", "Winged Nosk" },
            { "Jar Collector", "The Collector" },
            { "Lancer", "God Tamer" },
            { "Lobster", "God Tamer" },
            { "Mega Zombie Beam Miner (1)", "Crystal Guardian" },
            { "Zombie Beam Miner Rematch", "Enraged Guardian" },
            { "Mega Jellyfish GG", "Uumu" },
            { "Mantis Traitor Lord", "Traitor Lord" },
            { "Grey Prince", "Grey Prince Zenker" },
            { "Mage Knight", "Soul Warrior" },
            { "Mage Lord", "Soul Master" },
            { "Mage Lord Phase2", "Soul Master" },
            { "Dream Mage Lord", "Soul Tyrant" },
            { "Dream Mage Lord Phase2", "Soul Tyrant" },
            { "Dung Defender", "Dung Defender" },
            { "White Defender", "White Defender" },
            { "Black Knight 1", "Watcher Knight" },
            { "Black Knight 2", "Watcher Knight" },
            { "Black Knight 3", "Watcher Knight" },
            { "Black Knight 4", "Watcher Knight" },
            { "Black Knight 5", "Watcher Knight" },
            { "Black Knight 6", "Watcher Knight" },
            { "Ghost Warrior No Eyes", "No Eyes" },
            { "Ghost Warrior Marmu", "Marmu" },
            { "Ghost Warrior Xero", "Xero" },
            { "Ghost Warrior Markoth", "Markoth" },
            { "Ghost Warrior Galien", "Galien" },
            { "Ghost Warrior Slug", "Gorb" },
            { "Ghost Warrior Hu", "Elder Hu" },
            { "Mato", "Oro & Mato" },
            { "Oro", "Oro & Mato" },
            { "Sheo Boss", "Paintmaster Sheo" },
            { "Sly Boss", "Nailsage Sly" },
            { "HK Prime", "Pure Vessel" },
            { "Grimm Boss", "Grimm" },
            { "Nightmare Grimm Boss", "Nightmare King" },
            { "Absolute Radiance", "Radiance" },
        };
    }
}