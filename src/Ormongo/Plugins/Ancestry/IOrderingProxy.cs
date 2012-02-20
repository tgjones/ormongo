namespace Ormongo.Plugins.Ancestry
{
	internal interface IOrderingProxy
	{
		bool SiblingRepositionRequired { get; }

		void AssignDefaultPosition();
		void RepositionFormerSiblings();
		void MoveLowerSiblingsUp();
	}
}