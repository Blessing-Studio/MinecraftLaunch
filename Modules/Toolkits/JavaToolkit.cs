using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using MinecraftLaunch.Modules.Models.Launch;

namespace MinecraftLaunch.Modules.Toolkits;

public sealed class JavaToolkit
{
	public static JavaInfo GetJavaInfo(string javapath)
	{
	    FileInfo info = new(javapath);
		try
		{
			int? ires = null;
			string tempinfo = null;
			string pattern = "java version \"\\s*(?<version>\\S+)\\s*\"";

			using Process Program = new Process
			{
				StartInfo = new()
				{
                    Arguments = "-version",
                    FileName = javapath.EndsWith(".exe") ? Path.Combine(info.Directory!.FullName, "java.exe") : info.FullName,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

			Program.Start();
			Program.WaitForExit(8000);
			StreamReader res = Program.StandardError;
			bool end = false;
			while (res.Peek() != -1)
			{
				string temp = res.ReadLine();
				if (temp.Contains("java version"))
                    tempinfo = new Regex(pattern).Match(temp).Groups["version"].Value;
                else if (temp.Contains("openjdk version"))
				{
					pattern = pattern.Replace("java", "openjdk");
					tempinfo = new Regex(pattern).Match(temp).Groups["version"].Value;
				}
				else if (temp.Contains("64-Bit"))
                    end = true;
            }

            string[] sres = tempinfo.Split(".");
			if (sres.Length != 0)
                ires = ((int.Parse(sres[0]) == 1) ? new int?(int.Parse(sres[1])) : new int?(int.Parse(sres[0])));

            return new JavaInfo
			{
				Is64Bit = end,
				JavaDirectoryPath = info.Directory!.FullName,
				JavaSlugVersion = Convert.ToInt32(ires),
				JavaVersion = tempinfo,
				JavaPath = info.FullName,
			};
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static IEnumerable<JavaInfo> GetJavas(bool Isallsearch = true)
	{
		List<string> temp = new();
        List<JavaInfo> ret = new();

		try
		{
		    if (Isallsearch) {
                foreach (var item in DriveInfo.GetDrives().AsParallel()) {               
                    temp.AddRange(addSubDirectory(new DirectoryInfo(item.Name), "javaw.exe").Where(File.Exists));
                }

                GC.Collect();

				foreach (var i in temp.AsParallel()) {				
                    ret.Add(GetJavaInfo(i));
                }
            }
		}catch(Exception) { }

		return ret;
    }

    public static JavaInfo GetCorrectOfGameJava(IEnumerable<JavaInfo> Javas, GameCore gameCore)
	{
		JavaInfo res = null;
		foreach (JavaInfo j in Javas)
		{
			if (j.JavaSlugVersion == gameCore.JavaVersion && j.Is64Bit)
			{
				res = j;
			}
		}
		if (res == null)
		{
			foreach (JavaInfo i in Javas)
			{
				if (i.JavaSlugVersion == gameCore.JavaVersion)
				{
					res = i;
				}
			}
			return res;
		}
		return res;
	}

    static List<string> addSubDirectory(DirectoryInfo directory, string pattern)
    {
		List<string> files = new List<string>();
        try
        {
            foreach (FileInfo fi in directory.GetFiles(pattern).AsParallel()) {
				files.Add(fi.FullName);
			}

            foreach (DirectoryInfo di in directory.GetDirectories().AsParallel()) {           
                addSubDirectory(di, pattern);
            }
        }
		catch 
		{
		}
		finally
		{
			GC.Collect();
		}

		return files;	
    }
}
