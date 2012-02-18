namespace Ormongo.Plugins.Ancestry
{
	public class AncestryPlugin : PluginBase
	{
		public override void BeforeSave(object document)
		{
			var hasAncestryDocument = document as IHasAncestry;
			if (hasAncestryDocument != null)
				hasAncestryDocument.AncestryProxy.UpdateDescendantsWithNewAncestry();
			base.BeforeSave(document);
		}

		public override void AfterSave(object document)
		{
			var hasAncestryDocument = document as IHasAncestry;
			if (hasAncestryDocument != null)
				hasAncestryDocument.AncestryProxy.ResetChangedFields();
			base.AfterSave(document);
		}

		public override void BeforeDestroy(object document)
		{
			var hasAncestryDocument = document as IHasAncestry;
			if (hasAncestryDocument != null)
				ApplyOrphanStrategy(document);
			base.BeforeDestroy(document);
		}

		private static void ApplyOrphanStrategy(object document)
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
	}
}