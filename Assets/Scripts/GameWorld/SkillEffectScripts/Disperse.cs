using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;

namespace Frontiers.World.Gameplay
{
		public class Disperse : SkillEffectScript
		{
				public override void OnEffectStart()
				{
						IDispersible dispersible = (IDispersible)gameObject.GetComponent(typeof(IDispersible));
						if (dispersible != null) {
								dispersible.Disperse(ParentSkill.EffectTime);

								if (dispersible.IsDispersed) {
										Finish();
								}
						} else {
								Finish();
						}
				}
		}
}