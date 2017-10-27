using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

namespace BehaviorTree {

	/*
	A class which moves a character to a given target position. This class almost certainly 
	needs to be optimised as it does a lot during one tick. It is just an example for use in
	the demo.

	Fields:
	targetPosition: The xy Vector we should move to (Y value ignored)
	speed: The speed at which we will go there
	rotationSpeed: The speed at which we will turn towards the target

	success: Whether or not we actually reached the target
	maxY: The current highest point on the terrain we are on
	*/
	[Serializable]
	public class MoveToLOS : LeafNode {

		public Vector3 targetPosition;
		public float speed = 1;
		public float rotationSpeed = 1f;

		private bool success;
		private float maxY;

		/*
		Set up the variables
		*/
		public override void setup() {
			success = false;
			maxY = 0;
		}

		//TODO: Write a tester class to determine how long ticks take 
		/*
		Moves our character toward the target position. Does the entire calculation every frame and therefore
		is likely too costly. We may need to split this so it executes over multiple frames
		*/
		public override IEnumerator tick() {

			//While we are not at the target location
			while (Mathf.Abs((transform.position.x - targetPosition.x)) > 0.05f || Mathf.Abs((transform.position.z - targetPosition.z)) > 0.05f) {
				yield return null; //Wait a frame to account for the time it took to run setup

				RaycastHit hit; //Determine if we can see the target
				if (Physics.Linecast(transform.position, targetPosition, out hit, 8, QueryTriggerInteraction.Ignore)) {
					yield break;
				}

				//Find a direction without y component, look toward it using SLERP
				Vector3 direction = (targetPosition - transform.position).normalized;
				direction.y = 0;
				Quaternion rotation = Quaternion.LookRotation(direction);
				transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

				//Set the animator to the "Walk" animation
				GetComponent<Animator>().SetFloat("Walk", 0.3f);

				//Calculate a new position and set the posiiton using the maxY of the terrain
				Vector3 newPos = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed);
				transform.position = new Vector3(newPos.x, maxY, newPos.z);
			}

			//After we're done reest the animator and set success
			GetComponent<Animator>().SetFloat("Walk", 0.0f);
			success = true;
		}
		
		/*
		Set our return based on our success
		*/
		public override void tearDown(out bool childReturn) {
			childReturn = success;
		}

		/*
		Find the highest point of the terrain and set the maxY value accordingly

		Parameters:
		collision: The collision with an object
		*/
		void OnCollisionStay(Collision collision) {
			if (collision.gameObject.tag == "Terrain") {
				maxY = 0;
				foreach (ContactPoint pt in collision.contacts)
					if (pt.point.y > maxY) {
						maxY = pt.point.y;
					}
			}
		}

		/*
		Draws the fields so we can set them via editor 

		**Note: Found solution to the Serialization problem! :)
		*/
		protected override void drawProperties() {
			targetPosition = EditorGUILayout.Vector3Field("Target Position:", targetPosition);

			speed = EditorGUILayout.FloatField("Speed:", speed);

			rotationSpeed = EditorGUILayout.FloatField("Rotation Speed:", rotationSpeed);

		}
	}
}
