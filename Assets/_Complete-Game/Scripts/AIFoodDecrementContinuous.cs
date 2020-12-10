using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Completed
{
	using System.Collections.Generic;		//Allows us to use Lists. 
	using UnityEngine.UI;					//Allows us to use UI.
	
	public class AIFoodDecrementContinuous : ReinforcementAIContinuous 
	{
        // Using Serializable allows us to embed a class with sub properties in the inspector.
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
        public Count dirChangesRangeInput = new Count (10, 100);
        public Count foodDrecrementRangeOutput = new Count (-10, -1);
        private int threshold_favorable = 10;  // Threshold to differentiate between positve/negative feedback

        public override void updateGenerator(float feedback) {
            base.updateGenerator((feedback - dirChangesRangeInput.minimum) / (dirChangesRangeInput.maximum - dirChangesRangeInput.minimum));
        }

        public int getFoodDecrement() {
            float action = getOutput();
            return (int)(foodDrecrementRangeOutput.minimum + action * (foodDrecrementRangeOutput.maximum - foodDrecrementRangeOutput.minimum));
        }
	}
}

