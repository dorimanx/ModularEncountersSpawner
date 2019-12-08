﻿using System;
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
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using ModularEncountersSpawner;
using ModularEncountersSpawner.Configuration;
using ModularEncountersSpawner.Templates;
using ModularEncountersSpawner.Spawners;

namespace ModularEncountersSpawner.Api {
    public static class SpawnerLocalApi {

        public static void SendApiToMods() {



        }

        public static Dictionary<string, Delegate> GetApiDictionary() {

            var dict = new Dictionary<string, Delegate>();
            dict.Add("CustomSpawnRequest", new Action<List<string>, Vector3D, Vector3D, Vector3D, Vector3>(CustomSpawner.CustomSpawnRequest));
            return dict;

        }

        public static void CustomSpawnRequest() {



        }

    }

}
