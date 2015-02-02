using UnityEngine;
using System.Collections;
using System;

namespace Frontiers.World
{
		public class TriggerUnderground : WorldTrigger
		{
				public TriggerUndergroundState State = new TriggerUndergroundState();
				public Collider InnerTriggerCollider;
				public Collider OuterTriggerCollider;
				public PassThroughTriggerPair InnerTrigger;
				public PassThroughTriggerPair OuterTrigger;
				public PassThroughTrigger ParentTrigger;

				public override void OnInitialized()
				{
						GameObject innerTriggerColliderGameObject = gameObject.FindOrCreateChild("InnerTrigger").gameObject;
						innerTriggerColliderGameObject.layer = Globals.LayerNumTrigger;
						InnerTriggerCollider = innerTriggerColliderGameObject.GetOrAdd <BoxCollider>();
						InnerTriggerCollider.isTrigger = true;
						State.InnerTrigger.ApplyTo(InnerTriggerCollider.transform, true);
						InnerTrigger = innerTriggerColliderGameObject.GetOrAdd <PassThroughTriggerPair>();
						InnerTrigger.TriggerType = PassThroughTriggerType.Inner;

						GameObject outerTriggerColliderGameObject = gameObject.FindOrCreateChild("OuterTrigger").gameObject;
						outerTriggerColliderGameObject.layer = Globals.LayerNumTrigger;
						OuterTriggerCollider = outerTriggerColliderGameObject.GetOrAdd <BoxCollider>();
						OuterTriggerCollider.isTrigger = true;
						State.OuterTrigger.ApplyTo(OuterTriggerCollider.transform, true);
						OuterTrigger = outerTriggerColliderGameObject.GetOrAdd <PassThroughTriggerPair>();
						OuterTrigger.TriggerType = PassThroughTriggerType.Outer;

						//InnerTrigger.Sibling = OuterTrigger;
						//OuterTrigger.Sibling = InnerTrigger;
						//InnerTrigger.TargetObject = gameObject;
						//OuterTrigger.TargetObject = gameObject;

						ParentTrigger = gameObject.GetOrAdd <PassThroughTrigger>();
						ParentTrigger.TargetObject = gameObject;
						ParentTrigger.OuterTrigger = OuterTrigger;
						ParentTrigger.InnerTrigger = InnerTrigger;
						ParentTrigger.PassThroughFunctionName = "OnPassThroughTrigger";

						/*
						InnerTrigger.EnterFunctionName = "OnPassThroughTrigger";
						InnerTrigger.ExitFunctionName = "OnPassThroughTrigger";
						OuterTrigger.EnterFunctionName = "OnPassThroughTrigger";
						OuterTrigger.ExitFunctionName = "OnPassThroughTrigger";
						*/
				}
				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						GameObject innerTriggerColliderGameObject = gameObject.FindOrCreateChild("InnerTrigger").gameObject;
						innerTriggerColliderGameObject.layer = Globals.LayerNumTrigger;
						InnerTriggerCollider = innerTriggerColliderGameObject.GetOrAdd <BoxCollider>();
						InnerTriggerCollider.isTrigger = true;
						State.InnerTrigger.CopyFrom(InnerTriggerCollider.transform);
						InnerTrigger = innerTriggerColliderGameObject.GetOrAdd <PassThroughTriggerPair>();
						InnerTrigger.TriggerType = PassThroughTriggerType.Inner;

						GameObject outerTriggerColliderGameObject = gameObject.FindOrCreateChild("OuterTrigger").gameObject;
						outerTriggerColliderGameObject.layer = Globals.LayerNumTrigger;
						OuterTriggerCollider = outerTriggerColliderGameObject.GetOrAdd <BoxCollider>();
						OuterTriggerCollider.isTrigger = true;
						State.OuterTrigger.CopyFrom(OuterTriggerCollider.transform);
						OuterTrigger = outerTriggerColliderGameObject.GetOrAdd <PassThroughTriggerPair>();
						OuterTrigger.TriggerType = PassThroughTriggerType.Outer;

						//InnerTrigger.Sibling = OuterTrigger;
						//OuterTrigger.Sibling = InnerTrigger;
						//InnerTrigger.TargetObject = gameObject;
						//OuterTrigger.TargetObject = gameObject;

						/*
						InnerTrigger.EnterFunctionName = "OnPassThroughTrigger";
						InnerTrigger.ExitFunctionName = "OnPassThroughTrigger";
						OuterTrigger.EnterFunctionName = "OnPassThroughTrigger";
						OuterTrigger.ExitFunctionName = "OnPassThroughTrigger";
						*/

						ParentTrigger = gameObject.GetOrAdd <PassThroughTrigger>();
						ParentTrigger.TargetObject = gameObject;
						ParentTrigger.OuterTrigger = OuterTrigger;
						ParentTrigger.InnerTrigger = InnerTrigger;
						ParentTrigger.PassThroughFunctionName = "OnPassThroughTrigger";

						CrucialColliderGizmo ccgInner = InnerTriggerCollider.gameObject.GetOrAdd <CrucialColliderGizmo>();
						CrucialColliderGizmo ccgOuter = OuterTriggerCollider.gameObject.GetOrAdd <CrucialColliderGizmo>();
						ccgInner.fillColor = Colors.Alpha(Color.red, 0.25f);
						ccgOuter.fillColor = Colors.Alpha(Color.cyan, 0.25f);

						//there are a few things we can assume about this trigger
						State.Targets = WorldTriggerTarget.None;
						State.Behavior = AvailabilityBehavior.Max;
						State.NumTimesTriggered = 1;
						State.ColliderType = WIColliderType.None;//we make our own
				}
				#endif
				public void OnPassThroughTrigger()
				{
						if (Player.Local.Surroundings.IsUnderground) {
								GameWorld.Get.ShowAboveGround(true);
								Player.Local.Surroundings.ExitUnderground();
						} else {
								Player.Local.Surroundings.EnterUnderground();
								GameWorld.Get.ShowAboveGround(false);
						}
				}
		}

		[Serializable]
		public class TriggerUndergroundState : WorldTriggerState
		{
				public STransform InnerTrigger = new STransform();
				public STransform OuterTrigger = new STransform();
		}
}
