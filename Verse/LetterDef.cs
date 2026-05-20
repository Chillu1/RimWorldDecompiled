using System;
using RimWorld;
using UnityEngine;

namespace Verse;

public class LetterDef : Def
{
	public Type letterClass = typeof(StandardLetter);

	public Color color = Color.white;

	public Color flashColor = Color.white;

	public float flashInterval = 90f;

	public bool bounce;

	public SoundDef arriveSound;

	[NoTranslate]
	public string icon = "UI/Letters/LetterUnopened";

	public AutomaticPauseMode pauseMode = AutomaticPauseMode.AnyLetter;

	public bool forcedSlowdown;

	[Unsaved(false)]
	private Texture2D iconTex;

	public Texture2D Icon
	{
		get
		{
			if (iconTex == null && !icon.NullOrEmpty())
			{
				iconTex = ContentFinder<Texture2D>.Get(icon);
			}
			return iconTex;
		}
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		if (arriveSound == null)
		{
			arriveSound = SoundDefOf.LetterArrive;
		}
	}
}
