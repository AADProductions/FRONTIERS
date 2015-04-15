using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.WIScripts;
using Frontiers.World.Gameplay;
using UnityEditor;
using ExtensionMethods;

public class CharacterTools : MonoBehaviour
{
	[MenuItem ("Frontiers/Characters/Save to disk")]
	static void MoveWorldItemsToGeneric ()
	{
		WorldItem characterWorldItem = Selection.activeGameObject.GetComponent <WorldItem> ();
		if (characterWorldItem != null) {
			StackItem characterStackItem = characterWorldItem.GetStackItem (WIMode.None);
			Mods.Get.Runtime.SaveMod <StackItem> (characterStackItem, "Character", characterStackItem.FileName);
		}
	}

	[MenuItem ("Frontiers/Characters/Add Required Components")]
	static void AddRequiredComponents ()
	{
		foreach (GameObject selectedObject in Selection.gameObjects) {
			selectedObject.GetOrAdd <Animator> ();
			selectedObject.GetOrAdd <Rigidbody> ();
			selectedObject.GetOrAdd <CharacterAnimator> ();
			selectedObject.GetOrAdd <BodyTransforms> ();
			selectedObject.GetOrAdd <CharacterBody> ();
			selectedObject.GetOrAdd <CharacterSounds> ();
			CapsuleCollider cc = selectedObject.GetComponent <CapsuleCollider> ();
			if (cc != null) {
				GameObject.DestroyImmediate (cc);
			}
			if (selectedObject.animation != null) {
				GameObject.DestroyImmediate (selectedObject.animation);
			}
			TNObject tnObject = selectedObject.GetOrAdd <TNObject> ();
			TNAutoSync tnAutoSync = selectedObject.GetOrAdd <TNAutoSync> ();
		}
	}

	[MenuItem ("Frontiers/Creatures/Audo find body transforms")]
	static void CreaturesAudioFindBodyTransforms ()
	{
		foreach (GameObject selectedObject in Selection.gameObjects) {
			BodyTransforms bodyTransforms = selectedObject.GetComponent <BodyTransforms> ();
		
			bodyTransforms.Hips = findChildSlowly (bodyTransforms.transform, "Pelvis");
			bodyTransforms.Head = findChildSlowly (bodyTransforms.transform, "Head");
			if (bodyTransforms.Head != null) {
				bodyTransforms.HeadTop = bodyTransforms.Head.gameObject.FindOrCreateChild ("HeadTop").transform;
			}
			bodyTransforms.Chest = findChildSlowly (bodyTransforms.transform, "Spine3");
			bodyTransforms.FaceJaw = findChildSlowly (bodyTransforms.transform, "_jaw");
			if (bodyTransforms.FaceJaw == null) {
				bodyTransforms.FaceJaw = findChildSlowly (bodyTransforms.transform, "bon_gob_war_jaw");
			}
			bodyTransforms.Finger1L = findChildSlowly (bodyTransforms.transform, "LDigit21");
			bodyTransforms.Finger2L = findChildSlowly (bodyTransforms.transform, "LDigit31");
			bodyTransforms.Finger3L = findChildSlowly (bodyTransforms.transform, "LDigit41");
			bodyTransforms.Finger4L = findChildSlowly (bodyTransforms.transform, "LDigit51");
			bodyTransforms.ThumbL = findChildSlowly (bodyTransforms.transform, "LDigit11");
			bodyTransforms.Finger1R = findChildSlowly (bodyTransforms.transform, "RDigit21");
			bodyTransforms.Finger2R = findChildSlowly (bodyTransforms.transform, "RDigit31");
			bodyTransforms.Finger3R = findChildSlowly (bodyTransforms.transform, "RDigit41");
			bodyTransforms.Finger4R = findChildSlowly (bodyTransforms.transform, "RDigit51");
			bodyTransforms.ThumbR = findChildSlowly (bodyTransforms.transform, "RDigit11");
			bodyTransforms.FootL = findChildSlowly (bodyTransforms.transform, "L Foot");
			bodyTransforms.FootR = findChildSlowly (bodyTransforms.transform, "R Foot");
			bodyTransforms.KneeL = findChildSlowly (bodyTransforms.transform, "L Calf");
			bodyTransforms.KneeR = findChildSlowly (bodyTransforms.transform, "R Calf");
			bodyTransforms.LegL = findChildSlowly (bodyTransforms.transform, "L Thigh");
			bodyTransforms.LegR = findChildSlowly (bodyTransforms.transform, "R Thigh");
			bodyTransforms.ElbowL = findChildSlowly (bodyTransforms.transform, "L Forearm");
			bodyTransforms.ElbowR = findChildSlowly (bodyTransforms.transform, "R Forearm");
			bodyTransforms.ShoulderL	= findChildSlowly (bodyTransforms.transform, "L Upperarm");
			bodyTransforms.ShoulderR	= findChildSlowly (bodyTransforms.transform, "R Upperarm");		
			bodyTransforms.WristL = findChildSlowly (bodyTransforms.transform, "L Palm");
			bodyTransforms.WristR = findChildSlowly (bodyTransforms.transform, "R Palm");
			bodyTransforms.Neck = findChildSlowly (bodyTransforms.transform, "Neck");
			bodyTransforms.Hips = findChildSlowly (bodyTransforms.transform, "Hips");
		
			SkinnedMeshRenderer[] meshRenderers = Selection.activeGameObject.transform.GetComponentsInChildren <SkinnedMeshRenderer> ();
			CharacterBody cb = Selection.activeGameObject.GetComponent <CharacterBody> ();
			cb.Renderers.Clear ();
			cb.Renderers.AddRange (meshRenderers);
		}
	}

	[MenuItem ("Frontiers/Characters/Audo find body transforms")]
	static void AudioFindBodyTransforms ()
	{
		foreach (GameObject selectedObject in Selection.gameObjects) {
			BodyTransforms bodyTransforms = selectedObject.GetComponent <BodyTransforms> ();

			bodyTransforms.Hips = findChildSlowly (bodyTransforms.transform, "Pelvis");
			bodyTransforms.Head = findChildSlowly (bodyTransforms.transform, "Head");
			if (bodyTransforms.Head != null) {
				bodyTransforms.HeadTop = bodyTransforms.Head.gameObject.FindOrCreateChild ("HeadTop").transform;
			}
			bodyTransforms.Chest = findChildSlowly (bodyTransforms.transform, "Spine3");
			bodyTransforms.FaceJaw = findChildSlowly (bodyTransforms.transform, "_jaw");
			bodyTransforms.Finger1L = findChildSlowly (bodyTransforms.transform, "LDigit21");
			bodyTransforms.Finger2L = findChildSlowly (bodyTransforms.transform, "LDigit31");
			bodyTransforms.Finger3L = findChildSlowly (bodyTransforms.transform, "LDigit41");
			bodyTransforms.Finger4L = findChildSlowly (bodyTransforms.transform, "LDigit51");
			bodyTransforms.ThumbL = findChildSlowly (bodyTransforms.transform, "LDigit11");
			bodyTransforms.Finger1R = findChildSlowly (bodyTransforms.transform, "RDigit21");
			bodyTransforms.Finger2R = findChildSlowly (bodyTransforms.transform, "RDigit31");
			bodyTransforms.Finger3R = findChildSlowly (bodyTransforms.transform, "RDigit41");
			bodyTransforms.Finger4R = findChildSlowly (bodyTransforms.transform, "RDigit51");
			bodyTransforms.ThumbR = findChildSlowly (bodyTransforms.transform, "RDigit11");
			bodyTransforms.FootL = findChildSlowly (bodyTransforms.transform, "LFoot");
			bodyTransforms.FootR = findChildSlowly (bodyTransforms.transform, "RFoot");
			bodyTransforms.KneeL = findChildSlowly (bodyTransforms.transform, "LCalf");
			bodyTransforms.KneeR = findChildSlowly (bodyTransforms.transform, "RCalf");
			bodyTransforms.LegL = findChildSlowly (bodyTransforms.transform, "LThigh");
			bodyTransforms.LegR = findChildSlowly (bodyTransforms.transform, "RThigh");
			bodyTransforms.ElbowL = findChildSlowly (bodyTransforms.transform, "LForearm");
			bodyTransforms.ElbowR = findChildSlowly (bodyTransforms.transform, "RForearm");
			bodyTransforms.ShoulderL	= findChildSlowly (bodyTransforms.transform, "LUpperarm");
			bodyTransforms.ShoulderR	= findChildSlowly (bodyTransforms.transform, "RUpperarm");		
			bodyTransforms.WristL = findChildSlowly (bodyTransforms.transform, "LPalm");
			bodyTransforms.WristR = findChildSlowly (bodyTransforms.transform, "RPalm");
			bodyTransforms.Neck = findChildSlowly (bodyTransforms.transform, "Neck");
			bodyTransforms.Hips = findChildSlowly (bodyTransforms.transform, "Hips");

			SkinnedMeshRenderer[] meshRenderers = Selection.activeGameObject.transform.GetComponentsInChildren <SkinnedMeshRenderer> ();
			CharacterBody cb = Selection.activeGameObject.GetComponent <CharacterBody> ();
			cb.Renderers.Clear ();
			cb.Renderers.AddRange (meshRenderers);

			Vector3 headTopPosition = bodyTransforms.HeadTop.position;
			headTopPosition.y = 0f;
			foreach (Renderer renderer in cb.Renderers) {
				if (renderer.bounds.extents.y > headTopPosition.y) {
					headTopPosition.y = renderer.bounds.extents.y;
				}
			}
			bodyTransforms.HeadTop.position = headTopPosition;
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

	[MenuItem ("Frontiers/Characters/Copy Wearable Parts")]
	static void CopyWearableParts ()
	{
		GameObject activeCharacterGo = Selection.gameObjects [1];
		GameObject secondaryCharacterGo	= Selection.gameObjects [0];

		CharacterBody body2 = activeCharacterGo.GetComponent <CharacterBody> ();
		CharacterBody body1 = secondaryCharacterGo.GetComponent <CharacterBody> ();

		if (body1.WearableParts.Count <= 0) {
			CharacterBody body2a = body1;
			body1 = body2;
			body2 = body2a;
		}

		Debug.Log ("Copying wearable parts from " + body1.name + " to " + body2.name);

		foreach (WearablePart wearablePart in body1.WearableParts) {
			string searchName = wearablePart.name;
			Debug.Log ("Looking for wearable part " + wearablePart.name);
			Transform otherWearablePartTransform = null;
			otherWearablePartTransform = findChildSlowly (body2.transform, searchName);
			if (otherWearablePartTransform == null) {
				//find the parent and create it under that
				searchName = wearablePart.transform.parent.name.Replace (body1.TransformPrefix, body2.TransformPrefix);
				Debug.Log ("Looking for parent " + wearablePart.transform.parent.name + " as " + searchName);
				Transform otherWearablePartParent = findChildSlowly (body2.transform, searchName);
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

		foreach (EquippablePart equippablePart in body1.EquippableParts) {
			string searchName = equippablePart.name;//.Replace (body2.TransformPrefix, body1.TransformPrefix);
			Debug.Log ("Looking for equippable part " + equippablePart.name);
			Transform otherEquippablePartTransform = null;
			otherEquippablePartTransform = findChildSlowly (body2.transform, searchName);
			if (otherEquippablePartTransform == null) {
				//find the parent and create it under that
				searchName = equippablePart.transform.parent.name.Replace (body1.TransformPrefix, body2.TransformPrefix);
				Debug.Log ("Looking for parent " + equippablePart.transform.parent.name + " as " + searchName);
				Transform otherEquippablePartParent = findChildSlowly (body2.transform, searchName);
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
	}

	[MenuItem ("Frontiers/Characters/Copy Body Parts")]
	static void CopyBodyParts ()
	{
		GameObject activeCharacterGo = Selection.gameObjects [1];
		GameObject secondaryCharacterGo	= Selection.gameObjects [0];

		WorldBody body2 = activeCharacterGo.GetComponent <WorldBody> ();
		WorldBody body1 = secondaryCharacterGo.GetComponent <WorldBody> ();

		if (body1.BodyParts.Count <= 0) {
			//swap
			WorldBody body2a = body1;
			body1 = body2;
			body2 = body2a;
		}

		Debug.Log ("Copying body parts from " + body1.name + " to " + body2.name);

		foreach (BodyPart bodyPart in body1.BodyParts) {
			Transform otherBodyPartTransform = null;
			Transform[] transforms = body2.GetComponentsInChildren <Transform> ();
			string searchName = bodyPart.name.Replace (body1.TransformPrefix, body2.TransformPrefix);
			bool foundPart = false;
			foreach (Transform bodyPartTransform in transforms) {
				if (bodyPartTransform.name == searchName) {
					otherBodyPartTransform = bodyPartTransform;
					foundPart = true;
					break;
				}
			}

			if (!foundPart) {
				//search using body parts

			}

			if (otherBodyPartTransform != null) {
				BodyPart otherBodyPart = otherBodyPartTransform.gameObject.GetOrAdd <BodyPart> ();
				otherBodyPart.Type = bodyPart.Type;
				SphereCollider sphereCollider = bodyPart.GetComponent <SphereCollider> ();
				if (sphereCollider == null) {
					BoxCollider boxCollider = bodyPart.GetComponent <BoxCollider> ();
					if (boxCollider == null) {
						CapsuleCollider capsuleCollider = bodyPart.GetComponent <CapsuleCollider> ();
						if (capsuleCollider != null) {
							CapsuleCollider otherCapCollider = otherBodyPart.gameObject.GetOrAdd <CapsuleCollider> ();
							otherCapCollider.radius = capsuleCollider.radius;
							otherCapCollider.height = capsuleCollider.height;
							otherCapCollider.direction = capsuleCollider.direction;
							otherCapCollider.center = capsuleCollider.center;
						}
					} else {
						BoxCollider otherBoxCollider = otherBodyPart.gameObject.GetOrAdd <BoxCollider> ();
						otherBoxCollider.size = boxCollider.size;
						otherBoxCollider.center = boxCollider.center;
					}
				} else {
					SphereCollider otherSphereCollider	= otherBodyPart.gameObject.GetOrAdd <SphereCollider> ();
					otherSphereCollider.radius = sphereCollider.radius;
					otherSphereCollider.center = sphereCollider.center;
				}

				if (bodyPart.ParentPart != null) {
					Transform parentBodyPartTransform = body2.transform.FindChild (bodyPart.ParentPart.name);
					if (parentBodyPartTransform != null) {
						BodyPart parentBodyPart = parentBodyPartTransform.GetComponent <BodyPart> ();
						if (parentBodyPart != null) {
							otherBodyPart.ParentPart = parentBodyPart;
						}
					}
				}
			} else {
				Debug.Log ("Couldn't find " + bodyPart.transform.name + " as " + searchName);
			}
		}
	}

	[MenuItem ("Frontiers/Characters/Audo-detect body parts")]
	static void AudoDetectBodyParts ()
	{
		foreach (GameObject selection in Selection.gameObjects) {
			WorldBody body = selection.GetComponent <WorldBody> ();
		
			Component[] bodyPartComponents = selection.GetComponentsInChildren (typeof(BodyPart));
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

			Component[] wearablePartComponents = selection.GetComponentsInChildren (typeof(WearablePart));
			List <WearablePart> wearableParts = body.WearableParts;
			wearableParts.Clear ();

			foreach (Component wearablePart in wearablePartComponents) {
				WearablePart componentAsWearablePart = wearablePart as WearablePart;
				if (componentAsWearablePart != null) {
					wearableParts.Add (componentAsWearablePart);
				}
			}

			Component[] equippablePartComponents = selection.GetComponentsInChildren (typeof(EquippablePart));
			List <EquippablePart> equippableParts = body.EquippableParts;
			equippableParts.Clear ();

			foreach (Component equippablePart in equippablePartComponents) {
				EquippablePart componentAsEquippablePart = equippablePart as EquippablePart;
				if (componentAsEquippablePart != null) {
					equippableParts.Add (componentAsEquippablePart);
				}
			}
		}
	}

	[MenuItem ("Frontiers/Creatures/Audo-detect body parts")]
	static void CreatureAudoDetectBodyParts ()
	{
		WorldItem characterWorldItem = Selection.activeGameObject.GetComponent <WorldItem> ();
		CharacterBody body = Selection.activeGameObject.GetComponent <CharacterBody> ();
		Creature animal = Selection.activeGameObject.GetComponent <Creature> ();

		Component[] bodyPartComponents = Selection.activeGameObject.GetComponentsInChildren (typeof(BodyPart));
		List <BodyPart> bodyParts = null;
//		if (animal != null)
//		{
//			characterWorldItem.Colliders.Clear ( );
//			bodyParts = animal.BodyParts;
//		}
		if (body != null) {
			if (body.BodyParts == null) {
				body.BodyParts = new List <BodyPart> ();
			}
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

				if (animal != null) {
					characterWorldItem.Colliders.Add (componentAsBodyPart.collider);
					componentAsBodyPart.Owner = characterWorldItem;
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
	}
}