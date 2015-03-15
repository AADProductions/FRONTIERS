using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Frontiers.GUI
{
		public class GUIBrowserObjectBase : GUIObject, IGUIBrowserObject
		{
				public override void Awake ( ) {

						base.Awake();

						mColliders.Clear();
						transform.GetComponentsInChildren <Collider>(true, mColliders);
						//make sure all buttons are tagged so GUI manager can identify them
						for (int i = 0; i < mColliders.Count; i++) {
								mColliders[i].tag = Globals.TagBrowserObject;
						}
						gameObject.tag = Globals.TagBrowserObject;
				}

				public float Padding = 10.0f;

				public bool DeleteRequest { get; set; }

				public virtual bool AutoSelect { get { return mAutoSelect; } set { mAutoSelect = value; } }

				public IBrowser ParentBrowser { get; set; }

				public virtual float Size {
						get {
								return 100.0f;
						}
				}

				public int CompareTo (IGUIBrowserObject other) {
						return name.CompareTo(other.name);
				}

				protected bool mAutoSelect = true;
				protected static List <Collider> mColliders = new List <Collider> ();//for re-use
		}

		public interface IGUIBrowserObject : IComparable <IGUIBrowserObject> {
				IBrowser ParentBrowser { get; set; }
				GameObject gameObject { get; }
				Transform transform { get; }
				string name { get; set; }
				float Size { get; }
				bool DeleteRequest { get; set; }
				bool AutoSelect { get; set; }
		}
}