using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Completed
{
	using System.Collections.Generic;		//Allows us to use Lists. 
	using UnityEngine.UI;					//Allows us to use UI.
	
	public class AIFoodCount : ReinforcementAI
	{
        private int threshold_favorable = 20;  // Threshold to differentiate between positve/negative feedback

        public override void updateGenerator(int feedback) {
            // Transform feedback to +1 or -1
            int modified_feedback = 0;
            if(feedback > threshold_favorable) {
                modified_feedback = 1;
            }
            else {
                modified_feedback = -1;
            }
            // Update probabilities based on feedback
            base.updateGenerator(modified_feedback);
        }

        public int getFoodWeight() {
            // Translate +1 or -1 outcome to positive and negative outcomes for food decrement
            int action = getOutput();
            // (int, int) -> (food counter increment, food consume increment)
            if(action == 1){
                // -1 : Food Decrement
                return 10;
            }
            else{
                // -2 : Food Decrement
                return 6;
            }
        }
	}
}

