using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using Debug = UnityEngine.Debug;
using Enumerable = System.Linq.Enumerable;

// ReSharper disable once CheckNamespace
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
        /// 打包命令传入: 目标平台
        /// </summary>
        protected static string TargetPlatform = "";

        #endregion

        #region Hook相关

        private delegate void UcmdBuildHook(HookType t);

        private static UcmdBuildHook BuildHook { set; get; }

        protected static void ExecuteHook(HookType hType)
        {
            BuildHook?.Invoke(hType);
        }

        #endregion

        #region 外部语言传参

        protected static  void CheckOptionArgs()
        {
            var args = Environment.GetCommandLineArgs();
            foreach (var s in args)
            {
                //检查是否有buildSymbols(option)参数
                if (s.Contains("-buildSymbols:"))
                {
                    BuildSymbols = s.Split(':')[1];
                }

                //检查是否是isRelease
                if (s.Contains("-isRelease:"))
                {
                    IsRelease = s.Split(':')[1] != "false";
                }

                //检查目标平台
                if (s.Contains("-targetPlatform:"))
                {
                    TargetPlatform = s.Split(':')[1];
                }

            }

            Debug.Log($@"
***********************************
Option params in command:
isRelease : {IsRelease}
buildSymbols: {BuildSymbols}
targetPlatform: {TargetPlatform}
***********************************
");
        }

        protected static void CheckRequireArgs()
        {
            //检查是否有isDev(require)参数
            var args = Environment.GetCommandLineArgs();
            //TODO no require params
        }

        #endregion
    }
}