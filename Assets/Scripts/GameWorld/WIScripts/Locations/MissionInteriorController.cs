using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World.WIScripts
{
		public class MissionInteriorController : WIScript
		{		//used a LOT to control when & where mission-related characters & items spawn
				//usually if there's a bug with character spawning it's coming from this script
				public MissionInteriorControllerState State = new MissionInteriorControllerState();
				public Structure structure = null;
				public MissionInteriorCondition LastTopCondition = null;

				public override void OnInitialized()
				{
						if (worlditem.Is <Structure>(out structure)) {
								structure.OnPreparingToBuild += OnPreparingToBuild;
								worlditem.OnVisible += OnVisible;
								LastTopCondition = null;

								if (structure.State.ForceBuildInterior) {
										State.OnlyRefreshOnVisible = true;
								}

								if (State.OnlyRefreshOnVisible) {
										Player.Get.AvatarActions.Subscribe(AvatarAction.MissionUpdated, new ActionListener(MissionUpdated));
								}
						}
				}

				public bool MissionUpdated(double timeStamp)
				{
						if (mDestroyed)
								return true;

						if (State.OnlyRefreshOnVisible)
								return true;

						if (worlditem.Is(WIActiveState.Active | WIActiveState.Visible)) {
								OnVisible();
						}

						return true;
				}

				public void ForceRebuild()
				{
						LastTopCondition = null;
						OnVisible();
				}

				public void OnVisible()
				{
						if (!mUpdatingOverTime) {
								mUpdatingOverTime = true;
								StartCoroutine(OnMissionUpdated());
						}
				}

				public void OnPreparingToBuild()
				{
						if (structure.Is(StructureLoadState.InteriorLoading | StructureLoadState.InteriorLoaded) || Player.Local.Surroundings.IsVisitingStructure(structure)) {
								if (Player.Local.HasSpawned) {
										return;
								}
						}

						//check the mission states to figure out which interiors to build
						LastTopCondition = GetTopCondition();

						structure.State.AdditionalInteriorVariants.Clear();
						structure.State.AdditionalInteriorVariants.AddRange(LastTopCondition.StateVariable.InteriorVariants);

						structure.State.InteriorCharacters.Clear();
						structure.State.OwnerSpawn = LastTopCondition.StateVariable.OwnerSpawn;
						structure.State.InteriorCharacters.AddRange(LastTopCondition.StateVariable.AdditionalInteriorCharacters);
				}

				public MissionInteriorCondition GetTopCondition()
				{
						int topConditionIndex = 0;
						MissionInteriorCondition topCondition = null;
						if (MissionCondition.CheckConditions <MissionInteriorCondition>(State.Conditions, out topConditionIndex)) {
								topCondition = State.Conditions[topConditionIndex];
						} else {
								topCondition = State.Default;
						}
						return topCondition;
				}

				protected IEnumerator OnMissionUpdated()
				{
						double waitUntil = Frontiers.WorldClock.AdjustedRealTime + 1f;
						while (Frontiers.WorldClock.AdjustedRealTime < waitUntil) {
								yield return null;
						}

						if (!Player.Local.Surroundings.IsVisitingStructure(structure)) {
								if (LastTopCondition != null) {
										//if we've been through this before
										//check the last top condition again
										MissionInteriorCondition newTopCondition = GetTopCondition();
										if (newTopCondition != LastTopCondition) {
												LastTopCondition = newTopCondition;

												structure.State.AdditionalInteriorVariants.Clear();
												structure.State.AdditionalInteriorVariants.AddRange(LastTopCondition.StateVariable.InteriorVariants);

												structure.State.InteriorCharacters.Clear();
												structure.State.OwnerSpawn = LastTopCondition.StateVariable.OwnerSpawn;
												structure.State.InteriorCharacters.AddRange(LastTopCondition.StateVariable.AdditionalInteriorCharacters);
												//if we have a DIFFERENT condition than the last time
												//we want to wipe the slate clean
												//so add the interior to unload immediately
												Structures.AddInteriorToUnload(structure);
										}
								}
						}
						mUpdatingOverTime = false;
						yield break;
				}

				protected bool mUpdatingOverTime = false;
		}

		[Serializable]
		public class MissionInteriorControllerState
		{
				public List <MissionInteriorCondition> Conditions = new List <MissionInteriorCondition>();
				public MissionInteriorCondition Default = new MissionInteriorCondition();
				public bool OnlyRefreshOnVisible;
		}

		[Serializable]
		public class MissionInteriorCondition : MissionCondition <MissionInteriorStructureState>
		{

		}

		[Serializable]
		public class MissionInteriorStructureState
		{
				public List <int> InteriorVariants = new List <int>();
				public List <StructureSpawn> AdditionalInteriorCharacters = new List<StructureSpawn>();
				public StructureSpawn OwnerSpawn = new StructureSpawn();
		}
}