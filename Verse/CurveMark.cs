using UnityEngine;

namespace Verse
{
	public struct CurveMark
	{
		private float x;

		private string message;

		private Color color;

		public float X => x;

		public string Message => message;

		public Color Color => color;

		public CurveMark(float x, string message, Color color)
		{
			this.x = x;
			this.message = message;
			this.color = color;
		}
	}
}
