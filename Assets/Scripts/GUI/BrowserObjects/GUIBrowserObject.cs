using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.GUI
{
		public class GUIBrowserObject : GUIBrowserObjectBase
		{
				public static bool GetBrowserObjectScrollbar(Transform browserObjectTransform, out UIScrollBar scrollBar)
				{
						scrollBar = null;
						int maxLoops = 10;
						int currentLoop = 0;
						while ((currentLoop < maxLoops) && (browserObjectTransform != null) && browserObjectTransform.CompareTag(Globals.TagBrowserObject)) {
								browserObjectTransform = browserObjectTransform.parent;
								currentLoop++;//just in case
						}
						UIDraggablePanel panel = null;
						if (browserObjectTransform != null && browserObjectTransform.CompareTag(Globals.TagActiveObject) && browserObjectTransform.gameObject.HasComponent <UIDraggablePanel>(out panel)) {
								scrollBar = panel.verticalScrollBar;
						}
						return scrollBar != null;
				}

				public static bool GetNextBrowserObject(GameObject startBrowserObject, out GameObject nextBrowserObject)
				{
						nextBrowserObject = null;
						gStartTransform = startBrowserObject.transform;
						gParent = startBrowserObject.transform.parent;
						gBrowserTransforms.Clear();
						foreach (Transform child in gParent) {
								gBrowserTransforms.Add(child);
						}
						gBrowserTransforms.Sort(CompareTransform);
						for (int i = 0; i < gBrowserTransforms.Count; i++) {
								if (gBrowserTransforms[i] == gStartTransform) {
										if (i < gBrowserTransforms.LastIndex()) {
												nextBrowserObject = gBrowserTransforms[i + 1].gameObject;
										}
										break;
								}
						}
						return nextBrowserObject != null;
				}

				public static bool GetPrevBrowserObject(GameObject startBrowserObject, out GameObject prevBrowserObject)
				{
						prevBrowserObject = null;
						gStartTransform = startBrowserObject.transform;
						gParent = startBrowserObject.transform.parent;
						gBrowserTransforms.Clear();
						foreach (Transform child in gParent) {
								gBrowserTransforms.Add(child);
						}
						gBrowserTransforms.Sort(CompareTransform);
						for (int i = 0; i < gBrowserTransforms.Count; i++) {
								if (gBrowserTransforms[i] == gStartTransform) {
										if (i > 0) {
												prevBrowserObject = gBrowserTransforms[i - 1].gameObject;
										}
										break;
								}
						}
						return prevBrowserObject != null;
				}

				public UILabel Name;
				public UILabel Description;
				public UIButtonMessage EditButton;
				public UISlicedSprite EditButtonBackground;
				public UISlicedSprite ObjectBackground;
				public GameObject TexturePreview;

				private static int CompareTransform(Transform A, Transform B)
				{
						return A.name.CompareTo(B.name);
				}

				protected static List <Transform> gBrowserTransforms = new List<Transform> ();
				protected static Transform gStartTransform;
				protected static Transform gParent;
		}
}