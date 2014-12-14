using UnityEngine;
using System.Collections;
using System;
using Frontiers.World.Gameplay;

namespace Frontiers.World
{
		public class Machine : WIScript
		{
				public MachineState State = new MachineState();

				public bool IsBroken {
						get {
								return State.IsBroken;
						}
				}

				public bool Repair(Skill repairSkill)
				{
						if (State.IsBroken && repairSkill.LastSkillValue > State.RepairDifficulty) {
								State.IsBroken = false;
								return true;
						}
						return false;
				}

				public override void PopulateExamineList(System.Collections.Generic.List<WIExamineInfo> examine)
				{
						if (State.IsBroken) {
								examine.Add(new WIExamineInfo("It appears to be broken."));
						}
				}
		}

		[Serializable]
		public class MachineState
		{
				public bool IsBroken = false;
				public float RepairDifficulty = 0.25f;
		}
}
