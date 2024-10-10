using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using WTRev.TKTLib.Modding.InfoCls;
using WTRev.TKTLib.Modding.ModManager;
using static TGZG.公共空间;
using static 战雷革命.公共空间;

namespace 战雷革命 {
	public static partial class 公共空间 { 
		public static void 模组系统初始化() {
			模组管理器类 = new ModManager();
			加载所有模组();
		}

		private static void 加载所有模组() {
			DirectoryInfo _ModDirectory = new DirectoryInfo(Path.Join(Application.persistentDataPath, "Mod"));
			if (!_ModDirectory.Exists)
				return;
			IEnumerable<FileSystemInfo> _FSObjInModDirectory = _ModDirectory.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly);
			List<FileInfo> _ModPackFiles = new List<FileInfo>(64);			//嗯，硬编码预分配内存大小。
			_ModPackFiles.AddRange(from _FSObject in _FSObjInModDirectory
									   where _FSObject is FileInfo && Regex.IsMatch(_FSObject.Name, @"\.zip$")
									   select _FSObject as FileInfo);
			//应该显示一个加载模组中的屏幕！不然这个操作是在主线程上进行的，会卡住游戏！
			//应该有加载模组错误的提示！
			foreach (FileInfo _ModPackFile in _ModPackFiles) {
				FileStream _ModPackFileStream = null;
				try {
					//打开模组包文件
					_ModPackFileStream = _ModPackFile.OpenRead();
					//加载模组。
					ModOperateResult _Result = 模组管理器类.LoadMod(_ModPackFileStream);
					if ((_Result & ModOperateResult.失败) != 0) {
						//如果有失败标志
						Debug.LogError($"加载模组包文件\"{_ModPackFile.FullName}\"失败！错误值: {string.Format("0x{0:X16}", (ulong)_Result)}");
					}
				} finally {
					_ModPackFileStream.Close();
				}
			}
		}
	}
}