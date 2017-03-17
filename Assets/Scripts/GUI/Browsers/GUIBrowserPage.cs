using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using ExtensionMethods;

namespace Frontiers.GUI
{
		public class GUIBrowserPage : MonoBehaviour
		{
		}

		public class GUIBrowserPageObject
		{
		}

		[Serializable]
		public class GUIGenericObject
		{
				public GUIGenericObject()
				{

				}

				public GUIGenericObject(GameObject item)
				{
						Elements = new List<NGUIElement>();
						Functions = new List<NGUIFunction>();
						Children = new List<GUIGenericObject>();
			
						Name = item.name;
						PrefabType = NGUIPrefab.None;
						Transform.Position	= item.transform.localPosition;
						Transform.Rotation	= item.transform.localRotation.eulerAngles;
						Transform.Scale = item.transform.localScale;
						Layer = item.gameObject.layer;

						if (item.GetComponent<Collider>() != null) {
								HasCollider = true;
								ColliderCenter = item.GetComponent<Collider>().bounds.center;
								ColliderScale	= item.GetComponent<Collider>().bounds.size;
						} else {
								HasCollider = false;
						}

						//First check to see if it's a prefab
						if (item.HasComponent <GUIObject>()) {
								GUIObject prefab = item.GetComponent <GUIObject>();
								PrefabType = NGUIPrefab.Standard;
								PrefabName = prefab.PrefabName;
								InitArgument = prefab.InitArgument;
						}

						if (PrefabType == NGUIPrefab.None) {
								if (item.gameObject.HasComponent <UIPanel>()) {
										Elements.Add(NGUIElement.Panel);
								}

								UIWidget widget = item.GetComponent <UIWidget>();
								bool isSprite	= false;
								bool isLabel	= false;

								if (widget != null) {
										Depth = widget.depth;
										Pivot = widget.pivot;

										if (item.gameObject.HasComponent <UILabel>()) {
												Elements.Add(NGUIElement.Label);
												isLabel = true;
												isSprite = true;
										} else if (item.gameObject.HasComponent <UISlicedSprite>()) {
												Elements.Add(NGUIElement.SlicedSprite);
												isSprite = true;
										} else if (item.gameObject.HasComponent <UITiledSprite>()) {
												Elements.Add(NGUIElement.TiledSprite);
												isSprite	= true;
										} else if (item.gameObject.HasComponent <UISprite>()) {
												Elements.Add(NGUIElement.Sprite);
												isSprite	= true;
										}

										if (isSprite) {
												UISprite sprite = item.GetComponent <UISprite>();
												AtlasName = sprite.atlas.name;
												SpriteName = sprite.spriteName;
												TintColor = sprite.color;
										}

										if (isLabel) {
												UILabel label = item.GetComponent <UILabel>();
												EffectColor = label.effectColor;
												LabelEffect = label.effectStyle;
										}
								}

								foreach (Transform child in item.transform) {
										Children.Add(new GUIGenericObject(child.gameObject));
								}
						}
				}

				public string Name;
				public STransform Transform;
				public NGUIPrefab PrefabType;
				public string PrefabName;
				public string InitArgument;
				public int Depth;
				public int Layer;
				public List <NGUIElement> Elements;
				public UIWidget.Pivot Pivot;
				public string AtlasName;
				public string SpriteName;
				public string FontName;
				public SColor TintColor;
				public SColor EffectColor;
				public UILabel.Effect LabelEffect;
				public List <NGUIFunction> Functions;
				public string ButtonMessage;
				public string ComponentName;
				public bool HasCollider;
				public SVector3 ColliderScale;
				public SVector3 ColliderCenter;
				public List <GUIGenericObject>	Children;
		}
}