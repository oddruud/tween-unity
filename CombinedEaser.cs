using System;

namespace Rucrede
{
	public class CombinedEaser:IEaser
	{	

		private EasingFunction _ease1;
		private EasingFunction _ease2;
		private EasingFunction _combineEase;

		public CombinedEaser (EasingFunction ease1, EasingFunction ease2, EasingFunction combineEase)
		{
			_ease1 = ease1;
			_ease2 = ease2;
			_combineEase = combineEase;
		}
	
		public float ease (float progress)
		{
			float value1 = _ease1 (0f, 1f, progress); 
			float value2 = _ease2 (0f, 1f, progress);
			float combine = _combineEase (0f, 1f, progress);	
			return value1 * (1f - combine) + value2 * combine; 	
		}
	}
}


