using System;

namespace Rucrede
{
	public class Easer: IEaser
	{	
		protected EasingFunction _ease;
	
		public Easer (EasingFunction easeFunction)
		{
			_ease = easeFunction;	
		}

		public float ease (float progress)
		{
			return _ease (0.0f, 1.0f, progress); 	
		}		
	}
}