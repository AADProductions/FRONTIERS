using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

				public IGUIBrowser ParentBrowser { get; set; }

				public virtual float Size {
						get {
								return 100.0f;
						}
				}

				protected static List <Collider> mColliders = new List <Collider> ();//for re-use
		}

		public interface IGUIBrowserObject {
				IGUIBrowser ParentBrowser { get; set; }
				GameObject gameObject { get; }
				Transform transform { get; }
				string name { get; set; }
				float Size { get; }
				bool DeleteRequest { get; set; }
		}
}