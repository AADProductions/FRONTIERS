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
		public GUIEditor ()
		{
			mGUIEditorID = GUIManager.GetNextGUIID ();
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

		public void ReceiveFromParentEditor (R editObject)
		{
			ReceiveFromParentEditor (editObject, null);
		}

		public virtual void ReceiveFromParentEditor (R editObject, ChildEditorCallback <R> callBack)
		{
			mEditObject = editObject;
			mCallBack = callBack;

			if (Manager.IsAwake <GUIManager> ()) {
				GUIManager.Get.GetFocus (this);
			}
			//if it's a generic dialog result subscribe to the refresh action
			GenericDialogResult gdr = mEditObject as GenericDialogResult;
			if (gdr != null) {
				gdr.RefreshAction += Refresh;
			}
			PushEditObjectToNGUIObject ();
			if (!Visible) {
				Show ();
			}
		}

		public virtual void Refresh ()
		{
			PushEditObjectToNGUIObject ();
		}

		protected override void OnFinish ()
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
				mCallBack (mEditObject, this);
			}

			base.OnFinish ();
		}

		public abstract void PushEditObjectToNGUIObject ();

		protected virtual void OnEditObjectChange ()
		{

		}

		protected ulong mGUIEditorID = 0;
		protected GameObject mNGUIObject = null;
		protected R mEditObject = default (R);
		protected ChildEditorCallback <R> mCallBack = null;
	}

	public abstract class GUIBrowser <R> : GUIEditor <IEnumerable <R>>, IGUIChildEditor <IEnumerable <R>>
	{
		public GameObject BrowserObjectPrototype;
		public GameObject BrowserObjectsParent;
		public GameObject DividerObjectPrototype;
		public UIScrollBar ScrollBar;
		public float GridPadding = 0f;

		public virtual IEnumerable <R> FetchItems ()
		{
			return null;
		}

		public override void Hide ()
		{
			ClearAll ();
			base.Hide ();
		}

		public override void Show ()
		{
			base.Show ();
			IEnumerable <R> items = FetchItems ();
			if (items != null) {
				ReceiveFromParentEditor (items);
			} else {
				Refresh ();
			}
		}

		public List <GameObject> BrowserObjectsList {
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

		public override void WakeUp ()
		{
			mGrid = BrowserObjectsParent.GetComponent <UIGrid> ();
		}

		public virtual void ClearItems ()
		{
			mEditObject = null;
			mDividers.Clear ();
		}

		public virtual void ClearAll ()
		{
			ClearItems ();
			ClearBrowserObjects ();

			UIDraggablePanel draggablePanel = null;
			if (UsesGrid && mGrid.gameObject.HasComponent <UIDraggablePanel> (out draggablePanel)) {
				if (draggablePanel.verticalScrollBar != null) {
					draggablePanel.verticalScrollBar.scrollValue = 0f;
				}
				draggablePanel.ResetPosition ();
			}
		}

		public void AddItems (IEnumerable <R> items)
		{
			if (items == null) {
				//			////Debug.Log ("ITEMS WAS NULL");
				return;
			}

			if (mEditObject == null) {
				mEditObject = items;
			} else {
				mEditObject = Enumerable.Concat <R> (mEditObject, items);
			}
		}

		public void AddItems (IEnumerable <R> items, string dividerText, Color dividerColor)
		{
			int dividerIndex = 0;

			if (items == null) {
				return;
			}

			if (mEditObject == null) {
				mEditObject = items;
				mDividers.Add (new DividerProps (dividerIndex, dividerText, dividerColor));
			} else {
				foreach (R r in mEditObject) {
					//this is terrible!
					//using because IEnumerable doesn't have a Count property
					//consider switching to ICollection
					dividerIndex++;
				}
				mEditObject = Enumerable.Concat <R> (mEditObject, items);
				mDividers.Add (new DividerProps (dividerIndex, dividerText, dividerColor));
			}
		}

		public override void PushEditObjectToNGUIObject ()
		{
			SetGridProperties ();
			ClearBrowserObjects ();
			CreateBrowserObjects ();
			BrandBrowserObjects ();
			AlignBrowserObjects ();
		}

		public override void Refresh ()
		{
			RefreshBrowserObjects ();
			AlignBrowserObjects ();
			base.Refresh ();
		}

		protected GameObject CreateDivider (DividerProps dividerProps)
		{
			GameObject newDividerObject = NGUITools.AddChild (BrowserObjectsParent, DividerObjectPrototype);
			GUIBrowserDivider divider = newDividerObject.GetComponent <GUIBrowserDivider> ();
			divider.Label.text = dividerProps.DividerText;
			divider.DividerColor = dividerProps.DividerColor;
			divider.Refresh ();
			return newDividerObject;
		}

		public virtual void OnClickBrowserObject (GameObject obj)
		{
			if (IsOurBrowserObject (obj, mGUIEditorID) == false) {
				return;
			}
			mSelectedObject = mEditObjectLookup [obj.transform.gameObject];
		}

		protected virtual GameObject ConvertEditObjectToBrowserObject (R editObject)
		{
			//instantiate using NGUITools
			GameObject newBrowserObject = NGUITools.AddChild (BrowserObjectsParent, BrowserObjectPrototype);
			//rename to get ride of (clone)
			newBrowserObject.name = BrowserObjectPrototype.name;

			return newBrowserObject;
		}

		protected virtual void RefreshEditObjectToBrowserObject (R editObject, GameObject browserObject)
		{

		}

		protected void RefreshBrowserObjects ()
		{
			foreach (KeyValuePair <GameObject, R> editObjectBrowser in mEditObjectLookup) {
				RefreshEditObjectToBrowserObject (editObjectBrowser.Value, editObjectBrowser.Key);
			}
		}

		protected virtual void ClearBrowserObjects ()
		{
			if (Application.isPlaying) {
				foreach (GameObject browserObject in mBrowserObjectsList) {
					GUIManager.TrashNGUIObject (browserObject);
				}

				mBrowserObjectsList.Clear ();
				mEditObjectLookup.Clear ();
				//reset the grid
				if (UsesGrid) {
					mGrid.Reposition ();
				}
			}
		}

		protected virtual void CreateBrowserObjects ()
		{
			if (mEditObject == null) {
				return;
			}

			foreach (R editObject in mEditObject) {
				GameObject newBrowserObject = ConvertEditObjectToBrowserObject (editObject);
				//check for divider
				mBrowserObjectsList.Add (newBrowserObject);
				mEditObjectLookup.Add (newBrowserObject, editObject);
			}
			int offset = 0;
			foreach (DividerProps divider in mDividers) {
				GameObject newDivider = CreateDivider (divider);
				int insertionIndex = divider.Index + offset;

				if (insertionIndex >= mBrowserObjectsList.Count) {
					mBrowserObjectsList.Add (newDivider);
				} else {
					mBrowserObjectsList.Insert (insertionIndex, newDivider);
				}
				offset++;
			}
		}

		protected void BrandBrowserObjects ()
		{
			foreach (GameObject browserObject in mBrowserObjectsList) {
				string browserNamePrefix = GetBrowserNamePrefix (mGUIEditorID);
				browserObject.name = browserNamePrefix + browserObject.name;
				foreach (Transform child in browserObject.transform) {
					child.name = browserNamePrefix + child.name;
				}
			}
		}

		protected void AlignBrowserObjects ()
		{
			mGrid = BrowserObjectsParent.GetComponent <UIGrid> ();

			if (UsesGrid) {
				mGrid.repositionNow = true;
			} else {
				float padding = 10.0f;
				//float dividerHeight = DividerObjectPrototype.GetComponent <GUIBrowserDivider> ().DividerHeight;
				float objectHeight = BrowserObjectPrototype.GetComponent <BoxCollider> ().size.y + padding;

				float currentOffset = 0.0f;
				Vector3	currentPosition = Vector3.zero;
				foreach (GameObject browserObject in mBrowserObjectsList) {
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
					StartCoroutine (ResetScrollBar ());
				}
			}
		}

		protected void SetGridProperties ()
		{
			if (UsesGrid) {
				float padding = 20.0f;
				if (GridPadding > 0f) {
					padding = GridPadding;
				}
				BoxCollider collider = BrowserObjectPrototype.GetComponent <BoxCollider> ();
				//set grid Y dimensions to the browser object Y dimensions
				mGrid.cellHeight = collider.size.y + padding;
				mGrid.cellWidth = collider.size.x + padding;
			}
		}

		protected IEnumerator ResetScrollBar ()
		{
			yield return null;
			ScrollBar.scrollValue = 0f;
			mResettingScrollBar = false;
		}

		protected bool mResettingScrollBar = false;
		protected Dictionary <GameObject,R> mEditObjectLookup = new Dictionary <GameObject,R> ();
		protected List <DividerProps> mDividers = new List <DividerProps> ();
		protected GameObject mBrowserObject = null;
		protected List <GameObject> mBrowserObjectsList = new List <GameObject> ();
		protected R mSelectedObject = default (R);
		protected UIGrid mGrid = null;

		protected class DividerProps
		{
			public DividerProps (int index, string dividerText, Color dividerColor)
			{
				Index = index;
				DividerText = dividerText;
				DividerColor = dividerColor;
			}

			public int Index;
			public string DividerText;
			public Color	DividerColor;
		}

		protected static string GetBrowserNamePrefix (ulong guiEditorID)
		{
			return ("BO_" + guiEditorID.ToString () + "_");
		}

		protected static bool IsOurBrowserObject (GameObject browserObject, ulong guiEditorID)
		{
			return browserObject.name.Contains (GetBrowserNamePrefix (guiEditorID));
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

		public override void PushEditObjectToNGUIObject ()
		{
			base.PushEditObjectToNGUIObject ();

			if (mSelectedObject != null && PushToViewerAutomatically) {
				PushSelectedObjectToViewer ();
			}
		}

		public override void OnClickBrowserObject (GameObject obj)
		{
			if (!IsOurBrowserObject (obj, mGUIEditorID)) {
				return;
			}

			mSelectedObject	= mEditObjectLookup [obj.transform.gameObject];

			PushSelectedObjectToViewer ();
		}

		public virtual void PushSelectedObjectToViewer ()
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

		public virtual void ReceiveFromChildEditor (R editObject, IGUIChildEditor<R> childEditor)
		{
			//clear everything and start over
			PushEditObjectToNGUIObject ();
			//as the parent, we are responsible for initiating transitions
			GUIManager.ScaleUpEditor (this.gameObject).Proceed ();
			GUIManager.ScaleDownEditor (mLastSpawnedChildEditor).Proceed ();
			//retire the GUI - it'll be destroyed after the transition is complete
			GUIManager.RetireGUIChildEditor (mLastSpawnedChildEditor);

			mLastSpawnedChildEditor = null;
		}

		protected override void CreateBrowserObjects ()
		{
			//create the browser objects normally
			base.CreateBrowserObjects ();

			//then initialize the 'new' button
			GameObject browserObjectCreate = NGUITools.AddChild (BrowserObjectsParent, BrowserObjectCreatePrototype);
			browserObjectCreate.name = "000_" + browserObjectCreate.name;
			GUIBrowserObjectCreate boc	= browserObjectCreate.GetComponent<GUIBrowserObjectCreate> ();
			boc.CreateButton.target = this.gameObject;
			//add to browser objects list
			mBrowserObjectsList.Add (browserObjectCreate);

		}

		public override void OnClickBrowserObject (GameObject obj)
		{
			if (IsOurBrowserObject (obj, mGUIEditorID) == false) {
				return;
			}

			mSelectedObject = mEditObjectLookup [obj.transform.gameObject];
			mLastSpawnedChildEditor = GUIManager.SpawnNGUIChildEditor (this.gameObject, NGUIUpdateEditObjectPrefab);

			GUIManager.SendEditObjectToChildEditor<R> (this, mLastSpawnedChildEditor, mSelectedObject);
		}

		public virtual void OnClickCreateEditObject (GameObject obj)
		{
			if (IsOurBrowserObject (obj, mGUIEditorID) == false) {
				return;
			}

			mSelectedObject = new R ();
			mLastSpawnedChildEditor = GUIManager.SpawnNGUIChildEditor (this.gameObject, NGUICreateEditObjectPrefab);

			GUIManager.SendEditObjectToChildEditor<R> (this, mLastSpawnedChildEditor, mSelectedObject);

			//mEditObject.Add (mSelectedObject);
		}

		public virtual void OnClickDeleteEditObject (GameObject obj)
		{
			//R editObject = mEditObjectsList[Mathf.FloorToInt(browserObjectIndex)];
			//mDeleteEditObject (editObject);
		}

		protected GameObject mLastSpawnedChildEditor;
	}
}