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
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Game.ObjectBuilders;
using VRage.Game.ObjectBuilders.ComponentSystem;
using VRage.Utils;
using VRageMath;
using ModularEncountersSpawner;
using ModularEncountersSpawner.Configuration;
using ModularEncountersSpawner.Templates;
using ModularEncountersSpawner.Spawners;

namespace ModularEncountersSpawner{
	
	public static class GridUtilities{

        /*
		public static void CreateAndAttachCrew(MyObjectBuilder_CubeGrid cubeGrid, ImprovedSpawnGroup spawnGroup, long ownerId){
			
			if(spawnGroup.FillSeatsWithNpcCrew == false){
				
				Logger.AddMsg("Grid Not Set To Receive Crew", true);
				
			}
			
			try{
				
				var seatList = new List<MyObjectBuilder_Cockpit>();
				
				foreach(var block in cubeGrid.CubeBlocks){
					
					if((block as MyObjectBuilder_Cockpit) == null){
						
						continue;
						
					}
					
					if(block.IntegrityPercent == 1 && block.BuildPercent == 1){
						
						seatList.Add(block as MyObjectBuilder_Cockpit);
						
					}
					
				}
				
				if(seatList.Count == 0 || spawnGroup.CrewCharacterSubtypeIds.Count == 0){
					
					Logger.AddMsg("No Valid Seats On Grid", true);
					return;
					
				}
				
				ulong steamId = 0;
				IMyCharacter tempCharacter = null;
				
				foreach(var player in MES_SessionCore.PlayerList){
					
					if(player == null){
						
						continue;
						
					}
					
					if(player.Character == null || player.IsBot == true){
						
						continue;
						
					}
					
					tempCharacter = player.Character;
					steamId = player.SteamUserId;
					break;
					
				}
				
				foreach(var seat in seatList){
					
					string crewName = spawnGroup.CrewNaming;
					
					if(spawnGroup.UseRandomCrewNames == true){
						
						//GetRandomCrewName();
						
					}
					
					var charcterModel = spawnGroup.CrewCharacterSubtypeIds[SpawnResources.rnd.Next(0, spawnGroup.CrewCharacterSubtypeIds.Count)];
					var character = CreateCrewCharacter(charcterModel, charcterModel, crewName, MatrixD.Identity, Vector3.Zero, (Vector3)seat.ColorMaskHSV, false, ownerId, steamId, 100);
					
					if(character == null){
						
						Logger.AddMsg("Attempt To Create Crew Character Resulted Null", true);
						continue;
						
					}
					
					Logger.AddMsg("Attaching Crew To Seat", true);
					seat.Pilot = character;
					
				}
				
			}catch(Exception exc){
				
				Logger.AddMsg("Error while Adding NPC Crew");
				
			}
	
		}
		
		public static MyObjectBuilder_Character CreateCrewCharacter(string subtype, string modelName, string displayName, MatrixD matrix, Vector3 velocity, Vector3 color, bool broadcasting, long ownerId, ulong steamId, float health){
			
			MyObjectBuilder_Character crew = null;
			
			try{
					
				var battery = new MyObjectBuilder_Battery();
				battery.CurrentCapacity = 1;
				
				var characterOb = new MyObjectBuilder_Character();
				characterOb.SubtypeName = subtype;
				characterOb.PersistentFlags = MyPersistentEntityFlags2.InScene;
				characterOb.EntityId = 0;
				characterOb.PositionAndOrientation = new MyPositionAndOrientation(matrix.Translation, (Vector3)matrix.Forward, (Vector3)matrix.Up);
				characterOb.LinearVelocity = velocity;
				characterOb.CharacterModel = modelName;
				characterOb.LightEnabled = false;
				characterOb.ColorMaskHSV = (SerializableVector3)color;
				characterOb.DampenersEnabled = true;
				characterOb.CharacterGeneralDamageModifier = 3;
				characterOb.JetpackEnabled = false;
				//characterOb.Health = health;
				characterOb.DisplayName = displayName;
				characterOb.IsPersistenceCharacter = true;
				characterOb.OwningPlayerIdentityId = ownerId;
				characterOb.AIMode = false;
				characterOb.Battery = battery;
				characterOb.EnableBroadcasting = broadcasting;
				characterOb.NeedsOxygenFromSuit = false;
				characterOb.PlayerSteamId = steamId;
				
				//Add Health Component to Character
				var statComponent = new MyObjectBuilder_ComponentContainer.ComponentData();
				statComponent.TypeId = "MyEntityStatComponent";
				var stats = new MyObjectBuilder_EntityStat();
				stats.SubtypeName = "SpaceCharacterHealth";
				stats.Value = 1;
				stats.MaxValue = 1;
				stats.StatRegenAmountMultiplier = 1;
				stats.StatRegenAmountMultiplierDuration = 0;
				var charStatComp = new MyObjectBuilder_CharacterStatComponent();
				string[] scriptNames = {"SpaceStatEffect"};
				charStatComp.Stats = new MyObjectBuilder_EntityStat[]{stats};
				charStatComp.ScriptNames = scriptNames;
				statComponent.Component = charStatComp;
				characterOb.ComponentContainer = new MyObjectBuilder_ComponentContainer();
				characterOb.ComponentContainer.Components.Add(statComponent);
				
				//var characterEntity = MyAPIGateway.Entities.CreateFromObjectBuilderAndAdd(characterOb);
				crew = characterOb;
				
			}catch(Exception exc){

				Logger.AddMsg("Error while creating NPC Crew Entity");
				
			}
			
			return crew;
			
		}
		*/

        public static void NonPhysicalAmmoProcessing(IMyCubeGrid cubeGrid) {

            if(cubeGrid == null || MyAPIGateway.Entities.Exist(cubeGrid) == false) {

                return;

            }

            var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(cubeGrid);
            var blockList = new List<IMyUserControllableGun>();
            gts.GetBlocksOfType<IMyUserControllableGun>(blockList);

            foreach(var block in blockList) {

                try {

                    if(block.GetInventory().Empty() == true) {

                        continue;

                    }

                    var firstItem = block.GetInventory().GetItems()[0];
                    var ammoMagId = new MyDefinitionId(firstItem.Content.TypeId, firstItem.Content.SubtypeName);
                    var ammoMagDefinition = MyDefinitionManager.Static.GetAmmoMagazineDefinition(ammoMagId);

                    if(ammoMagDefinition == null) {

                        continue;

                    }

                    int amount = (int)ammoMagDefinition.Capacity * (int)firstItem.Amount;

                    var gunbase = (IMyGunObject<MyGunBase>)block;

                    if(gunbase?.GunBase == null) {

                        continue;

                    }

                    block.GetInventory().Clear();
                    gunbase.GunBase.CurrentAmmo = amount;

                } catch(Exception e) {

                    Logger.AddMsg("Issue Processing Non-Physical Ammo For Grid: " + cubeGrid.CustomName + " - Block: " + block.CustomName);

                }

            }

        }

        public static void RemoveGridContainerComponents(IMyCubeGrid cubeGrid) {

            if(cubeGrid == null || MyAPIGateway.Entities.Exist(cubeGrid) == false) {

                return;

            }

            var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(cubeGrid);
            var blockList = new List<IMyTerminalBlock>();
            gts.GetBlocksOfType<IMyTerminalBlock>(blockList);

            foreach(var block in blockList) {

                bool targetBlock = false;

                if(block as IMyCargoContainer != null) {

                    targetBlock = true;

                } else if(block as IMyShipConnector != null) {

                    targetBlock = true;

                } else if(block as IMyCollector != null) {

                    targetBlock = true;

                } else if(block as IMyCockpit != null) {

                    targetBlock = true;

                }

                if(targetBlock == false) {

                    continue;

                }

                if(block.HasInventory == true) {

                    block.GetInventory().Clear();

                }

            }

        }


        public static void ReplenishGridSystems(IMyCubeGrid cubeGrid, bool randomReplaced){
			
			var errorLogBuilder = new StringBuilder();
			errorLogBuilder.Append("Error: Something has gone wrong with Spawner Inventory Replenishment.").AppendLine();
			errorLogBuilder.Append("Please provide this data to Mod Author of Modular Encounters Spawner.").AppendLine();
			errorLogBuilder.Append(" - Start Replenish Of Inventories").AppendLine();
			
			try{
				
				errorLogBuilder.Append(" - Get Grid Terminal System").AppendLine();
				var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(cubeGrid);
				var blockList = new List<IMyTerminalBlock>();
				gts.GetBlocksOfType<IMyTerminalBlock>(blockList);
				
				errorLogBuilder.Append(" - Iterate Through Blocks").AppendLine();
				foreach(var block in blockList){
					
					//Weapon Replenishing
					
					if((block as IMyUserControllableGun) != null){
																		
						var weaponBlockDefinition = block.SlimBlock.BlockDefinition as MyWeaponBlockDefinition;
						
						if(weaponBlockDefinition != null){
							
							errorLogBuilder.Append("   - Identified Weapon: ").Append(weaponBlockDefinition.Id.ToString()).AppendLine();
							var weaponDefinition = MyDefinitionManager.Static.GetWeaponDefinition(weaponBlockDefinition.WeaponDefinitionId);
							
							if(weaponDefinition == null){
								
								errorLogBuilder.Append("   - Weapon Has No Weapon Definition. Skip.").AppendLine();
								continue;
								
							}
							
							var ammoMagazineDefinitionList = new List<MyAmmoMagazineDefinition>();
							
							foreach(var ammoId in weaponDefinition.AmmoMagazinesId){
								
								var tempAmmoMag = MyDefinitionManager.Static.GetAmmoMagazineDefinition(ammoId);
								ammoMagazineDefinitionList.Add(tempAmmoMag);
								
							}
							
							if(ammoMagazineDefinitionList.Count == 0){
								
								errorLogBuilder.Append("   - Weapon Has No Ammo Magazine Definitions. Skip.").AppendLine();
								continue;
								
							}
							
							var ammo = ammoMagazineDefinitionList[SpawnResources.rnd.Next(0, ammoMagazineDefinitionList.Count)];
							int totalAdds = 0;
							
							errorLogBuilder.Append("   - Add Ammo To Weapon.").AppendLine();
							while(totalAdds < 100){
								
								var definitionId = new MyDefinitionId(typeof(MyObjectBuilder_AmmoMagazine), ammo.Id.SubtypeId.ToString());
								var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(definitionId);
								MyObjectBuilder_InventoryItem inventoryItem = new MyObjectBuilder_InventoryItem { Amount = 1, Content = content };
								
								if(block.GetInventory().CanItemsBeAdded(1, definitionId) == true){
									
									errorLogBuilder.Append("   - Adding Ammo.").AppendLine();
									block.GetInventory().AddItems(1, inventoryItem.Content);
									totalAdds++;
										
								}else{
									
									totalAdds = 101;
									
								}
								
							}
							
							errorLogBuilder.Append("   - Ammo Adding Complete.").AppendLine();
							errorLogBuilder.Append("   - Assign Ammo To Gun Object.").AppendLine();
							
							var weaponBlock = block as IMyUserControllableGun;
							var gunObject = weaponBlock as IMyGunObject<Sandbox.Game.Weapons.MyDeviceBase>;
							
							if(gunObject != null){
								
								var switchAmmo = gunObject.GunBase.SwitchAmmoMagazineToNextAvailable();
								
							}
							
							errorLogBuilder.Append("   - Ammo Assign To Gun Object Complete.").AppendLine();
							
							var turretBlock = block as IMyLargeTurretBase;
							
							if(turretBlock != null && randomReplaced == true){
								
								errorLogBuilder.Append("   - Weapon Is Randomly Added Turret. Set Targeting.").AppendLine();
								
								try{
									
									//turretBlock.SetValue<bool>("TargetSmallShips", true);
									//turretBlock.SetValue<bool>("TargetLargeShips", true);
									//turretBlock.SetValue<bool>("TargetStations", true);
									//turretBlock.SetValue<bool>("TargetNeutrals", true);
									//turretBlock.SetValue<bool>("TargetCharacters", true);
									
								}catch(Exception exc){
									
									errorLogBuilder.Append("   - Something Went Wrong With Random Turret Targeting...").AppendLine();
									
								}
								
							}
							
						}
			
					}
					if(block as IMyReactor != null){
						
						errorLogBuilder.Append(" - Identified Reactor Block.").AppendLine();
						errorLogBuilder.Append("   - Filling Reactor With Uranium.").AppendLine();
						
						var powerDef = block.SlimBlock.BlockDefinition as MyPowerProducerDefinition;
						var totalFuelAdd = (MyFixedPoint)powerDef.MaxPowerOutput;
						
						var definitionId = new MyDefinitionId(typeof(MyObjectBuilder_Ingot), "Uranium");
						var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(definitionId);
						MyObjectBuilder_InventoryItem inventoryItem = new MyObjectBuilder_InventoryItem { Amount = totalFuelAdd, Content = content };
						
						if(block.GetInventory().CanItemsBeAdded(totalFuelAdd, definitionId) == true){
							
							errorLogBuilder.Append("   - Adding Fuel.").AppendLine();
							block.GetInventory().AddItems(totalFuelAdd, inventoryItem.Content);
							
						}
						
						errorLogBuilder.Append("   - Completed Reactor Filling.").AppendLine();
						
					}
					
					if(block as IMyParachute != null){
						
						errorLogBuilder.Append(" - Identified Parachute Block.").AppendLine();
						errorLogBuilder.Append("   - Filling Parachute With Canvas Components.").AppendLine();
						
						MyFixedPoint totalAdd = 5;
						
						if(cubeGrid.GridSizeEnum == MyCubeSize.Large){
							
							totalAdd *= 5;
							
						}
						
						var definitionId = new MyDefinitionId(typeof(MyObjectBuilder_Component), "Canvas");
						var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(definitionId);
						MyObjectBuilder_InventoryItem inventoryItem = new MyObjectBuilder_InventoryItem { Amount = totalAdd, Content = content };
						
						if(block.GetInventory().CanItemsBeAdded(totalAdd, definitionId) == true){
							
							errorLogBuilder.Append("   - Adding Canvas'.").AppendLine();
							block.GetInventory().AddItems(totalAdd, inventoryItem.Content);
							
						}
						
						errorLogBuilder.Append("   - Completed Parachute Filling.").AppendLine();
						
					}
					
					if(block as IMyGasGenerator != null){
						
						errorLogBuilder.Append(" - Identified O2/H2 Generator Block.").AppendLine();
						errorLogBuilder.Append("   - Filling Generator With Ice.").AppendLine();
						
						var availableVolume = (block.GetInventory().MaxVolume - block.GetInventory().CurrentVolume) * 1000;
						availableVolume -= 10;
						
						var definitionId = new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Ice");
						var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(definitionId);
						MyObjectBuilder_InventoryItem inventoryItem = new MyObjectBuilder_InventoryItem { Amount = 1, Content = content };
						
						var itemDef = MyDefinitionManager.Static.GetPhysicalItemDefinition(definitionId);
						
						if(itemDef == null){
							
							errorLogBuilder.Append("   - Ice Definition Not Found. Skipping.").AppendLine();
							continue;
							
						}
						
						var totalAdd = (double)availableVolume / ((double)itemDef.Volume * 1000);
						
						if(block.GetInventory().CanItemsBeAdded((MyFixedPoint)totalAdd, definitionId) == true){
							
							errorLogBuilder.Append("   - Adding Ice.").AppendLine();
							block.GetInventory().AddItems((MyFixedPoint)totalAdd, inventoryItem.Content);
							
						}
						
						errorLogBuilder.Append("   - Completed O2/H2 Generator Filling.").AppendLine();
						
					}

				}
				
			}catch(Exception exc){
				
				
				//Logger.AddMsg(errorLogBuilder.ToString());
				
			}
			
			//Logger.AddMsg(errorLogBuilder.ToString(), true);
			
		}
		
		public static bool FixUnfinishedBlock(IMyInventory inv, IMySlimBlock slimBlock, long owner){
			
			bool success = true;
			
			Dictionary<string, int> missingParts = new Dictionary<string, int>();
			slimBlock.GetMissingComponents(missingParts);
			
			if(missingParts.Keys.Count == 0){
				
				return success;
				
			}
			
			foreach(var part in missingParts.Keys.ToList()){
				
				MyDefinitionId defId = new MyDefinitionId(typeof(MyObjectBuilder_Component), part);
				var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(defId);
				MyObjectBuilder_InventoryItem inventoryItem = new MyObjectBuilder_InventoryItem { Amount = 1, Content = content };
				
				while(missingParts[part] > 0){
					
					if(inv.CanItemsBeAdded(1, defId) == true){
							
					inv.AddItems(1, inventoryItem.Content);
					missingParts[part]--;
							
					}else{
						
						//Logger.AddMsg("Failed To Add Repair Component To Container", true);
						success = false;
						break;
						
					}
					
				}
				
				slimBlock.MoveItemsToConstructionStockpile(inv);
				slimBlock.IncreaseMountLevel(10000, owner, inv);

			}
			
			return success;
			
		}

        public static Vector3 RoundColorHSV(Vector3D originalColor) {

            var newColor = originalColor;
            newColor.X = Math.Round(newColor.X, 4);
            newColor.Y = Math.Round(newColor.Y, 4);
            newColor.Z = Math.Round(newColor.Z, 4);
            return (Vector3)newColor;

        }
		
	}
	
}