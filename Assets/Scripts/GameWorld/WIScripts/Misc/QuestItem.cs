using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Frontiers;

namespace Frontiers.World
{
		public class QuestItem : WIScript
		{
				public static List <string> RecentlyDestroyedQuestItems = new List <string>();
				public QuestItemState State = new QuestItemState();

				public override bool CanBeDropped {
						get {
								return State.CanBeDropped;
						}
				}

				public override bool CanBePlacedOn(IItemOfInterest targetObject, Vector3 point, Vector3 normal, ref string errorMessage)
				{
						return State.CanBePlaced && base.CanBePlacedOn(targetObject, point, normal, ref errorMessage);
				}

				public override void OnInitialized()
				{
						//set quest name so it's treated as a quest item
						worlditem.Props.Name.QuestName = State.QuestName;
						worlditem.Props.Name.DisplayName = State.DisplayName;
						Missions.Get.AddQuestItem(worlditem);

						Damageable damageable = null;
						if (worlditem.Is <Damageable>(out damageable)) {
								damageable.OnDie += OnDie;
						}

						worlditem.OnStateChange += OnStateChange;
						worlditem.OnAddedToGroup += OnAddedToGroup;
						worlditem.OnAddedToPlayerInventory += OnAddedToPlayerInventory;
						GameWorld.Get.ActiveQuestItems.SafeAdd(worlditem);
				}

				public override void OnInitializedFirstTime()
				{
						if (State.LockVisibility) {
								State.VisibleNow = State.VisibleOnStartup;
						}
				}

				public void SetQuestItemVisibility(bool visibleNow)
				{
						if (State.LockVisibility) {
								State.VisibleNow = visibleNow;
								worlditem.ActiveStateLocked = false;
								if (visibleNow) {
										worlditem.ActiveState = WIActiveState.Active;
								} else {
										worlditem.ActiveState = WIActiveState.Invisible;
								}
								worlditem.ActiveStateLocked = true;
						}
				}

				public void OnAddedToPlayerInventory()
				{
						if (State.LockVisibility && State.UnlockVisibilityOnAddedToPlayerInventory) {
								State.LockVisibility = false;
						}
				}

				public override bool AutoIncrementFileName {
						get {
								return false;
						}
				}

				public override void OnStateChange()
				{
						Player.Get.AvatarActions.ReceiveAction(AvatarAction.ItemQuestItemSetState, WorldClock.Time);
				}

				public void OnAddedToGroup()
				{
						SetQuestItemVisibility(State.VisibleNow);
				}

				public override string GenerateUniqueFileName(int increment)
				{
						return State.QuestName;
				}

				public override string DisplayNamer(int increment)
				{
						if (!string.IsNullOrEmpty(State.DisplayName)) {
								return WorldItems.CleanWorldItemName(State.DisplayName);
						}
						return worlditem.Props.Name.DisplayName;
				}

				public override string StackNamer(int increment)
				{
						return worlditem.Props.Name.FileName;
				}
				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						WorldItem worlditem = gameObject.GetComponent <WorldItem>();
						worlditem.Props.Name.QuestName = State.QuestName;
						worlditem.Props.Name.FileName = State.QuestName;
						worlditem.Props.Name.StackName = State.QuestName;
				}
				#endif
				public void OnDie()
				{
						GameWorld.Get.State.DestroyedQuestItems.Add(State.QuestName);
						Player.Get.AvatarActions.ReceiveAction(new PlayerAvatarAction(AvatarAction.ItemQuestItemDie), WorldClock.Time);
				}

				public override void OnEnable()
				{
						base.OnEnable();
						//hijack all naming scripts
						worlditem.Props.Local.DisplayNamerScript = "QuestItem";
						worlditem.Props.Local.StackNamerScript = "QuestItem";
				}
		}

		[Serializable]
		public class QuestItemState
		{
				public string QuestName = "Quest Item";
				public string DisplayName = "Item";
				public bool LockVisibility = false;
				public bool VisibleOnStartup = true;
				public bool VisibleNow = true;
				public bool UnlockVisibilityOnAddedToPlayerInventory = true;
				public bool CanBeDropped = false;
				public bool CanBePlaced = true;
		}
}