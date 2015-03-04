using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

namespace Frontiers.GUI
{
		public class GUICircleBrowser : GUIEditor <CircleBrowserResult>
		{
				public GUICircularBrowserObject BrowserObjectPrefab;
				public List <GUICircularBrowserObject>	BrowserObjects = new List <GUICircularBrowserObject>();
				public UISprite CenterSprite;
				public UILabel CenterLabel;
				public UISprite CenterShadow;
				public GameObject RotationPivot;
				public GameObject OffsetTarget;
				public float CenterLabelTargetAlpha = 0f;
				public float CenterLabelShadowMultiplier;
				public int FocusIndex;

				public int NumObjects {
						get {
								return BrowserObjects.Count;
						}
				}

				public override void Start()
				{
						base.Start();
						CenterLabelShadowMultiplier = CenterShadow.alpha;
						Subscribe(InterfaceActionType.SelectionNext, new ActionListener(SelectionNext));
						Subscribe(InterfaceActionType.SelectionPrev, new ActionListener(SelectionPrev));
						CenterLabelTargetAlpha = 0f;
						CenterLabel.text = string.Empty;
				}

				public void ClearBrowserObjects()
				{
						foreach (GUICircularBrowserObject browserObject in BrowserObjects) {
								GameObject.Destroy(browserObject.gameObject);
						}
						BrowserObjects.Clear();
						mRotationTargets.Clear();
				}

				public override void PushEditObjectToNGUIObject()
				{
						transform.localPosition = mEditObject.Offset;
						OffsetTarget.transform.localPosition = new Vector3(0f, mEditObject.CenterSize, 0f);
						CenterSprite.transform.localScale = Vector3.one * mEditObject.CenterSize;
						AddBrowserObjects();
						GUIManager.Get.GetFocus(this);
				}

				public void AddBrowserObjects()
				{
						for (int i = 0; i < mEditObject.Objects.Count; i++) {
								CircleBrowserObjectTemplate objectTemplate = mEditObject.Objects[i];	
								GameObject newBrowserObjectGameObject = NGUITools.AddChild(RotationPivot, BrowserObjectPrefab.gameObject);
								GUICircularBrowserObject newBrowserObject = newBrowserObjectGameObject.GetComponent <GUICircularBrowserObject>();
								newBrowserObject.ParentBrowser = this;
								newBrowserObject.StackNumberLabel.text = objectTemplate.CenterLabelText;
								newBrowserObject.InventoryItemName.text = objectTemplate.BottomLabelText;
								newBrowserObject.WeightLabel.text = objectTemplate.TopLabelText;
				
								if (!GenericWorldItem.IsNullOrEmpty(objectTemplate.DopplegangerProps)) {
										newBrowserObject.Doppleganger = WorldItems.GetDoppleganger(
												objectTemplate.DopplegangerProps,
												newBrowserObject.transform,
												newBrowserObject.Doppleganger,
												WIMode.Stacked,
												1.0f);
								}
				
								if (!string.IsNullOrEmpty(objectTemplate.BackgroundSpriteName)) {
										newBrowserObject.Background.spriteName = objectTemplate.BackgroundSpriteName;
								}
						
								BrowserObjects.Add(newBrowserObject);
						}
			
						RefreshBrowserObjects();
				}

				public void RefreshBrowserObjects()
				{
						mRotationTargets.Clear();
						Quaternion currentPivotRotation = RotationPivot.transform.localRotation;
						RotationPivot.transform.localRotation = Quaternion.identity;
						float rotationInterval = 360f / NumObjects;
						float currentRotation = 0f;
						float offset = mEditObject.CenterSize / 2f;
			
						for (int i = 0; i < NumObjects; i++) {			
								BrowserObjects[i].transform.parent = transform;
								BrowserObjects[i].transform.localPosition = new Vector3(0f, offset + BrowserObjects[i].Dimensions.y / 2f, 0f);
								BrowserObjects[i].transform.parent = RotationPivot.transform;
								BrowserObjects[i].Index = i;
				
								RotationPivot.transform.Rotate(0f, 0f, rotationInterval);
								mRotationTargets.Add(i, currentRotation);
								currentRotation += rotationInterval;
						}
						RotationPivot.transform.localRotation = currentPivotRotation;
			
						mMaxOffset = 0f;
						for (int i = 0; i < NumObjects; i++) {			
								mMaxOffset = Mathf.Max(mMaxOffset, Vector3.Distance(BrowserObjects[i].transform.position, OffsetTarget.transform.position));
				
								if (!mEditObject.RotateToFocus) {
										BrowserObjects[i].transform.localRotation = Quaternion.identity;
								}
						}
						mMaxOffset = Mathf.Clamp(mMaxOffset, 0.05f, 1000.0f);//? wth was i doing here?
				}

				public void ClickOn(int index)
				{
						mEditObject.ClickIndex = index;
						Finish();
				}

				public bool SelectionNext(double timeStamp)
				{
						FocusOn(FocusIndex + 1);
						return true;
				}

				public bool SelectionPrev(double timeStamp)
				{
						FocusOn(FocusIndex - 1);
						return true;
				}

				public override bool ActionCancel(double timeStamp)
				{
						base.ActionCancel(timeStamp);
						mEditObject.Cancelled = true;
						mEditObject.FocusListener.SendMessage(mEditObject.OnFocusMessage, -1, SendMessageOptions.RequireReceiver);
						Finish();
						return true;
				}

				public bool FocusOn(int focusIndex)
				{
						if (mEditObject.RotateToFocus && WorldClock.RealTime < mFocusCooldown) {
								return false;
						}
			
						if (NumObjects > 0) {
								if (focusIndex != FocusIndex) {
										RefreshBrowserObjects();
								}
				
								FocusIndex = focusIndex;
				
								if (mEditObject.RotateToFocus) {
										float rotationTargetZ = mRotationTargets[FocusIndex];
										mRotationTarget = Quaternion.Euler(0f, 0f, rotationTargetZ);
										mFocusCooldown = (float)(WorldClock.RealTime + gFocusCooldownInterval);
								}
					
								if (!string.IsNullOrEmpty(mEditObject.OnFocusMessage) && mEditObject.FocusListener != null) {
										mEditObject.FocusListener.SendMessage(mEditObject.OnFocusMessage, FocusIndex, SendMessageOptions.RequireReceiver);
								}

								if (!string.IsNullOrEmpty(mEditObject.Objects[FocusIndex].OnFocusTitle)) {
										CenterLabel.text = Paths.CleanPathName(mEditObject.Objects[FocusIndex].OnFocusTitle);
										CenterLabelTargetAlpha = 1f;
								}
						}
						return true;
				}

				public new void Update()
				{
						base.Update();
			
						if (NumObjects == 0) {
								return;
						}

						CenterLabel.alpha = Mathf.Lerp(CenterLabel.alpha, CenterLabelTargetAlpha, 0.5f);
						CenterShadow.alpha = CenterLabel.alpha * CenterLabelShadowMultiplier;
			
						if (mEditObject.RotateToFocus) {		
								RotationPivot.transform.localRotation = Quaternion.Lerp(RotationPivot.transform.localRotation, mRotationTarget, 0.15f);
				
								Vector3 pivotRotation = RotationPivot.transform.localEulerAngles;
								Vector3 objectRotation = Vector3.zero;
								Vector3 objectScale = Vector3.one;
								float positionalScale	= 1.0f;
								foreach (GUICircularBrowserObject browserObject in BrowserObjects) {
										float offset = Vector3.Distance(browserObject.transform.position, OffsetTarget.transform.position);
										positionalScale = 1.0f - (mMaxScale * (offset / mMaxOffset));
					
										objectScale.x = positionalScale;
										objectScale.y = positionalScale;
					
										objectRotation.z = (360f - pivotRotation.z);
					
										browserObject.transform.localRotation = Quaternion.Euler(objectRotation);
										browserObject.transform.localScale = objectScale;
								}
						}
				}

				protected float mMaxOffset = 0f;
				protected static float mMaxScale = 0.25f;
				public static float gFocusCooldownInterval	= 0.4f;
				public static int gMaxObjects = 10;
				protected Dictionary <int,float> mRotationTargets = new Dictionary <int, float>();
				protected Quaternion mRotationTarget = Quaternion.identity;
				protected float mFocusCooldown = 0.0f;
		}

		public class CircleBrowserResult
		{
				public GameObject FocusListener = null;
				public string OnFocusMessage = "OnFocus";
				public bool RotateToFocus = false;
				public int ClickIndex = 0;
				public Vector3 Offset = Vector3.zero;
				public float CenterSize = 180f;
				public List <CircleBrowserObjectTemplate> Objects = new List <CircleBrowserObjectTemplate>();
				public bool Cancelled = false;
		}

		public class CircleBrowserObjectTemplate
		{
				public string BackgroundSpriteName = null;
				public string CenterLabelText = null;
				public string TopLabelText = null;
				public string BottomLabelText = null;
				public string OnFocusTitle = null;
				public GenericWorldItem DopplegangerProps = null;
		}
}