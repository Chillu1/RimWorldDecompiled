using System;
using System.Collections.Generic;

namespace Verse
{
	public class LoadFolder : IEquatable<LoadFolder>
	{
		public string folderName;

		public List<string> requiredPackageIds;

		public List<string> disallowedPackageIds;

		private readonly int hashCodeCached;

		public bool ShouldLoad
		{
			get
			{
				if (requiredPackageIds.NullOrEmpty() || ModLister.AnyFromListActive(requiredPackageIds))
				{
					if (!disallowedPackageIds.NullOrEmpty())
					{
						return !ModLister.AnyFromListActive(disallowedPackageIds);
					}
					return true;
				}
				return false;
			}
		}

		public LoadFolder(string folderName, List<string> requiredPackageIds, List<string> disallowedPackageIds)
		{
			this.folderName = folderName;
			this.requiredPackageIds = requiredPackageIds;
			this.disallowedPackageIds = disallowedPackageIds;
			hashCodeCached = (folderName?.GetHashCode() ?? 0);
			hashCodeCached = Gen.HashCombine(hashCodeCached, requiredPackageIds?.GetHashCode() ?? 0);
			hashCodeCached = Gen.HashCombine(hashCodeCached, disallowedPackageIds?.GetHashCode() ?? 0);
		}

		public bool Equals(LoadFolder other)
		{
			if (other != null)
			{
				return hashCodeCached == other.GetHashCode();
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			LoadFolder other;
			if ((other = (obj as LoadFolder)) != null)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return hashCodeCached;
		}
	}
}
