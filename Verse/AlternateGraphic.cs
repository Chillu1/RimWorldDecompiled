using UnityEngine;

namespace Verse
{
	public class AlternateGraphic
	{
		private float weight = 0.5f;

		private string texPath;

		private Color? color;

		private Color? colorTwo;

		private GraphicData graphicData;

		public float Weight => weight;

		public Graphic GetGraphic(Graphic other)
		{
			if (graphicData == null)
			{
				graphicData = new GraphicData();
			}
			graphicData.CopyFrom(other.data);
			if (!texPath.NullOrEmpty())
			{
				graphicData.texPath = texPath;
			}
			graphicData.color = color ?? other.color;
			graphicData.colorTwo = colorTwo ?? other.colorTwo;
			return graphicData.Graphic;
		}
	}
}
