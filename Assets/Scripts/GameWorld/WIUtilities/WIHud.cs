using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers.World
{
		public class WIHud : MonoBehaviour
		{
				public WorldItem worlditem	= null;
				public bool Use = true;

				public bool GetPlayerAttention {
						get {
								return mGetPlayerAttention;
						}
						set {
								mGetPlayerAttention = value;
								Refresh();
						}
				}

				public HudActiveType ActiveType {
						get {
								return mActiveType;
						}
				}

				public bool SetActiveType(HudActiveType newType)
				{
						mActiveType |= newType;
						Refresh();
						return IsActive;
				}

				public void Awake()
				{
						worlditem = gameObject.GetComponent <WorldItem>();
				}

				public bool IsActive {
						get {
								return mIsActive;
						}
				}

				public void Refresh()
				{
						////Debug.Log ("Refreshing hud");
						if (GameManager.Get.TestingEnvironment) {
								return;
						}

						mIsActive = true;//TEMP TODO remove
						/*
						mIsActive = mGetPlayerAttention;
						if (Flags.Check <HudActiveType> (ActiveType, HudActiveType.OnPlayerFocus, Flags.CheckType.MatchAny)) {
							mIsActive |= worlditem.HasPlayerFocus;
						}
						if (Flags.Check <HudActiveType> (ActiveType, HudActiveType.OnWorldMode, Flags.CheckType.MatchAny)) {
							mIsActive |= worlditem.Mode == WIMode.World;
						}
						if (Flags.Check <HudActiveType> (ActiveType, HudActiveType.OnPlayerAttention, Flags.CheckType.MatchAny)) {
							mIsActive |= worlditem.HasPlayerAttention;
						}
						if (Flags.Check <HudActiveType> (ActiveType, HudActiveType.OnDeadMode, Flags.CheckType.MatchAny)) {
							mIsActive |= worlditem.Mode == WIMode.Destroyed;
						}
						*/

						//if (mIsActive) {
						if (mHud == null) {
								mHud = Frontiers.GUI.HUDManager.Get.CreateWorldItemHud(worlditem.HudTarget, worlditem.Props.Global.HUDOffset);
						}
						//} else {
						//	Retire ();
						//}
				}

				public void Retire()
				{
						if (mHud != null) {
								Frontiers.GUI.HUDManager.Get.RetireWorldItemHUD(mHud);
						}			
				}

				public GUIHudElement GetOrAddElement(HudElementType type, string elementName)
				{
						if (mHud == null) {
								mHud = Frontiers.GUI.HUDManager.Get.CreateWorldItemHud(worlditem.HudTarget, worlditem.Props.Global.HUDOffset);
						}
						return mHud.GetOrAddElement(type, elementName);
				}

				public void RemoveElement(string elementName)
				{
						if (mHud != null) {
								mHud.RemoveElement(elementName);
						}
				}

				protected HudActiveType mActiveType = HudActiveType.None;
				protected NGUIHUD mHud = null;
				protected bool mIsActive = false;
				protected bool mGetPlayerAttention = false;
		}

		[Flags]
		public enum HudActiveType
		{
				None = 0,
				OnWorldMode = 1,
				OnDeadMode = 2,
				OnPlayerFocus = 4,
				OnPlayerAttention	= 8,
				All = OnWorldMode | OnDeadMode | OnPlayerFocus | OnPlayerAttention,
		}
}