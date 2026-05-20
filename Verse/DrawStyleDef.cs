using System;
using UnityEngine;

namespace Verse;

public class DrawStyleDef : Def
{
	[NoTranslate]
	private string uiIconPath;

	public bool drawOutline;

	public bool drawArea = true;

	public bool drawShortSideMeasurement = true;

	public bool canSnap = true;

	public Type drawStyleType;

	public Texture2D uiIcon = BaseContent.BadTex;

	public DrawStyle DrawStyleWorker => GenWorker<DrawStyle>.Get(drawStyleType);

	public override void PostLoad()
	{
		base.PostLoad();
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			if (!uiIconPath.NullOrEmpty())
			{
				uiIcon = ContentFinder<Texture2D>.Get(uiIconPath);
			}
		});
	}
}
