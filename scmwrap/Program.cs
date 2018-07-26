using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Xml;
using Microsoft.Win32;

namespace SettingContent
{
	class Program
	{
		[DllImport("shell32.dll", EntryPoint = "ShellExecute")]
		public static extern long ShellExecute(int hwnd, string cmd, string file, string param1, string param2, int swmode);
		
		[DllImport("kernel32.dll")]
		public static extern IntPtr GetConsoleWindow();
		
		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
		
		const int SW_HIDE = 0;
		const int SW_SHOW = 5;
		
		public static void Main(string[] args)
		{
			if(args.Length > 0)
			{
				if(args[0].Equals("-install"))
				{
					if(IsAdministrator())
					{
						SetAssociation(".SettingContent-ms", "NewSettingContentMs", System.Reflection.Assembly.GetEntryAssembly().Location);
						Console.WriteLine("Successfully create .SettingContent-ms extension association");
					} else
					{
						Console.WriteLine("Error, requires Administrator privileges");
					}
				} else
				{
					ShowWindow(GetConsoleWindow(), SW_HIDE);

					XmlDocument XmlDoc = new XmlDocument();
					
					XmlDoc.Load(args[0]);
					
					bool FoundDeepLink = false;
					
					XmlNodeList Nodes = XmlDoc.GetElementsByTagName("DeepLink");
		            for (int i = 0; i <= Nodes.Count - 1; i++)
		            {
		            	ShellExecute (0, "open", "cmd.exe", "/C " + Nodes[i].InnerText.Replace("\n","").Replace("\r", ""), "", 0);
		            	
		            	Console.WriteLine("Call: " + Nodes[i].InnerText.Replace("\n","").Replace("\r", ""));
		            	
		            	FoundDeepLink = true;
		            }
		            
		            if(!FoundDeepLink)
		            {
		            	Console.WriteLine("No deep link found");
		            }
		            
		            Console.ReadLine();
				}
			} else
			{
				Console.WriteLine("Missing command line argument");
				Console.WriteLine("scmwrap.exe -install");
				Console.WriteLine("scmwrap.exe file.settingscontent-ms");
			}
		}
	
		/// <summary>
		/// Set default file open associations for .fileext and OpenWith
		/// </summary>
		/// <param name="Extension"></param>
		/// <param name="KeyName"></param>
		/// <param name="OpenWith">Application Path</param>
		public static void SetAssociation(string Extension, String KeyName, string OpenWith)
		{
		   String Bat = "FTYPE " + KeyName + "=\"" + OpenWith + "\" \"%%1\"\r\n" +
		   	"ASSOC " + Extension + "=" + KeyName;
		   
		   const String TmpBat = "tmp.bat";
		   
		   File.WriteAllText(TmpBat, Bat);
		   
		   Process.Start(TmpBat).WaitForExit();
		   
		   File.Delete(TmpBat);  
		}
			
	
		
		/// <summary>
		/// Check admin status.
		/// </summary>
		/// <returns></returns>
		public static bool IsAdministrator()
		{
		    WindowsIdentity Identity = WindowsIdentity.GetCurrent();
		    WindowsPrincipal Principal = new WindowsPrincipal(Identity);
		    return Principal.IsInRole(WindowsBuiltInRole.Administrator);
		}
	}
}