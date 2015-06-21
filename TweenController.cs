using UnityEngine;
using System.Collections;

namespace Rucrede
{
	public class TweenController : MonoBehaviour
	{
	
		protected static TweenController _instance;

		public static TweenController Singleton {
			get { return _instance; }
		}

		public event VoidDelegate UpdateEvent;
	
		void Awake ()
		{
			_instance = this;
		}
	
		void Update ()
		{
			if (UpdateEvent != null)
				UpdateEvent.Invoke ();
		}

		public void Pause ()
		{
			foreach (Tween theTween in Tween.Tweens) {
				theTween.Pause ();
			}	
		}
	
		public void Resume ()
		{
			foreach (Tween theTween in Tween.Tweens) {
				theTween.Resume ();
			}	
		}
	}
}


