using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using System.Reflection;
using Frontiers.Data;

namespace Frontiers.World.Gameplay
{
		public class Resurrect : RespawnSkill
		{
				public ResurrectExtensions Extensions = new ResurrectExtensions();

				public override bool DoesContextAllowForUse(IItemOfInterest targetObject)
				{
						if (targetObject.IOIType == ItemOfInterestType.Player) {
								Usage.ListOptionDisplayName = "Resurrect";//TODO can we clean this up?
								return !MobileReference.IsNullOrEmpty(Extensions.ResurrectionMarker);
						} else {
								Usage.ListOptionDisplayName = "Set Resurrection Point";//TODO can we clean this up?
								return base.DoesContextAllowForUse(targetObject);
						}
				}

				protected override void RespawnPlayer(LocalPlayer player)
				{
						SpawnManager.Get.SpawnInBed(Extensions.ResurrectionMarker, Player.Local.Status.OnRespawnBedFound);
				}
		}

		[Serializable]
		public class ResurrectExtensions : SkillExtensions
		{
				public int UnskilledRepPenaltyOnResurrect;
				public int SkilledRepPenaltyOnResurrect;
				public float MasterRepPenalty;
				public PlayerStatusRestore UnskilledHungerPenaltyOnResurrect;
				public PlayerStatusRestore SkilledHungerPenaltyOnResurrect;
				public float MasterHungerPenalty;
				public MobileReference ResurrectionMarker = null;
		}
}