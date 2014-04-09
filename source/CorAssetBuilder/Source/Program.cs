using System;
using System.IO;
using System.Runtime.InteropServices;
using ServiceStack.Text;
using System.Linq;
using System.Collections.Generic;
using Cor;
using System.Reflection;
using Cor.Assets.Builders;

namespace CorAssetBuilder
{
    public static class DateTimeHelper
    {
        public static DateTime FromUnixTime (Int32 epoch)
        {
            var dateTime = new DateTime (1970, 1, 1, 0, 0, 0, 0);
            return dateTime.AddSeconds (epoch);
        }
        
        public static Int32 ToUnixTime(DateTime time)
        {
            var span = time - new DateTime (1970, 1, 1, 0, 0, 0, 0);
            return (Int32) span.TotalSeconds;
        }
    }
    
	public class Program
	{
        static Configuration.ProjectDefinition projectDefinition;
        
        static string currentPlatform;
        
        public static void Main (string[] args)
        {   
            Console.WriteLine ("Cor Asset Builder");
            Console.WriteLine ("=================");
            
            Console.WriteLine ("");
            
            AssImp.AssImp.PrintVersion ();

            Console.WriteLine (
                "CAB build version: " +
                File.ReadAllText (
                    Path.Combine (
                        Environment.GetEnvironmentVariable ("HOME"), 
                        ".cba.installation"))
                .FromJson<Configuration.InstallInfo> ()
                .InstallDateTime);

            Console.WriteLine ("");
            
            
            
            if (args.Length > 0)
            {
                currentPlatform = args[0];
            }
            
            if (args.Length > 1)
            {
                Console.WriteLine ("Setting working dir to: " + args [0]);
                
                if (args [1].Contains ("~/"))
                {
                    args [1] = args [1].Replace ("~/", "");
                    args [1] = Path.Combine (
                        Environment.GetEnvironmentVariable ("HOME"), 
                        args [1]
                    );
                }
                
                Directory.SetCurrentDirectory (args [1]);
                Console.WriteLine ("");
                Console.WriteLine (Directory.GetCurrentDirectory () + " $");
            }
            Console.WriteLine ("");
            Console.Write ("Looking for project definition .cab file... ");

            if (File.Exists (".cab"))
            {
                Console.WriteLine ("OK");
            }
            else
            {
                Console.WriteLine ("Failed to find .cab file.");
                return;
            }
            
            projectDefinition = 
                File.ReadAllText (".cab")
                    .FromJson<Configuration.ProjectDefinition> ();

            Console.WriteLine ("\tResources Folder: " + projectDefinition.ResourcesFolder);
            Console.WriteLine ("\tAsset Definitions Folder: " + projectDefinition.AssetDefinitionsFolder);
            Console.WriteLine ("\tDestination Folder: " + projectDefinition.DestinationFolder);
            Console.WriteLine ("");
            
            string[] assetDefinitionFiles = Directory.GetFiles (projectDefinition.AssetDefinitionsFolder);
            
            var assetDefinitions = assetDefinitionFiles
                .Select (
                    file =>
                    file.ReadAllText ()
                        .FromJson<Configuration.AssetDefinition> ())
                .ToList ();
       
            var platformIds = assetDefinitions
                .SelectMany (x => x.SourceSets)
                .SelectMany (x => x.Platforms)
                .Distinct ()
                .ToList ();
            
            Console.WriteLine ("Target Platforms:");
            platformIds.ForEach (x => Console.WriteLine ("\t" + x));
            Console.WriteLine ("");
            
            if (currentPlatform != null)
                platformIds = new List<string> { currentPlatform };
            
            Builders.Init();
            
            foreach (var platformId in platformIds)
            {
                var pId = platformId;
                var platformAssetDeinitions = assetDefinitions
                    .Where (
                        x =>
                        x.HasSourceSetForPlatform (pId))
                    .ToList ();
                
                ProcessAssetsForPlatform (pId, platformAssetDeinitions);
            }
		}
        
        static void ProcessAssetsForPlatform (
            String platformId, 
            List<Configuration.AssetDefinition> assetDefinitions)
        {
            Console.WriteLine ("Processing Platform: " + platformId);
            Console.WriteLine ("");
            foreach (var assetDefinition in assetDefinitions)
            {
                var sourceset = assetDefinition.GetSourceForPlatform (platformId);
                
                var sourcefiles = sourceset.Files
                    .Select (x => Path.Combine (projectDefinition.ResourcesFolder, x))
                    .ToList ();
                
                sourcefiles.ForEach (x => Console.WriteLine ("\t+ " + x));
                
                String assetfile = 
                    Path.Combine (
                        projectDefinition.DestinationFolder, 
                        platformId, 
                        assetDefinition.AssetId + ".cba");
                
                Console.WriteLine ("\t= " + assetfile);
                
                try
                {
                    //Type assetType = Type.GetType (assetDefinition.AssetType);
                    
                    Type resourceBuilderType = Type.GetType (sourceset.ResourceBuilderType + ",.dll");
                    Type assetBuilderType = Type.GetType (sourceset.AssetBuilderType);
                    
                    IResource r = BuildResource (resourceBuilderType, sourcefiles, sourceset.ResourceBuilderSettings);
                    IAsset a = BuildAsset (assetBuilderType, r, sourceset.AssetBuilderSettings);
                    
                    WriteAsset (a, assetfile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine ("\t! failed to build: " + ex.GetType () + " - " + ex.Message.Replace(Environment.NewLine, "  "));
                }
                
                Console.WriteLine ("");
            }
        }
        
        static IResource BuildResource (
            Type resourceBuilderType,
            List<String> sourceFiles,
            Dictionary<String, String> settings)
        {
            Console.WriteLine ("\t\tabout to build resource with " + resourceBuilderType);
            
            var resourceBuilder = Activator.CreateInstance (resourceBuilderType) as ResourceBuilder;
            
            var resourceBuilderInput = new ResourceBuilderInput ();
            resourceBuilderInput.Files = sourceFiles;
            resourceBuilderInput.ResourceBuilderSettings = new ResourceBuilderSettings ();
            resourceBuilderInput.ResourceBuilderSettings.Settings = settings;
            var output = resourceBuilder.BaseImport (resourceBuilderInput);
            
            var resourceType = resourceBuilderType.BaseType ().GenericTypeArguments ()[0];
            
            
            Console.WriteLine ("\t\tresource type = " + resourceType);
            
            return output.Resource;
        }
        
        static IAsset BuildAsset (
            Type assetBuilderType,
            IResource resource,
            Dictionary<String, String> settings)
        {
            Console.WriteLine ("\t\tabout to build asset with " + assetBuilderType);
            
            var assetBuilder = Activator.CreateInstance (assetBuilderType) as AssetBuilder;
            
            var resourceType = assetBuilderType.BaseType ().GenericTypeArguments ()[0];
            var assetType = assetBuilderType.BaseType ().GenericTypeArguments ()[1];
            Console.WriteLine ("\t\tasset type = " + assetType);
            
            var abiType = typeof (AssetBuilderInput<>);
            
            Type genericAbiType = abiType.MakeGenericType(resourceType);
            var assetBuilderInputObject =  Activator.CreateInstance(genericAbiType);
            
            var assetBuilderInput = assetBuilderInputObject as AssetBuilderInput;
            
            assetBuilderInput.Resource = resource;
            assetBuilderInput.AssetBuilderSettings = new AssetBuilderSettings ();
            assetBuilderInput.AssetBuilderSettings.Settings = settings;
            
            var output = assetBuilder.BaseProcess (assetBuilderInput);
            
            
            return output.Asset;
        }
        
        static void WriteAsset (IAsset a, string destination)
        {
            Console.WriteLine ("\t\tabout to write asset to " + destination);
        }
	}
}

