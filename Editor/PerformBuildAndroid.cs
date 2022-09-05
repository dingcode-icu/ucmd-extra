using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Ucmd.BuildPlayer
{
    public class PerformBuildAndroid : BuildBase
    {
        /// <summary>
        /// 打包命令传入：目标arch
        /// </summary>
        private static string ArchTarget = "";


        /// <summary>
        /// 打包命令传入：目标subTarget
        /// </summary>
        private static string SubTarget = "";


        /// <summary>
        /// 打包命令传入：是否导出工程
        /// </summary>
        private static bool IsExport = true;

        internal PerformBuildAndroid()
        {
            ArchTarget = StaticCall.ArgMap.ContainsKey("archTarget") ? StaticCall.ArgMap["archTarget"] : "arm-v7";
            SubTarget = StaticCall.ArgMap.ContainsKey("subTarget") ? StaticCall.ArgMap["subTarget"] : "unset";
            IsExport = StaticCall.ArgMap.ContainsKey("isExport") ? StaticCall.ArgMap["isExport"] == "false": true;

            Debug.Log($@"
***********************************
Android params in ucmd is 
archTarget:{ArchTarget} 
isExport:{IsExport.ToString()}
subTarget:{SubTarget}
***********************************
");
        }

#if UNITY_ANDROID_API
        private static AndroidArchitecture TargetArchitectures = AndroidArchitecture.ARMv7;

        private static MobileTextureSubtarget SubTargetAnd =  MobileTextureSubtarget.ASTC;
#endif
        /// <summary>
        /// 获取要打包的所有的scene
        /// </summary>
        /// <returns></returns>
        private static string[] GetBuildScenes()
        {
            return (from e in EditorBuildSettings.scenes where e != null where e.enabled select e.path).ToArray();
        }

        private static void CommandBuild()
        {
            var scenes = GetBuildScenes();
            //isRelease?
            EditorUserBuildSettings.development = IsRelease;
            //isExport? 
            EditorUserBuildSettings.exportAsGoogleAndroidProject = IsExport;
            //是否有额外编译宏
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, BuildSymbols);

#if UNITY_ANDROID_API
            //android subTarget
            switch (SubTarget)
            {
                default:
                    Debug.LogWarning("Not find right $SubTarget, so use editor-setting default!");
                    //android默认目标
                    break;
            }
            EditorUserBuildSettings.androidBuildSubtarget = SubTargetAnd;

            //android archTarget
            switch (ArchTarget)
            {
                case "armv8":
                    TargetArchitectures = AndroidArchitecture.ARM64;
                    break;
                case "armv7;armv8":
                    TargetArchitectures = AndroidArchitecture.All;
                    break;
                default:
                    Debug.LogWarning("Not find right $ArchTarget, so armv7 default!");
                    TargetArchitectures = AndroidArchitecture.ARMv7;
                    break;
            }
            PlayerSettings.Android.targetArchitectures = TargetArchitectures;
#endif
            var e = BuildHelper.CheckBuildPath(OutputPath);
            var path = IsExport ? e : $"{e}/{DateTime.Now:yyyyMMdd-HH_mm_ss}.apk";
            BuildHelper.CleanPath(path);
            Debug.Log($"Path: \"export target is  --->>{path}\"");
            #if UNITY_2021_3_OR_NEWER
                var option = BuildOptions.CleanBuildCache;
                BuildPipeline.BuildPlayer(scenes, path, BuildTarget.Android, option);
            #else 
                var option = BuildOptions.None;
                BuildHelper.CheckBuildPath(path);
                Debug.Log(($"all scene is {scenes.Length.ToString()}"));
                BuildPipeline.BuildPlayer(scenes, path, BuildTarget.Android, option);
            #endif
                
        }

        /// <summary>
        /// Ucmd外部调用函数入口
        /// </summary>
        public void Run()
        {
            CommandBuild();
            ExecuteHook(HookType.Finish);
        }
    }
}
