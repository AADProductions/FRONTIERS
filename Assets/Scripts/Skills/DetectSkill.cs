using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

namespace Frontiers.World.Gameplay
{
		public class DetectSkill : Skill
		{
				public static int gMaxDetectedItems = 100;

				public override bool ActionUse(double timeStamp)
				{
						return true;
				}

				public virtual bool Use(bool successfully)
				{
						if (IsInUse || !RequirementsMet) {
								return false;
						}

						StartCoroutine(DetectOverTime());
						return true;
				}

				public override bool Use(IItemOfInterest targetObject, int flavorIndex)
				{
						if (IsInUse || !RequirementsMet) {
								return false;
						}
						StartCoroutine(DetectOverTime());
						return true;
				}

				public override bool Use(int flavorIndex)
				{
						if (IsInUse || !RequirementsMet) {
								return false;
						}

						StartCoroutine(DetectOverTime());
						return true;
				}

				public IEnumerator DetectOverTime()
				{
						//get all the scripts that this affects
						List <WorldItem> detectedItems = new List <WorldItem>();
						//start in the chunk group
						var getAllChildrenByType = WIGroups.GetAllChildrenByType(
								                           GameWorld.Get.PrimaryChunk.ChunkGroup.Props.PathName,
								                           Usage.TargetWIScriptNames,
								                           detectedItems,
								                           Player.Local.Position,
								                           EffectRadius,
								                           gMaxDetectedItems);
						while (getAllChildrenByType.MoveNext()) {
								yield return getAllChildrenByType.Current;
						}

						//did we detect anything?
						if (detectedItems.Count > 0) {
								//tell these items to light up temporarily
								foreach (WorldItem detectedItem in detectedItems) {
										if (!string.IsNullOrEmpty(Usage.SendMessageArgument)) {
												detectedItem.SendMessage(Usage.SendMessageToTargetObject, Usage.SendMessageArgument, SendMessageOptions.DontRequireReceiver);
										} else {
												detectedItem.SendMessage(Usage.SendMessageToTargetObject, SendMessageOptions.DontRequireReceiver);
										}
										//spawn any vfx on detection
										if (!string.IsNullOrEmpty(Effects.FXOnSuccess)) {
												FXManager.Get.SpawnFX(detectedItem.transform.position, Effects.FXOnSuccess);
												if (!string.IsNullOrEmpty(Effects.FXOnSuccessMasteredBooster)) {
														//if we have a 'mastered' booster effect
														//spawn that if we've mastered
														FXManager.Get.SpawnFX(detectedItem.transform.position, Effects.FXOnSuccessMasteredBooster);
												}
										}
										yield return WorldClock.WaitForSeconds(1.0);//wait a bit between things
								}
								OnSuccess();
						}
						yield break;
				}
		}
}
