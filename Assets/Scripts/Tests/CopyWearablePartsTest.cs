using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

public class CopyWearablePartsTest : MonoBehaviour {

	public CharacterBody CopyFrom;
	public List <CharacterBody> CopyTo = new List <CharacterBody> ();

	public void CopyWearableParts ( )
	{
		foreach (WearablePart wp in CopyFrom.WearableParts) {
			wp.name = wp.BodyPart.ToString ( ) + "-" + wp.Orientation.ToString ( ) + "-" + wp.Type.ToString ();
		}

		foreach (CharacterBody bodyToClear in CopyTo) {
			WearablePart[] parts = bodyToClear.GetComponentsInChildren <WearablePart> (true);
			foreach (WearablePart wp in parts) {
				if (wp.name.Contains (bodyToClear.TransformPrefix)) {
					//just delete the component
					GameObject.DestroyImmediate (wp);
				} else {
					GameObject.DestroyImmediate (wp.gameObject);
				}
			}
		}

		foreach (CharacterBody copyTo in CopyTo) {

			foreach (WearablePart wearablePart in CopyFrom.WearableParts) {
				string searchName = wearablePart.name;
				Debug.Log ("Looking for wearable part " + wearablePart.name);
				Transform otherWearablePartTransform = null;
				otherWearablePartTransform = findChildSlowly (copyTo.transform, searchName);
				if (otherWearablePartTransform == null) {
					//find the parent and create it under that
					searchName = wearablePart.transform.parent.name.Replace (CopyFrom.TransformPrefix, copyTo.TransformPrefix);
					Debug.Log ("Looking for parent " + wearablePart.transform.parent.name + " as " + searchName);
					Transform otherWearablePartParent = findChildSlowly (copyTo.transform, searchName);
					if (otherWearablePartParent != null) {
						otherWearablePartTransform = otherWearablePartParent.gameObject.CreateChild (wearablePart.name);
						otherWearablePartTransform.localPosition = wearablePart.transform.localPosition;
						otherWearablePartTransform.localRotation = wearablePart.transform.localRotation;
					}
				}
				//this should not be null at this point
				if (otherWearablePartTransform != null) {
					WearablePart otherWearablePart = otherWearablePartTransform.gameObject.GetOrAdd <WearablePart> ();
					otherWearablePart.Type = wearablePart.Type;
					otherWearablePart.Orientation = wearablePart.Orientation;
					otherWearablePart.BodyPart = wearablePart.BodyPart;
				}
			}

			foreach (EquippablePart equippablePart in CopyFrom.EquippableParts) {
				string searchName = equippablePart.name;//.Replace (copyTo.TransformPrefix, CopyFrom.TransformPrefix);
				Debug.Log ("Looking for equippable part " + equippablePart.name);
				Transform otherEquippablePartTransform = null;
				otherEquippablePartTransform = findChildSlowly (copyTo.transform, searchName);
				if (otherEquippablePartTransform == null) {
					//find the parent and create it under that
					searchName = equippablePart.transform.parent.name.Replace (CopyFrom.TransformPrefix, copyTo.TransformPrefix);
					Debug.Log ("Looking for parent " + equippablePart.transform.parent.name + " as " + searchName);
					Transform otherEquippablePartParent = findChildSlowly (copyTo.transform, searchName);
					if (otherEquippablePartParent != null) {
						otherEquippablePartTransform = otherEquippablePartParent.gameObject.CreateChild (equippablePart.name);
						otherEquippablePartTransform.localPosition = equippablePart.transform.localPosition;
						otherEquippablePartTransform.localRotation = equippablePart.transform.localRotation;
					}
				}
				//this should not be null at this point
				if (otherEquippablePartTransform != null) {
					EquippablePart otherEquippablePart = otherEquippablePartTransform.gameObject.GetOrAdd <EquippablePart> ();
					otherEquippablePart.Type = equippablePart.Type;
				}
			}

			AudoDetectBodyParts (copyTo);
		}
	}

	static void AudoDetectBodyParts (CharacterBody body)
	{
		Component[] bodyPartComponents = body.GetComponentsInChildren (typeof(BodyPart));
		List <BodyPart> bodyParts = null;
		if (body != null) {
			bodyParts = body.BodyParts;
		}
		bodyParts.Clear ();

		BodyPart chestBodyPart = null;
		List <BodyPart> bodyPartsInNeedOfParents = new List <BodyPart> ();

		foreach (Component bodyPart in bodyPartComponents) {
			BodyPart componentAsBodyPart = bodyPart as BodyPart;
			if (componentAsBodyPart != null) {
				if (componentAsBodyPart.Type == BodyPartType.Chest) {
					chestBodyPart = componentAsBodyPart;
				} else if (componentAsBodyPart.ParentPart == null) {
					bodyPartsInNeedOfParents.Add (componentAsBodyPart);
				}
				bodyParts.Add (componentAsBodyPart);
			}
		}

		foreach (BodyPart bodyPartInNeedOfParent in bodyPartsInNeedOfParents) {
			bool foundParent = false;
			Transform current	= bodyPartInNeedOfParent.transform.parent;
			while (!foundParent) {
				BodyPart potentialBodyPart = current.GetComponent <BodyPart> ();
				if (potentialBodyPart == null) {
					current = current.transform.parent;
					if (current == null) {
						//whoops, we've reached the end
						bodyPartInNeedOfParent.ParentPart = chestBodyPart;
						foundParent = true;
					}
				} else {
					bodyPartInNeedOfParent.ParentPart = potentialBodyPart;
					foundParent = true;
				}
			}
		}

		Component[] wearablePartComponents = body.GetComponentsInChildren (typeof(WearablePart));
		List <WearablePart> wearableParts = body.WearableParts;
		wearableParts.Clear ();

		foreach (Component wearablePart in wearablePartComponents) {
			WearablePart componentAsWearablePart = wearablePart as WearablePart;
			if (componentAsWearablePart != null) {
				wearableParts.Add (componentAsWearablePart);
			}
		}

		Component[] equippablePartComponents = body.GetComponentsInChildren (typeof(EquippablePart));
		List <EquippablePart> equippableParts = body.EquippableParts;
		equippableParts.Clear ();

		foreach (Component equippablePart in equippablePartComponents) {
			EquippablePart componentAsEquippablePart = equippablePart as EquippablePart;
			if (componentAsEquippablePart != null) {
				equippableParts.Add (componentAsEquippablePart);
			}
		}
	}

	public static Transform findChildSlowly (Transform startTransform, string searchString)
	{
		Transform childItemGo = null;
		Transform[] transforms = startTransform.GetComponentsInChildren <Transform> ();
		string searchStringToLower	= searchString.ToLower ();
		if (startTransform.name.ToLower ().Contains (searchStringToLower)) {
			return startTransform;
		}

		foreach (Transform bodyPartTransform in transforms) {
			string toLowerString = bodyPartTransform.name.ToLower ();
			if (toLowerString.Contains (searchStringToLower)) {
				childItemGo = bodyPartTransform;
				break;
			}
		}
		return childItemGo;
	}
}
