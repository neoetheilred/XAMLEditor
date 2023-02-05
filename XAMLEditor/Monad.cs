using System;

namespace XAMLEditor
{
    internal class Monad<T>
    {
        public T Value { get; set; }
        public Monad(T value) => Value = value;
        public static Monad<T> Of(T value) => new Monad<T>(value != null ? value : throw new ArgumentNullException());
        public Monad<TV> Bind<TV>(Func<T, TV> func) => Monad<TV>.Of(func(Value));

        public Monad<T> Pipe(Action<T> action)
        {
            action(Value);
            return this;
        }
    }
}