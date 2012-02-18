using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Builders;
using Ormongo.Internal;

namespace Ormongo.Plugins.Ancestry
{
	public class AncestryProxy<T> : IAncestryProxy
		where T : Document<T>
	{
		private const string AncestryKey = "Ancestry";
		private const string AncestryWasKey = "AncestryWas";
		private const string AncestryChangedKey = "AncestryChanged";
		private const string DisableAncestryCallbacksKey = "DisableAncestryCallbacks";

		private readonly T _instance;

		private bool AncestryChanged
		{
			get { return _instance.TransientData.SafeGet<bool>(AncestryChangedKey); }
			set { _instance.TransientData.SafeSet(AncestryChangedKey, value); }
		}

		private string AncestryWas
		{
			get { return _instance.TransientData.SafeGet<string>(AncestryWasKey); }
			set { _instance.TransientData.SafeSet(AncestryWasKey, value); }
		}

		#region Static

		private static AncestryProxy<T> GetAncestryProxy(T instance)
		{
			return ((IHasAncestry<T>) instance).Ancestry;
		}

		//public static IQueryable<T> Roots
		//{
		//    get { return Document<T>.Find(i => i.AncestryData == null); }
		//}
		
		// scope :ancestors_of, lambda { |object| where(to_node(object).ancestor_conditions) }

		//public static IQueryable<T> AncestorsOf(T item)
		//{
		//    return Document<T>.Find(i => item.Ancestry.AncestorIDs.Contains(i.ID));
		//}

		#endregion

		#region Constructor

		public AncestryProxy(T instance)
		{
			_instance = instance;
			AncestryWas = Ancestry;
		}

		#endregion

		#region Instance

		public string Ancestry
		{
			get { return _instance.ExtraData.SafeGet<string>(AncestryKey); }
			set { _instance.ExtraData.SafeSet(AncestryKey, value); }
		}

		void IAncestryProxy.ResetChangedFields()
		{
			AncestryWas = Ancestry;
		}

		private void UpdateAncestryData(string newValue)
		{
			Ancestry = newValue;
			AncestryChanged = true;
		}

		/// <summary>
		/// The ancestry value for this record's children
		/// </summary>
		internal string ChildAncestry
		{
			get
			{
				// New records cannot have children
				if (_instance.IsNewRecord)
					throw new InvalidOperationException("No child ancestry for new record. Save record before performing tree operations.");

				return (String.IsNullOrEmpty(AncestryWas))
					? _instance.ID.ToString()
					: String.Format("{0}/{1}", AncestryWas, _instance.ID);
			}
		}

		#region Ancestors

		public IEnumerable<ObjectId> AncestorIDs
		{
			get { return (String.IsNullOrEmpty(Ancestry)) ? new List<ObjectId>() : Ancestry.Split('/').Select(ObjectId.Parse); }
		}

		public IQueryable<T> Ancestors
		{
			get { return Document<T>.Find(d => AncestorIDs.Contains(d.ID)); }
		}

		public IEnumerable<ObjectId> PathIDs
		{
			get { return AncestorIDs.Union(new[] { _instance.ID }); }
		}

		public IQueryable<T> Path
		{
			get { return Document<T>.Find(d => PathIDs.Contains(d.ID)); }
		}

		public int Depth
		{
			get { return AncestorIDs.Count(); }
		}

		#endregion

		#region Parent

		[BsonIgnore]
		public T Parent
		{
			get { return (ParentID == ObjectId.Empty) ? null : Document<T>.FindOneByID(ParentID); }
			set { UpdateAncestryData((value == null) ? null : GetAncestryProxy(value).ChildAncestry); }
		}

		[BsonIgnore]
		public ObjectId ParentID
		{
			get { return AncestorIDs.Any() ? AncestorIDs.Last() : ObjectId.Empty; }
			set { Parent = (value == ObjectId.Empty) ? null : Document<T>.FindOneByID(value); }
		}

		#endregion

		#region Root

		public ObjectId RootID
		{
			get { return AncestorIDs.Any() ? AncestorIDs.First() : _instance.ID; }
		}

		public T Root
		{
			get { return (RootID == _instance.ID) ? _instance : Document<T>.FindOneByID(RootID); }
		}

		public bool IsRoot
		{
			get { return String.IsNullOrEmpty(Ancestry); }
		}

		#endregion

		#region Children

		public IQueryable<T> Children
		{
			get { return Document<T>.Find(d => d.ExtraData[AncestryKey] == ChildAncestry); }
		}

		public IEnumerable<ObjectId> ChildIDs
		{
			get { return Children.Select(d => d.ID); }
		}

		public bool HasChildren
		{
			get { return Children.Any(); }
		}

		public bool IsChildless
		{
			get { return !HasChildren; }
		}

		#endregion

		#region Siblings

		public IQueryable<T> Siblings
		{
			get { return Document<T>.Find(d => d.ExtraData[AncestryKey] == Ancestry); }
		}

		public IEnumerable<ObjectId> SiblingIDs
		{
			get { return Siblings.Select(d => d.ID); }
		}

		public bool HasSiblings
		{
			get { return Siblings.Count() > 1; }
		}

		public bool IsOnlyChild
		{
			get { return !HasSiblings; }
		}

		#endregion

		#region Descendants

		private static string GetExtraDataPropertyName()
		{
			return ExpressionUtility.GetPropertyName<T, DataDictionary>(d => d.ExtraData);
		}

		public IQueryable<T> Descendants
		{
			get
			{
				return Document<T>.FindNative(Query.Or(
					Query.Matches(GetExtraDataPropertyName() + "." + AncestryKey, "^" + ChildAncestry + "/"),
					Query.EQ(GetExtraDataPropertyName() + "." + AncestryKey, ChildAncestry)));
			}
		}

		public IEnumerable<ObjectId> DescendantIDs
		{
			get { return Descendants.Select(d => d.ID); }
		}

		#endregion

		#region Subtree

		public IQueryable<T> Subtree
		{
			get
			{
				return Document<T>.FindNative(Query.Or(
					Query.EQ("_id", _instance.ID),
					Query.Matches(GetExtraDataPropertyName() + "." + AncestryKey, "^" + ChildAncestry + "/"),
					Query.EQ(GetExtraDataPropertyName() + "." + AncestryKey, ChildAncestry)));
			}
		}

		public IEnumerable<ObjectId> SubtreeIDs
		{
			get { return Subtree.Select(d => d.ID); }
		}

		#endregion

		#region Callbacks

		private void WithoutAncestryCallbacks(Action callback)
		{
			_instance.TransientData.SafeSet(DisableAncestryCallbacksKey, true);
			callback();
			_instance.TransientData.SafeSet(DisableAncestryCallbacksKey, false);
		}

		void IAncestryProxy.UpdateDescendantsWithNewAncestry()
		{
			// Skip this if callbacks are disabled.
			if (_instance.TransientData.SafeGet<bool>(DisableAncestryCallbacksKey))
				return;

			// Skip this if it's a new record or ancestry wasn't updated.
			if (_instance.IsNewRecord || !_instance.TransientData.SafeGet<bool>(AncestryChangedKey))
				return;

			// For each descendant...
			foreach (var descendant in Descendants)
			{
				// Replace old ancestry with new ancestry.
				GetAncestryProxy(descendant).WithoutAncestryCallbacks(() =>
				{
					string forReplace = (String.IsNullOrEmpty(Ancestry))
						? _instance.ID.ToString()
						: String.Format("{0}/{1}", Ancestry, _instance.ID);
					string newAncestry = Regex.Replace((string)descendant.ExtraData[AncestryKey], "^" + ChildAncestry, forReplace);
					descendant.ExtraData[AncestryKey] = newAncestry;
					descendant.Save();
				});
			}
		}

		void IAncestryProxy.ApplyOrphanStrategy()
		{

		}

		/*
		 * /*
		 *  # Apply orphan strategy
      def apply_orphan_strategy
        # Skip this if callbacks are disabled
        unless ancestry_callbacks_disabled?
          # If this isn't a new record ...
          unless new_record?
            # ... make al children root if orphan strategy is rootify
            if self.base_class.orphan_strategy == :rootify
              descendants.each do |descendant|
                descendant.without_ancestry_callbacks do
                  val = \
                    unless descendant.ancestry == child_ancestry
                      descendant.read_attribute(descendant.class.ancestry_field).gsub(/^#{child_ancestry}\//, '')
                    end
                  descendant.update_attribute descendant.class.ancestry_field, val
                end
              end
              # ... destroy all descendants if orphan strategy is destroy
            elsif self.base_class.orphan_strategy == :destroy
              descendants.all.each do |descendant|
                descendant.without_ancestry_callbacks { descendant.destroy }
              end
              # ... throw an exception if it has children and orphan strategy is restrict
            elsif self.base_class.orphan_strategy == :restrict
              raise Error.new('Cannot delete record because it has descendants.') unless is_childless?
            end
          end
        end
      end
		 * */

		#endregion

		#endregion
	}
}