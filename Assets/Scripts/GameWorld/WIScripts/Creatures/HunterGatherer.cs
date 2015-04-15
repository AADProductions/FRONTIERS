using UnityEngine;
using System.Collections;
using Frontiers;
using Frontiers.World.Gameplay;


namespace Frontiers.World.WIScripts
{
	//uses looker to search for items of interest
	//when it sees one, it uses motile to hunt it
	//if it's alive it uses hostile to kill it
	//then it gathers it and/or adds it to its container
	//this script is stateless by design
	public class HunterGatherer : WIScript
	{
		public HunterMode Mode = HunterMode.Looking;
		public BehaviorTOD TimeType = BehaviorTOD.Nocturnal;
		public GatherMode CurrentGatherMode = GatherMode.KillHostile;
		public bool RoutineActive = false;
		public GameObject WanderBase;
		public float WanderRange;
		public float GatherRange = 5.0f;

//		public override void OnInitialized ()
//		{
//			StartCoroutine (HuntAndGatherOverTime ());
//		}
//
//		protected IEnumerator HuntAndGatherOverTime ()
//		{
//			Motile motile = null;
//			worlditem.Is<Motile> (out motile);
//			MotileAction wanderAction = new MotileAction ();
//			wanderAction.Range = 10f;
//			wanderAction.Type = MotileActionType.WanderIdly;
//			wanderAction.Expiration = MotileExpiration.Never;
//			wanderAction.YieldBehavior = MotileYieldBehavior.YieldAndFinish;
//			wanderAction.LiveTarget = WanderBase;//this will most likely be the animal den
////			wanderAction.Range = WanderRange;//this will most likely be the animal den radius
//			motile.PushMotileAction (wanderAction, MotileActionPriority.ForceTop);
//			//wait a tick to give other scripts time
//			yield return new WaitForSeconds (0.1f);
//
//			while (!GameManager.Is (FGameState.InGame)) {
//				yield return null;
//			}
//
//			while (!worlditem.Is (WIMode.RemovedFromGame)) {
////				if (!RoutineActive) {
////					//if we haven't started our routine
////					if (!BehaviorTODActive (TimeType)) {
////						//wait for our routine TOD to kick in
////						yield return new WaitForSeconds (UnityEngine.Random.Range (5f, 10f));//we can wait a long time
////					} else {
////						//otherwise active our routine
////						yield return StartCoroutine (ActivateRoutine ());
////					}
////				} else {
//
////				Looker looker = worlditem.GetOrAdd <Looker> ();
//
//
////				switch (Mode) {
////				case HunterMode.Looking:
////				default:
////					////Debug.Log ("HUNTERGATHERER: Looking");
////						//if the looker hasn't spotted a goal
////						//and it isn't looking for a goal
////						//tell it to look
////					if (!looker.HasNearestGoal) {
////						Mode = HunterMode.Looking;
////						//start a wander around motile action to get ourselves moving
////						Motile motile = null;
////						if (worlditem.Is<Motile> (out motile)) {
////							MotileAction wanderAction = new MotileAction ();
////							wanderAction.Type = MotileActionType.WanderIdly;
////							wanderAction.Expiration = MotileExpiration.Never;
////							wanderAction.YieldBehavior = MotileYieldBehavior.YieldAndFinish;
////							wanderAction.LiveTarget = WanderBase;//this will most likely be the animal den
////							wanderAction.Range = WanderRange;//this will most likely be the animal den radius
////						}
////
////						while (!looker.HasNearestGoal) {
////							yield return new WaitForSeconds (0.15f);
////						}
////					} else {
////						////Debug.Log ("HUNTERGATHERER: Found a thing, setting to hunting!");
////						Mode = HunterMode.Hunting;
////
////						//hooray we have a goal
////						//now it's time to hunt the goal!
////						//what do we do with the object? first see what it is
////
////						Hostile hostile = null;
////
////						switch (looker.NearestGoalType) {
////						case Looker.GoalType.Player:
////								//if we're hunting and gathering the player, then we're hostle
////							CurrentGatherMode = GatherMode.KillHostile;
////							hostile = worlditem.GetOrAdd <Hostile> ();
////							hostile.PrimaryTarget = Player.Local.transform;
////							break;
////
////						case Looker.GoalType.WorldItem:
////							WorldItem wiGoal = looker.NearestGoal.GetComponent <WorldItem> ();
////							if (wiGoal.Is<Creature> () || wiGoal.Is<Character> ()) {
////								//if it's a living thing, we gather it by killing it
////								CurrentGatherMode = GatherMode.KillHostile;
////								hostile = worlditem.GetOrAdd <Hostile> ();
////								hostile.PrimaryTarget = wiGoal.transform;
////							} else {
////								//otherwise we gather it by just gathering it
////								CurrentGatherMode = GatherMode.AddToStackContainer;
////								//if it's hostile it'll add its own motile action
////								//but for resource gathering we'll have to add one ourselves
////								Motile motile = null;
////								if (worlditem.Is<Motile> (out motile)) {
////									MotileAction gotoAction = new MotileAction ();
////									gotoAction.Type = MotileActionType.FollowGoal;
////									gotoAction.Expiration = MotileExpiration.TargetInRange;
////									gotoAction.Range = 1.0f;
////									gotoAction.LiveTarget = wiGoal.gameObject;
////									gotoAction.YieldBehavior = MotileYieldBehavior.YieldAndWait;
////
////									motile.PushMotileAction (gotoAction, MotileActionPriority.Normal);
////								}
////							}
////							break;
////
////						case Looker.GoalType.ActionNode:
////								//handle this by going to action node
////							break;
////
////						default:
////								//don't really know what to do with this one
////							Mode = HunterMode.Looking;//TEMP
////							break;
////						}
////					}
////					break;
////
////				case HunterMode.Hunting:
////						////Debug.Log ("HUNTERGATHERER: Hunting");
////						//if we're no longer aware of our goal
////					if (!looker.IsAwareOfGoal) {
////						//aw crap better go back to looking
////						Mode = HunterMode.Looking;
////					} else {
////						//otherwise keep hunting it
////						//have we killed it yet?
////						if (CurrentGatherMode == GatherMode.KillHostile) {
////							//check if we've killed hostile
////						} else {
////							//check if we're focusing on hostile
////							Motile motile = worlditem.Get<Motile> ();
////							MotileAction topAction = motile.TopAction;
////							if (topAction.Type == MotileActionType.FocusOnTarget
////							     &&	topAction.LiveTarget == looker.NearestGoal) {
////								//we've reached the target!
////							}
////						}
////					}
////					break;
////
////				case HunterMode.Gathering:
////						////Debug.Log ("HUNTERGATHERER: Gathering");
////					Mode = HunterMode.Looking;
////					yield return new WaitForSeconds (5.0f);
////					break;
////				}
//
//				//at the very end of this cycle
////					if (!BehaviorTODActive (TimeType)) {
////						yield return StartCoroutine (DeactivateRoutine ());
////					}
////				}
//				yield return new WaitForSeconds (1.0f);//we can wait a long time
//			}
//		}
//		//kicks off the routine
//		protected IEnumerator ActivateRoutine ()
//		{
//			RoutineActive = true;
//			yield break;
//		}
//		//goes home to den for our off hours
//		protected IEnumerator DeactivateRoutine ()
//		{
//			RoutineActive = false;
//			yield break;
//		}
//
//
////		public static bool BehaviorTODActive (BehaviorTOD type)
////		{
////			bool active = false;
////			switch (type) {
////			case BehaviorTOD.Nocturnal:
////				active = !Biomes.Get.IsDaytime;
////				break;
////
////			case BehaviorTOD.Diurnal:
////				active = Biomes.Get.IsDaytime;
////				break;
////
////			default:
////				break;
////			}
////			return active;
////		}
	}
}