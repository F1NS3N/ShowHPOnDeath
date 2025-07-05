using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using Modding;
using UnityEngine;

namespace ShowHPOnDeath
{
    [Serializable]
    public class GlobalSettings
    {
        public int Remnant = 0;
    }

    public class ShowHPOnDeath : Mod, IGlobalSettings<GlobalSettings>
    {
        public string LastBossName = "";
        public static GlobalSettings GS { get; private set; } = new();
        public void OnLoadGlobal(GlobalSettings s) => GS = s;
        public GlobalSettings OnSaveGlobal() => GS;

        public ShowHPOnDeath() : base("ShowHPOnDeath") { }
        public override string GetVersion() => "1.0.0";

        private static HealthManager _currentBoss = null;

        public override void Initialize()
        {
            On.HealthManager.OnEnable += OnHealthManagerEnable;
            ModHooks.BeforeSceneLoadHook += BeforeSceneLoad;
            CreateUI(); // Создаем UI для отображения
        }

        private string BeforeSceneLoad(string newSceneName)
        {

            UpdateBossRemnantHP();

            // Показываем HP босса, если он был
            if (GS.Remnant > 0 && !string.IsNullOrEmpty(LastBossName))
            {
                UpdateDisplay($"[{LastBossName}]\nHP: {GS.Remnant}");
            }

            return newSceneName;
        }

        private void UpdateBossRemnantHP()
        {
            if (_currentBoss != null && _currentBoss.hp > 0)
            {
                GS.Remnant = _currentBoss.hp;
                Log($"[BOSS REMNANT] HP босса сохранён: {GS.Remnant}");
            }
            else
            {
                GS.Remnant = 0;
                Log($"[BOSS REMNANT] Нет активного босса или он мёртв");
            }
        }

        private void OnHealthManagerEnable(On.HealthManager.orig_OnEnable orig, HealthManager self)
        {
            if (BossNames.TryGetValue(self.gameObject.name, out string displayName))
            {
                Log($"[ShowHPOnDeath] Обнаружен босс: {self.gameObject.name} ({displayName})");
                _currentBoss = self;
                LastBossName = displayName;
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