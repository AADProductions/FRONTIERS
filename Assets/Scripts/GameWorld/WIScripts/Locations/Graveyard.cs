using UnityEngine;
using System.Collections;
using Frontiers;
using System;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class Graveyard : WIScript
		{
				public GraveyardState State = new GraveyardState();
				Location location = null;

				public override void OnInitialized()
				{
						State.GraveyardStructure.StructureOwner = worlditem;
						location = worlditem.Get <Location>();
						location.OnLocationGroupLoaded += OnLocationGroupLoaded;
				}

				public void OnLocationGroupLoaded()
				{
						Structures.AddMinorToload(State.GraveyardStructure, 0, worlditem);

						if (!State.CreatedHeadstones) {
								//create headstones the first time we load the location group
								//after that they'll just load and unload normally
								WorldItem headstoneWorldItem = null;
								State.CreatedHeadstones = true;
								for (int i = 0; i < State.HeadstoneSpawnPoints.Count; i++) {
										if (WorldItems.CloneRandomFromCategory(State.HeadstoneCategory, location.LocationGroup, State.HeadstoneSpawnPoints[i], out headstoneWorldItem)) {
												headstoneWorldItem.Initialize();
												HeadstoneAvatar hsa = headstoneWorldItem.Get <HeadstoneAvatar>();
												if (i < State.Headstones.Count) {
														hsa.State.HeadstoneName = State.Headstones[i];
												}
												hsa.RefreshProps();
										}
								}
						}
				}
				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						List <STransform> newHeadstonePoints = new List<STransform>();
						StructureBuilder sb = null;
						foreach (Transform child in transform) {
								if (child.name == "Headstone") {
										newHeadstonePoints.Add(child);
								} else if (child.gameObject.HasComponent <StructureBuilder>(out sb)) {
										State.GraveyardStructure.TemplateName = StructureBuilder.GetTemplateName(child.name);
										State.GraveyardStructure.Position = child.localPosition;
										State.GraveyardStructure.Rotation = child.localRotation.eulerAngles;
								}
						}
						if (newHeadstonePoints.Count > 0) {
								State.HeadstoneSpawnPoints.Clear();
								State.HeadstoneSpawnPoints.AddRange(newHeadstonePoints);
						}
						State.CreatedHeadstones = false;
				}
				#endif
				public void OnDrawGizmos()
				{
						Gizmos.color = Color.white;
						foreach (STransform headstoneSpawnPoint in State.HeadstoneSpawnPoints) {
								Gizmos.DrawSphere(transform.TransformPoint(headstoneSpawnPoint.Position), 0.5f);
						}
				}
		}

		[Serializable]
		public class GraveyardState
		{
				public bool CreatedHeadstones = false;
				[FrontiersCategoryNameAttribute]
				public string HeadstoneCategory = string.Empty;
				public MinorStructure GraveyardStructure = new MinorStructure();
				public List <STransform> HeadstoneSpawnPoints = new List <STransform>();
				public List <string> Headstones = new List <string>();
		}
}