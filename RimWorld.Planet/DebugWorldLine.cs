using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class DebugWorldLine
	{
		public Vector3 a;

		public Vector3 b;

		public int ticksLeft;

		private bool onPlanetSurface;

		public int TicksLeft
		{
			get
			{
				return ticksLeft;
			}
			set
			{
				ticksLeft = value;
			}
		}

		public DebugWorldLine(Vector3 a, Vector3 b, bool onPlanetSurface)
		{
			this.a = a;
			this.b = b;
			this.onPlanetSurface = onPlanetSurface;
			ticksLeft = 100;
		}

		public DebugWorldLine(Vector3 a, Vector3 b, bool onPlanetSurface, int ticksLeft)
		{
			this.a = a;
			this.b = b;
			this.onPlanetSurface = onPlanetSurface;
			this.ticksLeft = ticksLeft;
		}

		public void Draw()
		{
			float num = Vector3.Distance(a, b);
			if (num < 0.001f)
			{
				return;
			}
			if (onPlanetSurface)
			{
				float averageTileSize = Find.WorldGrid.averageTileSize;
				int num2 = Mathf.Max(Mathf.RoundToInt(num / averageTileSize), 0);
				float num3 = 0.05f;
				for (int i = 0; i < num2; i++)
				{
					Vector3 vector = Vector3.Lerp(a, b, (float)i / (float)num2);
					Vector3 vector2 = Vector3.Lerp(a, b, (float)(i + 1) / (float)num2);
					vector = vector.normalized * (100f + num3);
					vector2 = vector2.normalized * (100f + num3);
					GenDraw.DrawWorldLineBetween(vector, vector2);
				}
			}
			else
			{
				GenDraw.DrawWorldLineBetween(a, b);
			}
		}
	}
}
