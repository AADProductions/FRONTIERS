using UnityEngine;
using System.Collections;
using Frontiers.World.Gameplay;
using System;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class WaterTrap : WIScript, ITrap
		{
				public WaterTrapState State = new WaterTrapState();

				#region ITrap implementation

				public double TimeLastChecked { get { return State.TimeLastChecked; } set { State.TimeLastChecked = value; } }

				public double TimeSet { get { return State.TimeSet; } }

				public float SkillOnSet { get { return State.SkillOnSet; } }

				public WorldItem Owner { get { return worlditem; } }

				public string TrappingSkillName { get { return "Fishing"; } }

				public List <string> CanCatch {
						get {
								return gCanCatch;
						}
				}

				public List <string> Exceptions {
						get {
								return gExceptions;
						}
				}

				public bool SkillUpdating { get; set; }

				public TrapMode Mode {
						get {
								return State.Mode;
						}
						set {
								State.Mode = value;
								Refresh();
						}
				}

				public List <CreatureDen> IntersectingDens { get { return mIntersectingDens; } }

				#endregion

				public override void OnInitialized()
				{
						worlditem.OnVisible += Refresh;
				}

				public override void OnModeChange()
				{
						if (worlditem.Is(WIMode.Frozen)) {
								//fishing traps are always set
								Mode = TrapMode.Set;
						} else {
								Mode = TrapMode.Disabled;
						}
				}

				public void Refresh()
				{
						if (Mode == TrapMode.Set && !SkillUpdating) {
								Skill skill = null;
								if (Skills.Get.HasLearnedSkill(TrappingSkillName, out skill)) {
										TrappingSkill trappingSkill = skill as TrappingSkill;
										trappingSkill.UpdateTrap(this);
								}
						}
				}

				protected List <CreatureDen> mIntersectingDens = new List<CreatureDen>();
				protected static List <string> gCanCatch = new List <string>() { "Fish" };
				protected static List <string> gExceptions = new List <string>();
		}

		[Serializable]
		public class WaterTrapState
		{
				public double TimeLastChecked = 0f;
				public TrapMode Mode = TrapMode.Disabled;
				public float SkillOnSet = 0.25f;
				public double TimeSet = 0f;
		}
}
