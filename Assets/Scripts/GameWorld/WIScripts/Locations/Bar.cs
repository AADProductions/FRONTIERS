using UnityEngine;
using System;
using System.Collections;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
		public class Bar : WIScript
		{
				public Signboard Sign;
				public Location location = null;
				public BarState State = new BarState();

				public override void OnInitialized()
				{
						location = worlditem.Get <Location>();
						location.OnLocationGroupLoaded += CreateSign;
				}

				public void CreateSign()
				{
						Sign = Signboards.AddBar(Sign, worlditem, location.LocationGroup, State.SignboardOffset, State.SignboardTexture);
				}
				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						Transform signboard = transform.FindChild("Signboard");
						if (signboard != null) {
								State.SignboardOffset.CopyFrom(signboard);
						} else {
								Structure parentStructure = gameObject.GetComponent <Structure>();
								signboard = parentStructure.CreateSignboardTransform();
								signboard.transform.parent = parentStructure.transform;
								State.SignboardOffset.CopyFrom(signboard);
								signboard.transform.parent = parentStructure.StructureBase.transform;
						}
						Location location = gameObject.GetComponent <Location>();
						location.State.Type = "Tavern";
				}
				#endif
				public void OnDrawGizmos()
				{
						if (State.SignboardOffset != null) {
								Gizmos.DrawWireSphere((transform.position + State.SignboardOffset.Position), 0.5f);
						}
				}
		}

		[Serializable]
		public class BarState
		{
				[FrontiersAvailableModsAttribute("Signboard")]
				public string SignboardTexture;
				public STransform SignboardOffset;
		}
}