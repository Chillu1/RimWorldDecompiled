using System;
using UnityEngine;

namespace Verse
{
	public static class SimpleColorExtension
	{
		public static Color ToUnityColor(this SimpleColor color)
		{
			switch (color)
			{
			case SimpleColor.White:
				return Color.white;
			case SimpleColor.Red:
				return Color.red;
			case SimpleColor.Green:
				return Color.green;
			case SimpleColor.Blue:
				return Color.blue;
			case SimpleColor.Magenta:
				return Color.magenta;
			case SimpleColor.Yellow:
				return Color.yellow;
			case SimpleColor.Cyan:
				return Color.cyan;
			default:
				throw new ArgumentException();
			}
		}
	}
}
