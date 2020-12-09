using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Random = UnityEngine.Random; 		//Tells Random to use the Unity Engine random number generator.

namespace Completed {
	using System.Collections.Generic;		//Allows us to use Lists. 
	using UnityEngine.UI;					//Allows us to use UI.
	
	public class ReinforcementAIContinuous : MonoBehaviour {
        private float generator_value;  // Value passed to sigmoid to determine probability of 1
        protected float learning_rate = 10.0f;  // Weightage for generator_value modification
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
            float sigmoid_intent = sigmoidConstrained(intent);
            generator_value = generator_value + learning_rate * (feedback - sigmoid_intent) * sigmoid_intent * (1 - sigmoid_intent);
            float sigmoid_output = sigmoidConstrained(output);
            generator_value = generator_value + learning_rate * multiplier * (feedback - sigmoid_output) * sigmoid_output * (1 - sigmoid_output);
            deviation = Mathf.Abs(deviation - generator_value);
        }

        private float sigmoidConstrained(float value) {
            float return_value = 1.0f / (1.0f + (float) Mathf.Exp(-value));  // Get value in [0, 1]
            return return_value;
        }

        public float getOutput() {
            Debug.Log("Generator Seed: " + generator_value);
            float prob = sigmoidConstrained(Random.Range(generator_value - deviation, generator_value + deviation));
            return prob;
        }
	}
}

