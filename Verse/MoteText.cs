using UnityEngine;

namespace Verse;

public class MoteText : MoteThrown
{
	public string text;

	public Color textColor = Color.white;

	public float overrideTimeBeforeStartFadeout = -1f;

	protected float TimeBeforeStartFadeout
	{
		get
		{
			if (!(overrideTimeBeforeStartFadeout >= 0f))
			{
				return base.SolidTime;
			}
			return overrideTimeBeforeStartFadeout;
		}
	}

	protected override bool EndOfLife => base.AgeSecs >= TimeBeforeStartFadeout + def.mote.fadeOutTime;

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
	}

	public override void DrawGUIOverlay()
	{
		float a = 1f - (base.AgeSecs - TimeBeforeStartFadeout) / def.mote.fadeOutTime;
		GenMapUI.DrawText(textColor: new Color(textColor.r, textColor.g, textColor.b, a), worldPos: new Vector2(exactPosition.x, exactPosition.z), text: text);
	}
}
