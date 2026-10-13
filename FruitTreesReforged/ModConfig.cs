namespace FruitTreesReforged
{
    public sealed class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        
        public bool MC_bSaplingIncubator { get; set; } = true;               //Adds the Sapling Incubator BC to the game so you can reproduce fruit trees
      
        public bool MC_bFruitTreeFertilizer { get; set; } = true;            //Add the Fruit Tree Fertilizer to the game so you can speed up their growth
        
        public bool MC_bRebalanceVanilla { get; set; } = true;              //Activates the rebalance of vanilla fruit trees and their fruits (number produced, gold value, stats)
        
        public bool MC_bRebalanceCornucopia { get; set; } = true;            //Activates the rebalance of Cornucopia fruit trees and their fruits (number produced, gold value, stats)
        
        public bool MC_bRebalanceGrowableForage { get; set; } = true;        //Activates the rebalance of Growable Forage fruit trees and their fruits (number produced)

        public bool MC_bEvergreenEucalyptus { get; set; } = false;           //If set, Cornucopia's eucalyptus tree will produce leaves and bark all year round
        
        public bool MC_bHardwoodWalnutTree { get; set; } = false;           //If set, Cornucopia's walnut tree will produce fewer walnuts but will give hardwood in spring and summer
        
        public bool MC_bRebalanceSVE { get; set; } = true;                  //Activates the rebalance of Stardew Valley Expanded fruit trees and their fruits (number produced, gold value, stats)
        
        public bool MC_bRebalanceRidgeside { get; set; } = true;            //Activates the rebalance of Ridgeside fruit trees and their fruits (number produced, gold value, stats)
       
        public bool MC_bRebalanceVMV { get; set; } = true;                  //Activates the rebalance of Visit Mount Vapius fruit trees and their fruits (number produced, gold value, stats)
        
        public bool MC_bRebalanceNAG { get; set; } = true;                  //Activates the rebalance of Nando's Artisan Goods fruit trees and their fruits (number produced, gold value, stats)

        public bool MC_bRebalanceWAG { get; set; } = true;                  //Activates the rebalance of Wildflour's Artisan Goods fruit trees and their fruits (gold value mainly)

        public bool MC_bReplaceWAGSaplingMaking { get; set; } = true;       //Activates the seedmaker and the incubator for WAG, and so replaces the built-in WAG's sapling making method
        
        public bool MC_bRebalanceSunberry { get; set; } = true;                  //Activates the rebalance of Sunberry Village fruit trees and their fruits (number produced, gold value, stats)
        
        public bool MC_bRebalanceRNA { get; set; } = true;                  //Activates the rebalance of Rose and the Alchemist fruit trees and their fruits (number produced, gold value, stats)
        
        public bool MC_bRebalanceNME { get; set; } = true;                  //Activates the rebalance of Night Market Expansion fruit trees and their fruits (number produced, gold value, stats)
        
        public bool MC_bRebalanceKalash { get; set; } = true;               //Activates the rebalance of Kalash's More Fruit Trees fruit trees and their fruits (number produced, gold value, stats)
        
        public bool MC_bKeepKalashSaplingMaker { get; set; } = false;        //Keep the original propagation method for trees from Kalash More Fruit Trees. Does not affect other trees.
        
        public bool MC_bKeepKalashCoconutCrafting { get; set; } = false;     //Replace the incubation of coconuts by the crafting recipe from Kalash More Fruit Trees.

        public bool MC_bEasyIncubatorRecipe { get; set; } = false;          //Makes it easier to craft incubators
        
        public bool MC_bEasyFertilizerRecipe { get; set; } = false;          //Makes it easier to craft fruit tree fertilizer
        
        public bool MC_bEasyQualitySaplings { get; set; } = false;          //Makes quality seeds always give quality saplings
        
        public bool MC_bEasyRNASeeds { get; set; } = false;                 //Makes Rose & The Alchemist's fruits (like Meteor Sour Cherry) always give the corresponding seeds and never their vanilla counterparts (like a regular cherry pit).

        public bool MC_bTransgenderPomegranateTree { get; set; } = false;   //Produces Pomegranates with a tint to emulate the transgender flag
    }
}
