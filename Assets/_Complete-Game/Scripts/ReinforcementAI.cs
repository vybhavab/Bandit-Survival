using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Completed {
	using System.Collections.Generic;		//Allows us to use Lists. 
	using UnityEngine.UI;					//Allows us to use UI.
	
	public class ReinforcementAI : MonoBehaviour {
        private float generator_value = 0.0f;
        protected float weight = 1.0f;
        protected float multiplier = 2.0f;

        private int output = 1; // {1 : Positive, -1 : Negative}
        private int intent = 1; // {1 : if generator_value >= 0, -1 : otherwise}

        protected void Start() {

        }

        public virtual void updateGenerator(int feedback) {
            if(intent == feedback) {
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
            }
        }

        private float sigmoidConstrained(float value) {
            float return_value = 1.0f / (1.0f + (float) Mathf.Exp(-value));
            if (return_value > 0.8f) {
                return_value = 0.8f;
            }
            if (return_value < 0.2f) {
                return_value = 0.2f;
            }
            return return_value;
        }

        public int getOutput() {
            Debug.Log("Generator Seed: " + generator_value);
            float prob = sigmoidConstrained(generator_value);
            if(prob >= 0.5) {
                intent = 1;
            }
            else {
                intent = -1;
            }
            if(Random.Range(0.0f, 1.0f) <= prob) {
                output = intent;
            }
            else {
                output = intent * -1;
            }
            Debug.Log("Intent: " + intent);
            Debug.Log("Output: " + output);
            return output;
        }
	}
}

