using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Locations;
using Frontiers.World.Gameplay;
using System.Text.RegularExpressions;
using Frontiers.GUI;

namespace Frontiers.World.Gameplay
{
		public class FollowPath : Skill
		{
				public override bool IsInUse {
						get {
								return mIsInUse | Paths.HasActivePath;
						}
				}
				//this will cause the icon fade to get dimmer as the player gets farther from paths
				public override float NormalizedEffectTimeLeft {
						get {
								return 1f - Paths.NormalizedDistanceFromPath;
						}
				}

				public void FollowPathPassively()
				{
						UseStart(true);
				}
		}
}