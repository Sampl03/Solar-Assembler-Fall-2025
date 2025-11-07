using Solar.EntitySystem.Behavior;

namespace Solar.EntitySystem.Exceptions
{
    public class CannotMergeException : EntitySystemException
    {
        IMergeable? MergeDestination;
        IMergeable? MergeOther;

        public CannotMergeException()
        {
        }

        public CannotMergeException(string message) : base(message)
        {
        }

        public CannotMergeException(string? message, Exception inner) : base(message, inner)
        {
        }

        public CannotMergeException(string message, IMergeable destination, IMergeable other) : this(message)
        {
            MergeDestination = destination;
            MergeOther = other;
        }

        public CannotMergeException(string? message, Exception inner, IMergeable destination, IMergeable other) : this(message, inner)
        {
            MergeDestination = destination;
            MergeOther = other;
        }
    }
}
