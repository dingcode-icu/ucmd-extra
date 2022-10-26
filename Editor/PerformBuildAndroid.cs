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
        private static string _archTarget = "";


        /// <summary>
        /// 打包命令传入：目标subTarget
        /// </summary>
        private static string _subTarget = "";


        /// <summary>
        /// 打包命令传入：是否导出工程
        /// </summary>
        private static bool _isExport = true;

        internal PerformBuildAndroid()
        {
            _archTarget = StaticCall.ArgMap.ContainsKey("archTarget") ? StaticCall.ArgMap["archTarget"] : "arm-v7";
            _subTarget = StaticCall.ArgMap.ContainsKey("subTarget") ? StaticCall.ArgMap["subTarget"] : "unset";
            _isExport = !StaticCall.ArgMap.ContainsKey("isExport") || StaticCall.ArgMap["isExport"] == "false";

            Debug.Log($@"
***********************************
Android params in ucmd is 
archTarget:{_archTarget} 
isExport:{_isExport.ToString()}
subTarget:{_subTarget}
***********************************
");
        }
        
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
            EditorUserBuildSettings.exportAsGoogleAndroidProject = _isExport;
            //是否有额外编译宏
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, BuildSymbols);

            //android subTarget
            switch (_subTarget)
            {
                default:
                    Debug.LogWarning("Not find right $SubTarget, so use editor-setting default!");
                    //android默认目标
                    break;
            }
            //android archTarget
            switch (_archTarget)
            {
                case "armv8":
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                    break;
                case "armv7;armv8":
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
                    break;
                default:
                    Debug.LogWarning("Not find right $ArchTarget, so use editor-setting default!");
                    break;
            }
            
            Debug.LogWarning($"[performbuildandroid]android target architecture is {PlayerSettings.Android.targetArchitectures.ToString()}");
            var e = BuildHelper.CheckBuildPath(OutputPath);
            var path = _isExport ? e : $"{e}/{DateTime.Now:yyyyMMdd-HH_mm_ss}.apk";
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
