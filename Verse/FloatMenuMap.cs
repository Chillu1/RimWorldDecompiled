using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class FloatMenuMap : FloatMenu
{
	private Vector3 clickPos;

	private static Dictionary<Vector3, List<FloatMenuOption>> cachedChoices = new Dictionary<Vector3, List<FloatMenuOption>>();

	private List<FloatMenuOption> lastOptionsForRevalidation;

	private int nextOptionToRevalidate;

	public const int RevalidateEveryFrame = 4;

	public FloatMenuMap(List<FloatMenuOption> options, string title, Vector3 clickPos)
		: base(options, title)
	{
		this.clickPos = clickPos;
	}

	public override void DoWindowContents(Rect inRect)
	{
		if (!Find.Selector.AnyPawnSelected)
		{
			Find.WindowStack.TryRemove(this);
			return;
		}
		bool flag = options.Count >= 3;
		if (Time.frameCount % 4 == 0 || lastOptionsForRevalidation == null)
		{
			lastOptionsForRevalidation = FloatMenuMakerMap.GetOptions(Find.Selector.SelectedPawns, clickPos, out var _);
			cachedChoices.Clear();
			cachedChoices.Add(clickPos, lastOptionsForRevalidation);
			if (!flag)
			{
				for (int i = 0; i < options.Count; i++)
				{
					RevalidateOption(options[i]);
				}
			}
		}
		else if (flag)
		{
			if (nextOptionToRevalidate >= options.Count)
			{
				nextOptionToRevalidate = 0;
			}
			int num = Mathf.CeilToInt((float)options.Count / 3f);
			int num2 = nextOptionToRevalidate;
			int num3 = 0;
			while (num2 < options.Count && num3 < num)
			{
				RevalidateOption(options[num2]);
				nextOptionToRevalidate++;
				num2++;
				num3++;
			}
		}
		base.DoWindowContents(inRect);
		void RevalidateOption(FloatMenuOption option)
		{
			if (!option.Disabled && !StillValid(option, lastOptionsForRevalidation))
			{
				option.Disabled = true;
			}
		}
	}

	private static bool StillValid(FloatMenuOption opt, List<FloatMenuOption> curOpts)
	{
		if (opt.revalidateClickTarget == null)
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
			if (!opt.targetsDespawned && !opt.revalidateClickTarget.Spawned)
			{
				return false;
			}
			Vector3 key = opt.revalidateClickTarget.PositionHeld.ToVector3Shifted();
			if (!cachedChoices.TryGetValue(key, out var value))
			{
				FloatMenuContext context;
				List<FloatMenuOption> list = FloatMenuMakerMap.GetOptions(Find.Selector.SelectedPawns, key, out context);
				cachedChoices.Add(key, list);
				value = list;
			}
			for (int j = 0; j < value.Count; j++)
			{
				if (OptionsMatch(opt, value[j]))
				{
					return !value[j].Disabled;
				}
			}
		}
		return false;
	}

	public override void PreOptionChosen(FloatMenuOption opt)
	{
		base.PreOptionChosen(opt);
		if (!opt.Disabled && !StillValid(opt, FloatMenuMakerMap.GetOptions(Find.Selector.SelectedPawns, clickPos, out var _)))
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
