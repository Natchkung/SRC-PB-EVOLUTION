using System;
using System.Diagnostics;
using System.IO;

namespace PointBlank.Server
{
	
	internal class Program
	{
		
		private static void Main(string[] args)
		{
			bool flag = false;
			bool flag2 = File.Exists("PointBlank.Auth.exe");
			if (flag2)
			{
				Process.Start("PointBlank.Auth.exe");
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Not Found PointBlank.Auth.exe");
			}
			bool flag3 = File.Exists("PointBlank.Battle.exe");
			if (flag3)
			{
				Process.Start("PointBlank.Battle.exe");
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Not Found PointBlank.Battle.exe");
			}
			bool flag4 = File.Exists("PointBlank.Game.exe");
			if (flag4)
			{
				Process.Start("PointBlank.Game.exe");
			}
			else
			{
				flag = true;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Not Found PointBlank.Game.exe");
			}
			bool flag5 = flag;
			if (flag5)
			{
				Process.GetCurrentProcess().WaitForExit();
			}
		}
	}
}
