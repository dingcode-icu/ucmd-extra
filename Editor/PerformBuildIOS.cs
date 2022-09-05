using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace Ucmd.BuildPlayer
{
    public class PerformBuildIOS : BuildBase
    {
        internal PerformBuildIOS()
        {
            Debug.Log($"@" +
                      $"isRelease:{IsRelease}" +
                      $"buildTarget{BuildSymbols}" +
                      $"outputPath{OutputPath}");
        }
        
        private static string[] GetBuildScenes()
        {
            return (from e in EditorBuildSettings.scenes where e != null where e.enabled select e.path).ToArray();
        }
        
        /// <summary>
        /// Ucmd外部调用函数入口
        /// </summary>
        public void Run()
        {   
            var scenes = GetBuildScenes();
            var path = BuildHelper.CheckBuildPath(OutputPath);
            //是否是测试包
            EditorUserBuildSettings.development = IsRelease;
            //是否有额外编译宏
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, BuildSymbols);


            #if UNITY_2021_3_OR_NEWER
                var option = BuildOptions.CleanBuildCache;
                BuildPipeline.BuildPlayer(scenes, path, BuildTarget.iOS, option);
            #else 
                var option = BuildOptions.None;
                BuildPipeline.BuildPlayer(scenes, path, BuildTarget.iOS, option);
            #endif


            ExecuteHook(HookType.Finish);
        }
    }
}