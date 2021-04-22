using System;
using System.IO;

namespace Unity.Reflect.Viewer.UI
{
    public class ArgumentParser
    {
        public ArgumentParser() {}

        public string[] Args { get; private set; }
        public string AppPath { get; private set; } = string.Empty;
        public string TrailingArg { get; private set; } = string.Empty;

        /// <summary>
        /// Parse the command line arguments given to the executable, as well as the trailing argument. The trailing
        /// argument is only non-empty if it doesn't start with '-'
        /// </summary>
        public void Parse()
        {
            Args = Environment.GetCommandLineArgs();

            // Unity usual start command have the path to application as single argument
            // Some Build will mishandle spaces and artificially creates more than 1 argument
            int appPathLen = 0;

            for (var i=0;i<Args.Length;i++)
            {
                if (i > 0)
                {
                    AppPath += " ";
                }
                AppPath += Args[i];
                appPathLen++;

                if (File.Exists(AppPath))
                {
                    break;
                }
            }

            // If the executable path length is less than the Args length, that means we have a trailing argument
            // We also check that it does not start with a dash, in case we want to call it with optional args such as
            // -batchmode
            if (appPathLen < Args.Length && !Args[Args.Length - 1].StartsWith("-"))
            {
                TrailingArg = Args[Args.Length - 1];
            }
        }

    }
}
