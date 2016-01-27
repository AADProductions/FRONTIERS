//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright � 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// This script, when attached to a panel allows dragging of the said panel's contents efficiently by using UIDragPanelContents.
/// </summary>

[ExecuteInEditMode]
//[RequireComponent(typeof(UIPanel))]
[AddComponentMenu("NGUI/Interaction/Draggable Panel")]
public class UIDraggablePanel : IgnoreTimeScale
{
		public enum DragEffect
		{
				None,
				Momentum,
				MomentumAndSpring,
		}

		public enum ShowCondition
		{
				Always,
				OnlyIfNeeded,
				WhenDragging,
		}

		/// <summary>
		/// Whether the dragging will be restricted to be within the parent panel's bounds.
		/// </summary>

		public bool restrictWithinPanel = true;
		/// <summary>
		/// Whether dragging will be disabled if the contents fit.
		/// </summary>

		public bool disableDragIfFits = false;
		/// <summary>
		/// Effect to apply when dragging.
		/// </summary>

		public DragEffect dragEffect = DragEffect.MomentumAndSpring;
		/// <summary>
		/// Scale value applied to the drag delta. Set X or Y to 0 to disallow dragging in that direction.
		/// </summary>

		public Vector3 scale = Vector3.one;
		/// <summary>
		/// Effect the scroll wheel will have on the momentum.
		/// </summary>

		public float scrollWheelFactor = 0f;
		/// <summary>
		/// How much momentum gets applied when the press is released after dragging.
		/// </summary>

		public float momentumAmount = 35f;
		/// <summary>
		/// Starting position of the clipped area. (0, 0) means top-left corner, (1, 1) means bottom-right.
		/// </summary>

		public Vector2 relativePositionOnReset = Vector2.zero;
		/// <summary>
		/// Whether the position will be reset to the 'startingDragAmount'. Inspector-only value.
		/// </summary>

		public bool repositionClipping = false;
		/// <summary>
		/// Horizontal scrollbar used for visualization.
		/// </summary>

		public UIScrollBar horizontalScrollBar;
		/// <summary>
		/// Vertical scrollbar used for visualization.
		/// </summary>

		public UIScrollBar verticalScrollBar;
		/// <summary>
		/// Condition that must be met for the scroll bars to become visible.
		/// </summary>

		public ShowCondition showScrollBars = ShowCondition.OnlyIfNeeded;
		Transform mTrans;
		UIPanel mPanel;
		Plane mPlane;
		Vector3 mLastPos;
		bool mPressed = false;
		Vector3 mMomentum = Vector3.zero;
		float mScroll = 0f;
		Bounds mBounds;
		bool mCalculatedBounds = false;
		bool mShouldMove = false;
		bool mIgnoreCallbacks = false;
		int mTouches = 0;

		/// <summary>
		/// Calculate the bounds used by the widgets.
		/// </summary>

		public Bounds bounds {
				get {
						if (!mCalculatedBounds) {
								mCalculatedBounds = true;
								mBounds = NGUIMath.CalculateRelativeWidgetBounds(mTrans, mTrans);
						}
						return mBounds;
				}
		}

		/// <summary>
		/// Whether the panel should be able to move horizontally (contents don't fit).
		/// </summary>

		public bool shouldMoveHorizontally {
				get {
						float size = bounds.size.x;
						if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
								size += mPanel.clipSoftness.x * 2f;
						return size > mPanel.clipRange.z;
				}
		}

		/// <summary>
		/// Whether the panel should be able to move vertically (contents don't fit).
		/// </summary>

		public bool shouldMoveVertically {
				get {
						float size = bounds.size.y;
						if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
								size += mPanel.clipSoftness.y * 2f;
						return size > mPanel.clipRange.w;
				}
		}

		/// <summary>
		/// Whether the contents of the panel should actually be draggable depends on whether they currently fit or not.
		/// </summary>

		bool shouldMove {
				get {
						if (!disableDragIfFits)
								return true;

						if (mPanel == null)
								mPanel = GetComponent<UIPanel>();
						gClip = mPanel.clipRange;
						gBounds = bounds;

						float hx = gClip.z * 0.5f;
						float hy = gClip.w * 0.5f;

						if (!Mathf.Approximately(scale.x, 0f)) {
								if (gBounds.min.x < gClip.x - hx)
										return true;
								if (gBounds.max.x > gClip.x + hx)
										return true;
						}

						if (!Mathf.Approximately(scale.y, 0f)) {
								if (gBounds.min.y < gClip.y - hy)
										return true;
								if (gBounds.max.y > gClip.y + hy)
										return true;
						}
						return false;
				}
		}

		protected static Bounds gBounds;
		protected static Vector4 gClip;

		/// <summary>
		/// Current momentum, exposed just in case it's needed.
		/// </summary>

		public Vector3 currentMomentum { get { return mMomentum; } set { mMomentum = value; } }

		/// <summary>
		/// Cache the transform and the panel.
		/// </summary>

		void Awake()
		{
				mTrans = transform;
				mPanel = GetComponent<UIPanel>();
		}

		/// <summary>
		/// Set the initial drag value and register the listener delegates.
		/// </summary>

		void Start()
		{
				UpdateScrollbars(true, false);

				if (horizontalScrollBar != null) {
						horizontalScrollBar.onChange += OnHorizontalBar;
						horizontalScrollBar.alpha = ((showScrollBars == ShowCondition.Always) || shouldMoveHorizontally) ? 1f : 0f;
				}

				if (verticalScrollBar != null) {
						verticalScrollBar.onChange += OnVerticalBar;
						verticalScrollBar.alpha = ((showScrollBars == ShowCondition.Always) || shouldMoveVertically) ? 1f : 0f;
				}

				tag = Globals.TagActiveObject;
		}

		/// <summary>
		/// Restrict the panel's contents to be within the panel's bounds.
		/// </summary>

		public void RestrictWithinBounds(bool instant)
		{
				gContraint = mPanel.CalculateConstrainOffset(bounds.min, bounds.max);

				if (gContraint.magnitude > 0.001f) {
						if (!instant && dragEffect == DragEffect.MomentumAndSpring) {
								// Spring back into place
								SpringPanel.Begin(mPanel.gameObject, mTrans.localPosition + gContraint, 13f);
						} else {
								// Jump back into place
								MoveRelative(gContraint);
								mMomentum = Vector3.zero;
								mScroll = 0f;
						}
				} else {
						// Remove the spring as it's no longer needed
						DisableSpring();
				}
		}

		protected static Vector3 gContraint;

		/// <summary>
		/// Disable the spring movement.
		/// </summary>

		public void DisableSpring()
		{
				SpringPanel sp = GetComponent<SpringPanel>();
				if (sp != null)
						sp.enabled = false;
		}

		/// <summary>
		/// Update the values of the associated scroll bars.
		/// </summary>

		public void UpdateScrollbars(bool recalculateBounds, bool updateValue)
		{
				if (mPanel == null)
						return;

				if (horizontalScrollBar != null || verticalScrollBar != null) {
						if (recalculateBounds) {
								mCalculatedBounds = false;
								mShouldMove = shouldMove;
						}

						if (horizontalScrollBar != null) {
								Bounds b = bounds;
								Vector3 boundsSize = b.size;

								if (boundsSize.x > 0f) {
										Vector4 clip = mPanel.clipRange;
										float extents = clip.z * 0.5f;
										float min = clip.x - extents - b.min.x;
										float max = b.max.x - extents - clip.x;

										if (mPanel.clipping == UIDrawCall.Clipping.SoftClip) {
												min += mPanel.clipSoftness.x;
												max -= mPanel.clipSoftness.x;
										}

										min = Mathf.Clamp01(min / boundsSize.x);
										max = Mathf.Clamp01(max / boundsSize.x);

										float sum = min + max;
										mIgnoreCallbacks = true;
										horizontalScrollBar.barSize = 0.20f;//1f - sum;
										if (updateValue) {
												horizontalScrollBar.scrollValue = (sum > 0.001f) ? min / sum : 0f;
										}
										mIgnoreCallbacks = false;
								}
						}

						if (verticalScrollBar != null) {
								gBounds = bounds;
								gBoundsSize = gBounds.size;

								if (gBoundsSize.y > 0f) {
										Vector4 clip = mPanel.clipRange;
										float extents = clip.w * 0.5f;
										float min = clip.y - extents - gBounds.min.y;
										float max = gBounds.max.y - extents - clip.y;

										if (mPanel.clipping == UIDrawCall.Clipping.SoftClip) {
												min += mPanel.clipSoftness.y;
												max -= mPanel.clipSoftness.y;
										}

										min = Mathf.Clamp01(min / gBoundsSize.y);
										max = Mathf.Clamp01(max / gBoundsSize.y);
										float sum = min + max;

										mIgnoreCallbacks = true;
										verticalScrollBar.barSize = 0.20f;//1f - sum;
										if (updateValue) {
												verticalScrollBar.scrollValue = (sum > 0.001f) ? 1f - min / sum : 0f;
										}
										mIgnoreCallbacks = false;
								}
						}
				} else if (recalculateBounds) {
						mCalculatedBounds = false;
				}
		}

		protected static Vector3 gBoundsSize;

		/// <summary>
		/// Changes the drag amount of the panel to the specified 0-1 range values.
		/// (0, 0) is the top-left corner, (1, 1) is the bottom-right.
		/// </summary>

		public void SetDragAmount(float x, float y, bool updateScrollbars)
		{
				DisableSpring();

				gBounds = bounds;
				if (gBounds.min.x == gBounds.max.x || gBounds.min.y == gBounds.max.x)
						return;
				gClip = mPanel.clipRange;

				float hx = gClip.z * 0.5f;
				float hy = gClip.w * 0.5f;
				float left = gBounds.min.x + hx;
				float right = gBounds.max.x - hx;
				float bottom = gBounds.min.y + hy;
				float top = gBounds.max.y - hy;

				if (mPanel.clipping == UIDrawCall.Clipping.SoftClip) {
						left -= mPanel.clipSoftness.x;
						right += mPanel.clipSoftness.x;
						bottom -= mPanel.clipSoftness.y;
						top += mPanel.clipSoftness.y;
				}

				// Calculate the offset based on the scroll value
				float ox = Mathf.Lerp(left, right, x);
				float oy = Mathf.Lerp(top, bottom, y);

				// Update the position
				if (!updateScrollbars) {
						Vector3 pos = mTrans.localPosition;
						if (scale.x != 0f)
								pos.x += gClip.x - ox;
						if (scale.y != 0f)
								pos.y += gClip.y - oy;
						mTrans.localPosition = pos;
				}

				// Update the clipping offset
				gClip.x = ox;
				gClip.y = oy;
				mPanel.clipRange = gClip;

				// Update the scrollbars, reflecting this change
				if (updateScrollbars)
						UpdateScrollbars(false, false);
		}

		/// <summary>
		/// Reset the panel's position to the top-left corner.
		/// It's recommended to call this function before AND after you re-populate the panel's contents (ex: switching window tabs).
		/// Another option is to populate the panel's contents, reset its position, then call this function to reposition the clipping.
		/// </summary>

		public void ResetPosition()
		{
				// Invalidate the bounds
				mCalculatedBounds = false;

				// First move the position back to where it would be if the scroll bars got reset to zero
				SetDragAmount(relativePositionOnReset.x, relativePositionOnReset.y, false);

				// Next move the clipping area back and update the scroll bars
				SetDragAmount(relativePositionOnReset.x, relativePositionOnReset.y, true);
		}

		/// <summary>
		/// Triggered by the horizontal scroll bar when it changes.
		/// </summary>

		void OnHorizontalBar(UIScrollBar sb)
		{
				if (!mIgnoreCallbacks) {
						float x = (horizontalScrollBar != null) ? horizontalScrollBar.scrollValue : 0f;
						float y = (verticalScrollBar != null) ? verticalScrollBar.scrollValue : 0f;
						SetDragAmount(x, y, false);
				}
		}

		/// <summary>
		/// Triggered by the vertical scroll bar when it changes.
		/// </summary>

		void OnVerticalBar(UIScrollBar sb)
		{
				if (!mIgnoreCallbacks) {
						float x = (horizontalScrollBar != null) ? horizontalScrollBar.scrollValue : 0f;
						float y = (verticalScrollBar != null) ? verticalScrollBar.scrollValue : 0f;
						SetDragAmount(x, y, false);
				}
		}

		/// <summary>
		/// Move the panel by the specified amount.
		/// </summary>

		public void MoveRelative(Vector3 relative)
		{
				if (mTrans == null) {
						mPanel = GetComponent <UIPanel>();
						mTrans = transform;
				}
				mTrans.localPosition += relative;
				gClip = mPanel.clipRange;
				gClip.x -= relative.x;
				gClip.y -= relative.y;
				mPanel.clipRange = gClip;
				UpdateScrollbars(false, false);
		}

		public void MoveRelative(Vector3 relative, bool clampToMin, float minValue) {
				Vector3 newLocalPosition = mTrans.localPosition + relative;
				if (clampToMin && newLocalPosition.y < 0f) {//minValue) {
						relative.y = relative.y - newLocalPosition.y;
						newLocalPosition.y = 0f;//minValue;//minValue;
				}
				mTrans.localPosition = newLocalPosition;
				gClip = mPanel.clipRange;
				gClip.x -= relative.x;
				gClip.y -= relative.y;
				mPanel.clipRange = gClip;
				UpdateScrollbars(false, false);
		}

		public void MoveRelative(Vector3 relative, bool clampToZero)
		{
				MoveRelative(relative, clampToZero, 0f);
		}

		/// <summary>
		/// Move the panel by the specified amount.
		/// </summary>

		void MoveAbsolute(Vector3 absolute)
		{
				gAMove = mTrans.InverseTransformPoint(absolute);
				gBMove = mTrans.InverseTransformPoint(Vector3.zero);
				MoveRelative(gAMove - gBMove);
		}

		protected static Vector3 gAMove;
		protected static Vector3 gBMove;

		/// <summary>
		/// Create a plane on which we will be performing the dragging.
		/// </summary>

		public void Press(bool pressed)
		{
				if (enabled && gameObject.activeSelf) {
						mTouches += (pressed ? 1 : -1);
						mCalculatedBounds = false;
						mShouldMove = shouldMove;
						if (!mShouldMove)
								return;
						mPressed = pressed;

						if (pressed) {
								// Remove all momentum on press
								mMomentum = Vector3.zero;
								mScroll = 0f;

								// Disable the spring movement
								DisableSpring();

								// Remember the hit position
								mLastPos = UICamera.lastHit.point;

								// Create the plane to drag along
								mPlane = new Plane(mTrans.rotation * Vector3.back, mLastPos);
						} else if (restrictWithinPanel && mPanel.clipping != UIDrawCall.Clipping.None && dragEffect == DragEffect.MomentumAndSpring) {
								RestrictWithinBounds(false);
						}
				}
		}

		/// <summary>
		/// Drag the object along the plane.
		/// </summary>

		public void Drag(Vector2 delta)
		{
				if (enabled && gameObject.activeSelf && mShouldMove) {
						UICamera.currentTouch.clickNotification = UICamera.ClickNotification.BasedOnDelta;

						Ray ray = UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos);
						float dist = 0f;

						if (mPlane.Raycast(ray, out dist)) {
								Vector3 currentPos = ray.GetPoint(dist);
								Vector3 offset = currentPos - mLastPos;
								mLastPos = currentPos;

								if (offset.x != 0f || offset.y != 0f) {
										offset = mTrans.InverseTransformDirection(offset);
										offset.Scale(scale);
										offset = mTrans.TransformDirection(offset);
								}

								// Adjust the momentum
								mMomentum = Vector3.Lerp(mMomentum, mMomentum + offset * (0.01f * momentumAmount), 0.67f);

								// Move the panel
								MoveAbsolute(offset);

								// We want to constrain the UI to be within bounds
								if (restrictWithinPanel &&
								mPanel.clipping != UIDrawCall.Clipping.None &&
								dragEffect != DragEffect.MomentumAndSpring) {
										RestrictWithinBounds(false);
								}
						}
				}
		}

		/// <summary>
		/// If the object should support the scroll wheel, do it.
		/// </summary>

		public void Scroll(float delta)
		{
				if (enabled && gameObject.activeSelf) {
						mShouldMove = shouldMove;
						if (Mathf.Sign(mScroll) != Mathf.Sign(delta))
								mScroll = 0f;
						mScroll += delta * scrollWheelFactor;
				}
		}

		/// <summary>
		/// Apply the dragging momentum.
		/// </summary>

		void LateUpdate()
		{
				// If the panel's geometry changed, recalculate the bounds
				if (mPanel.changedLastFrame)
						UpdateScrollbars(true, false);

				// Inspector functionality
				if (repositionClipping) {
						repositionClipping = false;
						mCalculatedBounds = false;
						SetDragAmount(relativePositionOnReset.x, relativePositionOnReset.y, true);
				}

				if (!Application.isPlaying)
						return;

				float delta = UpdateRealTimeDelta();

				// Fade the scroll bars if needed
				if (showScrollBars != ShowCondition.Always) {
						bool vertical = false;
						bool horizontal = false;

						if (showScrollBars != ShowCondition.WhenDragging || mTouches > 0) {
								vertical = shouldMoveVertically;
								horizontal = shouldMoveHorizontally;
						}

						if (verticalScrollBar) {
								float alpha = verticalScrollBar.alpha;
								alpha += vertical ? delta * 6f : -delta * 3f;
								alpha = Mathf.Clamp01(alpha);
								if (verticalScrollBar.alpha != alpha)
										verticalScrollBar.alpha = alpha;
						}

						if (horizontalScrollBar) {
								float alpha = horizontalScrollBar.alpha;
								alpha += horizontal ? delta * 6f : -delta * 3f;
								alpha = Mathf.Clamp01(alpha);
								if (horizontalScrollBar.alpha != alpha)
										horizontalScrollBar.alpha = alpha;
						}
				}

				// Apply momentum
				if (mShouldMove && !mPressed) {
						mMomentum += scale * (-mScroll * 0.05f);

						if (mMomentum.magnitude > 0.0001f) {
								mScroll = NGUIMath.SpringLerp(mScroll, 0f, 20f, delta);

								// Move the panel
								mOffset = NGUIMath.SpringDampen(ref mMomentum, 9f, delta);
								MoveAbsolute(mOffset);

								// Restrict the contents to be within the panel's bounds
								if (restrictWithinPanel && mPanel.clipping != UIDrawCall.Clipping.None)
										RestrictWithinBounds(false);
								return;
						} else
								mScroll = 0f;
				} else
						mScroll = 0f;

				// Dampen the momentum
				NGUIMath.SpringDampen(ref mMomentum, 9f, delta);
		}

		protected Vector3 mOffset;

		#if UNITY_EDITOR
		/// <summary>
		/// Draw a visible orange outline of the bounds.
		/// </summary>

		void OnDrawGizmos()
		{
				if (mPanel != null && mPanel.debugInfo == UIPanel.DebugInfo.Gizmos) {
						Bounds b = bounds;
						Gizmos.matrix = transform.localToWorldMatrix;
						Gizmos.color = new Color(1f, 0.4f, 0f);
						Gizmos.DrawWireCube(new Vector3(b.center.x, b.center.y, b.min.z), new Vector3(b.size.x, b.size.y, 0f));
				}
		}
		#endif
}