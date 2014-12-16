using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using ExtensionMethods;
using System;

namespace Frontiers.GUI
{
		[ExecuteInEditMode]
		public class GUITabs : GUIObject, IGUITabOwner
		{
				public IGUITabOwner Owner;
				public int NumColumns = 3;
				public float TabButtonWidth = 100f;
				public float TabButtonHeight = 50f;

				public Action OnShow { get; set; }

				public Action OnHide { get; set; }

				public Action OnSetSelection;
				public List <GUITabButton> Buttons = new List <GUITabButton>();
				public List <GUITabPage> Pages = new List <GUITabPage>();
				public List <GUITabs> SubTabs = new List<GUITabs>();

				public string SelectedTab {
						get {
								if (string.IsNullOrEmpty(mLastPage)) {
										return DefaultPanel;
								}
								return mLastPage;
						}
				}

				public bool ManagesPanel(UIPanel panel)
				{
						for (int i = 0; i < Pages.Count; i++) {
								if (Pages[i].Panels.Contains(panel)) {
										return true;
								}
						}
						return false;
				}

				public string DefaultPanel = string.Empty;

				public bool Visible { 
						get {
								if (!mInitialized) {
										return false;
								}
								return Owner.Visible;
						}
				}

				public virtual bool CanShowTab(string tabName, GUITabs tabs)
				{
						return true;
				}

				public void Initialize(IGUITabOwner owner)
				{
						if (owner == this) {
								Debug.Log ("TABS CANNOT OWN THEMSELVES IN " + name);
								return;
						}

						Owner = owner;
						Owner.OnShow += Show;
						Owner.OnHide += Hide;
						Buttons.Clear();
						Pages.Clear();

						foreach (Transform child in transform) {
								GUITabButton tabButton = null;
								if (child.gameObject.HasComponent <GUITabButton>(out tabButton)) {
										tabButton.name = name + "-" + tabButton.Name;
										Buttons.Add(tabButton);
								}

								GUITabPage page = null;
								if (child.gameObject.HasComponent <GUITabPage>(out page)) {
										Pages.Add(page);
										GUITabs subTabs = null;
										if (child.gameObject.HasComponent <GUITabs>(out subTabs)) {
												page.SubTabs = subTabs;
												subTabs.Initialize(this);
												SubTabs.Add(subTabs);
										}
								}
						}

						for (int i = 0; i < Pages.Count; i++) {
								for (int j = 0; j < Buttons.Count; j++) {
										if (Pages[i].name == Buttons[j].Name) {
												Pages[i].Initialize(this, Buttons[j]);
												Buttons[j].Initialize(this, Pages[i]);
										}
								}
						}

						for (int i = 0; i < Buttons.Count; i++) {
								if (!Buttons[i].Initialized) {
										Buttons[i].Initialize(this, null);
								}
						}

						mInitialized = true;

						if (Visible) {
								Show();
						} else {
								Hide();
						}
				}

				public void OnClickButton(GUITabButton tabButton)
				{
						if (!mInitialized)
								return;

						SetSelection(tabButton.Name);
				}

				public void Hide()
				{
						if (!mInitialized)
								return;

						for (int i = 0; i < Pages.Count; i++) {
								Pages[i].Hide();
						}
						OnHide.SafeInvoke();
				}

				public void Show()
				{
						if (!mInitialized)
								return;

						OnShow.SafeInvoke();
						SetSelection(DefaultPanel);
				}

				public void Refresh()
				{
						if (!mInitialized)
								return;
				}

				public void ShowLastPage()
				{
						SetSelection(mLastPage);
				}

				public void SetSelection(string tabName)
				{
						if (!mInitialized)
								return;
							
						if (string.IsNullOrEmpty(tabName)) {
								tabName = DefaultPanel;
						}

						for (int i = 0; i < Buttons.Count; i++) {
								GUITabButton tabButton = Buttons[i];
								if (tabButton.Name == tabName) {
										tabButton.Selected = true;
								} else {
										tabButton.Selected = false;
								}
						}
						mLastPage = tabName;
						OnSetSelection.SafeInvoke();
				}

				protected bool mEnabledOnce = false;
				protected string mLastPage = string.Empty;
		}

		public interface IGUITabPageChild
		{
				void Hide();

				void Show();
		}

		public interface IGUITabOwner
		{
				bool Visible { get; }

				bool CanShowTab(string tabName, GUITabs tabs);

				Action OnShow { get; set; }

				Action OnHide { get; set; }
		}
}
