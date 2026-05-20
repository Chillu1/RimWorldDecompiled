using System;
using UnityEngine;

namespace Verse;

public static class SimpleColorExtension
{
	public static Color ToUnityColor(this SimpleColor color)
	{
		return color switch
		{
			SimpleColor.White => Color.white, 
			SimpleColor.Red => Color.red, 
			SimpleColor.Green => Color.green, 
			SimpleColor.Blue => Color.blue, 
			SimpleColor.Magenta => Color.magenta, 
			SimpleColor.Yellow => Color.yellow, 
			SimpleColor.Cyan => Color.cyan, 
			SimpleColor.Orange => ColorLibrary.Orange, 
			_ => throw new ArgumentException(), 
		};
	}
}
