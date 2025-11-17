using Solar.EntitySystem.Behavior;

namespace Solar.EntitySystem.Exceptions
{
    public class CannotMergeException : EntitySystemException
    {
        IMergeable? MergeOrigin;
        IMergeable? MergeDestination;

        public CannotMergeException()
        {
        }

        public CannotMergeException(string message) : base(message)
        {
        }

        public CannotMergeException(string? message, Exception inner) : base(message, inner)
        {
        }

        public CannotMergeException(string message, IMergeable origin, IMergeable destination) : this(message)
        {
            MergeOrigin = origin;
            MergeDestination = destination;
        }

        public CannotMergeException(string? message, Exception inner, IMergeable origin, IMergeable destination) : this(message, inner)
        {
            MergeOrigin = origin;
            MergeDestination = destination;
        }
    }
}
