using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using UI.Utitlities;

namespace NugetHelper
{
	public class MainWindowViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		public void NotifyChanged([CallerMemberName] string propName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}

		private string mCurrentVersion;
		public string CurrentVersion
		{
			get { return mCurrentVersion; }
			set { mCurrentVersion = value; NotifyChanged(); }
		}

		private string mTargetVersion;
		public string TargetVersion
		{
			get { return mTargetVersion; }
			set { mTargetVersion = value; NotifyChanged(); }
		}


		private string mSelectedSLN;
		public string SelectedSLN
		{
			get { return mSelectedSLN; }
			set { mSelectedSLN = value; NotifyChanged(); }
		}

		private string mSelectedSLNPath;
		public string SelectedSLNPath
		{
			get { return mSelectedSLNPath; }
			set { mSelectedSLNPath = value; NotifyChanged(); }
		}

		private ObservableCollection<string> mAvailableSLNs;
		public ObservableCollection<string> AvailableSLNs
		{
			get { return mAvailableSLNs; }
			set { mAvailableSLNs = value; NotifyChanged(); }
		}


		private ICommand mApplyCommand;
		public ICommand ApplyCommand => mApplyCommand ??
			(mApplyCommand = new RelayCommand((o) => Apply()));

		private void Apply()
		{
			try
			{
				var doc = new XmlDocument();
				doc.Load(GetBuildPropsPath());

				// 找到 MulanPlatformVersion 节点并修改
				var node = doc.SelectSingleNode("//MulanPlatformVersion");
				if (node == null) throw new Exception("未找到 <MulanPlatformVersion> 节点");

				doc.Save(GetBuildPropsPath());

				MessageBox.Show("版本号已更新到：" + node.InnerText, "成功", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show("出错：" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
			}

		}

		private string GetBuildPropsPath(string slnPath = "")
		{
			return "";
		}

		private ICommand mDetectCommand;
		public ICommand DetectCommand => mDetectCommand ??
			(mDetectCommand = new RelayCommand((o) => Detect()));

		private void Detect()
		{
			// 你可以根据实际用的 IDE/进程名调整这个列表
			var candidates = Process.GetProcessesByName("devenv")
						  .Concat(Process.GetProcessesByName("dotnet"));

			string foundSln = null;
			foreach (var p in candidates)
			{
				try
				{
					var cmd = GetCommandLine(p.Id);
					// 用正则找 .sln 完整路径
					var m = Regex.Match(cmd, @"[A-Za-z]\:\\[^\s""]+\.sln", RegexOptions.IgnoreCase);
					if (m.Success)
					{
						foundSln = m.Value.Trim('"');
						AvailableSLNs.Add(foundSln);
					}
				}
				catch { /* 无权限或读不到就跳过 */ }
			}
			SelectedSLN = AvailableSLNs.FirstOrDefault();

			if (foundSln == null)
			{
				MessageBox.Show("未找到打开的 .sln 进程。", "探测失败", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}
		}


		string GetCommandLine(int pid)
		{
			using var searcher = new ManagementObjectSearcher(
				$"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {pid}"
			);
			foreach (ManagementObject mo in searcher.Get())
			{
				return mo["CommandLine"]?.ToString() ?? "";
			}
			return "";
		}

		string FindUpwards(string startDir, string fileName)
		{
			var dir = new DirectoryInfo(startDir);
			while (dir != null)
			{
				var candidate = Path.Combine(dir.FullName, fileName);
				if (File.Exists(candidate)) return candidate;
				dir = dir.Parent;
			}
			return null;
		}

		string ResolveNugetCache(string slnFullPath)
		{
			var slnDir = Path.GetDirectoryName(slnFullPath);
			// 1) 找 nuget.config
			var config = FindUpwards(slnDir, "nuget.config");
			if (config != null)
			{
				var doc = new XmlDocument();
				doc.Load(config);
				var node = doc.SelectSingleNode("//add[@key='globalPackagesFolder']");
				if (node?.Attributes["value"] != null)
				{
					var val = node.Attributes["value"].Value;
					// 相对路径或绝对路径都支持
					if (!Path.IsPathRooted(val))
						return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(config), val));
					return val;
				}
			}
			// 2) 读进程环境或系统环境
			var env = Environment.GetEnvironmentVariable("NUGET_PACKAGES", EnvironmentVariableTarget.Process)
				  ?? Environment.GetEnvironmentVariable("NUGET_PACKAGES", EnvironmentVariableTarget.User)
				  ?? Environment.GetEnvironmentVariable("NUGET_PACKAGES", EnvironmentVariableTarget.Machine);
			if (!string.IsNullOrEmpty(env))
				return env;
			// 3) 默认路径
			return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
				".nuget", "packages"
			);
		}

		private ICommand mRestoreCommand;
		public ICommand RestoreCommand => mRestoreCommand ??
			(mRestoreCommand = new RelayCommand((o) => Restore()));

		private void Restore()
		{
			try
			{
				var psi = new ProcessStartInfo("dotnet", $"restore \"{mSelectedSLNPath}\"")
				{
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				};
				var proc = Process.Start(psi);
				string output = proc.StandardOutput.ReadToEnd();
				string err = proc.StandardError.ReadToEnd();
				proc.WaitForExit();

				if (proc.ExitCode == 0)
				{
					MessageBox.Show("Restore 成功！", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				else
				{
					MessageBox.Show("Restore 失败：\n" + err, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("执行失败：" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}
