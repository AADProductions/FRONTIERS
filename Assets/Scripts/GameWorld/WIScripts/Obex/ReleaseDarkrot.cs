using UnityEngine;
using System.Collections;
using Frontiers;
using System;

namespace Frontiers.World
{
		public class ReleaseDarkrot : WIScript
		{
				public ReleaseDarkrotState State = new ReleaseDarkrotState();

				public override void OnInitialized()
				{
						worlditem.OnAddedToPlayerInventory += OnAddedToPlayerInventory;
				}

				public void OnAddedToPlayerInventory()
				{
						if (State.ReleaseOnAddedToPlayerInventory) {
								Release();
						}
				}

				protected void Release()
				{
						//create a darkrot spawner
						Creatures.Get.CreateDarkrotSpawner(
								worlditem.tr.TransformPoint(State.SpawnerPositionOffset),
								worlditem.tr.localRotation.eulerAngles + State.SpawnerRotationOffset,
								worlditem.Group,
								State.NumDarkrotReleaesd,
								State.ReleaseDelay,
								State.ReleaseInterval);
						Finish();
				}
				#if UNITY_EDITOR
				public override void OnEditorRefresh()
				{
						Transform spawnerOffset = transform.FindChild("DarkrotSpawner");
						if (spawnerOffset != null) {
								State.SpawnerPositionOffset = spawnerOffset.localPosition;
								State.SpawnerRotationOffset = spawnerOffset.localRotation.eulerAngles;
						}
				}
				#endif
		}

		[Serializable]
		public class ReleaseDarkrotState
		{
				public bool ReleaseOnAddedToPlayerInventory = true;
				public float ReleaseDelay = 5f;
				public float ReleaseInterval = 5f;
				public int NumDarkrotReleaesd = 10;
				public SVector3 SpawnerPositionOffset = new SVector3();
				public SVector3 SpawnerRotationOffset = new SVector3();
		}
}