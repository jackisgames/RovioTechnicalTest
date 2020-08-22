using System;
using System.Collections.Generic;
using Assets.Scripts.Presentation.Entities;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Presentation.Levels
{
	public class LevelService
	{
		public LevelData LevelData;

		private Sprite[] tileSprites;
		private GameObject tilePrefab;

		private Sprite[] entitySprites;
		private GameObject entityPrefab;

		private Transform levelContainer;
		private Transform tilesContainer;
		private Transform entitiesContainer;

		private float quakeAnimationCooldown;
		private AudioComponent audio;

	    private string lastLoadedData;

		public LevelService()
		{
			tileSprites = Resources.LoadAll<Sprite>("Sprites/Tileset");
			tilePrefab = Resources.Load<GameObject>("Prefabs/Tile");

			entitySprites = Resources.LoadAll<Sprite>("Sprites/Entities");
			entityPrefab = Resources.Load<GameObject>("Prefabs/Entity");

			levelContainer = GameObject.Find("Level").transform;
			tilesContainer = levelContainer.transform.Find("Tiles");
			entitiesContainer = levelContainer.transform.Find("Entities");

			audio = GameObject.Find("Audio").GetComponent<AudioComponent>();
		}

		public void LoadLevel(string levelName)
		{
			var levelText = Resources.Load<TextAsset>($"Levels/{levelName}").text;
			LoadLevelData(levelText);
		}

	    public void LoadLastLevel()
	    {
            LoadLevelData(lastLoadedData);
	    }

	    public void LoadLevelData(string levelText)
	    {
            //clear old assets
            ClearChilds(tilesContainer);
            ClearChilds(entitiesContainer);

	        lastLoadedData = levelText;
	        var rows = levelText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
	        var width = int.Parse(rows[0]);
	        var height = int.Parse(rows[1]);

	        LevelData = new LevelData
	        {
	            Width = width,
	            Height = height,
	            Tiles = new LevelTileComponent[width, height],
	            Entities = new List<EntityComponent>()
	        };

	        // Ground
	        for (int y = 0; y < height; y++)
	        {
	            var row = rows[3 + y];

	            for (int x = 0; x < width; x++)
	            {
	                var tile = int.Parse(row[x].ToString()) - 1;
	                InstantiateTile(x, y, tile);
	            }
	        }

	        // Entities
	        Sprite entitySprite;
	        for (int y = 0; y < height; y++)
	        {
	            var row = rows[4 + height + y];

	            for (int x = 0; x < width; x++)
	            {
	                switch (row[x])
	                {
	                    case 'e':
	                        entitySprite = entitySprites[UnityEngine.Random.Range(0, 5)];
	                        InstantiateEntity(x, y, entitySprite, EntityType.Enemy);
	                        break;

	                    case 'p':
	                        entitySprite = entitySprites[UnityEngine.Random.Range(5, 10)];
	                        InstantiateEntity(x, y, entitySprite, EntityType.Player);
	                        break;

	                    case '#':
	                        entitySprite = tileSprites[49];
	                        InstantiateEntity(x, y, entitySprite, EntityType.Obstacle);
	                        break;
	                }
	            }
	        }

	        CenterCamera(height);
        }

	    private void ClearChilds(Transform target)
	    {
	        int childCount = target.childCount;
	        while (childCount>0)
	        {
	            childCount--;
                Object.Destroy(target.GetChild(childCount).gameObject);
	        }
	    }

		private void CenterCamera(int levelHeight)
		{
			var numberOfRowsBeforeAdjustmentIsNeeded = 9;
			var difference = Mathf.Max(0, levelHeight - numberOfRowsBeforeAdjustmentIsNeeded);

			var adjustment = -(0.33f * difference);
			adjustment = Mathf.RoundToInt(adjustment);
			Camera.main.transform.position = new Vector3(adjustment, adjustment, Camera.main.transform.position.z);
		}

		private void InstantiateTile(int x, int y, int index)
		{
			var tile = GameObject.Instantiate(tilePrefab, Vector3.zero, Quaternion.identity, tilesContainer);
			var renderer = tile.GetComponent<SpriteRenderer>();
			renderer.sortingOrder = LevelGrid.GetSortingOrder(x, y);
			renderer.sprite = tileSprites[index];
			renderer.color = (x ^ y) % 2 == 0 ? Color.white : new Color(0.95f, 0.95f, 0.95f);
			tile.name = $"Tile{x}_{y}";

			tile.transform.localPosition = LevelGrid.ToWorldCoordinates(x, y);

			LevelData.Tiles[x, y] = tile.GetComponent<LevelTileComponent>();
		}

		private EntityComponent InstantiateEntity(int x, int y, Sprite sprite, EntityType type)
		{
			var entity = GameObject.Instantiate(entityPrefab, Vector3.zero, Quaternion.identity, entitiesContainer).GetComponent<EntityComponent>();
			entity.name = entityPrefab.name;
			entity.Initialize(x, y, sprite, type);
			LevelData.Entities.Add(entity);
		    return entity;
		}

	    public EntityComponent InstantiateEntity(int x, int y, EntityType type)
	    {
	        Sprite entitySprite;
	        switch (type)
	        {
	            case EntityType.Enemy:
	                entitySprite = entitySprites[UnityEngine.Random.Range(0, 5)];
	                break;
	            case EntityType.Player:
	                entitySprite = entitySprites[UnityEngine.Random.Range(5, 10)];
                    break;
                default:
	                entitySprite = tileSprites[49];
	                break;
	        }

	        return InstantiateEntity(x, y, entitySprite, type);
        }

		public void ShowBreadCrumb(int x, int y, bool state, float delay = 0)
		{
			LevelData.Tiles[x, y].ShowBreadCrumb(state, delay);
		}

		public void HideAllBreadCrumbs()
		{
			for (int y = 0; y < LevelData.Height; y++)
			{
				for (int x = 0; x < LevelData.Width; x++)
				{
					LevelData.Tiles[x, y].ShowBreadCrumb(false);
				}
			}
		}

		public void PlayQuakeAnimation(int x, int y, int radius,float addDelay=0)
		{
			if (Time.realtimeSinceStartup < quakeAnimationCooldown)
			{
				return;
			}

			var calculatedTotalDurationOfAnimation = ((radius * 0.25f) * 0.5f) + 0.75f;
			quakeAnimationCooldown = Time.realtimeSinceStartup + calculatedTotalDurationOfAnimation;

			var center = new Vector2(x, y);
			var current = Vector2.zero;

			for (int y2 = y - radius; y2 <= y + radius; y2++)
			{
				for (int x2 = x - radius; x2 <= x + radius; x2++)
				{
					if (x2 < 0 || x2 >= LevelData.Width ||
						y2 < 0 || y2 >= LevelData.Height)
					{
						continue;
					}

					current.x = x2;
					current.y = y2;

					var distance = Vector2.Distance(current, center);

					if (distance <= radius)
					{
						var tile = LevelData.Tiles[x2, y2].transform;
						var originalY = tile.position.y;

						var delay = addDelay+(distance * 0.25f) * 0.5f;

						var sequence = DOTween.Sequence();
						sequence.PrependInterval(delay);
						sequence.Append(tile.DOLocalMoveY(originalY + 0.1f, 0.25f).SetEase(Ease.OutBack));
						sequence.Append(tile.DOLocalMoveY(originalY - 0.1f, 0.25f).SetEase(Ease.OutBack));
						sequence.Append(tile.DOLocalMoveY(originalY, 0.25f));

						for (int i = 0; i < LevelData.Entities.Count; i++)
						{
							var entity = LevelData.Entities[i];
							if (entity.gameObject.activeSelf&&
							    entity.GridPosition == current)
							{
								var sequence2 = DOTween.Sequence();
								sequence2.PrependInterval(delay);
								var entityTransform = entity.gameObject.transform;
								sequence2.Append(entityTransform.DOLocalMoveY(originalY + 0.1f, 0.25f).SetEase(Ease.OutBack));
								sequence2.Append(entityTransform.DOLocalMoveY(originalY - 0.1f, 0.25f).SetEase(Ease.OutBack));
								sequence2.Append(entityTransform.DOLocalMoveY(originalY, 0.25f));
							}
						}
					}
				}
			}

			audio.PlayQuake();
		}
	}
}