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
            Log($"[SCENE] Подготовка к загрузке новой сцены: {newSceneName}");

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
            if (Bosses.Contains(self.gameObject.name))
            {
                Log($"[ShowHPOnDeath] Обнаружен босс: {self.gameObject.name}");
                _currentBoss = self;
                LastBossName = self.gameObject.name;
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

        public static List<string> Bosses = new List<string>()
        {
            // Hall Of Gods
            "Giant Fly", // Gruz Mother
            "Giant Buzzer Col", "Giant Buzzer Col (1)", // Vengerfly King
            "Mawlek Body", // Brooding Mawlek
            "False Knight New", // False Knight
            "False Knight Dream", // Failed Champion
            "Hornet Boss 1", // Hornet Protector (1)
            "Hornet Boss 2", // Hornet Sentinel (2)
            "Mega Moss Charger", // Massive Moss Charger
            "Fluke Mother", // Flukemarm
            "Mantis Lord", // Mantis Lord Phase 1
            "Mantis Lord S1", "Mantis Lord S2", // Mantis Lord Phase 2
            "Mantis Lord S3", // Sisters Of Battle
            "Mega Fat Bee", "Mega Fat Bee (1)", // Oblobbles
            "Hive Knight", // Hive Knight
            "Infected Knight", // Broken Vessel
            "Lost Kin", // Lost Kin
            "Mimic Spider", // Nosk
            "Hornet Nosk", // Winged Nosk
            "Jar Collector", // The Collector
            "Lancer", "Lobster", // God Tamer
            "Mega Zombie Beam Miner (1)", // Crystal Guardian
            "Zombie Beam Miner Rematch", // Enraged Guardian
            "Mega Jellyfish GG", // Uumu
            "Mantis Traitor Lord", // Traitor Lord
            "Grey Prince", // GPZ
            "Mage Knight", // Soul Warrior
            "Mage Lord", "Mage Lord Phase2", // Soul Master
            "Dream Mage Lord", "Dream Mage Lord Phase2", // Soul Tyrant
            "Dung Defender", // Dung Defender
            "White Defender", // White Defender
            "Black Knight 1", "Black Knight 2", "Black Knight 3", "Black Knight 4", "Black Knight 5", "Black Knight 6", // Watchers Knight
            "Ghost Warrior No Eyes", // No Eyes
            "Ghost Warrior Marmu", // Marmu
            "Ghost Warrior Xero", // Xero
            "Ghost Warrior Markoth", // Markoth
            "Ghost Warrior Galien", // Galien
            "Ghost Warrior Slug", // Gorb
            "Ghost Warrior Hu", // Elder Hu
            "Mato", "Oro", // Oro & Mato
            "Sheo Boss", // Paintmaster Sheo
            "Sly Boss", // Nailsage Sly
            "HK Prime", // Pure Vessel
            "Grimm Boss", // Grimm
            "Nightmare Grimm Boss", // Nightmare King
            "Absolute Radiance", // Radiance
        };
    }
}