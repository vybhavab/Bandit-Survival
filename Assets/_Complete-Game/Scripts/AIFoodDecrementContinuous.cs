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
        private int threshold_favorable = 100;  // Threshold to differentiate between positve/negative feedback

        public override void updateGenerator(float feedback) {
            // Transform feedback to +1 or -1
            /*int modified_feedback = 0;
            if(feedback > threshold_favorable) {
                modified_feedback = 1;
            }
            else {
                modified_feedback = -1;
            }*/
            // Update probabilities based on feedback
            base.updateGenerator((feedback - dirChangesRangeInput.minimum) / (dirChangesRangeInput.maximum - dirChangesRangeInput.minimum));
        }

        public int getFoodDecrement() {
            // Translate +1 or -1 outcome to positive and negative outcomes for food decrement
            float action = getOutput();
            // (int, int) -> (food counter increment, food consume increment)
            /*if(action == 1){
                // -1 : Food Decrement
                return -1;
            }
            else{
                // -2 : Food Decrement
                return -2;
            }*/
            return (int)(foodDrecrementRangeOutput.minimum + action * (foodDrecrementRangeOutput.maximum - foodDrecrementRangeOutput.minimum));
        }
	}
}

