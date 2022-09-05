// ReSharper disable once CheckNamespace

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ucmd.BuildPlayer
{
    public class BuildBase
    {
        #region 基础属性及参数
        public enum HookType
        {
            Before,
            Finish
        }

        /// <summary>
        /// 打包命令传入：是否是release编译
        /// </summary>
        protected static bool IsRelease { set; get; }

        /// <summary>
        /// 打包命令传入：宏定义
        /// </summary>
        protected static string BuildSymbols = "";
        
        /// <summary>
        /// 打包命令传入：输出路径
        /// </summary>
        protected static string OutputPath = "";
        
        /// <summary>
        /// 打包命令传入：目标平台
        /// </summary>
        protected static string TargetPlatform = "";

        #endregion


        protected BuildBase()
        {
            IsRelease = StaticCall.ArgMap["isRelease"] == "true";
            BuildSymbols = StaticCall.ArgMap.ContainsKey("buildSymbols") ? StaticCall.ArgMap["buildSymbols"] : "";
            TargetPlatform =  StaticCall.ArgMap.ContainsKey("_targetPlatform")? StaticCall.ArgMap["_targetPlatform"] : "unknown";
            OutputPath = StaticCall.ArgMap.ContainsKey("_outputPath")? StaticCall.ArgMap["_outputPath"] : Application.dataPath + "/.ucmd_build";
            Debug.Log($@"
***********************************
Inner params in ucmd is:
_outputPath:{OutputPath}
_targetPlatform:{TargetPlatform}
***********************************
");
            
            Debug.Log($@"
***********************************
Global params in ucmd is
isRelease:{IsRelease} 
buildSymbols:{BuildSymbols}
***********************************
");
        }

        protected static string[] GetScenes()
        {
            var lScenes = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.path.StartsWith("_"))
                {
                    break;
                } 
                if (!scene.enabled) break;
                lScenes.Add(scene.path);
            }
            return lScenes.ToArray();
        } 

        #region Hook相关

        public delegate void UcmdBuildHook(HookType t);

        public static UcmdBuildHook BuildHook { set; get; }

        protected static void ExecuteHook(HookType hType)
        {
            BuildHook?.Invoke(hType);
        }

        #endregion
    }
}
