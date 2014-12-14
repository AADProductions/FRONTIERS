using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Frontiers.World
{
		public class BanditCamp : WIScript
		{
				//bandit camps are a simpler version of creature dens
				//bandits are characters so they manage more of their own AI
				//no need for den radius or anything like that
				public string SpeechOnVisit = string.Empty;
				public List <Character> SpawnedBandits = new List <Character>();
				public LookerBubble SharedLooker = null;
				public BanditCampState State = new BanditCampState();

				public void AddBandit(Character bandit)
				{
						SpawnedBandits.SafeAdd(bandit);
				}

				public override void OnInitialized()
				{
						worlditem.OnVisible += OnVisible;
						worlditem.OnInvisible += OnInvisible;

						Visitable visitable = worlditem.Get <Visitable>();
						visitable.OnPlayerVisit += OnPlayerVisit;
						visitable.OnPlayerLeave += OnPlayerLeave;
				}

				public void OnVisible()
				{
						enabled = true;
						mLeavingCamp = false;
						mVisitingCamp = false;
				}

				public void OnInvisible()
				{
						enabled = false;
						mLeavingCamp = false;
						mVisitingCamp = false;
				}

				public void OnPlayerVisit()
				{
						Debug.Log("Visiting camp");
						if (!mVisitingCamp) {
								//this will enable the script
								mLeavingCamp = false;
								mVisitingCamp = true;
								StartCoroutine(OnPlayerVisitCamp());
						}
				}

				public void OnPlayerLeave()
				{
						if (!mLeavingCamp) {
								//this will disable the script
								mVisitingCamp = false;
								mLeavingCamp = true;
								StartCoroutine(OnPlayerLeaveCamp());
						}
				}

				public static float GetInnerRadius(float radius)
				{
						return radius * 0.25f;
				}

				public void FixedUpdate()
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
												CreateSharedLooker();
										}
										if (!SharedLooker.IsInUse) {
												//if the looker is disabled that means it's done being used
												mUpdateBanditIndex = SpawnedBandits.NextIndex(mUpdateBanditIndex);
												if (SpawnedBandits[mUpdateBanditIndex] != null) {
														looker = SpawnedBandits[mUpdateBanditIndex].worlditem.GetOrAdd <Looker>();
														//listener is passive but looker is active
														//it needs to be told to look for the player
														//we stagger this because it's an expensive operation
														looker.LookForStuff(SharedLooker);
												}
										}
								}
						}
				}

				public void CreateSharedLooker()
				{
						GameObject sharedLookerObject = gameObject.FindOrCreateChild("SharedLooker").gameObject;
						SharedLooker = sharedLookerObject.GetOrAdd <LookerBubble>();
						SharedLooker.FinishUsing();
				}

				protected IEnumerator OnPlayerVisitCamp()
				{
						while (mVisitingCamp) {
								yield return null;
								enabled = true;
						}
						yield break;
				}

				protected IEnumerator OnPlayerLeaveCamp()
				{
						for (int i = 0; i < SpawnedBandits.Count; i++) {
								Hostile hostile = null;
								if (SpawnedBandits[i].worlditem.Is <Hostile>(out hostile)) {
										hostile.Finish();
								}
								yield return null;
						}
						enabled = false;
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
				public string TemplateName = "Bandit";
				public string WarningSpeech;
				public string AttackSpeech;
				public string TauntSpeech;
		}
}