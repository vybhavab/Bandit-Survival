using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Completed
{
    using System.Collections.Generic;       //Allows us to use Lists.
    using UnityEngine.UI;                   //Allows us to use UI.

    public class AIEnemyHealth : ReinforcementAI
    {
        private int threshold_favorable = 5;  // Threshold to differentiate between positve/negative feedback

        public override void updateGenerator(int feedback)
        {
            // Transform feedback to +1 or -1
            int modified_feedback;
            if (feedback > threshold_favorable)
            {
                modified_feedback = 1;
            }
            else
            {
                modified_feedback = -1;
            }
            // Update probabilities based on feedback
            base.updateGenerator(modified_feedback);
        }

        public int GetHealthChange(int level)
        {
            // Translate +1 or -1 outcome to positive and negative outcomes for food decrement
            int action = getOutput();
            // (int, int) -> (food counter increment, food consume increment)
            if (action == 1)
            {
                if (level <= 5) {
                    return 20;
                }
                else if (level > 5 && level <= 10)
                {
                    return 70;
                }
                else if (level > 10 && level <= 15)
                {
                    return 200;
                }
                else if (level > 15 && level <= 20)
                {
                    return 1000;
                }
                else
                {
                    return 10000;
                }
            }
            else
            {
                if (level <= 5)
                {
                    return -5;
                }
                else if (level > 5 && level <= 10)
                {
                    return -25;
                }
                else if (level > 10 && level <= 15)
                {
                    return -100;
                }
                else if (level > 15 && level <= 20)
                {
                    return -500;
                }
                else
                {
                    return -4000;
                }
            }
        }
    }
}
