using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.World.WIScripts
{
		public class ChemistryLab : WIScript
		{
				public GenericWorldItem ModernShard;
				public GenericWorldItem OldShard;
				public GenericWorldItem AntiquatedShard;
				public GenericWorldItem AncientShard;
				public GenericWorldItem PrehistoricShard;
				[FrontiersFXAttribute]
				public string FXOnDate;

				public bool DateShard()
				{
						if (Player.Local.Tool.IsEquipped) {
								ArtifactShard shard = null;
								if (Player.Local.Tool.worlditem.Is <ArtifactShard>(out shard)) {
										if (shard.DateShard()) {
												//once the shard is dated
												//swap it out with a new shard
												//use the generic world item as a base
												//set the state so the shard shape is the same
												GenericWorldItem newShard = null;
												switch (shard.State.Age) {
														case ArtifactAge.Modern:
														default:
																newShard = ModernShard;
																break;

														case ArtifactAge.Old:
																newShard = OldShard;
																break;

														case ArtifactAge.Antiquated:
																newShard = AntiquatedShard;
																break;

														case ArtifactAge.Ancient:
																newShard = AncientShard;
																break;

														case ArtifactAge.Prehistoric:
																newShard = PrehistoricShard;
																break;
												}

												//now turn it into a stack item
												//this will allow us to prevent it from randomizing itself
												StackItem newShardStackItem = newShard.ToStackItem();
												ArtifactShardState shardState = null;
												newShardStackItem.State = shard.worlditem.State;
												newShardStackItem.StackName = shard.worlditem.StackName;
												if (newShardStackItem.GetStateData <ArtifactShardState>(out shardState)) {
														shardState.HasChosenFragment = true;
												}

												WorldItems.ReplaceWorldItem(shard.worlditem, newShardStackItem);
												FXManager.Get.SpawnFX(transform.position, FXOnDate);
										}
								}
						}
						return false;
				}
		}
}
