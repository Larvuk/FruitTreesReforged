using System.Drawing;
using System.Reflection.PortableExecutable;
using Force.DeepCloner;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewModdingAPI;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.FruitTrees;
using StardewValley.GameData.Machines;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using Color = Microsoft.Xna.Framework.Color;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace FruitTreesReforged
{
    /**
    * Patch class used by Harmony
    */
    [HarmonyPatch]
    public class FruitTreesReforgedPatches
    {
        private static IMonitor Monitor;
        private static ModEntry? context;
        private static int FertilizerCoolDown = 0;

        public static void Initialize(ModEntry modEntry, IMonitor monitor)
        {
            context = modEntry;
            Monitor = monitor;
        }

        public static bool placementAction_Prefix(ref Object __instance, ref bool __result, GameLocation location, Farmer who, int x, int y)
        {
            //Prevents the sapling incubator to be set outside
            
            if (__instance.ItemId == "FruitTreesReforged_SaplingIncubator")
            {
                if (location.IsOutdoors && !location.IsGreenhouse)
                {
                    
                    var deniedMessage = context.Helper.Translation.Get("FruitTreesReforged.Tips.IncubatorIndoor");
                    Game1.showRedMessage(deniedMessage);
                    __result = false;
                    return false;
                }
            }
            
            //Handles the use of fruit tree fertilizer
    
            if (__instance.QualifiedItemId == "(O)FruitTreesReforged_FruitTreeFertilizer")
            {
                Vector2 vector2_1 = new Vector2((float) (x / 64 /*0x40*/), (float) (y / 64 /*0x40*/));
                TerrainFeature terrainFeature1;
                __result = location.terrainFeatures.TryGetValue(vector2_1, out terrainFeature1) && terrainFeature1 is FruitTree tree1 && FertilizeFruitTree(tree1);
                if (__result)
                {
                    //Game1.player.reduceActiveItemByOne();
                    Object.ConsumeInventoryItem(who, __instance, 1);
                }

                return false;
            }

            return true;
        }

        /**
         * prevent the tree shaking when clicking it with fertilizer in hands
         */
        [HarmonyBefore("larvuk.AdvancedFruitTreeFramework")]
        public static bool performUseAction_Prefix(ref FruitTree __instance, ref bool __result, Vector2 tileLocation)
        {
            var player = Game1.player;
            var activeObject = player.ActiveObject;
            
            if (activeObject is not { QualifiedItemId: "(O)FruitTreesReforged_FruitTreeFertilizer" }) return true;
            
            activeObject.placementAction(__instance.Location, (int)(tileLocation.X * 64.0),  (int)(tileLocation.Y * 64.0), player);
            
            return false;

        }

        /**
         * Make the new fertilizer item only usable on fruit trees
         */
        public static bool canBePlacedHere_Prefix(ref Object __instance, ref bool __result, GameLocation l, Vector2 tile)
        {
            if (__instance.QualifiedItemId != "(O)FruitTreesReforged_FruitTreeFertilizer") return true;
            
            if (l.terrainFeatures.GetValueOrDefault(tile) is FruitTree)
            {
                __result = true;
                return false;
            }
            __result = false;
            return false;
        }

        /**
         * Actually applying the fertilizer
         */
        public static bool FertilizeFruitTree(FruitTree fruitTree)
        {
            if (FertilizerCoolDown > 0)
            {
                FertilizerCoolDown--;
            }
            else
            {
                if (fruitTree.growthStage.Value == 4)
                {
                    Game1.showRedMessage(context.Helper.Translation.Get("FruitTreesReforged.Tips.FertilizerMature"));
                    FertilizerCoolDown = 3;
                    return false;
                }

                if (fruitTree.modData.TryGetValue($"{ModEntry.Context?.ModManifest.UniqueID}/fertilized", out var fertilized))
                {
                    Game1.showRedMessage(context.Helper.Translation.Get("FruitTreesReforged.Tips.FertilizerAlready"));
                    FertilizerCoolDown = 3;
                    return false;
                }

                fruitTree.modData[$"{ModEntry.Context?.ModManifest.UniqueID}/fertilized"] = "true";
                //fruitTree.growthRate.Value = 2;
                fruitTree.Location.playSound("dirtyHit");
                FertilizerCoolDown = 3;
                
                return true;
            }

            return false;
        }

        /**
         * Applies growth boost on fertilized trees
         */
        public static bool dayUpdate_Prefix(ref FruitTree __instance)
        {
            //If the tree's growth is not blocked
            if (FruitTree.IsGrowthBlocked(__instance.Tile, __instance.Location)) return true;

            //If the tree is fertilized
            if (!__instance.modData.TryGetValue($"{ModEntry.Context?.ModManifest.UniqueID}/fertilized",
                    out var fertilized)) return true;

            //If the tree already has a days_left property
            if (!__instance.modData.TryGetValue($"larvuk.AdvancedFruitTreeFramework/days_left",
                    out var savedDaysToMature)) return true;
            
            //Then reduce its days_left property
            var daysLeft = int.Parse(savedDaysToMature) - 1;
            __instance.modData[$"larvuk.AdvancedFruitTreeFramework/days_left"] = $"{daysLeft}";
            
            //Then run the dayUpdate (that will reduce days_left further)
            return true;
        }

        public static void draw_Postfix(ref FruitTree __instance, SpriteBatch spriteBatch)
        {
            Vector2 tile = __instance.Tile;
            Rectangle boundingBox = __instance.getBoundingBox();

            //Drawing fertilizer on the soil (for fertilized saplings)
            if (__instance.growthStage.Value < 4 && __instance.modData.TryGetValue($"{ModEntry.Context?.ModManifest.UniqueID}/fertilized", out var fertilized))
            {
                spriteBatch.Draw(
                    Game1.content.Load<Texture2D>(Path.Combine("FruitTreesReforged", "FertilizedSoil")),
                    Game1.GlobalToLocal(
                        Game1.viewport,
                        new Vector2(tile.X * 64f, tile.Y * 64f)
                    ),
                    null,
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    4f,
                    SpriteEffects.None,
                    (float)((double)boundingBox.Bottom / 10000.0 - (double)tile.X / 1000000.0 - 1)
                );
            }
        }
        
        /**
         * Generic output method for tree fruits in the seedmaker
         */
        public static Item GetSeedmakerOutput(Object machine, Item inputItem, bool probe, MachineItemOutput outputData,
            Farmer player, out int? overrideMinutesUntilReady)
        {
            overrideMinutesUntilReady = null;
            
            foreach (var contextTag in inputItem.GetContextTags())
            {
                if (!contextTag.StartsWith("SeedId_")) continue;
                
                var seedId = contextTag.Remove(0, "SeedId_".Length);
                return ItemRegistry.Create(seedId, 1, 0, true) ?? inputItem;
            }
            
            return inputItem;
        }

        /**
         * Method called from CP to change the quality of saplings based on the input
         * Iridium-quality potting soil version
         */
        public static Item GetIncubatorIridiumOutput(Object machine, Item inputItem, bool probe, MachineItemOutput outputData,
            Farmer player, out int? overrideMinutesUntilReady)
        {
            overrideMinutesUntilReady = null;
            return GetIncubatorOutputForQuality(inputItem, outputData, 4);
        }

        /**
         * Method called from CP to change the quality of saplings based on the input
         * gold-quality potting soil version
         */
        public static Item GetIncubatorGoldOutput(Object machine, Item inputItem, bool probe, MachineItemOutput outputData,
            Farmer player, out int? overrideMinutesUntilReady)
        {
            overrideMinutesUntilReady = null;
            return GetIncubatorOutputForQuality(inputItem, outputData, 2);
        }

        /**
         * Method called from CP to change the quality of saplings based on the input
         * silver-quality potting soil version
         */
        public static Item GetIncubatorSilverOutput(Object machine, Item inputItem, bool probe, MachineItemOutput outputData,
            Farmer player, out int? overrideMinutesUntilReady)
        {
            overrideMinutesUntilReady = null;
            return GetIncubatorOutputForQuality(inputItem, outputData, 1);
        }

        /**
         * Method called from CP to change the quality of saplings based on the input
         * normal-quality potting soil version
         */
        public static Item GetIncubatorNormalOutput(Object machine, Item inputItem, bool probe, MachineItemOutput outputData,
            Farmer player, out int? overrideMinutesUntilReady)
        {
            overrideMinutesUntilReady = null;
            return GetIncubatorOutputForQuality(inputItem, outputData, 0);
        }


        /**
         * Creates a random-quality sapling from a tree seed based on the potting soil quality
         */
        private static Item GetIncubatorOutputForQuality(Item inputItem, MachineItemOutput outputData, int pottingSoilQuality)
        {
            //Identify the sapling to create
            var treeId = "";
            if (outputData.ItemId != null)
            {
                //Sapling specified in the output rule
                treeId = outputData.ItemId;
            }
            else
            {
                //Sapling specified in the seed's context tags
                foreach (var contextTag in inputItem.GetContextTags())
                {
                    if (!contextTag.StartsWith("TreeId_")) continue;
                    if (treeId != "") continue;
                    treeId = contextTag.Remove(0, "TreeId_".Length);
                }
            }
            if(treeId == "") return inputItem; //No sapling found
            
            //Choose sapling quality
            int saplingQuality;
            if (ModEntry.Context.Config.MC_bEasyQualitySaplings)
            {
                //Easy mode: sapling quality is set to the seed quality
                saplingQuality = pottingSoilQuality;
            }
            else
            {
                //Normal mode: sapling quality is set to UP TO the seed quality
                if (pottingSoilQuality == 4) pottingSoilQuality = 3;
                saplingQuality = new Random().Next(0, pottingSoilQuality + 1);
                if (saplingQuality == 3) saplingQuality = 4;
            }
            
            //Creating the sapling
            var item = ItemRegistry.Create(treeId, 1, saplingQuality);
            return item ?? inputItem;
        }
    }
}