using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Completed
{
	using System.Collections.Generic;		//Allows us to use Lists. 
	using UnityEngine.UI;					//Allows us to use UI.
	
	public class AIWeaponCount : ReinforcementAI
	{
        private int threshold_favorable = 5;  // Threshold to differentiate between positve/negative feedback

        // Update weights
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

        // Output weapon percentage as 4 or 2
        public int getWeaponWeight() {
            // Translate +1 or -1 outcome to positive and negative outcomes for weapon percent
            int action = getOutput();
            if(action == 1){
                // 4 : Weapon percent
                return 4;
            }
            else{
                // 2 : Weapon Percent
                return 2;
            }
        }
	}
}

