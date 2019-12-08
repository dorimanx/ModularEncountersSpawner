using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders;
using VRage.Game.ObjectBuilders.ComponentSystem;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Serialization;
using VRage.Utils;
using VRageMath;
using ModularEncountersSpawner.Templates;
using ModularEncountersSpawner.Configuration;
using ModularEncountersSpawner.Spawners;

namespace ModularEncountersSpawner {
    public static class RivalAIHelper {

        public static List<string> RivalAiControlModules = new List<string>();
        public static Dictionary<string, string> RivalAiBehaviorProfiles = new Dictionary<string, string>();

        public static void SetupRivalAIHelper() {

            RivalAiControlModules.Add("RivalAIRemoteControlSmall");
            RivalAiControlModules.Add("RivalAIRemoteControlLarge");

            var itemList = MyDefinitionManager.Static.GetEntityComponentDefinitions();

            foreach(var item in itemList) {

                if(string.IsNullOrWhiteSpace(item.DescriptionText) == true) {

                    continue;

                }

                if(item.DescriptionText.Contains("[RivalAI Behavior]") == true || item.DescriptionText.Contains("[Rival AI Behavior]") == true) {

                    string val = "";

                    if(RivalAiBehaviorProfiles.TryGetValue(item.Id.SubtypeName, out val) == false) {

                        Logger.AddMsg("Found RivalAI Profile: " + item.Id.SubtypeName);
                        RivalAiBehaviorProfiles.Add(item.Id.SubtypeName, item.DescriptionText);

                    } else {

                        Logger.AddMsg("Duplicate RivalAI Profile Detected In Load Order: " + item.Id.SubtypeName);

                    }

                }

            }

        }

    }
}
