using System;

namespace LivePersonNet.Utilities
{
    public static class Args
    {
        /// Usage: 
        /// public Foo(string name) {
        ///     this.Name = Args.NotNull(name, nameof(name));
        /// }
        public static T NotNull<T>(T arg, string name)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(name);
            }

            return arg;
        }

        public static T NotNull<T>(T arg)
        {
            if (arg == null)
            {
                throw new ArgumentNullException();
            }

            return arg;
        }
    }
}
