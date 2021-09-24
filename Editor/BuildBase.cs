using System;
using System.Diagnostics;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Ucmd.BuildPlayer
{
    public class BuildBase
    {
        public enum HookType
        {
            Finish
        }

        protected static bool isDev { set; get; }

        protected static string buildSymbols = "";

        public delegate void UcmdBuildHook(HookType htype);

        public static UcmdBuildHook buildHook { set; get; }

        protected static void ExecuteHook(HookType hType)
        {
            buildHook?.Invoke(hType);
        }

        protected static void CheckRequireArgs()
        {
            //检查是否有isDev(require)参数
            var args = Environment.GetCommandLineArgs();
            var isRequire = false;
            foreach (var s in args)
            {
                if (!s.Contains("-isRelease:")) continue;
                isDev = s.Split(':')[1] == "debug";
                isRequire = true;
                break;
            }
            if (isRequire == false)
            {
                Debug.LogException(new Exception(@"
***********************************
Not found require command args:-isRelease:xxx
***********************************") );
                EditorApplication.Exit(101);
            }
            //检查是否有buildSympol(option)参数
            foreach (var s in args)
            {
                if (!s.Contains("-buildSymbols:")) continue;
                buildSymbols = s.Split(':')[1];
                break;
            }
        }
    }
}