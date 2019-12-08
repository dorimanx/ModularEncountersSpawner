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
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using ModularEncountersSpawner;
using ModularEncountersSpawner.Configuration;
using ModularEncountersSpawner.Templates;

namespace ModularEncountersSpawner.Spawners{
	
	public static class SpaceCargoShipSpawner{
		
		public static Dictionary<string, List<ImprovedSpawnGroup>> SpawnGroupSublists = new Dictionary<string, List<ImprovedSpawnGroup>>();
		public static Dictionary<string, int> EligibleSpawnsByModId = new Dictionary<string, int>();
		
		public static string AttemptSpawn(Vector3D startCoords){
			
			if(Settings.General.UseMaxNpcGrids == true){
				
				var totalNPCs = NPCWatcher.ActiveNPCs.Count;
				
				if(totalNPCs >= Settings.General.MaxGlobalNpcGrids){
					
					return "Spawning Aborted. Max Global NPCs Limit Reached.";
					
				}
				
			}
			
			if(NPCWatcher.ActiveNpcTypeLimitReachedForArea("SpaceCargoShip", startCoords, Settings.SpaceCargoShips.MaxShipsPerArea, Settings.SpaceCargoShips.AreaSize) == true){
				
				return "Too Many Space Cargo Ship Grids in Player Area";
				
			}

            var validFactions = new Dictionary<string, List<string>>();
            var spawnGroupList = GetSpaceCargoShips(startCoords, out validFactions);
			
			if(Settings.General.UseModIdSelectionForSpawning == true){
				
				spawnGroupList = SpawnResources.SelectSpawnGroupSublist(SpawnGroupSublists, EligibleSpawnsByModId);
				
			}

			if(spawnGroupList.Count == 0){
				
				return "No Eligible Spawn Groups Could Be Found To Spawn Near Player.";
				
			}
			
			var spawnGroup = spawnGroupList[SpawnResources.rnd.Next(0, spawnGroupList.Count)];
			var startPathCoords = Vector3D.Zero;
			var endPathCoords = Vector3D.Zero;
			bool successfulPath = false;
			MyPlanet planet = SpawnResources.GetNearestPlanet(startCoords);
			
			if(SpawnResources.LunarSpawnEligible(startCoords) == false){
				
				successfulPath = CalculateRegularTravelPath(spawnGroup.SpawnGroup, startCoords, out startPathCoords, out endPathCoords);
				
			}else{
				
				successfulPath = CalculateLunarTravelPath(spawnGroup.SpawnGroup, startCoords, out startPathCoords, out endPathCoords);
				
			}
			
			if(successfulPath == false){
				
				return "Could Not Generate Safe Travel Path For SpawnGroup.";
				
			}
			
			//Get Directions
			var spawnForwardDir = Vector3D.Normalize(endPathCoords - startPathCoords);
			var spawnUpDir = Vector3D.CalculatePerpendicularVector(spawnForwardDir);
			var spawnMatrix = MatrixD.CreateWorld(startPathCoords, spawnForwardDir, spawnUpDir);
            long gridOwner = 0;
            var randFactionTag = spawnGroup.FactionOwner;

            if(validFactions.ContainsKey(spawnGroup.SpawnGroupName)) {

                randFactionTag = validFactions[spawnGroup.SpawnGroupName][SpawnResources.rnd.Next(0, validFactions[spawnGroup.SpawnGroupName].Count)];

            }

            if(NPCWatcher.NPCFactionTagToFounder.ContainsKey(randFactionTag) == true) {

                gridOwner = NPCWatcher.NPCFactionTagToFounder[randFactionTag];

            } else {

                Logger.AddMsg("Could Not Find Faction Founder For: " + randFactionTag);

            }

            foreach(var prefab in spawnGroup.SpawnGroup.Prefabs){
				
				var options = SpawnGroupManager.CreateSpawningOptions(spawnGroup, prefab);
				var spawnPosition = Vector3D.Transform((Vector3D)prefab.Position, spawnMatrix);
				var speedL = prefab.Speed * (Vector3)spawnForwardDir;
				var speedA = Vector3.Zero;
				var gridList = new List<IMyCubeGrid>();
				
				
				//Speed Management
				if(Settings.SpaceCargoShips.UseMinimumSpeed == true && prefab.Speed < Settings.SpaceCargoShips.MinimumSpeed){
					
					speedL = Settings.SpaceCargoShips.MinimumSpeed * (Vector3)spawnForwardDir;
					
				}
				
				if(Settings.SpaceCargoShips.UseSpeedOverride == true){
					
					speedL = Settings.SpaceCargoShips.SpeedOverride * (Vector3)spawnForwardDir;
					
				}
				
				
				
				//Grid Manipulation
				GridBuilderManipulation.ProcessPrefabForManipulation(prefab.SubtypeId, spawnGroup, "SpaceCargoShip", prefab.Behaviour);

				try{
					
					MyAPIGateway.PrefabManager.SpawnPrefab(gridList, prefab.SubtypeId, spawnPosition, spawnForwardDir, spawnUpDir, speedL, speedA, prefab.BeaconText, options, gridOwner);
					
				}catch(Exception exc){
					
					Logger.AddMsg("Something Went Wrong With Prefab Spawn Manager.", true);
					
				}
				
				var pendingNPC = new ActiveNPC();
                pendingNPC.SpawnGroupName = spawnGroup.SpawnGroupName;
                pendingNPC.SpawnGroup = spawnGroup;
                pendingNPC.InitialFaction = randFactionTag;
                pendingNPC.faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(pendingNPC.InitialFaction);
                pendingNPC.Name = prefab.SubtypeId;
				pendingNPC.GridName = MyDefinitionManager.Static.GetPrefabDefinition(prefab.SubtypeId).CubeGrids[0].DisplayName;
				pendingNPC.StartCoords = startPathCoords;
				pendingNPC.CurrentCoords = startPathCoords;
				pendingNPC.EndCoords = endPathCoords;
				pendingNPC.SpawnType = "SpaceCargoShip";
                pendingNPC.AutoPilotSpeed = speedL.Length();
                pendingNPC.CleanupIgnore = spawnGroup.IgnoreCleanupRules;
				pendingNPC.ForceStaticGrid = spawnGroup.ForceStaticGrid;
				pendingNPC.KeenAiName = prefab.Behaviour;
				pendingNPC.KeenAiTriggerDistance = prefab.BehaviourActivationDistance;
				
				if(string.IsNullOrEmpty(pendingNPC.KeenAiName) == false){
					
					Logger.AddMsg("Stock AI Detected In Prefab: " + prefab.SubtypeId + " in SpawnGroup: " + spawnGroup.SpawnGroup.Id.SubtypeName);
					
				}
				
				if(spawnGroup.RandomizeWeapons == true){
						
					pendingNPC.ReplenishedSystems = false;
					pendingNPC.ReplacedWeapons = true;
					
				}else if((MES_SessionCore.NPCWeaponUpgradesModDetected == true || Settings.General.EnableGlobalNPCWeaponRandomizer == true) && spawnGroup.IgnoreWeaponRandomizerMod == false){
				
					pendingNPC.ReplenishedSystems = false;
					pendingNPC.ReplacedWeapons = true;
					
				}else if(spawnGroup.ReplenishSystems == true){
					
					pendingNPC.ReplenishedSystems = false;
					
				}
				
				NPCWatcher.PendingNPCs.Add(pendingNPC);
				
			}
			
			Logger.SkipNextMessage = false;
			return "Spawning Group - " + spawnGroup.SpawnGroup.Id.SubtypeName;
			
		}
		
		public static bool CalculateRegularTravelPath(MySpawnGroupDefinition spawnGroup, Vector3D startCoords, out Vector3D startPathCoords, out Vector3D endPathCoords){
			
			startPathCoords = Vector3D.Zero;
			endPathCoords = Vector3D.Zero;
			SpawnResources.RefreshEntityLists();
			MyPlanet planet = SpawnResources.GetNearestPlanet(startCoords);
			List<IMyEntity> nearbyEntities = new List<IMyEntity>();
			
			for(int i = 0; i < Settings.SpaceCargoShips.MaxSpawnAttempts; i++){
				
				var randDir = Vector3D.Normalize(MyUtils.GetRandomVector3D());
				
				var closestPathDist = (double)SpawnResources.rnd.Next((int)Settings.SpaceCargoShips.MinPathDistanceFromPlayer, (int)Settings.SpaceCargoShips.MaxPathDistanceFromPlayer);
				var closestPathPoint = randDir * closestPathDist + startCoords;
				
				bool tryInvertedDir = SpawnResources.IsPositionInGravity(closestPathPoint, planet);
				
				if(tryInvertedDir == true){
					
					randDir = randDir * -1;
					closestPathPoint = randDir * closestPathDist + startCoords;
					
					if(SpawnResources.IsPositionInGravity(closestPathPoint, planet) == true){
						
						continue;
						
					}
					
				}
				
				var pathDist = (double)SpawnResources.rnd.Next((int)Settings.SpaceCargoShips.MinPathDistance, (int)Settings.SpaceCargoShips.MaxPathDistance);
				var pathDir = Vector3D.Normalize(MyUtils.GetRandomPerpendicularVector(ref randDir));
				var pathHalfDist = pathDist / 2;
				
				var tempPathStart = pathDir * pathHalfDist + closestPathPoint;
				pathDir = pathDir * -1;
				var tempPathEnd = pathDir * pathHalfDist + closestPathPoint;
				
				bool badPath = false;
				
				IHitInfo hitInfo = null;
				
				if(MyAPIGateway.Physics.CastLongRay(tempPathStart, tempPathEnd, out hitInfo, true) == true){
					
					continue;
					
				}
					
				foreach(var entity in SpawnResources.EntityList){
					
					if(Vector3D.Distance(tempPathStart, entity.GetPosition()) < Settings.SpaceCargoShips.MinSpawnDistFromEntities){
						
						badPath = true;
						break;
						
					}
					
				}
				
				if(badPath == true){
					
					continue;
					
				}
				
				var upDir = Vector3D.CalculatePerpendicularVector(pathDir);
				var pathMatrix = MatrixD.CreateWorld(tempPathStart, pathDir, upDir);
				
				foreach(var prefab in spawnGroup.Prefabs){
					
					double stepDistance = 0;
					var tempPrefabStart = Vector3D.Transform((Vector3D)prefab.Position, pathMatrix);
					
					while(stepDistance < pathDist){

						stepDistance += Settings.SpaceCargoShips.PathCheckStep;
						var pathCheckCoords = pathDir * stepDistance + tempPrefabStart;
						
						if(SpawnResources.IsPositionInSafeZone(pathCheckCoords) == true || SpawnResources.IsPositionInGravity(pathCheckCoords, planet) == true){
							
							badPath = true;
							break;
							
						}
												
					}
					
					if(badPath == true){
							
						break;
						
					}

				}

				if(badPath == true){
					
					continue;
					
				}
				
				startPathCoords = tempPathStart;
				endPathCoords = tempPathEnd;
				return true;
				
			}
			
			return false;
			
		}
		
		public static bool CalculateLunarTravelPath(MySpawnGroupDefinition spawnGroup, Vector3D startCoords, out Vector3D startPathCoords, out Vector3D endPathCoords){
			
			startPathCoords = Vector3D.Zero;
			endPathCoords = Vector3D.Zero;
			SpawnResources.RefreshEntityLists();
			MyPlanet planet = SpawnResources.GetNearestPlanet(startCoords);
			
			if(planet == null){
				
				return false;
				
			}
			
			var planetEntity = planet as IMyEntity;
			
			for(int i = 0; i < Settings.SpaceCargoShips.MaxSpawnAttempts; i++){

				var spawnAltitude = (double)SpawnResources.rnd.Next((int)Settings.SpaceCargoShips.MinLunarSpawnHeight, (int)Settings.SpaceCargoShips.MaxLunarSpawnHeight);
				var abovePlayer = SpawnResources.CreateDirectionAndTarget(planetEntity.GetPosition(), startCoords, startCoords, spawnAltitude);
				var midpointDist = (double)SpawnResources.rnd.Next((int)Settings.SpaceCargoShips.MinPathDistanceFromPlayer, (int)Settings.SpaceCargoShips.MaxPathDistanceFromPlayer);
				var pathMidpoint = SpawnResources.GetRandomCompassDirection(abovePlayer, planet) * midpointDist + abovePlayer;
				var pathDist = (double)SpawnResources.rnd.Next((int)Settings.SpaceCargoShips.MinPathDistance, (int)Settings.SpaceCargoShips.MaxPathDistance);
				var pathDir = SpawnResources.GetRandomCompassDirection(abovePlayer, planet);
				var pathHalfDist = pathDist / 2;
				
				var tempPathStart = pathDir * pathHalfDist + pathMidpoint;
				pathDir = pathDir * -1;
				var tempPathEnd = pathDir * pathHalfDist + pathMidpoint;
				
				bool badPath = false;
				
				IHitInfo hitInfo = null;
				
				if(MyAPIGateway.Physics.CastLongRay(tempPathStart, tempPathEnd, out hitInfo, true) == true){
					
					continue;
					
				}
				
					
				foreach(var entity in SpawnResources.EntityList){
					
					if(Vector3D.Distance(tempPathStart, entity.GetPosition()) < Settings.SpaceCargoShips.MinSpawnDistFromEntities){
						
						badPath = true;
						break;
						
					}
					
				}
				
				if(badPath == true){
					
					continue;
					
				}
				
				var upDir = Vector3D.CalculatePerpendicularVector(pathDir);
				var pathMatrix = MatrixD.CreateWorld(tempPathStart, pathDir, upDir);
				
				foreach(var prefab in spawnGroup.Prefabs){
					
					double stepDistance = 0;
					var tempPrefabStart = Vector3D.Transform((Vector3D)prefab.Position, pathMatrix);
					
					while(stepDistance < pathDist){

						stepDistance += Settings.SpaceCargoShips.PathCheckStep;
						var pathCheckCoords = pathDir * stepDistance + tempPrefabStart;
						
						if(SpawnResources.IsPositionInSafeZone(pathCheckCoords) == true || SpawnResources.IsPositionInGravity(pathCheckCoords, planet) == true){
							
							badPath = true;
							break;
							
						}
												
					}
					
					if(badPath == true){
							
						break;
						
					}

				}

				if(badPath == true){
					
					continue;
					
				}
				
				startPathCoords = tempPathStart;
				endPathCoords = tempPathEnd;
				
				return true;
				
			}
			
			return false;
			
		}
		
		public static List<ImprovedSpawnGroup> GetSpaceCargoShips(Vector3D playerCoords, out Dictionary<string, List<string>> validFactions){
			
			MyPlanet planet = SpawnResources.GetNearestPlanet(playerCoords);
			bool allowLunar = false;
			string specificGroup = "";
			var planetRestrictions = new List<string>(Settings.General.PlanetSpawnsDisableList.ToList());
            validFactions = new Dictionary<string, List<string>>();
            SpawnGroupSublists.Clear();
			EligibleSpawnsByModId.Clear();
			
			if(planet != null){
				
				if(planetRestrictions.Contains(planet.Generator.Id.SubtypeName) == true){
					
					return new List<ImprovedSpawnGroup>();
					
				}
				
			}
			
			bool specificSpawnRequest = false;
			
			if(SpawnGroupManager.AdminSpawnGroup != ""){
				
				specificGroup = SpawnGroupManager.AdminSpawnGroup;
				SpawnGroupManager.AdminSpawnGroup = "";
				specificSpawnRequest = true;
				
			}
			
			if(SpawnResources.IsPositionInGravity(playerCoords, planet) == true){
				
				if(SpawnResources.LunarSpawnEligible(playerCoords) == true){
					
					allowLunar = true;
					
				}else{
					
					return new List<ImprovedSpawnGroup>();
					
				}
				
			}
			
			string planetName = "";
			
			if(planet != null){
				
				planetName = planet.Generator.Id.SubtypeId.ToString();
				
			}
			
			var eligibleGroups = new List<ImprovedSpawnGroup>();
			
			//Filter Eligible Groups To List
			foreach(var spawnGroup in SpawnGroupManager.SpawnGroups){
				
				if(specificGroup != "" && spawnGroup.SpawnGroup.Id.SubtypeName != specificGroup){
					
					continue;
					
				}
				
				if(specificGroup == "" && spawnGroup.AdminSpawnOnly == true){
					
					continue;
					
				}
				
				if(spawnGroup.SpaceCargoShip == false){
					
					if(allowLunar == true){
						
						if(spawnGroup.LunarCargoShip == false){
							
							continue;
							
						}
						
					}else{
						
						continue;
						
					}
					
				}
				
				if(SpawnResources.CheckCommonConditions(spawnGroup, playerCoords, planet, specificSpawnRequest) == false){
					
					continue;
					
				}

                var validFactionsList = SpawnResources.ValidNpcFactions(spawnGroup, playerCoords);

                if(validFactionsList.Count == 0) {

                    continue;

                }

                if(validFactions.ContainsKey(spawnGroup.SpawnGroupName) == false) {

                    validFactions.Add(spawnGroup.SpawnGroupName, validFactionsList);

                }
				
				if(spawnGroup.Frequency > 0){
					
					string modID = spawnGroup.SpawnGroup.Context.ModId;
					
					if(string.IsNullOrEmpty(modID) == true){
						
						modID = "KeenSWH";
						
					}
					
					if(SpawnGroupSublists.ContainsKey(modID) == false){
						
						SpawnGroupSublists.Add(modID, new List<ImprovedSpawnGroup>());
						
					}
					
					if(EligibleSpawnsByModId.ContainsKey(modID) == false){
						
						EligibleSpawnsByModId.Add(modID, 1);
						
					}else{
						
						EligibleSpawnsByModId[modID] += 1;
						
					}

					if(Settings.SpaceCargoShips.UseMaxSpawnGroupFrequency == true && spawnGroup.Frequency > Settings.SpaceCargoShips.MaxSpawnGroupFrequency * 10){
						
						spawnGroup.Frequency = (int)Math.Round((double)Settings.SpaceCargoShips.MaxSpawnGroupFrequency * 10);
						
					}
					
					for(int i = 0; i < spawnGroup.Frequency; i++){
						
						eligibleGroups.Add(spawnGroup);
						SpawnGroupSublists[modID].Add(spawnGroup);
						
					}
					
				}
				
			}
			
			return eligibleGroups;
			
		}
			
	}
	
}