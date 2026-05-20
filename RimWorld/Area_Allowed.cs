using UnityEngine;
using Verse;

namespace RimWorld;

public class Area_Allowed : Area
{
	private string labelInt;

	private Color colorInt = Color.red;

	public override string Label => labelInt;

	public override Color Color => colorInt;

	public override bool Mutable => true;

	public override int ListPriority => 500;

	public override string RenamableLabel
	{
		get
		{
			return labelInt ?? BaseLabel;
		}
		set
		{
			labelInt = value;
		}
	}

	public override string BaseLabel => "Area";

	public Area_Allowed()
	{
	}

	public Area_Allowed(AreaManager areaManager, string label = null)
		: base(areaManager)
	{
		base.areaManager = areaManager;
		if (!label.NullOrEmpty())
		{
			labelInt = label;
		}
		else
		{
			int num = 1;
			while (true)
			{
				labelInt = "AreaDefaultLabel".Translate(num);
				if (areaManager.GetLabeled(labelInt) == null)
				{
					break;
				}
				num++;
			}
		}
		colorInt = new Color(Rand.Value, Rand.Value, Rand.Value);
		colorInt = Color.Lerp(colorInt, Color.gray, 0.5f);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref labelInt, "label");
		Scribe_Values.Look(ref colorInt, "color");
	}

	public override bool AssignableAsAllowed()
	{
		return true;
	}

	public void SetColor(Color newColor)
	{
		colorInt = newColor;
		InvalidateColorTexture();
	}

	public void SetLabel(string newLabel)
	{
		labelInt = newLabel;
	}

	public override string GetUniqueLoadID()
	{
		return "Area_" + ID + "_Named_" + labelInt;
	}

	public override string ToString()
	{
		return labelInt;
	}
}
