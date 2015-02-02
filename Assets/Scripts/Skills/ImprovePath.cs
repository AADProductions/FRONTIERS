using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World.Gameplay
{
	public class ImprovePath : Skill
	{
		public override bool Use (IItemOfInterest targetObject, int flavorIndex)
		{
			PathMarker targetPathMarker = null;
			if (targetObject.worlditem.Is <PathMarker> (out targetPathMarker)) {
				//now see which flavor we used
				string flavorName = "Move";
				mLastflavors.TryGetValue (flavorIndex, out flavorName);
				switch (flavorName) {
				case "Move":
				default:
					if (Player.Local.ItemPlacement.ItemCarry (targetPathMarker.worlditem, true)) {
						OnSuccess ();
						return true;
					}
					break;

				case "Swap":
					//this is kind of an odd operation
					//we just set the state on the two objects
					//and boom! They're swapped
					PathMarker equippedPathMarker = null;
					bool hasEquippedPathMarker = false;
					if (Player.Local.Tool.IsEquipped && Player.Local.Tool.HasWorldItem) {
						if (Player.Local.Tool.worlditem.Is <PathMarker> (out equippedPathMarker)) {
							//we're not going to go through the type check song and dance
							//we'll just swap assuming it's all correct
							string downgradeState = targetPathMarker.worlditem.State;
							string upgradeState = equippedPathMarker.worlditem.State;
							targetPathMarker.worlditem.State = upgradeState;
							equippedPathMarker.worlditem.State = downgradeState;
						}
					}
					break;
				}
			}
			return false;
		}
		/*UPGRADE / SWAP OPTIONS
		Path types:
		-------
		Path
		Trail
		Road
		-----
		PathMarker->TrailMarker->RoadMarker		
		PathMarker->CrossPath
		TrailMarker->CrossTrail
		RoadMarker->CrossRoad
		*/
		public override WIListOption GetListOption (IItemOfInterest targetObject)
		{
			//Debug.Log ("Getting list option for improve path");

			WIListOption listOption = base.GetListOption (targetObject);
			listOption.Flavors.Clear ();
			mLastflavors.Clear ();
			HashSet <string> flavors = new HashSet <string> ();

			PathMarker targetPathMarker = null;
			if (targetObject.worlditem.Is <PathMarker> (out targetPathMarker)) {
				ImprovePathMarker (targetPathMarker, flavors);
			}

			//keep flavors in the dictionary, we don't know which will be relevant
			//it's only move and swap for now but there may be more
			int flavorNum = 0;
			foreach (string flavor in flavors) {
				//Debug.Log ("Adding flavor " + flavor);
				listOption.Flavors.Add (flavor);
				mLastflavors.Add (flavorNum, flavor);
				flavorNum++;
			}
			
			return listOption;
		}

		protected void ImprovePathMarker (PathMarker targetPathMarker, HashSet <string> flavors)
		{
			PathMarker equippedPathMarker = null;
			bool hasEquippedPathMarker = false;
			if (Player.Local.Tool.IsEquipped && Player.Local.Tool.HasWorldItem) {
				hasEquippedPathMarker = Player.Local.Tool.worlditem.Is <PathMarker> (out equippedPathMarker);
			}

			switch (targetPathMarker.worlditem.State) {
			case "PathMarker":
				flavors.Add ("Move");
				if (hasEquippedPathMarker) {
					switch (equippedPathMarker.worlditem.State) {
					case "CrossPath":
					case "TrailMarker":
						//Debug.Log ("Cross path is path marker, can upgrade");
						flavors.Add ("Swap");
						break;

					default:
						break;
					}
				}
				break;

			case "CrossPath":
				flavors.Add ("Move");
				if (hasEquippedPathMarker) {
					switch (equippedPathMarker.worlditem.State) {
					case "TrailMarker":
					case "CrossTrail":
						flavors.Add ("Swap");
						break;

					default:
						break;
					}
				}
				break;

			case "TrailMarker":
				flavors.Add ("Move");
				if (hasEquippedPathMarker) {
					switch (equippedPathMarker.worlditem.State) {
					case "RoadMarker":
					case "CrossTrail":
						flavors.Add ("Swap");
						break;

					default:
						break;
					}
				}
				break;

			case "RoadMarker":
				if (hasEquippedPathMarker) {
					switch (equippedPathMarker.worlditem.State) {
					case "CrossRoad":
						flavors.Add ("Swap");
						break;

					default:
						break;
					}
				}
				break;

			default:
				break;
			}
		}

		Dictionary <int,string> mLastflavors = new Dictionary <int, string> ();
	}
}