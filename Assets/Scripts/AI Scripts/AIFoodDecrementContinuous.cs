using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Completed
{
	using System.Collections.Generic;		//Allows us to use Lists. 
	using UnityEngine.UI;					//Allows us to use UI.
	
	public class AIFoodDecrementContinuous : ReinforcementAIContinuous 
	{
		public class Count
		{
			public int minimum; 			//Minimum value for our Count class.
			public int maximum; 			//Maximum value for our Count class.


			//Assignment constructor.
			public Count (int min, int max)
			{
				minimum = min;
				maximum = max;
			}
		}

		// Set Range for DirChange Values (Input)
        public Count dirChangesRangeInput = new Count (10, 100);
		// Set Range for foodDecrement Values (Output)
        public Count foodDrecrementRangeOutput = new Count (-10, -1);

		// Update generator with probability as current value in terms of range (x / (max - min))
        public override void updateGenerator(float feedback) {
            base.updateGenerator((feedback - dirChangesRangeInput.minimum) / (dirChangesRangeInput.maximum - dirChangesRangeInput.minimum));
        }
		// Output food decrement as an integer
        public int getFoodDecrement() {
            float action = getOutput();
            return (int)(foodDrecrementRangeOutput.minimum + action * (foodDrecrementRangeOutput.maximum - foodDrecrementRangeOutput.minimum));
        }
	}
}

