using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;


namespace MixedGearVisualFix
{
    public class SubModule : MBSubModuleBase
    {
        private Harmony? _harmony;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            try
            {
                bool lordsGearLoaded = IsModLoaded("LordsGear");
                bool dressTheWandererLoaded = IsModLoaded("DressTheWanderer");
                bool universalAutoEquipmentLoaded = IsModLoaded("UniversalAutoEquipment");
                bool balancedTournamentArmorLoaded = IsModLoaded("BalancedTournamentArmor");
                bool banditMilitiasReduxLoaded = IsModLoaded("BanditMilitiasRedux");

                if (lordsGearLoaded || dressTheWandererLoaded || universalAutoEquipmentLoaded || balancedTournamentArmorLoaded || banditMilitiasReduxLoaded)
                {
                    _harmony = new Harmony("MixedGearVisualFix");
                    _harmony.PatchAll(Assembly.GetExecutingAssembly());

                    string modsDetected = string.Empty;
                    if (lordsGearLoaded) modsDetected += "Lord's Gear";
                    if (dressTheWandererLoaded)
                    {
                        if (!string.IsNullOrEmpty(modsDetected)) modsDetected += " & ";
                        modsDetected += "Dress The Wanderer";
                    }
                    if (universalAutoEquipmentLoaded)
                    {
                        if (!string.IsNullOrEmpty(modsDetected)) modsDetected += " & ";
                        modsDetected += "Universal Auto Equipment";
                    }
                    if (balancedTournamentArmorLoaded)
                    {
                        if (!string.IsNullOrEmpty(modsDetected)) modsDetected += " & ";
                        modsDetected += "Balanced Tournament Armor";
                    }
                    if (banditMilitiasReduxLoaded)
                    {
                        if (!string.IsNullOrEmpty(modsDetected)) modsDetected += " & ";
                        modsDetected += "Bandit Militias Redux";
                    }

                    InformationManager.DisplayMessage(new InformationMessage(
                        $"Invisible Items Fix: {modsDetected} detected, boot restrictions active.",
                        Color.FromUint(0x00FF00FF)));
                }
                else
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        "Invisible Items Fix: No compatible mods detected (Lord's Gear, Dress The Wanderer, Universal Auto Equipment, Balanced Tournament Armor, or Bandit Militias Redux required).",
                        Color.FromUint(0xFFFF00FF)));
                }
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    $"Invisible Items Fix: Error loading - {ex.Message}",
                    Color.FromUint(0xFF0000FF)));
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (gameStarterObject is CampaignGameStarter campaignStarter)
            {
                campaignStarter.AddBehavior(new MixedGearVisualFix.WandererEquipmentCleanupBehavior());
            }
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
            _harmony?.UnpatchAll("MixedGearVisualFix");
        }

        private static bool IsModLoaded(string modName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == modName)
                    return true;
            }
            return false;
        }
    }

    public class WandererEquipmentCleanupBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.HeroCreated.AddNonSerializedListener(this, OnHeroCreated);
        }

        public override void SyncData(IDataStore dataStore) { }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            foreach (var hero in Hero.AllAliveHeroes.Where(h => h != null && h.IsWanderer))
                ExcludedItemSweeper.SweepCharacter(hero.CharacterObject);
        }

        private void OnHeroCreated(Hero hero, bool showNotification)
        {
            if (hero != null && hero.IsWanderer)
                ExcludedItemSweeper.SweepCharacter(hero.CharacterObject);
        }
    }
}