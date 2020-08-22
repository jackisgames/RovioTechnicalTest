using System.Linq;
using Assets.Scripts.Presentation.Entities;
using Assets.Scripts.Presentation.Levels;
using Assets.Scripts.UI;
using Logic.Gameplay;
using UnityEngine;

namespace Assets.Scripts
{
	public class LevelComponent : MonoBehaviour
	{
		private LevelService service;
		private UiComponent ui;

		private EntityComponent[] enemies;
		private bool selectionToggle;
		private bool selectionAttackTargetToggle;
		private bool bannerToggle;

		private void Awake()
		{
			// Load the level
			service = new LevelService();
			service.LoadLevel("Level1");

			// Grab all enemies
			enemies = service.LevelData.Entities.Where(p => p.Type == EntityType.Enemy).ToArray();
            
			ui = GameObject.Find("Canvas").GetComponent<UiComponent>();

		    //initialize gameplay
		    gameObject.AddComponent<GameManager>().Initialize(service,ui);
        }

		private void Update()
		{
			// How to detect what grid tile was clicked
			if (Input.GetMouseButtonDown(0))
			{
				Debug.Log("Clicked at grid coordinate " + LevelGrid.MouseToGridCoordinates());
			}

			if (Input.GetKeyDown(KeyCode.T))
			{
				bannerToggle = !bannerToggle;

				if (bannerToggle)
				{
					ui.ShowAndHideBanner("Player's turn");
				}
				else
				{
					ui.ShowAndHideBanner("Enemy turn");
				}
			}

			// This is how you move a character
			if (Input.GetKeyDown(KeyCode.M))
			{
				var enemy = enemies[0];
				enemy.Move(Direction.Down);
				enemy.Move(Direction.Right, 0.5f);
				enemy.Move(Direction.Down, 1);
				enemy.Move(Direction.Down, 1.5f);
				enemy.Move(Direction.Left, 2f);
			}

			// This is how you can trigger a quake animation :)
			if (Input.GetKeyDown(KeyCode.Q))
			{
				var x = Random.Range(0, service.LevelData.Width);
				var y = Random.Range(0, service.LevelData.Height);
				var radius = Random.Range(2, 8);
				service.PlayQuakeAnimation(x, y, radius);
			}

			// And this is how you trigger a damage animation
			if (Input.GetKeyDown(KeyCode.A))
			{
				enemies[0].PlayTakeDamageAnimation();
			}

			// This is how you alter the healthbar for an entity
			if (Input.GetKeyDown(KeyCode.H))
			{
				enemies[0].PlayHealthBarAnimation(Random.Range(0f, 1f));
			}

			// How to select a character
			if (Input.GetKeyDown(KeyCode.S))
			{
				selectionToggle = !selectionToggle;
				enemies[0].ShowSelection(selectionToggle);
			}

			// How to select a character (as an attack target)
			if (Input.GetKeyDown(KeyCode.X))
			{
				selectionAttackTargetToggle = !selectionAttackTargetToggle;
				enemies[0].ShowSelectionAttackTarget(selectionAttackTargetToggle);
			}

			// How to kill a character
			if (Input.GetKeyDown(KeyCode.D))
			{
				enemies[0].PlayDeathAnimation();
			}

			// How to show a breadcrumbs path
			if (Input.GetKeyDown(KeyCode.B))
			{
				service.ShowBreadCrumb(5, 1, true);
				service.ShowBreadCrumb(5, 2, true, 0.1f);
				service.ShowBreadCrumb(5, 3, true, 0.2f);
				service.ShowBreadCrumb(5, 4, true, 0.3f);
				service.ShowBreadCrumb(4, 4, true, 0.4f);
			}
			// And how to hide it...
			else if (Input.GetKeyDown(KeyCode.V))
			{
				service.HideAllBreadCrumbs();
			}
		}
	}
}