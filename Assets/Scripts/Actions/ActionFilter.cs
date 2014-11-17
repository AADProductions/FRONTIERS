using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Frontiers
{
	public class ActionFilter <T> : MonoBehaviour
	{
		public T Filter;
		public T FilterExceptions;
		public PassThroughBehavior Behavior = PassThroughBehavior.InterceptByFilter;
		public bool HasFocus = false;

		public virtual void WakeUp ()
		{

		}

		public virtual void Awake ()
		{
			WakeUp ();
		}

		public virtual void Update ()
		{
			List<ActionListener> listners;
			mUpdating = true;
			foreach (KeyValuePair<T,float> actionPair in mActionList) {
				listners = null;
				if (mListeners.TryGetValue (actionPair.Key, out listners)) {
					CallListeners (listners, actionPair.Key, actionPair.Value);
				}
			}
			mUpdating = false;
			mActionList.Clear ();

			foreach (KeyValuePair<T,float> updateAction in mActionListDuringUpdate) {
				mActionList.Add (updateAction);
			}

			mActionListDuringUpdate.Clear ();
		}

		public virtual bool GainFocus ()
		{
			HasFocus = true;
			return true;
		}

		public virtual bool LoseFocus ()
		{
			HasFocus = false;
			return true;
		}

		public virtual bool ReceiveAction (T action, double timeStamp)
		{
			if (!mSubscribersSet) {
				return false;
			}

			bool passThrough = true;
			bool isSubscribed = mSubscriptionCheck (mSubscribed, action);
			bool isException = mSubscriptionCheck (FilterExceptions, action);
				
			switch (Behavior) {
			case PassThroughBehavior.InterceptByFocus:
				if (!isException && HasFocus) {
					passThrough = false;
				}
				break;
			case PassThroughBehavior.InterceptByFilter:
				if (!isException && isSubscribed) {
					passThrough = false;
				}
				break;

			case PassThroughBehavior.InterceptBySubscription:
				if (!isException && isSubscribed) {
					passThrough = false;
				}
				break;

			case PassThroughBehavior.PassThrough:
				break;
				
			case PassThroughBehavior.InterceptAll:
				//this applies to exceptions!!!
				passThrough = false;
				break;
			}

			if (isSubscribed) {
				List <ActionListener> listenerList = null;
				if (mListeners.TryGetValue (action, out listenerList)) {
					CallListeners (listenerList, action, timeStamp);
				}
			}
			return passThrough;
		}

		protected void CallListeners (List <ActionListener> listenerList, T action, double timeStamp)
		{
			for (int listenerIndex = listenerList.Count - 1; listenerIndex >= 0; listenerIndex--) {
				ActionListener listener = listenerList [listenerIndex];
				if (listener != null) {
					listener (timeStamp);
				} else {
					listenerList.RemoveAt (listenerIndex);
				}
			}
		}

		public virtual void Subscribe (T action, ActionListener listner)
		{
			List <ActionListener> listnerList;
			if (mListeners.TryGetValue (action, out listnerList) == false) {
				listnerList = new List <ActionListener> ();
				mListeners.Add (action, listnerList);
			}
			listnerList.Add (listner);
			if (mSubscriptionAdd != null) {
				mSubscriptionAdd (ref mSubscribed, action);
				//take care of queued subscriptions at this point
				while (mQueuedSubscriptions.Count > 0) {
					mSubscriptionAdd (ref mSubscribed, mQueuedSubscriptions.Dequeue ());
				}
			} else {
				mQueuedSubscriptions.Enqueue (action);
			}
		}

		public void UnsubscribeAll ()
		{
			mSubscribed = mDefault;
		}

		protected SubscriptionAdd <T> mSubscriptionAdd;
		protected SubscriptionCheck <T> mSubscriptionCheck;
		protected T mSubscribed;
		protected T mDefault;
		protected HashSet <KeyValuePair <T,float>> mActionList = new HashSet <KeyValuePair <T,float>> ();
		protected HashSet <KeyValuePair <T,float>> mActionListDuringUpdate = new HashSet <KeyValuePair <T,float>> ();
		protected bool mUpdating = false;
		protected bool mSubscribersSet = false;
		protected Dictionary <T, List <ActionListener>>	mListeners = new Dictionary <T, List <ActionListener>> ();
		protected Queue <T> mQueuedSubscriptions = new Queue <T> ();
	}

	public enum PassThroughBehavior
	{
		InterceptByFocus,
		InterceptByFilter,
		InterceptBySubscription,
		PassThrough,
		InterceptAll,
	}
}