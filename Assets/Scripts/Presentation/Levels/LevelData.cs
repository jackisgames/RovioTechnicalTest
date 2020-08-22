using System.Collections.Generic;
using Assets.Scripts.Presentation.Entities;

namespace Assets.Scripts.Presentation.Levels
{
	public class LevelData
	{
		public int Width;
		public int Height;
		public LevelTileComponent[,] Tiles;
		public List<EntityComponent> Entities;
	}
}