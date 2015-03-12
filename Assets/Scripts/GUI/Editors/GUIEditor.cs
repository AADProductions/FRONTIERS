using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using System;

#pragma warning disable 0219//TODO get rid of this crap
namespace Frontiers.GUI
{
		public abstract class GUIEditor <R> : SecondaryInterface, IGUIChildEditor <R>, IGUIChildEditor
		{
				public GUIEditor()
				{
						mGUIEditorID = GUIManager.GetNextGUIID();
				}

				public ulong GUIEditorID {
						get {
								return mGUIEditorID;
						}
				}

				public R EditObject {
						get {
								return mEditObject;
						}
				}

				public bool HasEditObject {
						get {
								return mEditObject != null;
						}
				}

				public GameObject NGUIObject {
						get;
						set;
				}

				public GameObject NGUIParentObject {
						get;
						set;
				}

				public virtual bool IsFinished {
						get {
								return mFinished;
						}
				}

				public void ReceiveFromParentEditor(R editObject)
				{
						ReceiveFromParentEditor(editObject, null);
				}

				public virtual void ReceiveFromParentEditor(R editObject, ChildEditorCallback <R> callBack)
				{
						mEditObject = editObject;
						if (callBack != null) {
								//this will preserve the callback
								//if we call this again to reset items
								mCallBack = callBack;
						}

						if (Manager.IsAwake <GUIManager>()) {
								GUIManager.Get.GetFocus(this);
						}
						//if it's a generic dialog result subscribe to the refresh action
						GenericDialogResult gdr = mEditObject as GenericDialogResult;
						if (gdr != null) {
								gdr.RefreshAction += Refresh;
						}
						PushEditObjectToNGUIObject();
						if (!Visible) {
								Show();
						}
				}

				public virtual void Refresh()
				{
						PushEditObjectToNGUIObject();
				}

				protected override void OnFinish()
				{
						if (mFinished) {
								return;
						}

						//if the edit object inherits from GenericDialogResult set its EditorFinished property here
						GenericDialogResult gdr = mEditObject as GenericDialogResult;
						if (gdr != null) {
								gdr.EditorFinished = true;
								gdr.RefreshAction -= Refresh;
						}

						if (mCallBack != null) {
								//if the callback isn't null, then the parent will take care of the transition
								mCallBack(mEditObject, this);
						}

						base.OnFinish();
				}

				public abstract void PushEditObjectToNGUIObject();

				protected virtual void OnEditObjectChange()
				{

				}

				protected ulong mGUIEditorID = 0;
				protected GameObject mNGUIObject = null;
				protected R mEditObject = default (R);
				protected ChildEditorCallback <R> mCallBack = null;
		}

		public interface IGUIBrowser {
				bool UsesScrollBar { get; }
				UIScrollBar BrowserScrollBar { get; }
				List <IGUIBrowserObject> BrowserObjectsList { get; }
				UIDraggablePanel BrowserPagePanel { get; }
				UIPanel BrowserClipPanel { get; }
		}

		public abstract class GUIBrowser <R> : GUIEditor <IEnumerable <R>>, IGUIChildEditor <IEnumerable <R>>, IGUIBrowser
		{
				public GameObject BrowserObjectPrototype;
				public GameObject BrowserObjectsParent;
				public GameObject DividerObjectPrototype;
				public UIScrollBar ScrollBar;
				public float GridPadding = 0f;
				public float ScrollBarSensitivity = 0.05f;

				/*public override void GetActiveInterfaceObjects(List<Widget> currentObjects)
				{
						FrontiersInterface.Widget w = new Widget();
						w.SearchCamera = NGUICamera;
						for (int i = 0; i < mBrowserObjectCollidersList.Count; i++) {
								w.Collider = mBrowserObjectCollidersList[i];
								currentObjects.Add(w);
						}
						w.Collider = ScrollBar.foreground.GetComponent <BoxCollider>();
						currentObjects.Add(w);
				}*/

				public virtual IEnumerable <R> FetchItems()
				{
						return null;
				}

				public override void Hide()
				{
						if (!Visible)
								return;

						ClearAll();
						if (mBrowserPagePanel != null) {
								mBrowserPagePanel.enabled = false;
						}
						base.Hide();
				}

				public override void Show()
				{
						if (Visible)
								return;

						base.Show();
						if (mBrowserPagePanel != null) {
								mBrowserPagePanel.enabled = true;
						}
						IEnumerable <R> items = FetchItems();
						if (items != null) {
								ReceiveFromParentEditor(items);
						} else {
								Refresh();
						}
				}

				public List <IGUIBrowserObject> BrowserObjectsList {
						get {
								return mBrowserObjectsList;
						}
				}

				public R SelectedObject {
						get {
								return mSelectedObject;
						}
				}

				public bool UsesGrid {
						get {
								return mGrid != null;
						}
				}

				public bool UsesScrollBar {
						get { 
								return ScrollBar != null;
						}
				}

				public UIScrollBar BrowserScrollBar { get { return ScrollBar; } }

				public UIDraggablePanel BrowserPagePanel { get { return mBrowserPagePanel; } }

				public UIPanel BrowserClipPanel { get { return mBrowserClipPanel; } }

				public override void WakeUp()
				{
						mGrid = BrowserObjectsParent.GetComponent <UIGrid>();
						mBrowserPagePanel = BrowserObjectsParent.GetComponent <UIDraggablePanel>();
						mBrowserClipPanel = BrowserObjectsParent.GetComponent <UIPanel>();
						if (mBrowserPagePanel != null) {
								mBrowserPagePanel.enabled = false;
						}
				}

				public virtual void ClearItems()
				{
						mEditObject = null;
				}

				public virtual void ClearAll()
				{
						//Debug.Log("Clear all in GUIEditor " + name);
						ClearItems();
						ClearBrowserObjects();
						ClearDividerObjects();

						UIDraggablePanel draggablePanel = null;
						if (UsesGrid && mGrid.gameObject.HasComponent <UIDraggablePanel>(out draggablePanel)) {
								if (draggablePanel.verticalScrollBar != null) {
										draggablePanel.verticalScrollBar.scrollValue = 0f;
								}
								draggablePanel.ResetPosition();
						}
				}

				public void AddItems(IEnumerable <R> items)
				{
						if (items == null) {
								//			////Debug.Log ("ITEMS WAS NULL");
								return;
						}

						if (mEditObject == null) {
								mEditObject = items;
						} else {
								mEditObject = Enumerable.Concat <R>(mEditObject, items);
						}
				}

				public void AddItems(IEnumerable <R> items, string dividerText, Color dividerColor)
				{
						int dividerIndex = 0;

						if (items == null) {
								return;
						}

						if (mEditObject == null) {
								mEditObject = items;
						} else {
								foreach (R r in mEditObject) {
										//this is terrible!
										//using because IEnumerable doesn't have a Count property
										//consider switching to ICollection
										dividerIndex++;
								}
								mEditObject = Enumerable.Concat <R>(mEditObject, items);
						}
				}

				public override void PushEditObjectToNGUIObject()
				{
						ClearBrowserObjects();
						ClearDividerObjects();
						CreateBrowserObjects();
						CreateDividerObjects();
						BrandBrowserObjects();
						SetGridProperties();
						AlignBrowserObjects();
				}

				public virtual void CreateDividerObjects()
				{
						//divider objects are created & added to divider array
				}

				public override void Refresh()
				{
						RefreshBrowserObjects();
						AlignBrowserObjects();
						base.Refresh();
				}

				protected IGUIBrowserObject CreateDivider()
				{
						GameObject newDividerObjectGameObject = NGUITools.AddChild(BrowserObjectsParent, DividerObjectPrototype);
						IGUIBrowserObject newDividerObject = (IGUIBrowserObject)newDividerObjectGameObject.GetComponent(typeof(IGUIBrowserObject));
						mDividers.Add(newDividerObject);
						return newDividerObject;
				}

				public virtual void OnClickBrowserObject(GameObject obj)
				{
						Debug.Log("Clicked browser object " + obj.name);
						if (IsOurBrowserObject(obj, mGUIEditorID) == false) {
								return;
						}
						IGUIBrowserObject dividerObject = (IGUIBrowserObject)obj.GetComponent(typeof(IGUIBrowserObject));
						mSelectedObject = mEditObjectLookup[dividerObject];
				}

				protected virtual IGUIBrowserObject ConvertEditObjectToBrowserObject(R editObject)
				{
						//instantiate using NGUITools
						GameObject newBrowserGameObject = NGUITools.AddChild(BrowserObjectsParent, BrowserObjectPrototype);
						//rename to get ride of (clone)
						IGUIBrowserObject newBrowserObject = (IGUIBrowserObject) newBrowserGameObject.GetComponent(typeof(IGUIBrowserObject));
						newBrowserObject.name = BrowserObjectPrototype.name;

						return newBrowserObject;
				}

				protected virtual void RefreshEditObjectToBrowserObject(R editObject, IGUIBrowserObject browserObject)
				{

				}

				protected void RefreshBrowserObjects()
				{
						foreach (KeyValuePair <IGUIBrowserObject, R> editObjectBrowser in mEditObjectLookup) {
								RefreshEditObjectToBrowserObject(editObjectBrowser.Value, editObjectBrowser.Key);
						}
				}

				protected virtual void ClearDividerObjects()
				{
						if (Application.isPlaying) {
								foreach (IGUIBrowserObject dividerObject in mDividers) {
										GUIManager.TrashNGUIObject(dividerObject.gameObject);
								}

								mDividers.Clear();
						}
				}

				protected virtual void ClearBrowserObjects()
				{
						//Debug.Log("Clearing browser objects");

						if (Application.isPlaying) {
								foreach (IGUIBrowserObject browserObject in mBrowserObjectsList) {
										GUIManager.TrashNGUIObject(browserObject.gameObject);
								}

								mBrowserObjectsList.Clear();
								mEditObjectLookup.Clear();
								mBrowserObjectCollidersList.Clear();
								//reset the grid
								mGrid = BrowserObjectsParent.GetComponent <UIGrid>();
								if (UsesGrid) {
										mGrid.Reposition();
								}
						}
				}

				protected virtual void CreateBrowserObjects()
				{
						if (mEditObject == null) {
								Debug.Log("Edit object is null");
								return;
						}

						foreach (R editObject in mEditObject) {
								IGUIBrowserObject newBrowserObject = ConvertEditObjectToBrowserObject(editObject);
								gBoxColliderChildren.Clear();
								newBrowserObject.gameObject.GetComponentsInChildren<BoxCollider>(true, gBoxColliderChildren);
								mBrowserObjectCollidersList.AddRange(gBoxColliderChildren);
								//check for divider
								mBrowserObjectsList.Add(newBrowserObject);
								mEditObjectLookup.Add(newBrowserObject, editObject);
						}
				}

				protected static List <BoxCollider> gBoxColliderChildren = new List<BoxCollider>();
				protected static List <GUIButtonSetup> gButtonChildren = new List<GUIButtonSetup>();

				protected void BrandBrowserObjects()
				{
						string browserNamePrefix = GetBrowserNamePrefix(mGUIEditorID);
						foreach (IGUIBrowserObject browserObject in mBrowserObjectsList) {
								browserObject.name = browserNamePrefix + browserObject.name;
								foreach (Transform child in browserObject.transform) {
										child.name = browserNamePrefix + child.name;
								}
						}
						foreach (IGUIBrowserObject dividerObject in mDividers) {
								dividerObject.name = browserNamePrefix + dividerObject.name;
						}
				}

				protected void AlignBrowserObjects()
				{
						mGrid = BrowserObjectsParent.GetComponent <UIGrid>();
						if (UsesGrid) {
								mGrid.repositionNow = true;
						} else {
								float padding = GridPadding;
								//browserobjects are expected to either have a box collider on the base
								//OR to have an object called 'Button' that will have a box collider
								BoxCollider bc = BrowserObjectPrototype.GetComponent <BoxCollider>();
								if (bc == null) {
										bc = BrowserObjectPrototype.FindOrCreateChild("Button").GetComponent <BoxCollider>();
								}
								float objectHeight = bc.size.y + padding;

								float currentOffset = 0.0f;
								Vector3	currentPosition = Vector3.zero;
								foreach (IGUIBrowserObject browserObject in mBrowserObjectsList) {
										browserObject.transform.localPosition = currentPosition;
										currentPosition.y = currentOffset;
										//if (browserObject.name.Contains ("DIVIDER")) {
										//	currentOffset -= dividerHeight;
										//} else {
										currentOffset -= objectHeight;
										//}
								}
						}

						if (UsesScrollBar && !mResettingScrollBar) {
								ScrollBar.scrollValue = 0f;
								mResettingScrollBar = true;
								//this will be required if the gameobject is active
								if (gameObject.activeSelf) {
										StartCoroutine(ResetScrollBar());
								}
						}
				}

				protected void SetGridProperties()
				{
						if (UsesGrid) {
								float padding = 20.0f;
								if (GridPadding > 0f) {
										padding = GridPadding;
								}
								BoxCollider bc = BrowserObjectPrototype.GetComponent <BoxCollider>();
								if (bc == null) {
										bc = BrowserObjectPrototype.FindOrCreateChild("Button").GetComponent <BoxCollider>();
								}
								//set grid Y dimensions to the browser object Y dimensions
								mGrid.cellHeight = bc.size.y + padding;
								mGrid.cellWidth = bc.size.x + padding;
						}
				}

				protected IEnumerator ResetScrollBar()
				{
						yield return null;
						ScrollBar.scrollValue = 0f;
						mResettingScrollBar = false;
				}

				protected bool mResettingScrollBar = false;
				protected Dictionary <IGUIBrowserObject,R> mEditObjectLookup = new Dictionary <IGUIBrowserObject,R>();
				protected List <IGUIBrowserObject> mDividers = new List <IGUIBrowserObject>();
				protected IGUIBrowserObject mBrowserObject = null;
				protected List <IGUIBrowserObject> mBrowserObjectsList = new List <IGUIBrowserObject>();
				protected List <BoxCollider> mBrowserObjectCollidersList = new List<BoxCollider>();
				protected R mSelectedObject = default (R);
				protected UIGrid mGrid = null;
				protected UIDraggablePanel mBrowserPagePanel = null;
				protected UIPanel mBrowserClipPanel = null;

				protected class DividerProps
				{
						public DividerProps(int index, string dividerText, Color dividerColor)
						{
								Index = index;
								DividerText = dividerText;
								DividerColor = dividerColor;
						}

						public int Index;
						public string DividerText;
						public Color	DividerColor;
				}

				protected static string GetBrowserNamePrefix(ulong guiEditorID)
				{
						return ("BO_" + guiEditorID.ToString() + "_");
				}

				protected static bool IsOurBrowserObject(GameObject browserObject, ulong guiEditorID)
				{
						return browserObject.name.Contains(GetBrowserNamePrefix(guiEditorID));
				}
		}

		public abstract class GUIBrowserSelectView <R> : GUIBrowser <R>
		{
				public GameObject SelectedObjectViewer;

				public virtual bool PushToViewerAutomatically {
						get {
								return true;
						}
				}

				public override void PushEditObjectToNGUIObject()
				{
						base.PushEditObjectToNGUIObject();

						if (mSelectedObject != null && PushToViewerAutomatically) {
								PushSelectedObjectToViewer();
						}
				}

				public override void OnClickBrowserObject(GameObject obj)
				{
						if (!IsOurBrowserObject(obj, mGUIEditorID)) {
								return;
						}

						mBrowserObject = (IGUIBrowserObject) obj.GetComponent (typeof(IGUIBrowserObject));
						mSelectedObject	= mEditObjectLookup[mBrowserObject];

						if (mBrowserObject.DeleteRequest) {
								mBrowserObject.DeleteRequest = false;//reset the delete request
								DeleteSelectedObject();
						} else {
								PushSelectedObjectToViewer();
						}
				}

				public virtual void DeleteSelectedObject () {

				}

				public virtual void PushSelectedObjectToViewer()
				{
						//update the browser object viewer
				}
		}

		public abstract class GUIBrowserCreateUpdateDelete<R> : GUIBrowser<R>, IGUIParentEditor<R> where R : new()
		{
				public GameObject BrowserObjectCreatePrototype;
				public GameObject NGUICreateEditObjectPrefab;
				public GameObject NGUIUpdateEditObjectPrefab;
				public GameObject NGUIDeleteEditObjectPrefab;

				public virtual void ReceiveFromChildEditor(R editObject, IGUIChildEditor<R> childEditor)
				{
						//clear everything and start over
						PushEditObjectToNGUIObject();
						//as the parent, we are responsible for initiating transitions
						GUIManager.ScaleUpEditor(this.gameObject).Proceed();
						GUIManager.ScaleDownEditor(mLastSpawnedChildEditor).Proceed();
						//retire the GUI - it'll be destroyed after the transition is complete
						GUIManager.RetireGUIChildEditor(mLastSpawnedChildEditor);

						mLastSpawnedChildEditor = null;
				}

				protected override void CreateBrowserObjects()
				{
						//create the browser objects normally
						base.CreateBrowserObjects();

						//then initialize the 'new' button
						GameObject browserObjectCreate = NGUITools.AddChild(BrowserObjectsParent, BrowserObjectCreatePrototype);
						browserObjectCreate.name = "000_" + browserObjectCreate.name;
						GUIBrowserObjectCreate boc = browserObjectCreate.GetComponent<GUIBrowserObjectCreate>();
						boc.CreateButton.target = this.gameObject;
						//add to browser objects list
						mBrowserObjectsList.Add(boc);

				}

				public override void OnClickBrowserObject(GameObject obj)
				{
						if (IsOurBrowserObject(obj, mGUIEditorID) == false) {
								return;
						}

						IGUIBrowserObject browserObject = (IGUIBrowserObject)obj.GetComponent(typeof(IGUIBrowserObject));
						mSelectedObject = mEditObjectLookup[browserObject];
						mLastSpawnedChildEditor = GUIManager.SpawnNGUIChildEditor(this.gameObject, NGUIUpdateEditObjectPrefab);

						GUIManager.SendEditObjectToChildEditor<R>(this, mLastSpawnedChildEditor, mSelectedObject);
				}

				public virtual void OnClickCreateEditObject(GameObject obj)
				{
						if (IsOurBrowserObject(obj, mGUIEditorID) == false) {
								return;
						}

						mSelectedObject = new R();
						mLastSpawnedChildEditor = GUIManager.SpawnNGUIChildEditor(this.gameObject, NGUICreateEditObjectPrefab);

						GUIManager.SendEditObjectToChildEditor<R>(this, mLastSpawnedChildEditor, mSelectedObject);

						//mEditObject.Add (mSelectedObject);
				}

				public virtual void OnClickDeleteEditObject(GameObject obj)
				{
						//R editObject = mEditObjectsList[Mathf.FloorToInt(browserObjectIndex)];
						//mDeleteEditObject (editObject);
				}

				protected GameObject mLastSpawnedChildEditor;
		}
}