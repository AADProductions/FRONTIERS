using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers.World
{
		public class AudibleBubble : AwarenessBubble <IAudible>
		{
				public List <IListener> Listeners = new List<IListener>();
				public MasterAudio.SoundType SoundType;
				public string SoundName;

				public override void Awake()
				{
						base.Awake();
						gameObject.layer = Globals.LayerNumAwarenessBroadcaster;
				}

				protected override void OnStartUsing()
				{
						SoundType = ParentObject.LastSoundType;
						SoundName = ParentObject.LastSoundName;

						Collider.radius = ParentObject.AudibleRange + Globals.MaxAudibleRange;//this will ensure that any creature can potentially hear it
						Collider.height = Collider.radius;//we don't want a capsule shape
						Collider.center = Vector3.zero;
				}

				protected override void OnUpdateAwareness()
				{
						for (int i = 0; i < Listeners.Count; i++) {
								//the listener will determine whether they can actually hear it
								//based on the parent object's properties
								Listeners[i].HearSound(ParentObject, SoundType, SoundName);
						}
						Listeners.Clear();
				}

				protected IItemOfInterest mIoiCheck = null;
				protected Listener mlistenerCheck = null;
				protected IListener mlisteningItemCheck = null;

				protected override void HandleEncounter(UnityEngine.Collider other)
				{
						//this will cover most cases including the player
						mlisteningItemCheck = (IListener)other.GetComponent(typeof(IListener));
						if (mlisteningItemCheck == null) {
								//whoops, we have to do some heavy lifting
								//see if it's a world item
								mIoiCheck = null;
								mlistenerCheck = null;
								if (WorldItems.GetIOIFromCollider(other, out mIoiCheck) && mIoiCheck.IOIType == ItemOfInterestType.WorldItem && mIoiCheck.worlditem.Is <Listener>(out mlistenerCheck)) {
										mlisteningItemCheck = mlistenerCheck;
								}
						}
						//unlike the visibility bubble
						//listeners are responsible for figuring out whether they can hear the item
						//so just add it to the list and it'll be pushed in OnUpdateAwareness
						Listeners.SafeAdd(mlisteningItemCheck);
				}

				protected override void OnFinishUsing()
				{
						Listeners.Clear();
				}
		}
}
