using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class DropItemsOnDie : WIScript
		{
				[FrontiersAvailableModsAttribute("Category")]
				public string WICategoryName = string.Empty;

				public int NumItems {
						get {
								return SpawnPoints.Count;
						}
				}

				public float DropForce = 100.0f;
				public float RandomDropout = 0.1f;
				public bool DropEveryItemInCategory = false;
				public List <Vector3> SpawnPoints = new List <Vector3>();
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
				protected static WorldItem gdropItem = null;

				public void OnDie()
				{
						if (gSpawnPointTransform == null) {
								gSpawnPointTransform = new STransform();
						}

						if (DropEveryItemInCategory) {
								WICategory category = null;
								if (WorldItems.Get.Category(WICategoryName, out category)) {
										for (int i = 0; i < category.GenericWorldItems.Count; i++) {
												//create one for each instance
												for (int j = 0; j < category.GenericWorldItems[i].InstanceWeight; j++) {
														gSpawnPointTransform.Position = worlditem.tr.TransformPoint(SpawnPoints[i % SpawnPoints.Count]);
														gSpawnPointTransform.Rotation.x = Random.Range(0f, 360f);
														gSpawnPointTransform.Rotation.y = Random.Range(0f, 360f);
														gSpawnPointTransform.Rotation.z = Random.Range(0f, 360f);
														if (WorldItems.CloneWorldItem(category.GenericWorldItems[i], gSpawnPointTransform, false, WIGroups.Get.World, out gdropItem)) {
																gdropItem.Props.Local.Mode = WIMode.World;
																gdropItem.Initialize();
																gdropItem.SetMode(WIMode.World);

																if (!string.IsNullOrEmpty(DropEffect)) {
																		FXManager.Get.SpawnFX(gdropItem, DropEffect);
																}
														}
												}
										}
								}
						} else {
								for (int i = 0; i < SpawnPoints.Count; i++) {
										mSpawnPoint = SpawnPoints[i];
										if (Random.value > RandomDropout) {
												if (WorldItems.CloneRandomFromCategory(WICategoryName, WIGroups.Get.World, out gdropItem)) {	////Debug.Log ("Cloning item!");
														gdropItem.Props.Local.Transform.Position = worlditem.tr.TransformPoint(mSpawnPoint);
														gdropItem.Props.Local.Transform.Rotation.x = Random.Range(0f, 360f);
														gdropItem.Props.Local.Transform.Rotation.y = Random.Range(0f, 360f);
														gdropItem.Props.Local.Transform.Rotation.z = Random.Range(0f, 360f);
														gdropItem.Props.Local.FreezeOnStartup = false;
														gdropItem.Props.Local.Mode = WIMode.World;
														gdropItem.Initialize();
														gdropItem.SetMode(WIMode.World);

														if (!string.IsNullOrEmpty(DropEffect)) {
																FXManager.Get.SpawnFX(gdropItem, DropEffect);
														}
												}
										}
								}
						}
						Finish();
				}
		}
}