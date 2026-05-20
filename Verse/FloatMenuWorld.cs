using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;

namespace Verse;

public class FloatMenuWorld : FloatMenu
{
	private Vector2 clickPos;

	private const int RevalidateEveryFrame = 4;

	public FloatMenuWorld(List<FloatMenuOption> options, string title, Vector2 clickPos)
		: base(options, title)
	{
		this.clickPos = clickPos;
	}

	public override void DoWindowContents(Rect inRect)
	{
		if (!(Find.WorldSelector.SingleSelectedObject is Caravan caravan))
		{
			Find.WindowStack.TryRemove(this);
			return;
		}
		if (Time.frameCount % 4 == 0)
		{
			List<FloatMenuOption> list = FloatMenuMakerWorld.ChoicesAtFor(clickPos, caravan);
			List<FloatMenuOption> cachedChoices = list;
			Vector2 cachedChoicesForPos = clickPos;
			for (int i = 0; i < options.Count; i++)
			{
				if (!options[i].Disabled && !StillValid(options[i], list, caravan, ref cachedChoices, ref cachedChoicesForPos))
				{
					options[i].Disabled = true;
				}
			}
		}
		base.DoWindowContents(inRect);
	}

	private static bool StillValid(FloatMenuOption opt, List<FloatMenuOption> curOpts, Caravan forCaravan)
	{
		List<FloatMenuOption> cachedChoices = null;
		Vector2 cachedChoicesForPos = new Vector2(-9999f, -9999f);
		return StillValid(opt, curOpts, forCaravan, ref cachedChoices, ref cachedChoicesForPos);
	}

	private static bool StillValid(FloatMenuOption opt, List<FloatMenuOption> curOpts, Caravan forCaravan, ref List<FloatMenuOption> cachedChoices, ref Vector2 cachedChoicesForPos)
	{
		if (opt.revalidateWorldClickTarget == null)
		{
			for (int i = 0; i < curOpts.Count; i++)
			{
				if (OptionsMatch(opt, curOpts[i]))
				{
					return true;
				}
			}
		}
		else
		{
			if (!opt.revalidateWorldClickTarget.Spawned)
			{
				return false;
			}
			Vector2 vector = opt.revalidateWorldClickTarget.ScreenPos();
			vector.y = (float)UI.screenHeight - vector.y;
			List<FloatMenuOption> list;
			if (vector == cachedChoicesForPos)
			{
				list = cachedChoices;
			}
			else
			{
				cachedChoices = FloatMenuMakerWorld.ChoicesAtFor(vector, forCaravan);
				cachedChoicesForPos = vector;
				list = cachedChoices;
			}
			for (int j = 0; j < list.Count; j++)
			{
				if (OptionsMatch(opt, list[j]))
				{
					return !list[j].Disabled;
				}
			}
		}
		return false;
	}

	public override void PreOptionChosen(FloatMenuOption opt)
	{
		base.PreOptionChosen(opt);
		Caravan caravan = Find.WorldSelector.SingleSelectedObject as Caravan;
		if (!opt.Disabled && (caravan == null || !StillValid(opt, FloatMenuMakerWorld.ChoicesAtFor(clickPos, caravan), caravan)))
		{
			opt.Disabled = true;
		}
	}

	private static bool OptionsMatch(FloatMenuOption a, FloatMenuOption b)
	{
		if (a.Label == b.Label)
		{
			return true;
		}
		return false;
	}
}
