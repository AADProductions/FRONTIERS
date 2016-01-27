using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Frontiers.Data;

namespace Frontiers.World.WIScripts
{
	public class SpiderEgg : WIScript
	{
		//general purpose spawner script
		//this is used whenever we need random things spawned on a regular basis
		public SpiderEggState State = new SpiderEggState ();

		public override void OnInitialized ()
		{
			worlditem.OnVisible += OnVisible;
		}

		public virtual void OnActive ()
		{
			if (mIsSpawning) {
				return;
			}
			mIsSpawning = true;
			StartCoroutine (SpawnSpidersOverTime ());
		}

		public virtual void OnVisible ()
		{
			if (mIsSpawning) {
				return;
			}
			mIsSpawning = true;
			StartCoroutine (SpawnSpidersOverTime ());
		}

		protected IEnumerator SpawnSpidersOverTime () {
			yield break;
		}

		protected bool mIsSpawning = false;
	}

	[Serializable]
	public class SpiderEggState
	{
		public int MaxActiveSpiders = 5;
	}
}