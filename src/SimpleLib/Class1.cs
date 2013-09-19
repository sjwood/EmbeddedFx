namespace SimpleLib
{
    using System;

    public sealed class Class1
    {
        public Class1()
        {
            Console.WriteLine("Type '{0}' instantiated from Assembly '{1}'.", this.GetType().FullName, this.GetType().Assembly.FullName);
        }
    }
}
