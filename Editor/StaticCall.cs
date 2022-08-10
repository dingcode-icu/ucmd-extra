using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ucmd.BuildPlayer
{
    public class StaticCall
    {

        public static readonly Dictionary<string, string> ArgMap = new Dictionary<string, string>();

        public static void Run()
        {
            PrepareArgs();
            var isSuc = ArgMap.TryGetValue("_targetPlatform", out var plat);
            if (!isSuc) return;
            switch (plat)
            {
                case "android":
                    PerformBuildAndroid.Run();
                    break;
                case "ios":
                    PerformBuildIOS.Run();
                    break;
                default:
                    Debug.LogError($"Platform of <{plat}> is not support yet!");
                    break;
            }

        }

        private static void PrepareArgs()
        {
            var cmdArgs = Environment.GetCommandLineArgs();
            foreach (var v in cmdArgs)
            {
                if (v.StartsWith("-") && v.Split(":").Length > 1)
                {
                    var l = v.Split(":");
                    var k = l[0].Substring(1);
                    var val = l[1];
                    ArgMap[k] = val;
                }
            }
            Debug.Log($@"
***********************************
Option params in command:
_targetPlatform: {ArgMap["_targetPlatform"]}
{ArgMap}
***********************************
");
        }
    }
}