namespace Program.WithEmbeddedSimpleLib
{
    using System;
    using EmbeddedFx;
    using SimpleLib;

    public class Program
    {
        static Program()
        {
            EmbeddedAssemblyLoader.Register();
        }

        public static int Main(string[] args)
        {
            try
            {
                new Class1();
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception was thrown: {0}", e.Message);
                return 1;
            }

            return 0;
        }
    }
}
