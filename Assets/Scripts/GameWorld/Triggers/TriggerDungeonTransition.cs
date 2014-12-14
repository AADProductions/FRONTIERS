using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using System;

namespace Frontiers.World
{
		public class TriggerDungeonTransition : MonoBehaviour
		{
				public Action OnEnter;
				public Action OnExit;

				public void OnEnterTransition()
				{
						OnEnter.SafeInvoke();
				}

				public void OnExitTransition()
				{
						OnExit.SafeInvoke();
				}
		}
}