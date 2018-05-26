using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        private const string RESOURCE_MONITOR_DISPLAY_NAME = "SE_Display";

        // This has been designed to run occasionally, it is not efficient and should not run on a tight loop

        Dictionary<string, double[]> resources;
        Dictionary<string, string[]> resourceDisplayStrings;

        /**
         * Initializes the resource 'buckets' needed for tracking how much there are of things
         **/
        public Program()
        {
            resources = new Dictionary<string, double[]>();
            resources.Add("Stone", new double[2] { 0, 0 });
            resources.Add("Iron", new double[2] { 0, 0 });
            resources.Add("Nickel", new double[2] { 0, 0 });
            resources.Add("Cobalt", new double[2] { 0, 0 });
            resources.Add("Magnesium", new double[2] { 0, 0 });
            resources.Add("Silicon", new double[2] { 0, 0 });
            resources.Add("Silver", new double[2] { 0, 0 });
            resources.Add("Gold", new double[2] { 0, 0 });
            resources.Add("Platinum", new double[2] { 0, 0 });
            resources.Add("Uranium", new double[2] { 0, 0 });

            resourceDisplayStrings = new Dictionary<string, string[]>();
            resourceDisplayStrings.Add("Stone", new string[] { "Stone", "                          " });
            resourceDisplayStrings.Add("Iron", new string[] { "Iron                (Fe)", "       " });
            resourceDisplayStrings.Add("Nickel", new string[] { "Nickel            (Ni)", "        " });
            resourceDisplayStrings.Add("Cobalt", new string[] { "Cobalt            (Co)", "       " });
            resourceDisplayStrings.Add("Magnesium", new string[] { "Magnesium   (Mg)", "       " });
            resourceDisplayStrings.Add("Silicon", new string[] { "Silicon           (Si)", "        " });
            resourceDisplayStrings.Add("Silver", new string[] { "Silver             (Ag)", "       " });
            resourceDisplayStrings.Add("Gold", new string[] { "Gold               (Au)", "       " });
            resourceDisplayStrings.Add("Platinum", new string[] { "Platinum        (Pt)", "        " });
            resourceDisplayStrings.Add("Uranium", new string[] { "Uranium         (U)", "         " });

        }

        /**
         * Run when the programmable block is 'run'. Finds all the values for resources and displays them
         **/
        public void Main(string argument, UpdateType updateSource)
        {
            // Clear values from the last run so we're not accumulating
            foreach (KeyValuePair<string, double[]> resource in resources)
            {
                resource.Value[0] = 0;
                resource.Value[1] = 0;
            }

            IMyTextPanel textPanel = GridTerminalSystem.GetBlockWithName(RESOURCE_MONITOR_DISPLAY_NAME) as IMyTextPanel;
            textPanel.WritePublicText("                                 ---ORE---                ---INGOTS---\n", false);

            addResourcesForBlockWithName("RE_Storage_Ore");
            addResourcesForBlockWithName("RE_Storage_Ingots");

            addResourcesForGroupWithName("RE_Refineries");

            DisplayResourceNumbers(textPanel);
        }

        public void addResourcesForBlockWithName(string blockName)
        {
            IMyEntity storageBlock = GridTerminalSystem.GetBlockWithName(blockName) as IMyEntity;
            trackInventory(storageBlock);
        }

        public void addResourcesForGroupWithName(string groupName)
        {
            IMyBlockGroup storageGroup = GridTerminalSystem.GetBlockGroupWithName(groupName);
            List<IMyTerminalBlock> storageBlocks = new List<IMyTerminalBlock>();
            storageGroup.GetBlocks(storageBlocks);
            foreach (IMyTerminalBlock storageBlock in storageBlocks)
            {
                trackInventory(storageBlock);
            }
        }

        public void trackInventory(IMyEntity storage)
        {
            var items = storage.GetInventory(0).GetItems();
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];
                var content = item.Content;
                var isOre = content.TypeId.ToString() == "MyObjectBuilder_Ore";
                var isIngot = content.TypeId.ToString() == "MyObjectBuilder_Ingot";
                var resourceName = content.SubtypeName;

                if ((isOre || isIngot) && resources.ContainsKey(resourceName))
                {
                    double[] resourceValues = resources[resourceName];
                    if (isOre)
                    {
                        resourceValues[0] += (double)item.Amount;
                    }
                    else if (isIngot)
                    {
                        resourceValues[1] += (double)item.Amount;
                    }
                }
            }
        }

        public void DisplayResourceNumbers(IMyTextPanel textPanel)
        {
            foreach (KeyValuePair<string, double[]> resource in resources)
            {
                var resourceDisplayName = resourceDisplayStrings[resource.Key][0];
                var resourceValueSpacer = resourceDisplayStrings[resource.Key][1];
                var oreDisplayString = FormatNumber(resource.Value[0]);
                var ingotDisplayString = FormatNumber(resource.Value[1]);
                String text = resourceDisplayName + resourceValueSpacer
                    + oreDisplayString + new string(' ', (int)(30 - (oreDisplayString.Length * 1.8)))
                    + ingotDisplayString + "\n";
                textPanel.WritePublicText(text, true);
            }

            textPanel.ShowPublicTextOnScreen();
        }

        public string FormatNumber(double fixedPointVal)
        {
            if (fixedPointVal > 1000)
            {
                return (fixedPointVal / 1000).ToString("N1") + "k";
            }
            else
            {
                return (fixedPointVal).ToString("N1");
            }
        }
    }
}
