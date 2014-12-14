using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using System;

namespace Frontiers.World
{
		public class ArtifactShard : WIScript
		{
				public ArtifactShardState State = new ArtifactShardState();

				public void ChooseRandomShardState()
				{
						WIState state = worlditem.States.States[UnityEngine.Random.Range(0, worlditem.States.States.Count)];
						worlditem.State = state.Name;
						State.HasChosenFragment = true;
				}

				public bool CanDate {
						get {
								return !State.HasBeenDated && State.CanBeDated;
						}
				}

				public bool DateShard()
				{
						if (!State.HasBeenDated) {
								State.HasBeenDated = true;
								//TODO post some kind of introspection declaring how old it is
								return true;
						}

						return false;
				}
		}

		[Serializable]
		public class ArtifactShardState
		{
				public bool HasChosenFragment = false;
				public bool HasBeenDated = false;
				public bool CanBeDated = true;
				public ArtifactAge Age = ArtifactAge.Antiquated;
		}
}
