using System.Collections.Generic;
using UnityEngine;

namespace Verse.Sound
{
	public class AudioGrain_Folder : AudioGrain
	{
		[LoadAlias("clipPath")]
		[NoTranslate]
		public string clipFolderPath = "";

		public override IEnumerable<ResolvedGrain> GetResolvedGrains()
		{
			foreach (AudioClip item in ContentFinder<AudioClip>.GetAllInFolder(clipFolderPath))
			{
				yield return new ResolvedGrain_Clip(item);
			}
		}
	}
}
