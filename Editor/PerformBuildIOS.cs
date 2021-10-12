using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR_OSX
using UnityEditor.iOS.Xcode;
using UnityEditor.XCodeEditor;
using PBXProject = UnityEditor.iOS.Xcode.PBXProject;


namespace Ucmd.BuildPlayer
{
    public class PerformBuildIOS : BuildBase
    {
        private static readonly Dictionary<int, string> ErrorDesc = new Dictionary<int, string>
        {
            {101, "scene or path is empty!"}
        };

        private static bool isNewCreate = true;

        private static readonly string ProjBuildPath = Application.dataPath + "/../../../build/iPhone";

        /// <summary>
        /// 获取要打包的所有的scene
        /// </summary>
        /// <returns></returns>
        private static string[] GetBuildScenes()
        {
            return (from e in EditorBuildSettings.scenes where e != null where e.enabled select e.path).ToArray();
        }

        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
        {
            Debug.Log("on post process build");
            if (buildTarget == BuildTarget.iOS)
            {
                //main.mm中找不到UnityFramework/UnityFramework.h的问题
                var mainAppPath = Path.Combine(path, "MainApp", "main.mm");
                var mainContent = File.ReadAllText(mainAppPath);
                var newContent = mainContent.Replace("#include <UnityFramework/UnityFramework.h>",
                    @"#include ""../UnityFramework/UnityFramework.h""");
                File.WriteAllText(mainAppPath, newContent);

                // 添加额外的 AssetsLibrary.framework
                // 用于检测相册权限等功能
                string projPath = PBXProject.GetPBXProjectPath(path);
                PBXProject proj = new PBXProject();
                proj.ReadFromString(File.ReadAllText(projPath));

                // Get plist
                string plistPath = path + "/Info.plist";
                PlistDocument plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(plistPath));

                //add weixin scheme
                PlistElementArray schemes = null;

                //添加依赖库
                string targetUnity = proj.GetUnityFrameworkTargetGuid();
                string[] frameworks =
                {
                    "JavaScriptCore.framework",
                    "CoreTelephony.framework",
                    // "libc++.tbd",
                    // "libz.tbd"
                };
                foreach (var framework in frameworks)
                {
                    proj.AddFrameworkToProject(targetUnity, framework, false);
                    Debug.Log("AddFrameworkToProject------》" + framework);
                }

                Dictionary<string, string> pathDic = new Dictionary<string, string>
                {
                    {"project.json", ""},
                };

                string target = proj.GetUnityMainTargetGuid();

                foreach (var item in pathDic)
                {
                    string UnityCPPFile = Application.dataPath + "/Editor/CPPCode/" + item.Key;
                    string tagUnityCPPPath = path + item.Value;
                    string tagUnityCPPFile = tagUnityCPPPath + "/" + item.Key;

                    if (!Directory.Exists(tagUnityCPPPath))
                    {
                        Directory.CreateDirectory(tagUnityCPPPath);
                    }

                    if (File.Exists(tagUnityCPPFile))
                    {
                        File.Delete(tagUnityCPPFile);
                    }

                    File.Copy(UnityCPPFile, tagUnityCPPFile);
                    string resourcePath = item.Value + "/" + item.Key;
                    string fileGuid = proj.AddFolderReference(tagUnityCPPFile, resourcePath);
                    proj.AddFileToBuild(target, fileGuid);
                    // proj.AddFile(Path.Combine(path, item.Value + "/" + item.Key), item.Value , PBXSourceTree.Source);
                }

                proj.SetBuildProperty(targetUnity, "ENABLE_BITCODE", "NO");
                proj.SetBuildProperty(targetUnity, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");
                // proj.SetBuildProperty(targetUnity, "CLANG_ENABLE_OBJC_ARC", "NO");

                //保存修改配置
                File.WriteAllText(projPath, proj.WriteToString());

                EditorPlist(plistPath);

                //只 copy CPP 文件
                EditorCode(path);

                //运行pod安装sdk命令
                string[] sttrs = BuildHelper.RunCmd("pod", "install", path);
                Debug.Log("RunCmd out------》" + sttrs[0]);
                Debug.Log("RunCmd error------》" + sttrs[1]);
            }
        }

        private static void EditorPlist(string filePath)
        {
            XCPlist list = new XCPlist(filePath);

            string PlistAdd = @"  
            <key>NSAppTransportSecurity</key>
	<dict>
		<key>NSAllowsArbitraryLoads</key>
		<true/>
	</dict>";

            //在plist里面增加一行
            list.AddKey(PlistAdd);
            //保存
            list.Save();
        }

        private static void EditorCode(string projPath)
        {
            Dictionary<string, string> pathDic = new Dictionary<string, string>
            {
                {"UnityAppController.mm", "/Classes"},
                {"Podfile", ""},
                {"ExportOptions.plist", ""}
            };
            foreach (var item in pathDic)
            {
                string UnityCPPFile = Application.dataPath + "/Editor/CPPCode/" + item.Key;
                string tagUnityCPPPath = projPath + item.Value;
                string tagUnityCPPFile = tagUnityCPPPath + "/" + item.Key;

                Debug.Log("EditorCode------》" + item.Key);
                if (!Directory.Exists(tagUnityCPPPath))
                {
                    Directory.CreateDirectory(tagUnityCPPPath);
                }

                if (File.Exists(tagUnityCPPFile))
                {
                    File.Delete(tagUnityCPPFile);
                }

                File.Copy(UnityCPPFile, tagUnityCPPFile);
            }
        }

        public static void CommandLineBuild()
        {
            Debug.Log("CommandLineBuild start...\n------------------\n------------------");
            var scenes = GetBuildScenes();
            var path = BuildHelper.CheckBuildPath(ProjBuildPath);
            if (scenes == null || scenes.Length == 0 || path == null)
            {
                Debug.LogError($"Failed to build...{ErrorDesc[101]}");
                Process.GetCurrentProcess().Kill();
            }

            //检查是否有isDev(require)相关参数
            CheckRequireArgs();
            Debug.Log($@"
***********************************
Custom require command args:isDev = {isDev}
***********************************
");
            //是否是测试包
            EditorUserBuildSettings.development = isDev;
            Debug.Log($"Path: \"{path}\"");
            if (scenes != null)
                for (var i = 0; i < scenes.Length; ++i)
                {
                    Debug.Log($"Scene[{i}]: \"{scenes[i]}\"");
                }
            //是否有额外编译宏
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, buildSymbols);
            var option = isNewCreate ? BuildOptions.None : BuildOptions.AcceptExternalModificationsToPlayer; //is append
            BuildPipeline.BuildPlayer(scenes, path, BuildTarget.iOS, option);


            ExecuteHook(HookType.Finish);
        }

        /// <summary>
        /// Ucmd外部调用函数入口
        /// </summary>
        public static void ExportIPA()
        {
            var path = BuildHelper.CheckBuildPath(ProjBuildPath);
            CommandLineBuild();
            // //运行命令
            string[] sttrs = BuildHelper.RunCmd("xcodebuild", "clean", path);
            Debug.Log("RunCmd out------》" + sttrs[0]);
            Debug.Log("RunCmd error------》" + sttrs[1]);

            string[] sttrs2 = BuildHelper.RunCmd("xcodebuild", " archive -workspace " + path +
                                                               "/Unity-iPhone.xcworkspace -scheme Unity-iPhone -archivePath " +
                                                               path +
                                                               "/Unity-iPhone.xcarchive", path);
            Debug.Log("BuildHelper.RunCmd2 out------》" + sttrs2[0]);
            Debug.Log("BuildHelper.RunCmd2 error------》" + sttrs2[1]);
            string[] sttrs3 = BuildHelper.RunCmd("xcodebuild", "-exportArchive -archivePath " + path +
                                                               $"/Unity-iPhone.xcarchive -exportPath IPAFile -exportOptionsPlist " +
                                                               path + "/ExportOptions.plist", path);
            Debug.Log("BuildHelper.RunCmd3 out------》" + sttrs3[0]);
            Debug.Log("BuildHelper.RunCmd3 error------》" + sttrs3[1]);

            var c = DateTime.Now.ToString("yyyyMMdd-HH_mm_ss");
            string NginxPath = "/usr/local/Cellar/openresty/1.19.3.2_1/nginx/html";
            string[] sttrs4 = BuildHelper.RunCmd("cp",
                path + "/IPAFile/PaopaoPinyin.ipa " + NginxPath + $"/paopao_{c}.ipa", path);
            Debug.Log("RunCmd4 out------》" + sttrs4[0]);
            Debug.Log("RunCmd4 error------》" + sttrs4[1]);
        }
    }
}
#endif