using UnityEngine;

//using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using Frontiers;
using Frontiers.World.Gameplay;
using Hydrogen.Threading.Jobs;
using System.Xml.Serialization;

namespace Frontiers.World
{
		public class StructureTemplateGroup
		{
				public bool IsEmpty {
						get {
								return StaticStructureLayers.Count == 0
								&& CategoryWorldItems.Count == 0
								&& Triggers.Count == 0
								&& GenericWItems == "\n"
								&& GenericWindows == "\n"
								&& GenericDoors == "\n"
								&& GenericDynamic == "\n"
								&& GenericLights == "\n"
								&& DestroyedFX == "\n"
								&& DestroyedFires == "\n"
								&& ActionNodes.Count == 0
								&& UniqueDynamic.Count == 0
								&& StaticStructureColliders.Count == 0;
						}
				}

				public int NumVertices = 0;
				public string Description = string.Empty;
				public SVector3 GroupOffset = SVector3.zero;
				public SVector3 GroupRotation = SVector3.zero;
				public List <StructureLayer> StaticStructureLayers = new List <StructureLayer>();
				public List <StructureLayer> StaticStructureColliders = new List<StructureLayer>();
				public List <StructureLayer> CustomStructureColliders = new List<StructureLayer>();
				public string GenericWItems = "\n";
				public string GenericWindows = "\n";
				public string GenericDoors = "\n";
				public string GenericDynamic = "\n";
				public string GenericLights = "\n";
				public string DestroyedFX = "\n";
				public string DestroyedFires = "\n";
				public List <StackItem> UniqueDynamic = new List <StackItem>();
				public List <StackItem> UniqueWorlditems = new List <StackItem>();
				public List <WICatItem> CategoryWorldItems = new List<WICatItem>();
				public List <ActionNodeState> ActionNodes = new List <ActionNodeState>();
				public List <DynamicStructureTemplatePiece> UniqueWindows = new List <DynamicStructureTemplatePiece>();
				public List <DynamicStructureTemplatePiece> UniqueDoors = new List <DynamicStructureTemplatePiece>();
				public SDictionary <string, KeyValuePair <string,string>> Triggers = new SDictionary <string, KeyValuePair <string,string>>();
		}

		public class StructureLayer
		{
				public string PackName;
				public string PrefabName;
				public string Tag;
				public int Layer;
				public int NumInstances;
				public int NumVertices;
				public bool EnableSnow = false;
				public StructureDestroyedBehavior DestroyedBehavior = StructureDestroyedBehavior.None;
				public SDictionary <string,string> Substitutions = new SDictionary <string, string>();
				public List <string> AdditionalMaterials = new List <string>();
				public string Instances = "\n";

				public static bool AdditionalMaterialsMatch(List <string> mats, List <string> other)
				{
						if (mats == null) {
								return (other == null || other.Count == 0);
						}

						if (mats.Count == 0) {
								return (other == null || other.Count == 0);
						}

						if (mats.Count != other.Count) {
								//Debug.Log ("Different counts, not the same");
								return false;
						}

						for (int i = 0; i < mats.Count; i++) {
								if (mats[i] != other[i]) {
										return false;
								}
						}

						return false;
				}

				public static bool SubstitutionsMatch(SDictionary<string, string> substitutions, SDictionary<string, string> other)
				{
						if (substitutions == null) {
								return (other == null || other.Count == 0);
						}

						if (substitutions.Count == 0) {
								return (other == null || other.Count == 0);
						}

						if (substitutions.Count != other.Count) {
								return false;
						}

						//we're down to the wire now, have to check each entry
						foreach (KeyValuePair <string,string> sub in substitutions) {
								string otherSubValue = string.Empty;
								if (!other.TryGetValue(sub.Key, out otherSubValue) || otherSubValue != sub.Value) {
										//other contained entry that didn't match sub entry
										return false;
								}
						}
						//wow they're actually the same
						return true;
				}

				public static string AddInstance(string instances, Transform child)
				{
						return AddInstance(instances, child, null, StructureDestroyedBehavior.None);
				}

				public static string AddInstance(string instances, Transform child, Transform module)
				{
						return AddInstance(instances, child, module, StructureDestroyedBehavior.None);
				}

				public static string AddInstance(string instances, Transform child, Transform module, StructureDestroyedBehavior destroyedBehavior)
				{
						List <string> childLayerPieces	= new List <string>();
						//packname,childname,xpos,ypos,zpos,xrot,yrot,zrot,xscl,yscl,zscl,mat1|mat2|mat3\n\r
						childLayerPieces.Add("[Default]");
						childLayerPieces.Add("[Default]");
						//if the module is null then this is a direct parent
						//so local transform will be sufficient
						if (module == null) {
								childLayerPieces.Add(child.transform.localPosition.x.ToString());
								childLayerPieces.Add(child.transform.localPosition.y.ToString());
								childLayerPieces.Add(child.transform.localPosition.z.ToString());
								childLayerPieces.Add(child.transform.localRotation.eulerAngles.x.ToString());
								childLayerPieces.Add(child.transform.localRotation.eulerAngles.y.ToString());
								childLayerPieces.Add(child.transform.localRotation.eulerAngles.z.ToString());
								childLayerPieces.Add(child.transform.localScale.x.ToString());
								childLayerPieces.Add(child.transform.localScale.y.ToString());
								childLayerPieces.Add(child.transform.localScale.z.ToString());
						} else {
								//if module is not null then we need to transform the position using the module transform
								if (gModuleHelper == null) {
										if (Structures.Get == null) {
												Manager.WakeUp <Structures>("Frontiers_Structures");
										}
										gModuleHelper = Structures.Get.gameObject.FindOrCreateChild("ModuleHelper");
								}
								gModuleHelper.parent = module.parent;
								gModuleHelper.position = child.position;
								gModuleHelper.rotation = child.rotation;
								gModuleHelper.localScale = child.lossyScale;

								Vector3 childPosition = gModuleHelper.localPosition;// module.InverseTransformPoint (child.position);
								Vector3 childRotation = gModuleHelper.localRotation.eulerAngles;//(Quaternion.Inverse (module.localRotation) * child.rotation).eulerAngles;
								Vector3 childScale = gModuleHelper.localScale;//child.lossyScale;

								childLayerPieces.Add(childPosition.x.ToString());
								childLayerPieces.Add(childPosition.y.ToString());
								childLayerPieces.Add(childPosition.z.ToString());
								childLayerPieces.Add(childRotation.x.ToString());
								childLayerPieces.Add(childRotation.y.ToString());
								childLayerPieces.Add(childRotation.z.ToString());
								childLayerPieces.Add(childScale.x.ToString());
								childLayerPieces.Add(childScale.y.ToString());
								childLayerPieces.Add(childScale.z.ToString());
						}
						childLayerPieces.Add("[Default]");
						childLayerPieces.Add(((int)destroyedBehavior).ToString());
						return instances + (childLayerPieces.JoinToString(",\t") + "\n");
				}

				public static ChildPiece [] ExtractInstances(string instances)
				{
						List <string> splitLayer = Data.GameData.SplitString (instances, new string [] { "\n" });
						//string[] splitLayer = instances.Split(new string [] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
						ChildPiece[] childPieces = new ChildPiece [splitLayer.Count];
						if (splitLayer.Count > 0) {
								for (int i = 0; i < splitLayer.Count; i++) {
										ChildPiece childPiece = new ChildPiece();
										List <string> splitChild = Data.GameData.SplitString (splitLayer[i], new string [] { ",\t" });
										//string[] splitChild = splitLayer[i].Split(new string [] { ",\t" }, StringSplitOptions.RemoveEmptyEntries);
										//0:packname, 1:childname, 2:xpos, 3:ypos, 4:zpos, 5:xrot, 6:yrot, 7:zrot, 8:xscl, 9:yscl, 10:zscl, 11:mat1|mat2|mat3, 12:destroyedBehavior\n\r
										childPiece.PackName = splitChild[0];
										childPiece.ChildName = splitChild[1];
										childPiece.Position = Vector3.zero;
										childPiece.Position.x = float.Parse(splitChild[2]);
										childPiece.Position.y = float.Parse(splitChild[3]);
										childPiece.Position.z = float.Parse(splitChild[4]);
										childPiece.Rotation = Vector3.zero;
										childPiece.Rotation.x = float.Parse(splitChild[5]);
										childPiece.Rotation.y = float.Parse(splitChild[6]);
										childPiece.Rotation.z = float.Parse(splitChild[7]);
										childPiece.Scale = Vector3.zero;
										childPiece.Scale.x = float.Parse(splitChild[8]);
										childPiece.Scale.y = float.Parse(splitChild[9]);
										childPiece.Scale.z = float.Parse(splitChild[10]);
										childPiece.Materials = new string[0];
										if (splitChild.Count > 11) {
												childPiece.DestroyedBehavior = Int32.Parse(splitChild[11]);
										}
										childPieces[i] = childPiece;
										splitChild.Clear();
										splitChild = null;
								}
						}
						splitLayer.Clear();
						splitLayer = null;
						return childPieces;
				}

				public static Transform gModuleHelper = null;
		}

		[Serializable]
		public class StaticStructureTemplatePiece
		{
				public StaticStructureTemplatePiece(Transform piece, string packName)
				{
						Rotation = piece.transform.localRotation.eulerAngles;
						Scale = piece.transform.localScale;
						Position = piece.transform.localPosition;
						PrefabName = piece.name;
						PackName = packName;

						if (piece.renderer != null) {
								MaterialName = piece.renderer.sharedMaterial.name.Replace(" (Instance)", string.Empty);
						} else {
								MaterialName = string.Empty;
						}
				}

				public StaticStructureTemplatePiece()
				{
						PrefabName = string.Empty;
						Position = SVector3.zero;
						Rotation = SVector3.zero;
						Scale = SVector3.one;
						MaterialName = string.Empty;
				}

				public string PackName;
				public string PrefabName;
				public bool SubStructure;
				public SVector3 Position;
				public SVector3 Rotation;
				public SVector3 Scale;
				public string MaterialName;
		}

		[Serializable]
		public class DynamicStructureTemplatePiece : StackItem
		{
				//TODO look into ways to eliminate this now that it's been absorbed into StackItem
		}

		[Serializable]
		public class SubstructureTemplatePiece
		{
				public SubstructureTemplatePiece(Transform piece, string packName)
				{
						Rotation = piece.transform.localRotation.eulerAngles;
						Scale = piece.transform.localScale;
						Position = piece.transform.localPosition;
						ParentPosition = piece.parent.localPosition;
						ParentRotation = piece.parent.localRotation.eulerAngles;
						PrefabName = piece.name;
						PackName = packName;
				}

				public SubstructureTemplatePiece()
				{
						PackName = string.Empty;
						PrefabName = string.Empty;
						Position = SVector3.zero;
						Rotation = SVector3.zero;
						ParentPosition = SVector3.zero;
						ParentRotation = SVector3.zero;
						Scale = SVector3.one;
				}

				public string PackName;
				public string PrefabName;
				public SVector3 Position;
				public SVector3 Rotation;
				public SVector3 Scale;
				public SVector3 ParentPosition;
				public SVector3 ParentRotation;
		}

		public struct LightPiece
		{
				public static LightPiece Empty {
						get {
								return gEmpty;
						}
				}

				public string LightName;
				public string LightType;
				public float LightIntensity;
				public Color LightColor;
				public float LightRange;
				public float LightSpotAngle;
				public Vector3 Position;
				public Vector3 Rotation;

				public static LightPiece gEmpty;
		}

		public struct FXPiece
		{
				public string FXName;
				public string SoundName;
				public MasterAudio.SoundType SoundType;
				public float Delay;
				public float Duration;
				public float Scale;
				public Color FXColor;
				public Vector3 Position;
				public Vector3 Rotation;
				public bool Explosion;
				public bool JustForShow;
				[XmlIgnore]
				[NonSerialized]
				[HideInInspector]
				public Transform FXParent;
				[XmlIgnore]
				[NonSerialized]
				[HideInInspector]
				public double TimeAdded;

				public static FXPiece Empty {
						get {
								return gEmpty;
						}
				}

				public static FXPiece gEmpty;
		}

		public struct ChildPiece
		{
				public static ChildPiece Empty {
						get {
								return gEmpty;
						}
				}

				public string PackName;
				public string ChildName;
				public Vector3 Position;
				public Vector3 Rotation;
				public Vector3 Scale;

				public STransform Transform {
						get {
								//TODO this is BAD find a way to replace it
								return new STransform(Position, Rotation, Scale);
						}
				}

				public string[] Materials;
				public int DestroyedBehavior;
				public static ChildPiece gEmpty;
// = new ChildPiece (); //TODO this is BAD
		}
}