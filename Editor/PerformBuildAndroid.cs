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
        /// 打包命令传入：宏定义
        /// </summary>
        private static string ArchTarget = "";


        internal PerformBuildAndroid()
        {
            ArchTarget = StaticCall.ArgMap.ContainsKey("archTarget") ? StaticCall.ArgMap["archTarget"] : "arm-v7";
            Debug.Log($"@" +
                      $"isRelease:{IsRelease}" +
                      $"buildTarget{BuildSymbols}" +
                      $"outputPath{OutputPath}" +
                      $"archTarget{ArchTarget}");
        }

#if UNITY_ANDROID_API
        private static AndroidArchitecture TargetArchitectures = AndroidArchitecture.ARMv7;
#endif
        /// <summary>
        /// 获取要打包的所有的scene
        /// </summary>
        /// <returns></returns>
        private static string[] GetBuildScenes()
        {
            return (from e in EditorBuildSettings.scenes where e != null where e.enabled select e.path).ToArray();
        }

        private static void CommandBuild(bool isExport)
        {
            var scenes = GetBuildScenes();
            //isRelease?
            EditorUserBuildSettings.development = IsRelease;
            //isExport? 
            EditorUserBuildSettings.exportAsGoogleAndroidProject = isExport;
            //是否有额外编译宏
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, BuildSymbols);


#if UNITY_ANDROID_API
            //android默认目标
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
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
            var path = isExport ? e : $"{e}/{DateTime.Now:yyyyMMdd-HH_mm_ss}.apk";
            BuildHelper.CleanPath(path);
            Debug.Log($"Path: \"export target is  --->>{path}\"");
            var option = BuildOptions.CleanBuildCache;
            BuildPipeline.BuildPlayer(scenes, path, BuildTarget.Android, option);
        }

        /// <summary>
        /// Ucmd外部调用函数入口
        /// </summary>
        public void Run()
        {
            CommandBuild(true);
            ExecuteHook(HookType.Finish);
        }
    }
}