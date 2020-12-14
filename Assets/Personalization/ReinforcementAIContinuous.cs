using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Random = UnityEngine.Random; 		//Tells Random to use the Unity Engine random number generator.

namespace Completed {
	using System.Collections.Generic;		//Allows us to use Lists.
	using UnityEngine.UI;					//Allows us to use UI.

	public class ReinforcementAIContinuous : MonoBehaviour {
        private float generator_value;  // Value passed to sigmoid to determine probability
        protected float learning_rate = 10.0f;  // Weightage for generator_value modification
        protected float multiplier = 0.5f;  // Multiplier for weighted output modification

        private float output;  // {1 : Positive, -1 : Negative}
        private float intent;  // {1 : if generator_value >= 0, -1 : otherwise}

        private float deviation;  // Range of probabilities to generate

        // Object Initialization
        protected void Start() {
            // Set default values
            generator_value = 0.0f;
            output = 0.0f;
            intent = 0.0f;
            deviation = 0.0f;
        }

        // Update generator value
        public virtual void updateGenerator(float feedback) {
            deviation = generator_value;  // Set previous generator value to deviation
            float sigmoid_intent = sigmoidConstrained(intent);  // Calculate intent y
            generator_value = generator_value + learning_rate * (feedback - sigmoid_intent) * sigmoid_intent * (1 - sigmoid_intent);  // Update generator value based on intent
            float sigmoid_output = sigmoidConstrained(output);  // Calculate output y
            generator_value = generator_value + learning_rate * multiplier * (feedback - sigmoid_output) * sigmoid_output * (1 - sigmoid_output);  // Update generator value based on output
            deviation = Mathf.Abs(deviation - generator_value);  // Update deviation to be the difference between new and old generator value
        }

        // Output Sigmoid(value)
        private float sigmoidConstrained(float value) {
            float return_value = 1.0f / (1.0f + (float) Mathf.Exp(-value));  // Get value in [0, 1]
            return return_value;
        }

        // Generate probability output
        public float getOutput() {
            //Debug.Log("Generator Seed: " + generator_value);
            // Input value to sigmoid is chosen randomly from [generator - deviation, generator + deviation]
            float prob = sigmoidConstrained(Random.Range(generator_value - deviation, generator_value + deviation));
            return prob;
        }
	}
}

