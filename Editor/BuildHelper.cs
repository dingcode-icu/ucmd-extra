using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Ucmd.BuildPlayer
{
    public static class BuildHelper
    {
        public static readonly Dictionary<int, string> ErrorDesc = new Dictionary<int, string>
        {
            {101, "scene or path is empty!"}
        };

        public static bool IsNewCreate = false;

        /// <summary>
        /// 打包路径，不存在就创建
        /// </summary>
        /// <returns></returns>
        public static string CheckBuildPath(string buildPath)
        {
            if (Directory.Exists(buildPath)) return buildPath;
            Directory.CreateDirectory(buildPath);
            IsNewCreate = true;
            return buildPath;
        }


        /// <summary>
        /// 导出路径，存在就删除刷新
        /// </summary>
        /// <returns></returns>
        public static void CleanPath(string buildPath)
        {
            if (!Directory.Exists(buildPath)) return;
            Directory.Delete(buildPath, true);
        }


        /// <summary>
        /// 把一个文件夹下所有文件复制到另一个文件夹下 
        /// </summary>
        /// <param name="sourceDirectory">源目录</param>
        /// <param name="targetDirectory">目标目录</param>
        public static bool DirectoryCopy(string sourceDirectory, string targetDirectory, bool overwriteexisting)
        {
            if (Directory.Exists(targetDirectory) == false)
                Directory.CreateDirectory(targetDirectory);
            try
            {
                var dir = new DirectoryInfo(sourceDirectory);
                //获取目录下（不包含子目录）的文件和子目录
                var info = dir.GetFileSystemInfos();

                foreach (var i in info)
                {
                    if (i is DirectoryInfo) //判断是否文件夹
                    {
                        if (!Directory.Exists(Path.Combine(targetDirectory, i.Name)))
                        {
                            //目标目录下不存在此文件夹即创建子文件夹
                            Directory.CreateDirectory(Path.Combine(targetDirectory, i.Name));
                        }

                        //递归调用复制子文件夹
                        DirectoryCopy(i.FullName, Path.Combine(targetDirectory, i.Name), overwriteexisting);
                    }
                    else
                    {
                        if (i.Extension != ".meta")
                        {
                            //不是文件夹即复制文件，true表示可以覆盖同名文件
                            File.Copy(i.FullName, Path.Combine(targetDirectory, i.Name), overwriteexisting);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"copy dir raise error {ex.ToString()}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 把一个文件夹下所有文件复制到另一个文件夹下
        /// </summary>
        /// <param name="sourceDirectory">源目录</param>
        /// <param name="targetDirectory">目标目录</param>
        public static bool DirectoryMove(string sourceDirectory, string targetDirectory, bool overwriteexisting)
        {
            if (Directory.Exists(targetDirectory) == false)
                Directory.CreateDirectory(targetDirectory);
            try
            {
                var dir = new DirectoryInfo(sourceDirectory);
                //获取目录下（不包含子目录）的文件和子目录
                var info = dir.GetFileSystemInfos();

                foreach (var i in info)
                {
                    if (i is DirectoryInfo) //判断是否文件夹
                    {
                        if (!Directory.Exists(Path.Combine(targetDirectory, i.Name)))
                        {
                            //目标目录下不存在此文件夹即创建子文件夹
                            Directory.CreateDirectory(Path.Combine(targetDirectory, i.Name));
                        }

                        //递归调用复制子文件夹
                        DirectoryMove(i.FullName, Path.Combine(targetDirectory, i.Name), overwriteexisting);
                    }
                    else
                    {
                        if (i.Extension != ".meta")
                        {
                            //不是文件夹即复制文件，true表示可以覆盖同名文件
                            File.Move(i.FullName, Path.Combine(targetDirectory, i.Name));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"copy dir raise error {ex.ToString()}");
                return false;
            }

            return true;
        }

        public static bool MoveFile(string source, string tar)
        {
            Debug.Log($"Copy file from {source} to {tar}...");
            if (File.Exists(source))
            {
                FileInfo file = new FileInfo(tar);
                file.MoveTo(tar);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 运行命令
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <param name="args">命令的参数</param>
        /// <returns>string[] res[0]命令的stdout输出, res[1]命令的stderr输出</returns>
        public static string[] RunCmd(string cmd, string args, string workingDir = "")
        {
            string[] res = new string[2];
            var p = CreateCmdProcess(cmd, args, workingDir);
            res[0] = p.StandardOutput.ReadToEnd();
            if (res[0].Length > 0)
            {
                Debug.Log(res[0]);
            }

            res[1] = p.StandardError.ReadToEnd();
            if (res[1].Length > 0)
            {
                Debug.LogError(res[1]);
            }

            p.Close();
            return res;
        }


        /// <summary>
        /// 构建Process对象，并执行
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <param name="args">命令的参数</param>
        /// <param name="workingDir">工作目录</param>
        /// <returns>Process对象</returns>
        private static System.Diagnostics.Process CreateCmdProcess(string cmd, string args, string workingDir = "")
        {
            var en = System.Text.Encoding.UTF8;
            if (Application.platform == RuntimePlatform.WindowsEditor)
                en = System.Text.Encoding.GetEncoding("gb2312");

            var pStartInfo = new System.Diagnostics.ProcessStartInfo(cmd);
            pStartInfo.Arguments = args;
            pStartInfo.CreateNoWindow = false;
            pStartInfo.UseShellExecute = false;
            pStartInfo.RedirectStandardError = true;
            pStartInfo.RedirectStandardInput = true;
            pStartInfo.RedirectStandardOutput = true;
            pStartInfo.StandardErrorEncoding = en;
            pStartInfo.StandardOutputEncoding = en;
            if (!string.IsNullOrEmpty(workingDir))
                pStartInfo.WorkingDirectory = workingDir;
            return System.Diagnostics.Process.Start(pStartInfo);
        }
    }
}