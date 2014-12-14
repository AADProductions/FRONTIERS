using UnityEngine;
using System.Collections;
using System;

namespace Frontiers.World
{
		public class Residence : WIScript
		{
				public ResidenceState State = new ResidenceState();
				Structure structure;

				public override void OnInitialized()
				{
						if (worlditem.Is <Structure>(out structure)) {
								structure.OnOwnerCharacterSpawned += OnOwnerCharacterSpawned;
						}
				}

				public void OnOwnerCharacterSpawned()
				{
						State.OwnerCharacterName = structure.StructureOwner.worlditem.FileName;
				}
				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						Location location = gameObject.GetComponent <Location>();
						location.State.Type = "Residence";
						if (string.IsNullOrEmpty(State.OwnerCharacterName)) {
								location.State.Name.CommonName = "Residence";
						}
				}
				#endif
		}

		[Serializable]
		public class ResidenceState
		{
				public string OwnerCharacterName = string.Empty;
		}
}
