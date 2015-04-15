using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World.WIScripts
{
		public class DropItemsOnDie : WIScript
		{
				[FrontiersAvailableModsAttribute("Category")]
				public string WICategoryName = string.Empty;
				[FrontiersAvailableModsAttribute("Category")]
				public string WICategoryNameBurned = string.Empty;
				public float DropForce = 100.0f;
				public float RandomDropout = 0.1f;
				public bool DropEveryItemInCategory = false;
				public List <Vector3> SpawnPoints = new List <Vector3>();
				//these are used if there are no spawn points
				public int MinRandomItems = 3;
				public int MaxRandomItems = 5;
				public string DropEffect;

				public override void OnInitialized()
				{
						Damageable damageable = null;
						if (worlditem.Is <Damageable>(out damageable)) {
								damageable.OnDie += OnDie;
						} else {
								worlditem.OnModeChange += OnModeChange;
						}
				}

				protected Vector3 mSpawnPoint;
				protected static STransform gSpawnPointTransform;
				protected static WorldItem gDropItem = null;

				public void OnDie()
				{
						if (gSpawnPointTransform == null) {
								gSpawnPointTransform = new STransform();
						}

						string categoryName = WICategoryName;
						if (!string.IsNullOrEmpty(WICategoryNameBurned)) {
								if (worlditem.Get <Damageable>().State.LastDamageSource == "Fire") {
										categoryName = WICategoryNameBurned;
								}
						}

						if (DropEveryItemInCategory) {
								WICategory category = null;
								if (WorldItems.Get.Category(categoryName, out category)) {
										for (int i = 0; i < category.GenericWorldItems.Count; i++) {
												//create one for each instance
												for (int j = 0; j < category.GenericWorldItems[i].InstanceWeight; j++) {
														gSpawnPointTransform.Position = worlditem.tr.TransformPoint(SpawnPoints[i % SpawnPoints.Count]);
														gSpawnPointTransform.Rotation.x = Random.Range(0f, 360f);
														gSpawnPointTransform.Rotation.y = Random.Range(0f, 360f);
														gSpawnPointTransform.Rotation.z = Random.Range(0f, 360f);
														if (WorldItems.CloneWorldItem(category.GenericWorldItems[i], gSpawnPointTransform, false, WIGroups.Get.World, out gDropItem)) {
																gDropItem.Props.Local.Mode = WIMode.World;
																gDropItem.Initialize();
																gDropItem.SetMode(WIMode.World);

																if (!string.IsNullOrEmpty(DropEffect)) {
																		FXManager.Get.SpawnFX(gDropItem, DropEffect);
																}
														}
												}
										}
								}
						} else {
								//we prefer spawn points first
								if (SpawnPoints.Count > 0) {
										for (int i = 0; i < SpawnPoints.Count; i++) {
												mSpawnPoint = SpawnPoints[i];
												if (Random.value > RandomDropout) {
														if (WorldItems.CloneRandomFromCategory(categoryName, WIGroups.Get.World, out gDropItem)) {	////Debug.Log ("Cloning item!");
																gDropItem.Props.Local.Transform.Position = worlditem.tr.TransformPoint(mSpawnPoint);
																gDropItem.Props.Local.Transform.Rotation.x = Random.Range(0f, 360f);
																gDropItem.Props.Local.Transform.Rotation.y = Random.Range(0f, 360f);
																gDropItem.Props.Local.Transform.Rotation.z = Random.Range(0f, 360f);
																gDropItem.Props.Local.FreezeOnStartup = false;
																gDropItem.Props.Local.Mode = WIMode.World;
																gDropItem.Initialize();
																gDropItem.SetMode(WIMode.World);

																if (!string.IsNullOrEmpty(DropEffect)) {
																		FXManager.Get.SpawnFX(gDropItem, DropEffect);
																}
														}
												}
										}
								} else {
										//if no spawn points exist, spawn stuff using mesh vertices
										System.Random random = new System.Random(Profile.Get.CurrentGame.Seed);
										int numToSpawn = random.Next(MinRandomItems, MaxRandomItems);
										Renderer r = null;
										if (worlditem.HasStates) {
												r = worlditem.States.CurrentState.StateRenderer;
										} else {
												//just get the first renderer
												r = worlditem.Renderers[0];
										}
										//TODO make this more safe?
										Mesh sharedMesh = r.GetComponent <MeshFilter>().sharedMesh;
										Vector3[] vertices = sharedMesh.vertices;
										for (int i = 0; i < numToSpawn; i++) {
												if (WorldItems.CloneRandomFromCategory(categoryName, WIGroups.Get.World, out gDropItem)) {
														gDropItem.Props.Local.Transform.Position = worlditem.tr.TransformPoint(vertices[random.Next(0, vertices.Length)]);
														gDropItem.Props.Local.Transform.Rotation.x = Random.Range(0f, 360f);
														gDropItem.Props.Local.Transform.Rotation.y = Random.Range(0f, 360f);
														gDropItem.Props.Local.Transform.Rotation.z = Random.Range(0f, 360f);
														gDropItem.Props.Local.FreezeOnStartup = false;
														gDropItem.Props.Local.Mode = WIMode.World;
														gDropItem.Initialize();
														gDropItem.SetMode(WIMode.World);

														if (!string.IsNullOrEmpty(DropEffect)) {
																FXManager.Get.SpawnFX(gDropItem, DropEffect);
														}
												}
										}
								}
						}
						Finish();
				}
		}
}