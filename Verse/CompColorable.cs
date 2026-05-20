using UnityEngine;

namespace Verse;

public class CompColorable : ThingComp
{
	private Color? desiredColor;

	private Color color = Color.white;

	private bool active;

	public Color? DesiredColor
	{
		get
		{
			return desiredColor;
		}
		set
		{
			desiredColor = value;
		}
	}

	public Color Color
	{
		get
		{
			if (!active)
			{
				return parent.def.graphicData.color;
			}
			return color;
		}
	}

	public bool Active => active;

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		if (parent.def.colorGenerator != null && (parent.Stuff == null || parent.Stuff.stuffProps.allowColorGenerators))
		{
			SetColor(parent.def.colorGenerator.NewRandomizedColor());
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref desiredColor, "desiredColor");
		if (Scribe.mode != LoadSaveMode.Saving || active)
		{
			Scribe_Values.Look(ref color, "color");
			Scribe_Values.Look(ref active, "colorActive", defaultValue: false);
		}
	}

	public override void PostSplitOff(Thing piece)
	{
		base.PostSplitOff(piece);
		if (active)
		{
			piece.SetColor(color);
		}
	}

	public void Recolor()
	{
		if (!desiredColor.HasValue)
		{
			Log.Error($"Tried recoloring apparel {parent} which does not have a desired color set!");
		}
		else
		{
			SetColor(DesiredColor.Value);
		}
	}

	public void Disable()
	{
		active = false;
		color = Color.white;
		desiredColor = null;
		parent.Notify_ColorChanged();
	}

	public void SetColor(Color value)
	{
		if (!(value == color))
		{
			active = true;
			color = value;
			desiredColor = null;
			parent.Notify_ColorChanged();
		}
	}
}
