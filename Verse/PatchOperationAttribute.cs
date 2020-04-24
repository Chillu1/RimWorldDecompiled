namespace Verse
{
	public abstract class PatchOperationAttribute : PatchOperationPathed
	{
		protected string attribute;

		public override string ToString()
		{
			return $"{base.ToString()}({attribute})";
		}
	}
}
