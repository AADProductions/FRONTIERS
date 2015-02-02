using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers;

namespace Frontiers.GUI
{
		[Serializable]
		public class GUIHudElement
		{
				public string Name = "Type";

				public Vector2 Dimensions {
						get {
								Vector2 dimensions;
								switch (mType) {
										case HudElementType.Label:
										default:
												dimensions = new Vector2(50f, 50f);
												break;

										case HudElementType.Icon:
												dimensions = new Vector2(500f, 500f);
												break;

										case HudElementType.ProgressBar:
												dimensions = new Vector2(50f, 50f);
												break;
								}
								return dimensions;
						}
				}

				public int DisplayOrder = 0;
				public Vector3 TargetPosition = Vector3.zero;

				public string LabelText {
						get {
								return mLabelText;
						}
						set {
								if (mLabelText != value) {
										mIsDirty = true;
								}
								mLabelText = value;
						}
				}

				public bool IsDirty {
						get {
								return mIsDirty;
						}
				}

				public Icon HudIcon {
						get {
								return mHudIcon;
						}
						set {
								mHudIcon	= value;
								mIsDirty = true;
						}
				}

				public bool PingContinuously = false;
				public float PingIntensity = 0.0f;
				public float PingInterval = 1.0f;

				public float ProgressValue {
						get {
								return mProgressValue;
						}
						set {
								mIsDirty = true;
								mProgressValue = value;
						}
				}

				public Color LabelColor = Color.white;
				public Color MessageColor = Color.blue;
				public Color FGColor = Color.green;
				public Color BGColor = Color.red;
				public Color PingColor = Color.yellow;

				public float CustomScale {
						get {
								return mCustomScale;
						}
						set {
								mCustomScale = value;
								mIsDirty = true;
						}
				}

				public HudElementType Type {
						get {
								return mType;
						}
						set {
								if (!mInitialized) {
										mType = value;
								}
						}
				}

				public bool Initialized {
						get {
								return mInitialized;
						}
				}

				public bool Deactivated {
						get {
								return mDeactivated;
						}
				}

				public void Deactivate()
				{
						mDeactivated = true;
				}

				public void Initialize(Icon hudIcon)
				{
						mHudIcon = hudIcon;
						mInitialized = true;
						mIsDirty = true;
				}

				public void Initialize(Color labelColor, Color pingColor)
				{
						LabelColor = labelColor;
						PingColor = pingColor;
						mInitialized = true;
						mIsDirty = true;
				}

				public void Initialize(Color fgColor, Color bgColor, Color pingColor)
				{
						FGColor = fgColor;
						BGColor = bgColor;
						PingColor = pingColor;
						mInitialized = true;
						mIsDirty = true;
				}

				public void AddMessage(string message)
				{

				}

				public void AddNumber(float number)
				{

				}

				public void SetProgressValue(float progressValue, bool autoPing, bool autoMessage)
				{
						mLastProgressDifference = mProgressValue - progressValue;
						mProgressValue = progressValue;
				}

				public void Ping()
				{
						PingContinuously = false | PingContinuously;
						PingIntensity = 1.0f;
						mLastPingTime = WorldClock.AdjustedRealTime;
						mIsDirty = true;
				}

				public void Ping(bool continuously)
				{
						PingContinuously = continuously;
						PingIntensity = 1.0f;
						mLastPingTime = WorldClock.AdjustedRealTime;
						mIsDirty = true;
				}

				public void Ping(float pingInterval)
				{
						PingContinuously = true;
						PingInterval = pingInterval;
						PingIntensity = 1.0f;
						mLastPingTime = WorldClock.AdjustedRealTime;
						mIsDirty = true;
				}

				public void StopPing()
				{
						PingContinuously = false;
						mIsDirty = true;
				}

				protected HudElementType mType;
				protected string mLabelText = string.Empty;
				protected float mProgressValue = 0.0f;
				protected float mLastProgressDifference = 0.0f;
				protected double mLastPingTime = 0.0f;
				protected bool mInitialized = false;
				protected bool mDeactivated = false;
				protected bool mIsDirty = false;
				protected float mCustomScale = 1.0f;
				protected Icon mHudIcon = Icon.Empty;
		}
}