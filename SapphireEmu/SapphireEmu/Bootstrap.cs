using System;
using System.IO;
using System.Reflection;
using SapphireEmu.Data;
using SapphireEmu.Environment;

namespace SapphireEmu
{
    static class Bootstrap
    {
        static Bootstrap()
        {
            Bootstrap.ShowConsoleName();
            Bootstrap.ShowBranding();
            Bootstrap.ResolveReferenceInBin();
        }
        
        static void Main() => ApplicationManager.Initialization();

        static void ShowBranding()
        {
            Console.WriteLine("###########################");
            Console.WriteLine("# Application: " + BuildingInformation.ApplicationName);
            Console.WriteLine("# Version: " + BuildingInformation.ApplicationVersion + ", build " + BuildingInformation.ApplicationBuild);
            Console.WriteLine("# Author: " + BuildingInformation.Author);
            Console.WriteLine("# Thanks: " + BuildingInformation.Thanks);
            Console.WriteLine("###########################\n");
        }

        static void ShowConsoleName()
        {
            Console.Title = BuildingInformation.ApplicationName + ", v" + BuildingInformation.ApplicationVersion + ", build " + BuildingInformation.ApplicationBuild + " by ~ " + BuildingInformation.Author;
        }

        static void ResolveReferenceInBin()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string assemblyName = args.Name.Substring(0, args.Name.IndexOf(","));
                if (File.Exists(BuildingInformation.DirectoryBin + assemblyName + ".dll"))
                    return Assembly.LoadFrom(BuildingInformation.DirectoryBin + assemblyName + ".dll");
                return null;
            };
            System.Environment.CurrentDirectory = BuildingInformation.DirectoryBin;
        }
    }
}