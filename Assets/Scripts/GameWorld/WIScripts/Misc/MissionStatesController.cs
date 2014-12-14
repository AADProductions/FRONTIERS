using UnityEngine;
using System.Collections;
using Frontiers.World.Gameplay;
using Frontiers.Story;
using System;
using System.Collections.Generic;
using Frontiers.Data;

namespace Frontiers.World
{
		public class MissionStatesController : WIScript
		{		//this is used to change an object's state based on mission state
				//kind of similiar to structure interior controller but less complex
				//uses a lot of enums for basic logic because i would edit it in the Unity editor a lot
				//probably better / more efficient ways to do this
				public MissionStatesControllerState State = new MissionStatesControllerState();

				public override void OnInitialized()
				{
						worlditem.OnVisible += OnVisible;
						worlditem.OnPlayerEncounter += OnPlayerEncounter;
						Player.Get.AvatarActions.Subscribe(AvatarAction.MissionUpdated, new ActionListener(Refresh));
						CheckConditions();
				}

				public bool Refresh(double timeStamp)
				{
						if (mFinished | mDestroyed) {
								return true;
						}

						CheckConditions();

						return true;
				}

				public void OnVisible()
				{
						CheckConditions();
				}

				public void OnPlayerEncounter()
				{
						CheckConditions();
				}

				public void CheckConditions()
				{
						if (mCheckingConditions) {
								return;
						}

						mCheckingConditions = true;
						MissionStatesCondition topCondition = null;
						int topConditionIndex = 0;
						if (MissionCondition.CheckConditions <MissionStatesCondition>(State.Conditions, out topConditionIndex)) {
								topCondition = State.Conditions[topConditionIndex];
								if (topCondition.RemoveFromGame) {
										worlditem.SetMode(WIMode.RemovedFromGame);
								} else {
										if (topCondition.ExistingState == worlditem.State) {
												worlditem.State = topCondition.StateVariable;
										}
								}
						}

						mCheckingConditions = false;
				}

				protected bool mCheckingConditions = false;
		}

		[Serializable]
		public class MissionStatesControllerState
		{
				public List <MissionStatesCondition> Conditions = new List <MissionStatesCondition>();
		}

		[Serializable]
		public class MissionCondition
		{
				public bool CheckObjective {
						get {
								return !string.IsNullOrEmpty(ObjectiveName);
						}
				}

				public bool CheckVariable {
						get {
								return !string.IsNullOrEmpty(MissionVariableName);
						}
				}

				public bool CheckSecondMission {
						get {
								return SecondMissionOperator != MissionStatusOperator.None;
						}
				}

				public bool CheckSecondObjective {
						get {
								return SecondObjectiveOperator != MissionStatusOperator.None;
						}
				}

				public int Priority = 0;

				public string Description {
						get {
								return MissionName + " is " + Status.ToString() + "...";
						}
				}

				public string MissionName;
				public MissionStatus Status;
				public string SecondMissionName;
				public MissionStatusOperator SecondMissionOperator = MissionStatusOperator.None;
				public MissionStatus SecondMissionStatus;
				public string ObjectiveName;
				public MissionStatus ObjectiveStatus;
				public string SecondObjectiveName;
				public MissionStatusOperator SecondObjectiveOperator = MissionStatusOperator.None;
				public MissionStatus SecondObjectiveStatus;
				public string MissionVariableName;
				public int MissionVariableValue = 0;
				public VariableCheckType CheckType = VariableCheckType.EqualTo;

				public static bool CheckConditions <T>(List <T> conditions, out int topCondition) where T : MissionCondition
				{
						topCondition = -1;
						int topPriority = -1;
						//check conditions from bottom to top
						for (int i = conditions.LastIndex(); i >= 0; i--) {
								MissionCondition condition = conditions[i];

								if (topPriority >= 0) {
										if (condition.Priority < topPriority) {
												//don't bother if this condition is outranked
												continue;
										}
								}

								if (CheckCondition(condition)) {
										topCondition = i;
								}
						}
						return topCondition >= 0;
				}

				public static bool CheckCondition(MissionCondition condition)
				{
						bool missionObtains = true;
						bool objectiveObtains = true;
						bool variableObtains = true;

						MissionStatus missionStatus = MissionStatus.Dormant;
						//always check mission state regardless
						if (Missions.Get.MissionStatusByName(condition.MissionName, ref missionStatus)) {
								missionObtains = Flags.Check((uint)missionStatus, (uint)condition.Status, Flags.CheckType.MatchAny);
								//only check second mission if the operator is set to something
								if (condition.CheckSecondMission) {
										bool secondMissionObtains = true;
										if (Missions.Get.MissionStatusByName(condition.SecondMissionName, ref missionStatus)) {
												secondMissionObtains = Flags.Check((uint)missionStatus, (uint)condition.SecondMissionStatus, Flags.CheckType.MatchAny);
										}
										//now combine mission obtains and second mission obtains by operation type
										switch (condition.SecondMissionOperator) {
												case MissionStatusOperator.And:
												default:
														missionObtains = missionObtains & secondMissionObtains;
														break;

												case MissionStatusOperator.Or:
														missionObtains = missionObtains | secondMissionObtains;
														break;

												case MissionStatusOperator.Not:
														missionObtains = missionObtains & (!secondMissionObtains);
														break;
										}
								}
						}
						//if we're still good after checking the missions, move forward
						if (missionObtains && condition.CheckObjective) {
								MissionStatus objectiveStatus = MissionStatus.Dormant;
								if (Missions.Get.ObjectiveStatusByName(condition.MissionName, condition.ObjectiveName, ref objectiveStatus)) {									
										objectiveObtains = Flags.Check((uint)objectiveStatus, (uint)condition.ObjectiveStatus, Flags.CheckType.MatchAny);
										//only check second objective if the operator is set to something
										if (condition.CheckSecondObjective) {
												bool secondObjectiveObtains = true;
												if (Missions.Get.ObjectiveStatusByName(condition.MissionName, condition.SecondObjectiveName, ref objectiveStatus)) {
														secondObjectiveObtains = Flags.Check((uint)objectiveStatus, (uint)condition.SecondObjectiveStatus, Flags.CheckType.MatchAny);
												}
												//now combine mission obtains and second mission obtains by operation type
												switch (condition.SecondObjectiveOperator) {
														case MissionStatusOperator.And:
														default:
																missionObtains = objectiveObtains & secondObjectiveObtains;
																break;

														case MissionStatusOperator.Or:
																missionObtains = objectiveObtains | secondObjectiveObtains;
																break;

														case MissionStatusOperator.Not:
																missionObtains = objectiveObtains & (!secondObjectiveObtains);
																break;
												}
										}
								}
						}

						return missionObtains & objectiveObtains & variableObtains;
				}
		}

		[Serializable]
		public class MissionCondition <T> : MissionCondition
		{
				public T StateVariable;
				public bool Visible = false;
				//for editor
		}

		[Serializable]
		public class MissionStatesCondition : MissionCondition <string>
		{
				public bool CheckExistingState {
						get {
								return !string.IsNullOrEmpty(ExistingState);
						}
				}

				public string ExistingState;
				public bool RemoveFromGame = false;
		}

		public enum MissionStatusOperator
		{
				None,
				And,
				Or,
				Not,
		}
}
