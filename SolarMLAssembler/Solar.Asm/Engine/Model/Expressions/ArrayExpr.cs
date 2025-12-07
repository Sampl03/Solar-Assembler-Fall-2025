using Solar.Asm.Engine.Model.Exceptions;
using System.Collections.Immutable;

namespace Solar.Asm.Engine.Model.Expressions
{
    public sealed class ArrayExpr<TReturn> : Expression<ImmutableArray<TReturn>>
        where TReturn : IEquatable<TReturn>
    {
        public override bool IsConstantExpression
        {
            get
            {
                foreach (ArrayItem<TReturn> item in Items)
                {
                    if (!item.IsConstant())
                        return false;
                }
                return true;
            }
        }

        private readonly List<ArrayItem<TReturn>> Items = [];

        #region Add Methods
        /// <summary>
        /// Append a constant value to the array
        /// </summary>
        /// <param name="value"></param>
        public void AddConstant(in TReturn value)
        {
            GuardValidity();
            Items.Add(new ArrayItem<TReturn>(value));
        }

        /// <summary>
        /// Appends a collection of constant values to the current list of items.
        /// </summary>
        /// <param name="values"></param>
        public void AppendConstants(in IEnumerable<TReturn> values)
        {
            GuardValidity();
            Items.Add(new ArrayItem<TReturn>(values));
        }

        /// <summary>
        /// Add a single expression to the array
        /// </summary>
        /// <param name="expr"></param>
        public void AddSingleExpression(Expression<TReturn> expr)
        {
            GuardValidity();
            expr.GuardValidity();

            // If the expression is constant, we can just add its value directly
            if (expr.IsConstantExpression)
            {
                ExpressionResult<TReturn> result = expr.Evaluate();

                if (!result.HasValue)
                    throw new ExpressionHasNoValueException("Constant expression must return a value", expr);

                Items.Add(new ArrayItem<TReturn>(result.Value!));
                return;
            }

            Items.Add(new ArrayItem<TReturn>(expr));
        }

        /// <summary>
        /// Add a sub-array to the array
        /// </summary>
        /// <param name="subarray"></param>
        public void AddSubArray(ArrayExpr<TReturn> subarray)
        {
            GuardValidity();
            subarray.GuardValidity();

            // If the subarray is constant, we can just add its values directly
            if (subarray.IsConstantExpression)
            {
                ExpressionResult<ImmutableArray<TReturn>> result = subarray.Evaluate();

                if (!result.HasValue)
                    throw new ExpressionHasNoValueException("Constant sub-array expression must return a value", subarray);

                Items.Add(new ArrayItem<TReturn>(result.Value!));
                return;
            }

            Items.Add(new ArrayItem<TReturn>(subarray));
        }
        #endregion

        public override ExpressionResult<ImmutableArray<TReturn>> Evaluate()
        {
            GuardValidity();

            IEnumerable<TReturn> resultItems = [];

            foreach (ArrayItem<TReturn> item in Items)
            {
                var itemResults = item.GetValues(this);

                if (!itemResults.HasValue)
                    return new() { HasValue = false };

                resultItems = resultItems.Concat(itemResults.Value!);
            }
            
            return new() { HasValue = true, Value = [.. resultItems] };
        }

        public override void Simplify()
        {
            GuardValidity();

            // Simplify all items and replace with constant values where possible
            for (int i = 0; i < Items.Count; i++)
            {
                ArrayItem<TReturn> item = Items[i];
                switch (item.Kind)
                {
                    // Constant values are already simplified
                    case ArrayItemKind.ConstantSingle:
                    case ArrayItemKind.ConstantSubArray:
                        break;

                    // Single-value expressions can be simplified down to ConstantSingle
                    case ArrayItemKind.SingleExpression:
                        item.Expression!.Ref!.Simplify();

                        if (item.Expression!.Ref!.IsConstantExpression)
                        {
                            ExpressionResult<TReturn> evalResult = item.Expression!.Ref!.Evaluate();

                            if (!evalResult.HasValue)
                                throw new ExpressionHasNoValueException("Could not simplify array expression because constant sub-expression has no value.", item.Expression!.Ref!);

                            // Replace with constant value
                            item.Cleanup();
                            Items[i] = new ArrayItem<TReturn>(evalResult.Value!);
                        }

                        break;

                    // Sub-array expressions can also be simplified down to ConstantSubArray
                    case ArrayItemKind.SubArray:
                        item.SubArray!.Ref!.Simplify();

                        if (item.SubArray!.Ref!.IsConstantExpression)
                        {
                            ExpressionResult<ImmutableArray<TReturn>> subarrayResult = item.SubArray!.Ref!.Evaluate();

                            if (!subarrayResult.HasValue)
                                throw new ExpressionHasNoValueException("Could not simplify array expression because constant subarray-expression has no value.", item.SubArray!.Ref!);

                            // Replace with constant value
                            item.Cleanup();
                            Items[i] = new ArrayItem<TReturn>(subarrayResult.Value!);
                        }

                        break;
                    default:
                        throw new ExpressionException("Invalid ArrayExpr item encountered during simplification.", this);
                }
            }

            // Collapse adjacent constant items
            List<TReturn> collapsedItems = [];
            bool inConstantRun = false;
            int runStartIndex = 0;
            for (int i = 0; i < Items.Count - 1; i++)
            {
                // If the item is constant, add its values to the collapsed list
                if (Items[i].IsConstant())
                {
                    // If we're starting a new run, note the start index
                    if (!inConstantRun)
                    {
                        runStartIndex = i;
                        inConstantRun = true;
                    }

                    // Add this item's values to the collapsed list
                    collapsedItems.AddRange(Items[i].GetValues(this).Value!);

                    // Cleanup the items since it will be removed
                    Items[i].Cleanup();

                    continue;
                }
                // If the item isn't constant, flush the collapsed items if we were in a run
                else
                {
                    if (!inConstantRun)
                        continue;

                    // We were in a constant run, so save the collapsed items and reset
                    Items.RemoveRange(runStartIndex, i - runStartIndex);
                    Items.Insert(runStartIndex, new ArrayItem<TReturn>(collapsedItems));
                    collapsedItems.Clear();
                    inConstantRun = false;
                    i -= i - runStartIndex + 1; // Adjust index to account for removed items
                }
            }

            // Catch any remaining constant items at the end
            if (collapsedItems.Count != 0)
            {
                Items.RemoveRange(runStartIndex, Items.Count - runStartIndex);
                Items.Add(new ArrayItem<TReturn>(collapsedItems));
            }
        }

        protected override void OnInvalidated()
        {
            foreach (ArrayItem<TReturn> item in Items)
                item.Cleanup();
            Items.Clear();
        }
    }
}
