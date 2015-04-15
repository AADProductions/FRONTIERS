using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Frontiers;
using Frontiers.World.Gameplay;

namespace Frontiers.World.WIScripts
{
	public class FamilyAlbertConfrontation : WIScript {
		public static FamilyAlbertConfrontation AlbertsGuards;

		public List <ActionNode> GuardSpawnNodes = new List <ActionNode> ( );
		public List <Character> SpawnedGuards = new List <Character> ( );
		public Character SpawnedAlbert;
		public ActionNode AlbertSpawnNode;
		public Spline GuardPath;
		public bool HasCaughtPlayer = false;
		public bool HasConfrontedPlayer = false;
		public RVOTargetHolder GuardsFollowTarget;

		public FamilyAlbertConfrontationState State = new FamilyAlbertConfrontationState ( );

		public override bool EnableAutomatically {
			get {
				return true;
			}
		}

		public override void OnInitialized ()
		{
			AlbertsGuards = this;

			Transform splineNodeTransform = gameObject.CreateChild ("GuardPath");
			GuardPath = splineNodeTransform.gameObject.AddComponent <Spline> ();
			GuardPath.updateMode = Spline.UpdateMode.DontUpdate;
			GuardPath.autoClose = false;
			GuardPath.normalMode = Spline.NormalMode.UseGlobalSplineNormal;
			GuardPath.interpolationMode = Spline.InterpolationMode.Hermite;

			for (int i = 0; i < State.GuardPathNodes.Count; i++) {
				GameObject sn = GuardPath.AddSplineNode ();
				sn.transform.parent = splineNodeTransform;
				sn.transform.localPosition = State.GuardPathNodes [i];
			}

			GuardPath.UpdateSpline ();

			//latch on to the visitable for the parent location
			worlditem.OnActive += OnActive;
			worlditem.OnVisible += OnInvisible;
		}

		public void OnInvisible ( ) {
			enabled = false;
		}

		public void ActivateGuards ( ) {
			if (!mChasingPlayer) {
				mChasingPlayer = true;
				StartCoroutine (ChasePlayerOverTime ());
			}
			Debug.Log ("Activating guards");
		}

		public void OnActive ( ) {
			//check to see if it's time to spawn Albert
			if (mHasSpawnedGuards) {
				enabled = true;
				return;
			}

			MissionStatus status = MissionStatus.Dormant;
			if (Missions.Get.ObjectiveStatusByName ("Family", "GetBackIntoLab", ref status)) {
				if (Flags.Check ((uint)MissionStatus.Active, (uint)status, Flags.CheckType.MatchAny)) {
					Debug.Log ("Time to spawn guards");
					SpawnGuards ();
				}
			} else {
				Debug.Log ("Hasn't spawned guards yet because family / back into lab is " + status.ToString ());
			}
		}

		public void SpawnGuards ( ) {

			//get the spawn guard nodes
			GuardSpawnNodes.Clear ();
			WorldChunk chunk = worlditem.Group.GetParentChunk ();
			ActionNodeState nodeState = null;
			for (int i = 0; i < State.GuardSpawnNodeNames.Count; i++) {
				if (chunk.GetNode (State.GuardSpawnNodeNames [i], false, out nodeState)) {
					GuardSpawnNodes.Add (nodeState.actionNode);
				}
			}
			if (chunk.GetNode (State.AlbertSpawnNodeName, false, out nodeState)) {
				AlbertSpawnNode = nodeState.actionNode;
			}

			for (int i = 0; i < GuardSpawnNodes.Count; i++) {
				Character newGuard = null;
				if (Characters.SpawnCharacter (GuardSpawnNodes [i], State.GuardTemplateName, State.GuardFlags, worlditem.Group, out newGuard)) {
					//something something, tell them to follow
					SpawnedGuards.Add (newGuard);
				}
			}

			Characters.GetOrSpawnCharacter (AlbertSpawnNode, "Albert", worlditem.Group, out SpawnedAlbert);

			mHasSpawnedGuards = true;
			enabled = true;
		}

		public void Update ( ) {

			if (SpawnedAlbert == null) {
				return;
			}

			if (!HasConfrontedPlayer) {
				if (Vector3.Distance (Player.Local.Position, SpawnedAlbert.worlditem.Position) < State.ConfrontationDistance) {
					SpawnedAlbert.worlditem.Get <Talkative> ().ForceConversation ();
					HasConfrontedPlayer = true;
				}
			}
		}

		protected IEnumerator ChasePlayerOverTime ( ) {

			Transform targetHolderTransform = gameObject.CreateChild ("TargetHolder");
			GuardsFollowTarget = targetHolderTransform.gameObject.AddComponent <RVOTargetHolder> ();
			for (int i = 0; i < SpawnedGuards.Count; i++) {
				MotileAction chasePlayerAction = new MotileAction ();
				chasePlayerAction.Type = MotileActionType.FollowTargetHolder;
				chasePlayerAction.LiveTargetHolder = GuardsFollowTarget;
				chasePlayerAction.FollowType = MotileFollowType.Follower;
				chasePlayerAction.Expiration = MotileExpiration.Never;
				chasePlayerAction.YieldBehavior = MotileYieldBehavior.DoNotYield;
				SpawnedGuards [i].worlditem.Get <Motile> ().PushMotileAction (chasePlayerAction, MotileActionPriority.ForceTop);
			}

			while (!HasCaughtPlayer) {
				State.PursuitDistanceSoFar = GuardPath.GetClosestPointParam (Player.Local.Position, 1, 0f, 1f, 0.01f);
				targetHolderTransform.position = GuardPath.GetPositionOnSpline (State.PursuitDistanceSoFar);
				State.PursuitDistanceSoFar += (float)(WorldClock.ARTDeltaTime * 0.005f);
				yield return null;
			}
			mChasingPlayer = false;
			yield break;
		}

		protected bool mChasingPlayer = false;
		protected bool mHasSpawnedGuards = false;

		#if UNITY_EDITOR
		public override void OnEditorRefresh ()
		{
			State.GuardPathNodes.Clear ();
			foreach (SplineNode sn in GuardPath.splineNodesArray) {
				State.GuardPathNodes.Add (sn.transform.localPosition);
			}

			State.AlbertSpawnNodeName = AlbertSpawnNode.State.Name;
			State.GuardSpawnNodeNames.Clear ();
			for (int i = 0; i < GuardSpawnNodes.Count; i++) {
				State.GuardSpawnNodeNames.Add (GuardSpawnNodes [i].State.Name);
			}
		}
		#endif
	}

	[Serializable]
	public class FamilyAlbertConfrontationState {
		public List <string> GuardSpawnNodeNames = new List <string> ();
		public string AlbertSpawnNodeName;
		public List <SVector3> GuardPathNodes = new List <SVector3> ( );
		public string GuardTemplateName = "Guard";
		public CharacterFlags GuardFlags = new CharacterFlags ( );
		public float ConfrontationDistance = 3f;
		public float PursuitDistanceSoFar = 0f;
	}
}