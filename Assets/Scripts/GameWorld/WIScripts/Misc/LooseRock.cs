using UnityEngine;
using System.Collections;
using Frontiers;
using System;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class LooseRock : WIScript
		{
				public Transform MovableRock;
				public Vector3 MovedPosition;
				public LooseRockState State = new LooseRockState();
				public Collider HiddenCollider;
				public float MoveSpeed = 5f;

				public override void PopulateOptionsList(List<WIListOption> options, List<string> message)
				{
						if (State.Activated && !State.Moved) {
								options.Add(new WIListOption("Move Rock", "Move"));
						}
				}

				public void OnPlayerUseWorldItemSecondary(object secondaryResult)
				{
						WIListResult dialogResult = secondaryResult as WIListResult;

						switch (dialogResult.SecondaryResult) {
								case "Move":
										State.Moved = true;
										if (!mMovingOverTime) {
												mMovingOverTime = true;
												StartCoroutine(MoveOverTime());
										}
										break;

								default:
										break;
						}
				}

				public override void OnInitialized()
				{
						if (State.Activated) {
								gameObject.tag = Globals.TagGroundStone;
								if (State.Moved) {
										MovableRock.localPosition = MovedPosition;
								} else {
										MovableRock.localPosition = Vector3.zero;
								}
						} else {
								BookStatus status = BookStatus.None;
								if (Books.GetBookStatus(State.BookName, out status) && Flags.Check((int)status, (int)BookStatus.Read, Flags.CheckType.MatchAny)) {
										State.Activated = true;
										gameObject.tag = Globals.TagGroundStone;
								} else {
										gameObject.tag = Globals.TagNonInteractive;
										Player.Get.AvatarActions.Subscribe(AvatarAction.BookRead, new ActionListener(BookRead));
								}
						}

						worlditem.OnPlayerUse += OnPlayerUse;
				}

				public override int OnRefreshHud(int lastHudPriority)
				{
						if (State.Activated && !State.Moved) {
								lastHudPriority++;
								GUI.GUIHud.Get.ShowAction(worlditem, UserActionType.ItemUse, "Move", worlditem.HudTarget, GameManager.Get.GameCamera);
						}
						return lastHudPriority;
				}

				public void OnPlayerUse()
				{
						if (State.Activated && !State.Moved) {
								State.Moved = true;
								if (!mMovingOverTime) {
										mMovingOverTime = true;
										StartCoroutine(MoveOverTime());
								}
						}
				}

				public bool BookRead(double timeStamp)
				{
						if (State.Activated)
								return true;

						BookStatus status = BookStatus.None;
						if (Books.GetBookStatus(State.BookName, out status) && Flags.Check((int)status, (int)BookStatus.Read, Flags.CheckType.MatchAny)) {
								State.Activated = true;
								gameObject.tag = Globals.TagGroundStone;
						}
						return true;
				}

				protected IEnumerator MoveOverTime()
				{
						HiddenCollider.enabled = false;
						while (MovableRock.localPosition != MovedPosition) {
								MovableRock.localPosition = Vector3.Lerp(MovableRock.localPosition, MovedPosition, (float)(WorldClock.ARTDeltaTime * MoveSpeed));
								yield return null;
						}
						mMovingOverTime = false;
				}

				protected bool mMovingOverTime = false;
		}

		[Serializable]
		public class LooseRockState
		{
				public string BookName = "DanielsWill";
				public bool Activated = false;
				public bool Moved = false;
		}
}