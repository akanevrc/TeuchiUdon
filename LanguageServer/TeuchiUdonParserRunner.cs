using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace akanevrc.TeuchiUdon.Server
{
    public static class TeuchiUdonParserRunner
    {
        public static IServiceCollection AddParserServices(this IServiceCollection services)
        {
            var unityPath   = "D:/MyProgramFiles/UnityHub/2019.4.31f1/Editor";
            var projectPath = "D:/MyDocuments/UnityProjects/TeuchiUdon/Unity";
            var dllPaths = new string[]
            {
                Path.Combine(unityPath  , "Data/Managed/UnityEditor.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.AccessibilityModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.AIModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.AndroidJNIModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.AnimationModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.ARModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.AssetBundleModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.AudioModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.ClothModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.ClusterInputModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.ClusterRendererModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.CoreModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.CrashReportingModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.DirectorModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.DSPGraphModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.GameCenterModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.GridModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.HotReloadModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.ImageConversionModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.IMGUIModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.InputLegacyModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.InputModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.JSONSerializeModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.LocalizationModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.ParticleSystemModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.PerformanceReportingModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.Physics2DModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.PhysicsModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.ProfilerModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.ScreenCaptureModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.SharedInternalsModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.SpriteMaskModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.SpriteShapeModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.StreamingModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.SubstanceModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.SubsystemsModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.TerrainModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.TerrainPhysicsModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.TextCoreModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.TextRenderingModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.TilemapModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.TLSModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.UIElementsModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.UIModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.UmbraModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.UNETModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.UnityAnalyticsModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.UnityConnectModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.UnityTestProtocolModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.UnityWebRequestAssetBundleModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.UnityWebRequestAudioModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.UnityWebRequestModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.UnityWebRequestTextureModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.UnityWebRequestWWWModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.VehiclesModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.VFXModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.VideoModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.VRModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.WindModule.dll"),
                Path.Combine(unityPath  , "Data/Managed/UnityEngine/UnityEngine.XRModule.dll"),
                Path.Combine(projectPath, "Assets/VRCSDK/Plugins/VRCSDKBase.dll"),
                Path.Combine(projectPath, "Assets/VRCSDK/Plugins/VRCSDK3.dll"),
                Path.Combine(projectPath, "Assets/Udon/External/VRC.Udon.ClientBindings.dll"),
                Path.Combine(projectPath, "Assets/Udon/External/VRC.Udon.Common.dll"),
                Path.Combine(projectPath, "Assets/Udon/Editor/External/VRC.Udon.EditorBindings.dll"),
                Path.Combine(projectPath, "Assets/Udon/Editor/External/VRC.Udon.Graph.dll"),
                Path.Combine(projectPath, "Library/ScriptAssemblies/Cinemachine.dll"),
                Path.Combine(projectPath, "Library/ScriptAssemblies/Unity.Postprocessing.Runtime.dll"),
                Path.Combine(projectPath, "Library/ScriptAssemblies/Unity.TextMeshPro.dll"),
                Path.Combine(projectPath, "Library/ScriptAssemblies/VRC.Udon.dll"),
                Path.Combine(projectPath, "Library/ScriptAssemblies/VRC.Udon.Editor.dll")
            };

            var dllLoader    = new TeuchiUdonDllLoader(dllPaths);
            var primitives   = new TeuchiUdonPrimitives(dllLoader);
            var staticTables = new TeuchiUdonStaticTables(dllLoader, primitives);
            var invalids     = new TeuchiUdonInvalids();

            dllLoader   .Init();
            primitives  .Init();
            staticTables.Init();
            invalids    .Init();

            return
                services
                .AddSingleton(dllLoader)
                .AddSingleton(primitives)
                .AddSingleton(staticTables)
                .AddSingleton(invalids)
                .AddScoped<TeuchiUdonLogicalErrorHandler>()
                .AddScoped<TeuchiUdonTables>()
                .AddScoped<TeuchiUdonTypeOps>()
                .AddScoped<TeuchiUdonLabelOps>()
                .AddScoped<TeuchiUdonTableOps>()
                .AddScoped<TeuchiUdonQualifierStack>()
                .AddScoped<TeuchiUdonOutValuePool>()
                .AddScoped<TeuchiUdonSyntaxOps>()
                .AddScoped<TeuchiUdonParserResultOps>()
                .AddScoped<TeuchiUdonListener>();
        }

        public static (TargetResult? result, string error) ParseFromString(IServiceProvider services, string input)
        {
            using var inputReader = new StringReader(input);
            return ParseFromReader(services, inputReader);
        }

        public static (TargetResult? result, string error) ParseFromReader(IServiceProvider services, TextReader inputReader)
        {
            using var outputWriter = new StringWriter();
            using var errorWriter  = new StringWriter();
            
            var inputStream = CharStreams.fromTextReader(inputReader);
            var lexer       = new TeuchiUdonLexer(inputStream, outputWriter, errorWriter);
            var tokenStream = new CommonTokenStream(lexer);
            var parser      = new TeuchiUdonParser(tokenStream, outputWriter, errorWriter);

            using var scope = services.CreateScope();
            var provider    = scope.ServiceProvider;

            provider.GetService<TeuchiUdonLogicalErrorHandler>()!.SetParser(parser);
            var listener = provider.GetService<TeuchiUdonListener>()!;

            try
            {
                ParseTreeWalker.Default.Walk(listener, parser.target());
            }
            catch (Exception ex)
            {
                return (null, $"{ex}\n{errorWriter}");
            }

            return (listener.TargetResult, errorWriter.ToString());
        }
    }
}
