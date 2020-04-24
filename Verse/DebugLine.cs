using UnityEngine;

namespace Verse
{
	internal struct DebugLine
	{
		public Vector3 a;

		public Vector3 b;

		private int deathTick;

		private SimpleColor color;

		public bool Done => deathTick <= Find.TickManager.TicksGame;

		public DebugLine(Vector3 a, Vector3 b, int ticksLeft = 100, SimpleColor color = SimpleColor.White)
		{
			this.a = a;
			this.b = b;
			deathTick = Find.TickManager.TicksGame + ticksLeft;
			this.color = color;
		}

		public void Draw()
		{
			GenDraw.DrawLineBetween(a, b, color);
		}
	}
}
