using UnityEngine;
using System.Collections;
using Frontiers;

namespace Frontiers.GUI
{
	public class NGUIConversationVariable : MonoBehaviour
	{
		public Vector3 TargetSize;
		public UISprite GemSprite;
		public UILabel NumberLabel;
		public UISprite PingSprite;
		public Color LowValue;
		public Color MidValue;
		public Color HighValue;

		public float NormalizedValue {
			set {
				if (!Mathf.Approximately (value, mPreviousValue)) {
					Color gemSpriteColor = MidValue;
					if (value < 0.5f) {
						gemSpriteColor	= Color.Lerp (LowValue, MidValue, (value / 0.5f));
					} else if (value > 0.5f) {
						gemSpriteColor = Color.Lerp (MidValue, HighValue, ((value - 0.5f) / 0.5f));
					}
					GemSprite.color = gemSpriteColor;

					float difference = value - mPreviousValue;
					if (difference > 0) {
						PingSprite.color = Colors.Alpha (HighValue, 0.5f);
					} else {
						PingSprite.color = Colors.Alpha (LowValue, 0.5f);
					}
					mPreviousValue = value;
				}
				NumberLabel.text = mPreviousValue.ToString ("R1");
			}
		}

		public void Initialize (float normalizedValue)
		{
			mPreviousValue = Mathf.NegativeInfinity;
			NormalizedValue = normalizedValue;
		}

		public void Update ()
		{
			PingSprite.color = Color.Lerp (PingSprite.color, Colors.Alpha (PingSprite.color, 0f), (float) WorldClock.ARTDeltaTime);
			transform.localScale = Vector3.Lerp (transform.localScale, TargetSize, (float) WorldClock.ARTDeltaTime);
		}

		protected float mPreviousValue = 0.0f;
	}
}