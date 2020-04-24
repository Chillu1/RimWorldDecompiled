using Verse;

namespace RimWorld
{
	public static class PlantPosIndices
	{
		private static int[][][] rootList;

		private const int ListCount = 8;

		static PlantPosIndices()
		{
			rootList = new int[25][][];
			for (int i = 0; i < 25; i++)
			{
				rootList[i] = new int[8][];
				for (int j = 0; j < 8; j++)
				{
					int[] array = new int[i + 1];
					for (int k = 0; k < i; k++)
					{
						array[k] = k;
					}
					array.Shuffle();
					rootList[i][j] = array;
				}
			}
		}

		public static int[] GetPositionIndices(Plant p)
		{
			int maxMeshCount = p.def.plant.maxMeshCount;
			int num = (p.thingIDNumber ^ 0x2862FF0) % 8;
			return rootList[maxMeshCount - 1][num];
		}
	}
}
