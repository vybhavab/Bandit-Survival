using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Completed
{
	using System.Collections.Generic;		//Allows us to use Lists. 
	using UnityEngine.UI;					//Allows us to use UI.
	
	public class AIFoodDecrement : ReinforcementAI
	{
        private int threshold_favorable = 3;

        public override void updateGenerator(int feedback) {
            int modified_feedback = 0;
            if(feedback > threshold_favorable) {
                modified_feedback = 1;
            }
            else {
                modified_feedback = -1;
            }
            base.updateGenerator(modified_feedback);
        }

        public (int, int) getFoodDecrement() {
            int action = getOutput();
            if(action == 1){
                return (-2, 50);
            }
            else{
                return (-1, 10);
            }
        }
	}
}

