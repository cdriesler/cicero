﻿using s = System;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using sys = Box.System;

namespace Box.System.Test
{
    [TestClass]
    public static class TestInit
    {
        static bool initialized = false;
        static string systemDir = null;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            if (initialized)
            {
                throw new s.InvalidOperationException("AssemblyInitialize should only be called once");
            }
            initialized = true;

            context.WriteLine("Assembly init started");

            // Ensure we are 64 bit
            Assert.IsTrue(s.Environment.Is64BitProcess, "Tests must be run as x64");

            // Set path to rhino system directory
            string envPath = s.Environment.GetEnvironmentVariable("path");
            string programFiles = s.Environment.GetFolderPath(s.Environment.SpecialFolder.ProgramFiles);
            systemDir = s.IO.Path.Combine(programFiles, "Rhino WIP", "System");
            Assert.IsTrue(s.IO.Directory.Exists(systemDir), "Rhino system dir not found: {0}", systemDir);

            // Add rhino system directory to path (for RhinoLibrary.dll)
            s.Environment.SetEnvironmentVariable("path", envPath + ";" + systemDir);

            // Add hook for .Net assmbly resolve (for RhinoCommmon.dll)
            s.AppDomain.CurrentDomain.AssemblyResolve += ResolveRhinoCommon;

            // Start headless Rhino process
            LaunchInProcess(0, 0);
        }

        private static Assembly ResolveRhinoCommon(object sender, s.ResolveEventArgs args)
        {
            var name = args.Name;

            if (!name.StartsWith("RhinoCommon"))
            {
                return null;
            }

            var path = s.IO.Path.Combine(systemDir, "RhinoCommon.dll");
            return Assembly.LoadFrom(path);
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            // Shutdown the rhino process at the end of the test run
            ExitInProcess();
        }

        [DllImport("RhinoLibrary.dll")]
        internal static extern int LaunchInProcess(int reserved1, int reserved2);

        [DllImport("RhinoLibrary.dll")]
        internal static extern int ExitInProcess();
    }
}
