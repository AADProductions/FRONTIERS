using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Frontiers.World
{
	public class WorldItemSuperLoader : MonoBehaviour
	{
		//used by WIGroups when you need to load something >rightaway<
		//instead of waiting for the player's position to cause it to load
		public string GroupPath = string.Empty;
		public string ChildItemFileName = string.Empty;
		public Action <WorldItem> CallBack = null;
		public Stack <string> GroupsToLoad = null;
		public string NextGroupToLoad = string.Empty;
		public WIGroup LastGroupLoaded = null;
		public WorldItem LoadedWorldItem = null;
		public int Ticks = 0;
		public string State = string.Empty;

		public IEnumerator LoadGroupsOverTime ()
		{
			Debug.Log ("Superloading path " + GroupPath);
			if (string.IsNullOrEmpty (GroupPath)) {
				Debug.Log ("Path was empty!");
				OnFinish ();
				yield break;
			}
			GroupsToLoad = WIGroup.SplitPath (GroupPath);
			//pop the first - it will be the root
			GroupsToLoad.Pop ();
			LastGroupLoaded = WIGroups.Get.Root;
			LastGroupLoaded.Load ();
			if (GroupsToLoad.Count > 0) {
				NextGroupToLoad = GroupsToLoad.Peek ();
				//start at the root and then load the groups all the way down
				//we'll keep loading groups until we've loaded everything in the stack
				while (GroupsToLoad.Count > 0) {
					State = "Loading group";
					WIGroup nextGroup = null;
					while (!LastGroupLoaded.Is (WIGroupLoadState.Loaded) || !LastGroupLoaded.GetChildGroup (out nextGroup, NextGroupToLoad)) {
                        Debug.Log("Last group " + LastGroupLoaded.name + " loaded? " + LastGroupLoaded.Is(WIGroupLoadState.Loaded).ToString() + "\n" +
                            "Child group available? " + LastGroupLoaded.GetChildGroup(out nextGroup, NextGroupToLoad).ToString());
						State = "Waiting for child group " + NextGroupToLoad;
						Ticks++;
						yield return null;
					}
					//now that it has its child items, tell it to load its child groups
					LastGroupLoaded = nextGroup;
					GroupsToLoad.Pop ();
					if (GroupsToLoad.Count > 0) {
						NextGroupToLoad = GroupsToLoad.Peek ();
						Ticks++;
						yield return null;
					}
				}
			
				yield return null;
				//now we search the group to see if it holds the child item we're looking for
				while (!LastGroupLoaded.FindChildItem (ChildItemFileName, out LoadedWorldItem)) {	
					//LastGroupLoaded.State = WIGroupState.ForceLoad;
					Ticks++;
					State = "Waiting for child item " + ChildItemFileName;
					yield return null;
				}
			}
			//finished!
			OnFinish ();
			
			yield break;
		}

		public void OnFinish ()
		{
			if (CallBack != null) {	//call back even if the worlditem is null
				CallBack (LoadedWorldItem);
			}
			//destroy this after a bit, we don't need it any more
			GameObject.Destroy (gameObject, 0.5f);
		}
	}
}