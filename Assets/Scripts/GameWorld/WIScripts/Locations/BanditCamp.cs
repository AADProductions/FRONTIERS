using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.Story;

namespace Frontiers.World.WIScripts
{
	public class BanditCamp : WIScript, ITerritoryBase
	{
		//bandit camps are a simpler version of creature dens
		//bandits are characters so they manage more of their own AI
		//no need for den radius or anything like that
		public string SpeechOnVisit = string.Empty;
		public List <Character> SpawnedBandits = new List <Character> ();
		public LookerBubble SharedLooker = null;
		public bool HasWarnedPlayerRecently = false;
		public bool HasAttackedPlayerRecently = false;
		public bool HasTauntedPlayerRecently = false;

		public BanditCampState State = new BanditCampState ();

		#region ITerritoryBase implementation
		public float Radius { get { return worlditem.ActiveRadius; } }
		public float InnerRadius { get { return worlditem.ActiveRadius / 4f; } }
		public Vector3 Position { get { return worlditem.Position; } }
		public Transform transform { get { return worlditem.tr; } }
		#endregion

		public bool PlayerVisitingCamp {
			get {
				return mVisitingCamp;
			}
		}

		public int NumBanditsOnVisit;
		public int NumBanditsOnLeave;

		public void AddBandit (Character bandit)
		{
			Bandit b = bandit.worlditem.GetOrAdd <Bandit> ();
			b.ParentCamp = this;
			bandit.TerritoryBase = this;
			SpawnedBandits.SafeAdd (bandit);
		}

		public override void OnInitialized ()
		{
			worlditem.OnVisible += OnVisible;
			worlditem.OnInvisible += OnInvisible;

			Visitable visitable = worlditem.Get <Visitable> ();
			visitable.OnPlayerVisit += OnPlayerVisit;
			visitable.OnPlayerLeave += OnPlayerLeave;

			State.SpeechWarning = string.IsNullOrEmpty (State.SpeechWarning) ? Globals.BanditCampDefaultWarningSpeech : State.SpeechWarning;
			State.SpeechAttack = string.IsNullOrEmpty (State.SpeechAttack) ? Globals.BanditCampDefaultAttackSpeech : State.SpeechAttack;
			State.SpeechTaunt = string.IsNullOrEmpty (State.SpeechTaunt) ? Globals.BanditCampDefaultTauntSpeech : State.SpeechTaunt;
			State.SpeechFlee = string.IsNullOrEmpty (State.SpeechFlee) ? Globals.BanditCampDefaultFleeSpeech : State.SpeechFlee;
		}

		public void OnVisible ()
		{
			enabled = true;
			mLeavingCamp = false;
			mVisitingCamp = false;
		}

		public void OnInvisible ()
		{
			enabled = false;
			mLeavingCamp = false;
			mVisitingCamp = false;
			HasWarnedPlayerRecently = false;
			HasAttackedPlayerRecently = false;
			HasTauntedPlayerRecently = false;
		}

		public void OnPlayerVisit ()
		{
			for (int i = SpawnedBandits.LastIndex (); i >= 0; i--) {
				if (SpawnedBandits == null || SpawnedBandits [i].IsDead) {
					SpawnedBandits.RemoveAt (i);
				}
			}

			Debug.Log ("Visiting camp");

			NumBanditsOnVisit = SpawnedBandits.Count;
			if (NumBanditsOnVisit == 0) {
				return;
			}

			if (!mVisitingCamp) {
				//this will enable the script
				mLeavingCamp = false;
				mVisitingCamp = true;
				StartCoroutine (OnPlayerVisitCamp ());
			}
		}

		public void OnPlayerLeave ()
		{
			Debug.Log ("Leaving camp");

			for (int i = SpawnedBandits.LastIndex (); i >= 0; i--) {
				if (SpawnedBandits == null || SpawnedBandits [i].IsDead) {
					SpawnedBandits.RemoveAt (i);
				}
			}

			NumBanditsOnLeave = SpawnedBandits.Count;
			if (NumBanditsOnLeave == 0) {
				return;
			}

			if (!mLeavingCamp) {
				//this will disable the script
				mVisitingCamp = false;
				mLeavingCamp = true;
				StartCoroutine (OnPlayerLeaveCamp ());
			}
		}

		public static float GetInnerRadius (float radius)
		{
			return radius * 0.25f;
		}

		public void FixedUpdate ()
		{
			if (SpawnedBandits.Count == 0) {
				enabled = false;
			}

			mLookerCounter++;
			if (mLookerCounter > 2) {
				mLookerCounter = 0;
				if (SpawnedBandits.Count > 0) {
					Looker looker = null;
					if (SharedLooker == null) {
						CreateSharedLooker ();
					}
					if (!SharedLooker.IsInUse) {
						//if the looker is disabled that means it's done being used
						mUpdateBanditIndex = SpawnedBandits.NextIndex (mUpdateBanditIndex);
						if (SpawnedBandits [mUpdateBanditIndex] != null) {
							looker = SpawnedBandits [mUpdateBanditIndex].worlditem.GetOrAdd <Looker> ();
							//listener is passive but looker is active
							//it needs to be told to look for the player
							//we stagger this because it's an expensive operation
							looker.LookForStuff (SharedLooker);
						}
					}
				}
			}
		}

		public void CreateSharedLooker ()
		{
			GameObject sharedLookerObject = gameObject.FindOrCreateChild ("SharedLooker").gameObject;
			SharedLooker = sharedLookerObject.GetOrAdd <LookerBubble> ();
			SharedLooker.FinishUsing ();
		}

		protected IEnumerator OnPlayerVisitCamp ()
		{
			//get the bandits to warn the player
			while (mVisitingCamp) {
				yield return null;
				enabled = true;
			}
			yield break;
		}

		protected IEnumerator OnPlayerLeaveCamp ()
		{
			for (int i = 0; i < SpawnedBandits.Count; i++) {
				Hostile hostile = null;
				if (SpawnedBandits [i].worlditem.Is <Hostile> (out hostile)) {
					hostile.Finish ();
				}
				yield return null;
			}
			yield break;
		}

		protected bool mLeavingCamp = false;
		protected bool mVisitingCamp = false;
		protected int mUpdateBanditIndex = -1;
		protected int mLookerCounter = 0;
		protected int mDenCounter = 0;
		protected int mPlayerCounter = 0;
	}

	[Serializable]
	public class BanditCampState
	{
		public float PlayerFearLevel = 0f;
		public string TemplateName = "Bandit";
		public string SpeechWarning;
		public string SpeechAttack;
		public string SpeechTaunt;
		public string SpeechFlee;
	}
}