using System;
using System.Collections.Generic;
using System.Linq;
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
        public override string GetVersion() => "1.1.3";

        private static List<(string Name, int InitialHP, int CurrentHP)> BossHPData = new();

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
            if (!GS.EnabledMod) return newSceneName;

            foreach (var boss in UnityEngine.Object.FindObjectsOfType<HealthManager>())
            {
                if (boss != null && ShowHPOnDeath.BossNames.ContainsKey(boss.gameObject.name))
                {
                    string displayName = ShowHPOnDeath.BossNames[boss.gameObject.name];
                    var existing = BossHPData.Find(b => b.Name == displayName);

                    if (existing.Name != null)
                    {
                        // Обновляем только текущее HP
                        int index = BossHPData.IndexOf(existing);
                        BossHPData[index] = (existing.Name, existing.InitialHP, boss.hp);
                    }
                }
            }
            BossHPData.RemoveAll(b =>
            {
                // Ищем оригинальное имя босса в словаре
                string originalName = BossNames.FirstOrDefault(x => x.Value == b.Name).Key;
                if (string.IsNullOrEmpty(originalName)) return true;

                // Проверяем, существует ли такой босс на сцене
                HealthManager hm = UnityEngine.Object.FindObjectOfType<HealthManager>();
                return b.CurrentHP <= 0 || hm == null || hm.gameObject.name != originalName;
            });

            // ==== Формируем текст для отображения ====
            string displayText = "";
            foreach (var (name, initialHP, currentHP) in BossHPData)
            {
                float percent = initialHP > 0 ? ((float)currentHP / initialHP) * 100 : 0f;

                displayText += $"[{name}]\nHP: {currentHP} / {initialHP}\n";
            }

            UpdateDisplay(displayText.Trim());

            return newSceneName;
        }



        // ==== Ну тут понятно получение хп ====
        private void OnHealthManagerEnable(On.HealthManager.orig_OnEnable orig, HealthManager self)
        {
            if (!GS.EnabledMod) return;

            // Проверяем, есть ли такой босс в твоём словаре
            if (ShowHPOnDeath.BossNames.TryGetValue(self.gameObject.name, out string displayName))
            {
                // Сохраняем начальное HP, если такого босса ещё нет в списке
                if (!BossHPData.Any(b => b.Name == displayName))
                {
                    BossHPData.Add((displayName, self.hp, self.hp));
                    Log($"[ShowHPOnDeath] Найден босс: {displayName}, HP: {self.hp}");
                }
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
            { "Lobster", "Beast" },
            { "Mega Zombie Beam Miner (1)", "Crystal Guardian" },
            { "Zombie Beam Miner Rematch", "Enraged Guardian" },
            { "Mega Jellyfish GG", "Uumu" },
            { "Mantis Traitor Lord", "Traitor Lord" },
            { "Grey Prince", "Grey Prince Zote" },
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
            { "Mato", "Mato" },
            { "Oro", "Oro" },
            { "Sheo Boss", "Paintmaster Sheo" },
            { "Sly Boss", "Nailsage Sly" },
            { "HK Prime", "Pure Vessel" },
            { "Grimm Boss", "Grimm" },
            { "Nightmare Grimm Boss", "Nightmare King" },
            { "Absolute Radiance", "Radiance" },
        };
    }
}