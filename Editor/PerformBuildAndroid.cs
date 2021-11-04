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
        private static readonly string ProjBuildPath = Application.dataPath + "/../../../build/Android";
        private static readonly string NaAssetPath = Path.Combine(ProjBuildPath, "../UnityfoNa");


        /// <summary>
        /// 获取要打包的所有的scene
        /// </summary>
        /// <returns></returns>
        private static string[] GetBuildScenes()
        {
            return (from e in EditorBuildSettings.scenes where e != null where e.enabled select e.path).ToArray();
        }

        private static void CommandLineExport(bool isExport)
        {
            var scenes = GetBuildScenes();
            //检查是否有外部传入参数
            CheckRequireArgs();
            CheckOptionArgs();
            //是否是测试包
            EditorUserBuildSettings.development = IsRelease;
            //是否是导出工程 
            EditorUserBuildSettings.exportAsGoogleAndroidProject = isExport;
            //是否有额外编译宏
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, BuildSymbols);
            //android
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
            // EditorUserBuildSettings.
            var e = BuildHelper.CheckBuildPath(ProjBuildPath);
            var path = isExport ? e : $"{e}/{DateTime.Now:yyyyMMdd-HH_mm_ss}.apk";
            Debug.Log($"Path: \"export target is  --->>{path}\"");
            var option = BuildHelper.IsNewCreate | isExport == false
                ? BuildOptions.None
                : //new create || autorun
                BuildOptions.AcceptExternalModificationsToPlayer; //allow append || autorun
            BuildPipeline.BuildPlayer(scenes, path, BuildTarget.Android, option);
        }

        /// <summary>
        /// Ucmd外部调用函数入口
        /// </summary>
        public static void ExportProjAsset()
        {
            var res = new Dictionary<string, string>
            {
                {"unityLibrary/src/main/assets/bin", "unityLibrary/src/main/assets/bin"},
                {"unityLibrary/src/main/jniLibs/armeabi-v7a", "unityLibrary/src/main/jniLibs/armeabi-v7a"}
            };
            BuildHelper.CheckAssetBuildPath(NaAssetPath);
            CommandLineExport(true);
            foreach (var cell in res)
            {
                var f = Path.Combine(ProjBuildPath, cell.Key);
                BuildHelper.DirectoryCopy(f, Path.Combine(NaAssetPath, cell.Value), true);
            }
            ExecuteHook(HookType.Finish);
        }
    }
}