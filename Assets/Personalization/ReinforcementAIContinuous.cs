using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Random = UnityEngine.Random; 		//Tells Random to use the Unity Engine random number generator.

namespace Completed {
	using System.Collections.Generic;		//Allows us to use Lists. 
	using UnityEngine.UI;					//Allows us to use UI.
	
	public class ReinforcementAIContinuous : MonoBehaviour {
        private float generator_value;  // Value passed to sigmoid to determine probability of 1
        protected float learning_rate = 1.0f;  // Weightage for generator_value modification
        protected float multiplier = 0.5f;  // Multiplier for extreme generator_value modification

        private float output;  // {1 : Positive, -1 : Negative}
        private float intent;  // {1 : if generator_value >= 0, -1 : otherwise}

        private float deviation;

        protected void Start() {
            generator_value = 0.0f;
            output = 0.0f;
            intent = 0.0f;
            deviation = 0.0f;
        }

        public virtual void updateGenerator(float feedback) {
            deviation = generator_value;
            generator_value = generator_value + learning_rate * (intent - feedback) + learning_rate * multiplier * (output - feedback);
            deviation = Mathf.Abs(deviation - generator_value);
            /*if(intent == feedback) {
                if(intent == output) {
                    generator_value = generator_value + intent * weight;
                    Debug.Log("Intent == Feedback == Output");
                }
                else if(intent != output) {
                    generator_value = generator_value + intent * weight * multiplier;
                    Debug.Log("Intent == Feedback != Output");
                }
            }
            else if(intent != feedback) {
                if(intent == output) {
                    generator_value = generator_value - intent * weight;
                    Debug.Log("Intent != Feedback & Intent == Output");
                }
                else if(intent != output) {
                    generator_value = generator_value - intent * weight * multiplier;
                    Debug.Log("Intent != Feedback & Feedback == Output");
                }
            }*/
        }

        private float sigmoidConstrained(float value) {
            float return_value = 1.0f / (1.0f + (float) Mathf.Exp(-value));  // Get value in [0, 1]
            // Cap value so that is has to be in the range [0.2, 0.8]
            /*if (return_value > 0.8f) {
                return_value = 0.8f;
            }
            if (return_value < 0.2f) {
                return_value = 0.2f;
            }*/
            return return_value;
        }

        public float getOutput() {
            Debug.Log("Generator Seed: " + generator_value);
            float prob = sigmoidConstrained(Random.Range(generator_value - deviation, generator_value + deviation));
            // Determine int based on if probability >= 0.5 -> 1 else -1
            /*if(prob >= 0.5) {
                intent = 1;
            }
            else {
                intent = -1;
            }
            // Select output weighted by probability
            if(Random.Range(0.0f, 1.0f) <= prob) {
                output = intent;
            }
            else {
                output = intent * -1;
            }
            return output;*/
            return prob;
        }
	}
}

