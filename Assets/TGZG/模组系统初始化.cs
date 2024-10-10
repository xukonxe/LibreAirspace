using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using WTRev.TKTLib.Modding.InfoCls;
using WTRev.TKTLib.Modding.ModManager;
using static TGZG.�����ռ�;
using static ս�׸���.�����ռ�;

namespace ս�׸��� {
	public static partial class �����ռ� { 
		public static void ģ��ϵͳ��ʼ��() {
			ģ��������� = new ModManager();
			��������ģ��();
		}

		private static void ��������ģ��() {
			DirectoryInfo _ModDirectory = new DirectoryInfo(Path.Join(Application.persistentDataPath, "Mod"));
			if (!_ModDirectory.Exists)
				return;
			IEnumerable<FileSystemInfo> _FSObjInModDirectory = _ModDirectory.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly);
			List<FileInfo> _ModPackFiles = new List<FileInfo>(64);			//�ţ�Ӳ����Ԥ�����ڴ��С��
			_ModPackFiles.AddRange(from _FSObject in _FSObjInModDirectory
									   where _FSObject is FileInfo && Regex.IsMatch(_FSObject.Name, @"\.zip$")
									   select _FSObject as FileInfo);
			//Ӧ����ʾһ������ģ���е���Ļ����Ȼ��������������߳��Ͻ��еģ��Ῠס��Ϸ��
			//Ӧ���м���ģ��������ʾ��
			foreach (FileInfo _ModPackFile in _ModPackFiles) {
				FileStream _ModPackFileStream = null;
				try {
					//��ģ����ļ�
					_ModPackFileStream = _ModPackFile.OpenRead();
					//����ģ�顣
					ModOperateResult _Result = ģ���������.LoadMod(_ModPackFileStream);
					if ((_Result & ModOperateResult.ʧ��) != 0) {
						//�����ʧ�ܱ�־
						Debug.LogError($"����ģ����ļ�\"{_ModPackFile.FullName}\"ʧ�ܣ�����ֵ: {string.Format("0x{0:X16}", (ulong)_Result)}");
					}
				} finally {
					_ModPackFileStream.Close();
				}
			}
		}
	}
}