using GenericModConfigMenu;
using StardewModdingAPI;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Machines;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace FruitTreesReforged
{
    /// <summary>The mod entry point.</summary>
    public sealed class ModEntry : Mod
    {
        public static ModEntry? Context;
        public ModConfig? Config;
        
        /*********
         ** Public methods
         *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            //Generic modding stuff
            
            Context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;
            
            //__________________________________
            //Harmony patches
            
            FruitTreesReforgedPatches.Initialize(this, Monitor);
            var harmony = new Harmony(ModManifest.UniqueID);

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.DayStarted += DayStartedActions;
            
            harmony.Patch(
                original: AccessTools.Method(typeof(Object), "placementAction"),
                prefix: new HarmonyMethod(typeof(FruitTreesReforgedPatches), nameof(FruitTreesReforgedPatches.placementAction_Prefix))
            ); //prevents the player to set incubators outdoors + handles fruit tree fertilizer use
            
            harmony.Patch(
                original: AccessTools.Method(typeof(Object), "canBePlacedHere"),
                prefix: new HarmonyMethod(typeof(FruitTreesReforgedPatches), nameof(FruitTreesReforgedPatches.canBePlacedHere_Prefix))
            ); //specifies where fruit tree fertilizer can be used (spoiler: on fruit trees)
            
            harmony.Patch(
                original: AccessTools.Method(typeof(FruitTree), "performUseAction"),
                prefix: new HarmonyMethod(typeof(FruitTreesReforgedPatches), nameof(FruitTreesReforgedPatches.performUseAction_Prefix))
            ); //prevents tree shaking when placing fruit tree fertilizer
            
            harmony.Patch(
                original: AccessTools.Method(typeof(FruitTree), "dayUpdate"),
                prefix: new HarmonyMethod(typeof(FruitTreesReforgedPatches), nameof(FruitTreesReforgedPatches.dayUpdate_Prefix))
            ); //
            
            harmony.Patch(
                original: AccessTools.Method(typeof(FruitTree), "draw"),
                postfix: new HarmonyMethod(typeof(FruitTreesReforgedPatches), nameof(FruitTreesReforgedPatches.draw_Postfix))
            ); //handles fruit tree fertilizer display

        }
        
        /*
         * This mod's configuration options
         */
        public void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.Config)
            );

            // add some config options
            
            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("FruitTreesReforged.Config.FeaturesTitle")
            );
            
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("FruitTreesReforged.Config.IncubatorDisplayName"),
                tooltip: () => Helper.Translation.Get("FruitTreesReforged.Config.IncubatorDescription"),
                getValue: () => this.Config.MC_bSaplingIncubator,
                setValue: value => this.Config.MC_bSaplingIncubator = value
            );
            
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("FruitTreesReforged.Config.FertilizerDisplayName"),
                tooltip: () => Helper.Translation.Get("FruitTreesReforged.Config.FertilizerDescription"),
                getValue: () => this.Config.MC_bFruitTreeFertilizer,
                setValue: value => this.Config.MC_bFruitTreeFertilizer = value
            );
            
            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("FruitTreesReforged.Config.RebalanceTitle")
            );
            
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("FruitTreesReforged.Config.RebalanceVanillaDisplayName"),
                tooltip: () => Helper.Translation.Get("FruitTreesReforged.Config.RebalanceVanillaDescription"),
                getValue: () => this.Config.MC_bRebalanceVanilla,
                setValue: value => this.Config.MC_bRebalanceVanilla = value
            );
            
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("FruitTreesReforged.Config.TransgenderPomegranateTreeDisplayName"),
                tooltip: () => Helper.Translation.Get("FruitTreesReforged.Config.TransgenderPomegranateTreeDescription"),
                getValue: () => this.Config.MC_bTransgenderPomegranateTree,
                setValue: value => this.Config.MC_bTransgenderPomegranateTree = value
            );
            
            if (this.Helper.ModRegistry.IsLoaded("FlashShifter.SVECode"))
            {
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: () => Helper.Translation.Get("FruitTreesReforged.Config.RebalanceSVEDisplayName"),
                    tooltip: () => Helper.Translation.Get("FruitTreesReforged.Config.RebalanceSVEDescription"),
                    getValue: () => this.Config.MC_bRebalanceSVE,
                    setValue: value => this.Config.MC_bRebalanceSVE = value
                );
            }
            
            if (this.Helper.ModRegistry.IsLoaded("Rafseazz.RidgesideVillage"))
            {
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: () => Helper.Translation.Get("FruitTreesReforged.Config.RebalanceRidgesideDisplayName"),
                    tooltip: () =>Helper.Translation.Get("FruitTreesReforged.Config.RebalanceRidgesideDescription") ,
                    getValue: () => this.Config.MC_bRebalanceRidgeside,
                    setValue: value => this.Config.MC_bRebalanceRidgeside = value
                );
            }
            
            if (this.Helper.ModRegistry.IsLoaded("lumisteria.visitmountvapius.code"))
            {
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: () => Helper.Translation.Get("FruitTreesReforged.Config.RebalanceVMVDisplayName"),
                    tooltip: () =>Helper.Translation.Get("FruitTreesReforged.Config.RebalanceVMVDescription") ,
                    getValue: () => this.Config.MC_bRebalanceVMV,
                    setValue: value => this.Config.MC_bRebalanceVMV = value
                );
            }
            
            
            if (this.Helper.ModRegistry.IsLoaded("hayato2236.crops_stuff"))
            {
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: () => Helper.Translation.Get("FruitTreesReforged.Config.RebalanceNAGDisplayName"),
                    tooltip: () =>Helper.Translation.Get("FruitTreesReforged.Config.RebalanceNAGDescription") ,
                    getValue: () => this.Config.MC_bRebalanceNAG,
                    setValue: value => this.Config.MC_bRebalanceNAG = value
                );
            }
            
            if (this.Helper.ModRegistry.IsLoaded("Wildflour.AtelierGoods"))
            {
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: () => Helper.Translation.Get("FruitTreesReforged.Config.RebalanceWAGDisplayName"),
                    tooltip: () =>Helper.Translation.Get("FruitTreesReforged.Config.RebalanceWAGDescription") ,
                    getValue: () => this.Config.MC_bRebalanceWAG,
                    setValue: value => this.Config.MC_bRebalanceWAG = value
                );
                
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: () => Helper.Translation.Get("FruitTreesReforged.Config.ReplaceWAGSaplingMakingDisplayName"),
                    tooltip: () =>Helper.Translation.Get("FruitTreesReforged.Config.ReplaceWAGSaplingMakingDescription") ,
                    getValue: () => this.Config.MC_bReplaceWAGSaplingMaking,
                    setValue: value => this.Config.MC_bReplaceWAGSaplingMaking = value
                );
            }
            
            if (this.Helper.ModRegistry.IsLoaded("Cornucopia.MoreCrops") || this.Helper.ModRegistry.IsLoaded("Cornucopia.MoreFlowers"))
            {
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: () => Helper.Translation.Get("FruitTreesReforged.Config.RebalanceCornucopiaDisplayName"),
                    tooltip: () => Helper.Translation.Get("FruitTreesReforged.Config.RebalanceCornucopiaDescription"),
                    getValue: () => this.Config.MC_bRebalanceCornucopia,
                    setValue: value => this.Config.MC_bRebalanceCornucopia = value
                );
            }
            
            if (this.Helper.ModRegistry.IsLoaded("Cornucopia.GrowableForage"))
            {
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: () => Helper.Translation.Get("FruitTreesReforged.Config.RebalanceGrowableForageDisplayName"),
                    tooltip: () =>Helper.Translation.Get("FruitTreesReforged.Config.RebalanceGrowableForageDescription") ,
                    getValue: () => this.Config.MC_bRebalanceGrowableForage,
                    setValue: value => this.Config.MC_bRebalanceGrowableForage = value
                );
            }
            
            if (this.Helper.ModRegistry.IsLoaded("Cornucopia.MoreCrops"))
            {
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: () => Helper.Translation.Get("FruitTreesReforged.Config.CornucopiaAltEucalyptusDisplayName"),
                    tooltip: () =>Helper.Translation.Get("FruitTreesReforged.Config.CornucopiaAltEucalyptusDescription"),
                    getValue: () => this.Config.MC_bEvergreenEucalyptus,
                    setValue: value => this.Config.MC_bEvergreenEucalyptus = value
                );

                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: () => Helper.Translation.Get("FruitTreesReforged.Config.CornucopiaAltWalnutTreeDisplayName"),
                    tooltip: () =>Helper.Translation.Get("FruitTreesReforged.Config.CornucopiaAltWalnutTreeDescription"),
                    getValue: () => this.Config.MC_bHardwoodWalnutTree,
                    setValue: value => this.Config.MC_bHardwoodWalnutTree = value
                );
            }
            
            //Sunberry village
            if (this.Helper.ModRegistry.IsLoaded("SunberryTeam.SBVSMAPI"))
            {
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: () => Helper.Translation.Get("FruitTreesReforged.Config.RebalanceSunberryDisplayName"),
                    tooltip: () =>Helper.Translation.Get("FruitTreesReforged.Config.RebalanceSunberryDescription") ,
                    getValue: () => this.Config.MC_bRebalanceSunberry,
                    setValue: value => this.Config.MC_bRebalanceSunberry = value
                );
                if (this.Helper.ModRegistry.IsLoaded("Wildflour.SASS"))
                { //Rose n the Alchemist
                    configMenu.AddBoolOption(
                        mod: this.ModManifest,
                        name: () => Helper.Translation.Get("FruitTreesReforged.Config.RebalanceRNADisplayName"),
                        tooltip: () =>Helper.Translation.Get("FruitTreesReforged.Config.RebalanceRNADescription") ,
                        getValue: () => this.Config.MC_bRebalanceRNA,
                        setValue: value => this.Config.MC_bRebalanceRNA = value
                    );
                
                }
            }
            
            if (this.Helper.ModRegistry.IsLoaded("chocolatte12.NightMarketExpansion"))
            {
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: () => Helper.Translation.Get("FruitTreesReforged.Config.RebalanceNMEDisplayName"),
                    tooltip: () =>Helper.Translation.Get("FruitTreesReforged.Config.RebalanceNMEDescription") ,
                    getValue: () => this.Config.MC_bRebalanceNME,
                    setValue: value => this.Config.MC_bRebalanceNME = value
                );
            }
            
            if (this.Helper.ModRegistry.IsLoaded("Kalash.FruitTrees"))
            {
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: () => Helper.Translation.Get("FruitTreesReforged.Config.RebalanceKalashDisplayName"),
                    tooltip: () =>Helper.Translation.Get("FruitTreesReforged.Config.RebalanceKalashDescription") ,
                    getValue: () => this.Config.MC_bRebalanceKalash,
                    setValue: value => this.Config.MC_bRebalanceKalash = value
                );
                
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: () => Helper.Translation.Get("FruitTreesReforged.Config.KeepKalashSaplingMakerDisplayName"),
                    tooltip: () =>Helper.Translation.Get("FruitTreesReforged.Config.KeepKalashSaplingMakerDescription") ,
                    getValue: () => this.Config.MC_bKeepKalashSaplingMaker,
                    setValue: value => this.Config.MC_bKeepKalashSaplingMaker = value
                );
                
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: () => Helper.Translation.Get("FruitTreesReforged.Config.KeepKalashCoconutCraftingDisplayName"),
                    tooltip: () =>Helper.Translation.Get("FruitTreesReforged.Config.KeepKalashCoconutCraftingDescription") ,
                    getValue: () => this.Config.MC_bKeepKalashCoconutCrafting,
                    setValue: value => this.Config.MC_bKeepKalashCoconutCrafting = value
                );
            }
            
            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("FruitTreesReforged.Config.DifficultyTitle"),
                tooltip: () => Helper.Translation.Get("FruitTreesReforged.Config.DifficultyTooltip")
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("FruitTreesReforged.Config.IncubatorDifficultyDisplayName"),
                tooltip: () => Helper.Translation.Get("FruitTreesReforged.Config.IncubatorDifficultyDescription"),
                getValue: () => this.Config.MC_bEasyIncubatorRecipe,
                setValue: value => this.Config.MC_bEasyIncubatorRecipe = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("FruitTreesReforged.Config.FertilizerDifficultyDisplayName"),
                tooltip: () => Helper.Translation.Get("FruitTreesReforged.Config.FertilizerDifficultyDescription"),
                getValue: () => this.Config.MC_bEasyFertilizerRecipe,
                setValue: value => this.Config.MC_bEasyFertilizerRecipe = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("FruitTreesReforged.Config.QualitySaplingsDifficultyDisplayName"),
                tooltip: () => Helper.Translation.Get("FruitTreesReforged.Config.QualitySaplingsDifficultyDescription"),
                getValue: () => this.Config.MC_bEasyQualitySaplings,
                setValue: value => this.Config.MC_bEasyQualitySaplings = value
            );
            
            if (this.Helper.ModRegistry.IsLoaded("Wildflour.SASS"))
            {
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: () => Helper.Translation.Get("FruitTreesReforged.Config.RNASeedsDifficultyDisplayName"),
                    tooltip: () => Helper.Translation.Get("FruitTreesReforged.Config.RNASeedsDifficultyDescription"),
                    getValue: () => this.Config.MC_bEasyRNASeeds,
                    setValue: value => this.Config.MC_bEasyRNASeeds = value
                );
            }

            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => Helper.Translation.Get("FruitTreesReforged.Config.RestartWarningText")
            );

        }

        /**
        * Writing code here makes it execute when the game is fully loaded.
        */
        public void DayStartedActions(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            //Handles update of the incubator's sprite
            //Items in the location are only parsed if this is a location where incubators can have been set
            //Game1.locations.DoIf(PossiblyIncubatorsHere, UpdateIncubatorSprite);
            Utility.ForEachLocation(UpdateIncubatorsSprites, true, false);
        }

        private bool UpdateIncubatorsSprites(GameLocation location)
        {
            if (location.IsOutdoors || !(location.IsFarm || location.IsGreenhouse || location is Cellar || location is Shed))
            {
                return true;
            }

            Utility.ForEachItemIn(location, item =>
            {
                if (item is { ItemId: "FruitTreesReforged_SaplingIncubator" })
                {

                    var Incubator = item as Object;

                    if (Incubator?.heldObject.Value != null)
                    {
                        // -10 = unused

                        if (Incubator.MinutesUntilReady <= 0)
                        {
                            Incubator.ParentSheetIndex = 7; //Ready to harvest
                        }
                        else
                        {
                            //Incubator working - using a different sprite for each growth stage (Changes every 2 days)
                            double daysUntilReady = Incubator.MinutesUntilReady / 1600;
                            double totalDaysNeeded = 12; //TODO get the DaysUntilReady value from the .json model
                            Incubator.ParentSheetIndex = 6 - (int)Math.Ceiling(daysUntilReady * 6 / totalDaysNeeded);
                        }
                    }
                }

                return true;
            });
            return true;
        }
    }
}