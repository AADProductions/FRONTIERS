using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.GUI
{
		public class GUITabPage : GUIObject
		{
				public GUITabs TabParent;
				public GUITabs SubTabs;

				public bool HasSubTabs {
						get {
								return SubTabs != null;
						}
				}

				public GUITabButton Button;
				public Action OnSelected;
				public Action OnRefreshed;
				public Action OnDeselected;
				public List <UIPanel> Panels = new List <UIPanel>();
				public List <IGUITabPageChild> Children = new List<IGUITabPageChild>();

				public bool Selected {
						get {
								return Button.Selected;
						}
				}

				public void GetActiveInterfaceObjects(List<FrontiersInterface.Widget> currentObjects)
				{
						if (HasSubTabs) {
								SubTabs.GetActiveInterfaceObjects(currentObjects);
						} else {
								for (int i = 0; i < Panels.Count; i++) {
										if (Panels[i].gameObject.layer != Globals.LayerNumGUIRaycastIgnore) {
												FrontiersInterface.GetActiveInterfaceObjectsInTransform(Panels[i].transform, NGUICamera, currentObjects);
										}
								}
						}
				}

				public void Show()
				{
						gameObject.SetLayerRecursively(Globals.LayerNumGUIRaycast, Globals.TagIgnoreTab);
						for (int i = 0; i < Panels.Count; i++) {
								Panels[i].enabled = true;
						}
						for (int i = 0; i < Children.Count; i++) {
								Children[i].Show();
						}
						if (HasSubTabs) {
								SubTabs.Show();
						}
				}

				public void Hide()
				{
						gameObject.SetLayerRecursively(Globals.LayerNumGUIRaycastIgnore, Globals.TagIgnoreTab);
						for (int i = 0; i < Panels.Count; i++) {
								Panels[i].enabled = false;
						}
						for (int i = 0; i < Children.Count; i++) {
								Children[i].Hide();
						}
						if (HasSubTabs) {
								SubTabs.Hide();
						}
				}

				public void Initialize(GUITabs tabParent, GUITabButton button)
				{
						if (mInitialized) {
								return;
						}

						NGUICamera = tabParent.NGUICamera;
						TabParent = tabParent;
						Button = button;
						button.Page = this;
						UIPanel[] panels = gameObject.GetComponentsInChildren <UIPanel>(true);
						for (int i = 0; i < panels.Length; i++) {
								//make sure the panel isn't being managed by a sub-tab
								bool addPanel = true;
								for (int j = 0; j < TabParent.SubTabs.Count; j++) {
										if (TabParent.SubTabs[j].ManagesPanel(panels[i])) {
												addPanel = false;
										}
								}
								if (addPanel) {
										Panels.Add(panels[i]);
								}
						}
						UIPanel mainPanel = null;
						if (gameObject.HasComponent <UIPanel>(out mainPanel)) {
								Panels.SafeAdd(mainPanel);
						}

						Component[] tabPageChildren = gameObject.GetComponentsInChildren(typeof(IGUITabPageChild), true);
						for (int i = 0; i < tabPageChildren.Length; i++) {
								IGUITabPageChild tabPageChild = tabPageChildren[i] as IGUITabPageChild;
								Children.Add(tabPageChild);
						}

						OnSelected += Show;
						OnDeselected += Hide;
						TabParent.OnHide += Hide;

						mInitialized = true;
				}

				protected static List <BoxCollider> gGetColliders = new List<BoxCollider>();
		}
}
