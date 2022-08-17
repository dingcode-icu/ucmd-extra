// ReSharper disable once CheckNamespace

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ucmd.BuildPlayer
{
    public class BuildBase
    {
        #region 基础属性及参数
        protected enum HookType
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
        

        #endregion


        protected BuildBase()
        {
            IsRelease = StaticCall.ArgMap["isRelease"] == "true";
            BuildSymbols = StaticCall.ArgMap.ContainsKey("buildSymbols") ? StaticCall.ArgMap["buildSymbols"] : "";
            OutputPath = StaticCall.ArgMap.ContainsKey("_outputPath")? StaticCall.ArgMap["_outputPath"] : Application.dataPath + "/.ucmd_build";
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

        private delegate void UcmdBuildHook(HookType t);

        private static UcmdBuildHook BuildHook { set; get; }

        protected static void ExecuteHook(HookType hType)
        {
            BuildHook?.Invoke(hType);
        }

        #endregion
    }
}