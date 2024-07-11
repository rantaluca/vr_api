using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChangeOnHover : MonoBehaviour
		{
			private Color originalColor;

			void Start()
			{
				// Store the original color of the object
				var renderer = GetComponent<Renderer>();
				if (renderer != null)
				{
					originalColor = renderer.material.color;
				}
			}

			void OnTriggerEnter(Collider other)
			{
				// Check if the other collider is the controller
				if (other.CompareTag("Controller"))
				{
					SetObjectColor(Color.red);
				}
			}

			void OnTriggerExit(Collider other)
			{
				// Check if the other collider is the controller
				if (other.CompareTag("Controller"))
				{
					SetObjectColor(originalColor);
				}
			}

			void SetObjectColor(Color color)
			{
				var renderer = GetComponent<Renderer>();
				if (renderer != null)
				{
					renderer.material.color = color;
				}
			}
		}
