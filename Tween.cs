using System;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine;

namespace Rucrede
{ 
	public delegate float EasingFunction (float start,float end,float value);

	public delegate void TweenDelegate (Tween tween);

	public delegate void VoidDelegate ();
	
	public class TweenDescriptor
	{
		public Type ObjectType;
		public object fromValue;
		public object toValue;
		public Tween.EaseType easeType;
		public bool baseOnSpeed = false;
		public float speed;
		public float time;
		public float delay;
		public bool yoyo;
		public int loops;
		public TweenDelegate updateMethod;
		public TweenDelegate completeMethod;
	}
		
	public class Tween
	{	
		public static List<Tween> Tweens = new List<Tween> ();
		
		public event TweenDelegate UpdateEvent;
		public event TweenDelegate CompleteEvent;

		public bool Unpausable = false;
		
		public float Progress { get { return _easeValue; } }

		public object StartValue { get { return _startValue; } }

		public object EndValue { get { return _endValue; } }

		public object Value { get { return _value; } }

		public bool Active { get { return _running; } }

		public bool Yoyo { get { return _yoyo; } set { _yoyo = value; } }
		
		protected TweenController TweenController;
		protected int _tweenID;
		protected object _value;
		protected object _startValue;
		protected object _endValue;
		protected float _easeValue;
		protected float _progress;
		protected float _range;
		protected float _startTime;
		protected float _currentTime;
		protected float _duration;
		protected bool _running;
		protected float _delay;
		protected float _delayTimeStamp;
		protected bool _paused;
		protected bool _yoyo;
		protected int _loopCount;
		protected int _loops;
		protected bool _reversing = false;
		protected float _pauseTimeStamp;
		protected IEaser _easer;
		protected TweenDescriptor _descriptor;
				
		protected Tween (TweenDescriptor aDescriptor)
		{
			_descriptor = aDescriptor;

			_tweenID = Tweens.Count;	
			_startValue = aDescriptor.fromValue; 
			_endValue = aDescriptor.toValue;
		
			_range = GetRange (_startValue, _endValue);
			if (!aDescriptor.baseOnSpeed) {
				_duration = aDescriptor.time;
			} else {
				_duration = GetDuration (aDescriptor.speed, _startValue, _endValue); 
			}

			_easer = GetEaser (aDescriptor.easeType); 	
			_progress = 0.0f;
			_delayTimeStamp = Time.timeSinceLevelLoad; 	
			_delay = aDescriptor.delay;
			_yoyo = aDescriptor.yoyo;
			_loopCount = 0;
			_loops = aDescriptor.loops;
		
			if (_yoyo && _loops == 0) //if its a yoyo effect, the loop will be atleast 1:
				_loops = 1;		
			
			_reversing = false;
		
			if (aDescriptor.updateMethod != null)
				UpdateEvent += aDescriptor.updateMethod;

			if (aDescriptor.completeMethod != null)
				CompleteEvent += aDescriptor.completeMethod;

			Tweens.Add (this);
		
			TweenController.Singleton.UpdateEvent += Update;
			Play (); 
		}
	
		public void Update ()
		{
			if (_running == false || _paused)
				return;
		
			if ((_delayTimeStamp + _delay) > Time.timeSinceLevelLoad)
				return;
		
			_currentTime = (Time.timeSinceLevelLoad - _startTime);
			if (_currentTime >= _duration)
				_currentTime = _duration;

			if (_duration > 0f) {
				_progress = _currentTime / _duration;   
			} else {
				_progress = 1.0f;
			}
		
			float directionalProgress = _reversing ? 1.0f - _progress : _progress;
			_easeValue = _easer.ease (directionalProgress); 
			UpdateObjectValue (_easeValue); 
		
			if (UpdateEvent != null) {
				UpdateEvent.Invoke (this);
			}

			if (_progress == 1.0f) {
				if (_loopCount >= _loops && _loops != -1) { //-1 is infinite loop
					_running = false;
					if (CompleteEvent != null)
						CompleteEvent (this);
					Destroy ();
				} else {
					if (_yoyo)
						_reversing = !_reversing;

					_loopCount++;
					Rewind ();
				}	
			}
		}
	
		public bool isPaused ()
		{
			return _paused;
		}

		public void Play ()
		{		
			if (!_paused) {
				_startTime = Time.timeSinceLevelLoad + _delay;
				_delayTimeStamp = Time.timeSinceLevelLoad; 	
				_running = true;
			} else {
				Resume();	
			}
		}
	
		public void Pause ()
		{
			if (!Unpausable) {
				_paused = true;
				_pauseTimeStamp = Time.timeSinceLevelLoad;
			}
		}

		public void Resume ()
		{
			if (_paused) {
				_paused = false;
				float thePauseTime = Time.timeSinceLevelLoad - _pauseTimeStamp;

				_startTime += thePauseTime;
				_delayTimeStamp += thePauseTime;
			}
		}
	
		protected void Rewind ()
		{
			_startTime = Time.timeSinceLevelLoad;
		}

		public void Stop ()
		{
			_running = false;
			_loopCount = 0;
		}

		protected void UpdateObjectValue (float easeValue)
		{
		
			if (_startValue is float) {
				_value = (float)_startValue + _range * easeValue; 
			}
		
			if (_startValue is Vector2) { 
				Vector2 direction = ((Vector2)_endValue - (Vector2)_startValue).normalized;
				direction *= _range;
				_value = (Vector2)_startValue + direction * easeValue; 
			}
		
			if (_startValue is Vector3) {
				Vector3 direction = ((Vector3)_endValue - (Vector3)_startValue).normalized;
				direction *= _range; 
				_value = (Vector3)_startValue + direction * easeValue;
			}	
		
			if (_startValue is Quaternion) {
				_value = Quaternion.Lerp ((Quaternion)_startValue, (Quaternion)_endValue, easeValue);
			}		

			if (_startValue is Color) {
				float rangeR = ((Color)_endValue).r - ((Color)_startValue).r;	
				float rangeG = ((Color)_endValue).g - ((Color)_startValue).g;	
				float rangeB = ((Color)_endValue).b - ((Color)_startValue).b;	
				float rangeA = ((Color)_endValue).a - ((Color)_startValue).a;	
				_value = new Color (((Color)_startValue).r + rangeR * easeValue,
		                   		((Color)_startValue).g + rangeG * easeValue,
		                  		((Color)_startValue).b + rangeB * easeValue,
		                   		((Color)_startValue).a + rangeA * easeValue
				);
			}		
		}

		float GetDuration (float speed, object start, object end)
		{		
			return Mathf.Abs (GetRange (start, end) / speed);
		}
	
		float GetRange (object start, object end)
		{
			if (start is float) {
				return (float)end - (float)start;	
			}
		
			if (start is Vector2) {
				return Mathf.Abs (Vector2.Distance ((Vector2)start, (Vector2)end));	
			}
		
			if (start is Vector3) {
				return Vector3.Distance ((Vector3)start, (Vector3)end);	
			}		
		
			if (start is Quaternion) {
				return Quaternion.Angle ((Quaternion)start, (Quaternion)end);	
			}	

			return 0f;
		}

		public float LifeTime ()
		{
			return Time.timeSinceLevelLoad - _startTime;
		}
	
		public void OnCompleteEvent ()
		{
			Destroy (); 
			if (CompleteEvent != null)
				CompleteEvent (this);
	
		}
	
		public void Destroy ()
		{
			Stop ();

			TweenController.Singleton.UpdateEvent -= Update;

			if (_descriptor.updateMethod != null)
				UpdateEvent -= _descriptor.updateMethod;
	
			if (_descriptor.completeMethod != null)
				CompleteEvent -= _descriptor.completeMethod;

			//remove from list
			if (Tweens.Contains (this))
				Tweens.Remove (this); 		
		
		}
		
		public static Tween to (TweenDescriptor descriptor)
		{
			Tween t = new Tween (descriptor); 	
			return t;
		}

		public static Tween delayedCall (float aDelay, VoidDelegate onComplete)
		{
			return delayedCall (aDelay, (Tween Tween) => {
				onComplete.Invoke ();});
		}

		public static Tween delayedCall (float aDelay, TweenDelegate onComplete)
		{
			return to (0.0f, 1.0f, aDelay, Tween.EaseType.linear, null, onComplete, 0f);
		}

		public static Tween to (object startValue, object endValue, float time, Tween.EaseType easeType, TweenDelegate onUpdate = null, TweenDelegate onComplete = null, float delay = 0.0f, bool yoyo = false, int loops = 0)
		{	
			TweenDescriptor descriptor = new TweenDescriptor ();
			descriptor.fromValue = startValue; 
			descriptor.toValue = endValue;
			descriptor.easeType = easeType; 
			descriptor.delay = delay;
			descriptor.time = time;
			descriptor.yoyo = yoyo;
			descriptor.loops = loops;
			descriptor.updateMethod = onUpdate;
			descriptor.completeMethod = onComplete;
			return to (descriptor);
		}

		public static Tween toWithSpeed (object startValue, object endValue, float speed, Tween.EaseType easeType, TweenDelegate onUpdate = null, TweenDelegate onComplete = null, float delay = 0.0f, bool yoyo = false, int loops = 0)
		{
			TweenDescriptor descriptor = new TweenDescriptor ();
			descriptor.baseOnSpeed = true; 
			descriptor.fromValue = startValue; 
			descriptor.toValue = endValue;
			descriptor.easeType = easeType; 
			descriptor.delay = delay;
			descriptor.speed = speed;
			descriptor.yoyo = yoyo;
			descriptor.loops = loops;
			descriptor.updateMethod = onUpdate;
			descriptor.completeMethod = onComplete;
			return to (descriptor);
		}

		protected  IEaser GetEaser (EaseType easeType)
		{
			IEaser easer = null; 
			switch (easeType) {
			case EaseType.easeInQuad:
				easer = new Easer (new EasingFunction (EasingFunctions.easeInQuad));
				break;
			case EaseType.easeOutQuad:
				easer = new Easer (new EasingFunction (EasingFunctions.easeOutQuad));
				break;
			case EaseType.easeInOutQuad:
				easer = new Easer (new EasingFunction (EasingFunctions.easeInOutQuad));
				break;
			case EaseType.easeInCubic:
				easer = new Easer (new EasingFunction (EasingFunctions.easeInCubic));
				break;
			case EaseType.easeOutCubic:
				easer = new Easer (new EasingFunction (EasingFunctions.easeOutCubic));
				break;
			case EaseType.easeInOutCubic:
				easer = new Easer (new EasingFunction (EasingFunctions.easeInOutCubic));
				break;
			case EaseType.easeInQuart:
				easer = new Easer (new EasingFunction (EasingFunctions.easeInQuart));
				break;
			case EaseType.easeOutQuart:
				easer = new Easer (new EasingFunction (EasingFunctions.easeOutQuart));
				break;
			case EaseType.easeInOutQuart:
				easer = new Easer (new EasingFunction (EasingFunctions.easeInOutQuart));
				break;
			case EaseType.easeInQuint:
				easer = new Easer (new EasingFunction (EasingFunctions.easeInQuint));
				break;
			case EaseType.easeOutQuint:
				easer = new Easer (new EasingFunction (EasingFunctions.easeOutQuint));
				break;
			case EaseType.easeInOutQuint:
				easer = new Easer (new EasingFunction (EasingFunctions.easeInOutQuint));
				break;
			case EaseType.easeInSine:
				easer = new Easer (new EasingFunction (EasingFunctions.easeInSine));
				break;
			case EaseType.easeOutSine:
				easer = new Easer (new EasingFunction (EasingFunctions.easeOutSine));
				break;
			case EaseType.easeInOutSine:
				easer = new Easer (new EasingFunction (EasingFunctions.easeInOutSine));
				break;
			case EaseType.easeInExpo:
				easer = new Easer (new EasingFunction (EasingFunctions.easeInExpo));
				break;
			case EaseType.easeOutExpo:
				easer = new Easer (new EasingFunction (EasingFunctions.easeOutExpo));
				break;
			case EaseType.easeInOutExpo:
				easer = new Easer (new EasingFunction (EasingFunctions.easeInOutExpo));
				break;
			case EaseType.easeInCirc:
				easer = new Easer (new EasingFunction (EasingFunctions.easeInCirc));
				break;
			case EaseType.easeOutCirc:
				easer = new Easer (new EasingFunction (EasingFunctions.easeOutCirc));
				break;
			case EaseType.easeInOutCirc:
				easer = new Easer (new EasingFunction (EasingFunctions.easeInOutCirc));
				break;
			case EaseType.linear:
				easer = new Easer (new EasingFunction (EasingFunctions.linear));
				break;
			case EaseType.spring:
				easer = new Easer (new EasingFunction (EasingFunctions.spring));
				break;
			case EaseType.easeInBounce:
				easer = new Easer (new EasingFunction (EasingFunctions.easeInBounce));
				break;
			case EaseType.easeOutBounce:
				easer = new Easer (new EasingFunction (EasingFunctions.easeOutBounce));
				break;
			case EaseType.easeInOutBounce:
				easer = new Easer (new EasingFunction (EasingFunctions.easeInOutBounce));
				break;
			case EaseType.easeInBack:
				easer = new Easer (new EasingFunction (EasingFunctions.easeInBack));
				break;
			case EaseType.easeOutBack:
				easer = new Easer (new EasingFunction (EasingFunctions.easeOutBack));
				break;
			case EaseType.easeInOutBack:
				easer = new Easer (new EasingFunction (EasingFunctions.easeInOutBack));
				break;
			case EaseType.easeInElastic:
				easer = new Easer (new EasingFunction (EasingFunctions.easeInElastic));
				break;
			case EaseType.easeOutElastic:
				easer = new Easer (new EasingFunction (EasingFunctions.easeOutElastic));
				break;
			case EaseType.easeInOutElastic:
				easer = new Easer (new EasingFunction (EasingFunctions.easeInOutElastic));
				break;
		
			//COMBINED EASERS:	
			
			case EaseType.easeInQuartOutSine:
				easer = new CombinedEaser (new EasingFunction (EasingFunctions.easeInQuart), 
										new EasingFunction (EasingFunctions.easeOutSine), 
										new EasingFunction (EasingFunctions.linear));
				break;
			case EaseType.easeInQuartOutQuad:
				easer = new CombinedEaser (new EasingFunction (EasingFunctions.easeInQuart), 
										new EasingFunction (EasingFunctions.easeOutQuad), 
										new EasingFunction (EasingFunctions.linear));
				break;
			case EaseType.easeInQuartOutCubic:
				easer = new CombinedEaser (new EasingFunction (EasingFunctions.easeInQuart), 
										new EasingFunction (EasingFunctions.easeOutCubic), 
										new EasingFunction (EasingFunctions.linear));
				break;
			case EaseType.easeInSineOutQuart:
				easer = new CombinedEaser (new EasingFunction (EasingFunctions.easeInSine), 
										new EasingFunction (EasingFunctions.easeOutQuart), 
										new EasingFunction (EasingFunctions.linear));
				break;
			case EaseType.easeInSineOutQuad:
				easer = new CombinedEaser (new EasingFunction (EasingFunctions.easeInSine), 
										new EasingFunction (EasingFunctions.easeOutQuad), 
										new EasingFunction (EasingFunctions.linear));
				break;
			case EaseType.easeInSineOutCubic:
				easer = new CombinedEaser (new EasingFunction (EasingFunctions.easeInSine), 
										new EasingFunction (EasingFunctions.easeOutCubic), 
										new EasingFunction (EasingFunctions.linear));
				break;
			case EaseType.easeInQuadOutQuart:
				easer = new CombinedEaser (new EasingFunction (EasingFunctions.easeInQuad), 
										new EasingFunction (EasingFunctions.easeOutQuart), 
										new EasingFunction (EasingFunctions.linear));
				break;
			case EaseType.easeInQuadOutSine:
				easer = new CombinedEaser (new EasingFunction (EasingFunctions.easeInQuad), 
										new EasingFunction (EasingFunctions.easeOutSine), 
										new EasingFunction (EasingFunctions.linear));
				break;
			case EaseType.easeInQuadOutCubic:
				easer = new CombinedEaser (new EasingFunction (EasingFunctions.easeInQuad), 
										new EasingFunction (EasingFunctions.easeOutCubic), 
										new EasingFunction (EasingFunctions.linear));
				break;
			case EaseType.easeInCubicOutQuad:
				easer = new CombinedEaser (new EasingFunction (EasingFunctions.easeInCubic), 
										new EasingFunction (EasingFunctions.easeOutQuad), 
										new EasingFunction (EasingFunctions.linear));
				break;
			case EaseType.easeInCubicOutSine:
				easer = new CombinedEaser (new EasingFunction (EasingFunctions.easeInCubic), 
										new EasingFunction (EasingFunctions.easeOutSine), 
										new EasingFunction (EasingFunctions.linear));
				break;
			case EaseType.easeInCubicOutQuart:
				easer = new CombinedEaser (new EasingFunction (EasingFunctions.easeInCubic), 
										new EasingFunction (EasingFunctions.easeOutQuart), 
										new EasingFunction (EasingFunctions.linear));
				break;	
			case EaseType.easeInOutSineFlatLine:
				easer = new CombinedEaser (new EasingFunction (EasingFunctions.easeInOutSine), 
										new EasingFunction (EasingFunctions.linear), 
										new EasingFunction (EasingFunctions.constant));
				break;
			case EaseType.easeInOutSineOutLinear:
				easer = new CombinedEaser (new EasingFunction (EasingFunctions.easeInOutQuint),
									   new EasingFunction (EasingFunctions.easeInOutSine),
									   new EasingFunction (EasingFunctions.linear));
				break;
			}
			return easer;
		}

	
		public enum EaseType
		{
			easeInQuad,
			easeOutQuad,
			easeInOutQuad,
			easeInCubic,
			easeOutCubic,
			easeInOutCubic,
			easeInQuart,
			easeOutQuart,
			easeInOutQuart,
			easeInQuint,
			easeOutQuint,
			easeInOutQuint,
			easeInSine,
			easeOutSine,
			easeInOutSine,
			easeInExpo,
			easeOutExpo,
			easeInOutExpo,
			easeInCirc,
			easeOutCirc,
			easeInOutCirc,
			linear,
			spring,
			easeInBounce,
			easeOutBounce,
			easeInOutBounce,
			easeInBack,
			easeOutBack,
			easeInOutBack,
			easeInElastic,
			easeOutElastic,
			easeInOutElastic,
			punch, 
			//combined tweens:  
			easeInQuartOutSine,
			easeInQuartOutQuad,
			easeInQuartOutCubic,
			easeInSineOutQuart,
			easeInSineOutQuad,
			easeInSineOutCubic,
			easeInQuadOutQuart,
			easeInQuadOutSine,
			easeInQuadOutCubic,	
			easeInCubicOutQuad,
			easeInCubicOutSine,
			easeInCubicOutQuart,		
			easeInOutSineFlatLine,
			easeInOutSineOutLinear
		}
	}

}