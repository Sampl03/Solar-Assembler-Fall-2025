namespace Solar.Asm.Engine.Model.Utils
{
    internal static class WeakRefListExtensions
    {
        internal static void CleanupDeadReferences<T>(this IList<WeakReference<T>> weakList) where T : class
        {
            for (int i = weakList.Count - 1; i >= 0; i--)
                if (!weakList[i].TryGetTarget(out T? target))
                    weakList.RemoveAt(i);
        }

        internal static bool RemoveTarget<T>(this IList<WeakReference<T>> weakList, T targetToRemove) where T : class
        {
            for (int i = weakList.Count - 1; i >= 0; i--)
                if (weakList[i].TryGetTarget(out T? target) && ReferenceEquals(target, targetToRemove))
                {
                    weakList.RemoveAt(i);
                    return true;
                }

            return false;
        }

        internal static IEnumerable<T> GetLiveTargets<T>(this IList<WeakReference<T>> weakList) where T : class
        {
            foreach (var weakRef in weakList)
                if (weakRef.TryGetTarget(out T? target))
                    yield return target;
        }
    }
}
