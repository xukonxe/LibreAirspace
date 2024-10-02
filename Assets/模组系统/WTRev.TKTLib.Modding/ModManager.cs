using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WTRev.TKTLib.Modding.CSC;
using WTRev.TKTLib.Modding.InfoCls;
using WTRev.TKTLib.Modding.Interface;

/*
 * 一些程序设计文档:
 * 
 * 为什么要编写游戏的这一个部分？
 *   为了游戏的可自定义性。（Modding！还有虽然游戏的Unity项目是开源的，但是玩家想做模组得注册Unity账号，
 *     下载+安装上10G大小的Unity Hub/Unity Editor，这对玩家来说有点不太友好。）
 *   和游戏的热更新，（以后大概不能每一次更新都
 *     给玩家推送一次2位数~4位数兆字节不等大小的客户端文件吧）
 * 
 * 一些概念的定义:
 *   模组: 受本游戏的模组框架支持的游戏修改程序(及其资产)，
 *     模组的文件一般由"模组描述"元数据文档和C#程序文件和各种模组的资产文件组成。
 *   模组包: 模组的文件树以ZIP格式打包后的产物
 *   模组描述: 模组/模组包的元数据，以JSON格式存储。
 * 
 * 这个类库里面的各个类的一些描述
 *   ModManager类用于管理模组的加载, 卸载
 *   
 *   IModEventCallback接口是模组各类事件的回调程序的抽象接口类
 *   IModBehaviour接口是模组的"行为"类的抽象接口类（提一嘴，游戏加载模组文件到内存后，
 *     游戏调用模组里带WTRevModMainClassAttribute特性且继承了IModBehaviour接口的类的PostLoad方法，可以在这里初始化模组）
 *   
 *   WTRevModMainClassAttribute特性类用于标注模组中的程序中的一个类是模组的主程序类。
 *   ModInfo类是一个用于描述模组信息的类
 *   ModPackage类是"加载后的模组包"什么的东西的类
 *   ModDescriptor类是模组的"描述"元数据信息的类
 */
namespace WTRev.TKTLib.Modding.ModManager {

	public sealed class ModManager {
		private WTRCSC m_WTRCSC;

		private List<ModInfo> m_LoadedMods;

		private bool m_IsServerSide;

		public ModManager(bool isServerSide) {
			this.m_LoadedMods = new List<ModInfo>(64);      //嗯，硬编码预分配内存什么的。。。
			this.m_WTRCSC = new WTRCSC(GetAssemblyReference());
			this.m_IsServerSide = isServerSide;

			//这里做一下已加载程序集的日志。
			StringBuilder _mesgSB = new StringBuilder(65535);
			_mesgSB.AppendLine("Loaded Assemblies");
			foreach (Assembly _assembly in AppDomain.CurrentDomain.GetAssemblies()) {
				try {
					string _assemblyInfo = $"{_assembly.GetName().Name}                {_assembly.Location}";
					_mesgSB.AppendLine(_assemblyInfo);
				} catch {
					_mesgSB.AppendLine($"Unable to get assembly position for {_assembly.GetName().Name}");
					//Do nothing?
				}
			}
			TGZG.公共空间.Log(_mesgSB.ToString());
		}

		//这个B获取程序集引用的程序，会导致在编辑器下面使用模组系统会编译脚本失败！
		private IEnumerable<IEnumerable<byte>> GetAssemblyReference() {
			//在Unity的环境下？
#if TKTEK_DEVEL_MODSYSTEM_UNITY_MONO
			return
#if UNITY_EDITOR || UNITY_EDITOR_64 || UNITY_EDITOR_WIN
				this.GetAssemblyReferenceUnityEditor();
#elif UNITY_STANDALONE
				Directory.GetFiles(Path.GetDirectoryName(typeof(object).Assembly.Location))
					.Where(_filePath => Regex.IsMatch(_filePath, @"\.(dll|exe)$"))    /* TODO: 跨平台! 这个逻辑似乎只在Windows下能正常运行! */
					.Select(File.ReadAllBytes)
#else
				null
#endif
				;
#elif TKTEK_DEVEL_MODSYSTEM_DOTNET_CORE
			//如果是在.NET Core下面呢？
			foreach (Assembly _assembly in AppDomain.CurrentDomain.GetAssemblies()) {
				string _assemblyLocation = null;
				try {
					_assemblyLocation = _assembly.Location;
				} catch {
					//Do nothing?
				}
				if (!string.IsNullOrWhiteSpace(_assemblyLocation)) {
					yield return File.ReadAllBytes(_assemblyLocation);
				}
			}
#endif
		}

		//Hard-coded .NET assembly list!!
		private static readonly string[] ASM_LIST = {
			"Assembly-CSharp", "Dracodec", "Humanizer", "Microsoft.Bcl.AsyncInterfaces", "Microsoft.CodeAnalysis.CSharp", "Microsoft.CodeAnalysis", "Microsoft.CSharp", "Mono.Security", "mscorlib", "netstandard", "Newtonsoft.Json",
			"Siccity.GLTFUtility", "System.Buffers", "System.Collections.Immutable", "System.ComponentModel.Composition", "System.Composition.AttributedModel", "System.Composition.Convention", "System.Composition.Hosting", "System.Composition.Runtime",
			"System.Composition.TypedParts", "System.Configuration", "System.Core", "System.Data.DataSetExtensions", "System.Data", "System", "System.Drawing", "System.EnterpriseServices", "System.IO.Compression", "System.IO.Compression.FileSystem",
			"System.IO.Pipelines", "System.Memory", "System.Net.Http", "System.Numerics", "System.Reflection.Metadata", "System.Runtime.CompilerServices.Unsafe", "System.Runtime.Serialization", "System.Security", "System.ServiceModel.Internals",
			"System.Text.Encoding.CodePages", "System.Threading.Channels", "System.Threading.Tasks.Extensions", "System.Transactions", "System.Xml", "System.Xml.Linq", "TouchSocket", "Unity.Addressables", "Unity.ResourceManager", "Unity.ScriptableBuildPipeline",
			"Unity.TextMeshPro", "Unity.Timeline", "Unity.VisualScripting.Antlr3.Runtime", "Unity.VisualScripting.Core", "Unity.VisualScripting.Flow", "Unity.VisualScripting.State", "UnityEngine.AccessibilityModule", "UnityEngine.AIModule", "UnityEngine.AndroidJNIModule",
			"UnityEngine.AnimationModule", "UnityEngine.ARModule", "UnityEngine.AssetBundleModule", "UnityEngine.AudioModule", "UnityEngine.AutoStreamingModule", "UnityEngine.ClothModule", "UnityEngine.ClusterInputModule", "UnityEngine.ClusterRendererModule",
			"UnityEngine.CoreModule", "UnityEngine.CrashReportingModule", "UnityEngine.DirectorModule", "UnityEngine", "UnityEngine.DSPGraphModule", "UnityEngine.GameCenterModule", "UnityEngine.GIModule", "UnityEngine.GridModule", "UnityEngine.HotReloadModule",
			"UnityEngine.ImageConversionModule", "UnityEngine.IMGUIModule", "UnityEngine.InputLegacyModule", "UnityEngine.InputModule", "UnityEngine.JSONSerializeModule", "UnityEngine.LocalizationModule", "UnityEngine.NVIDIAModule", "UnityEngine.ParticleSystemModule",
			"UnityEngine.PerformanceReportingModule", "UnityEngine.Physics2DModule", "UnityEngine.PhysicsModule", "UnityEngine.ProfilerModule", "UnityEngine.RuntimeInitializeOnLoadManagerInitializerModule", "UnityEngine.ScreenCaptureModule", "UnityEngine.SharedInternalsModule",
			"UnityEngine.SpriteMaskModule", "UnityEngine.SpriteShapeModule", "UnityEngine.StreamingModule", "UnityEngine.SubstanceModule", "UnityEngine.SubsystemsModule", "UnityEngine.TerrainModule", "UnityEngine.TerrainPhysicsModule", "UnityEngine.TextCoreFontEngineModule",
			"UnityEngine.TextCoreTextEngineModule", "UnityEngine.TextRenderingModule", "UnityEngine.TilemapModule", "UnityEngine.TLSModule", "UnityEngine.UI", "UnityEngine.UIElementsModule", "UnityEngine.UIElementsNativeModule", "UnityEngine.UIModule",
			"UnityEngine.UmbraModule", "UnityEngine.UNETModule", "UnityEngine.UnityAnalyticsModule", "UnityEngine.UnityConnectModule", "UnityEngine.UnityCurlModule", "UnityEngine.UnityTestProtocolModule", "UnityEngine.UnityWebRequestAssetBundleModule",
			"UnityEngine.UnityWebRequestAudioModule", "UnityEngine.UnityWebRequestModule", "UnityEngine.UnityWebRequestTextureModule", "UnityEngine.UnityWebRequestWWWModule", "UnityEngine.VehiclesModule", "UnityEngine.VFXModule", "UnityEngine.VideoModule",
			"UnityEngine.VirtualTexturingModule", "UnityEngine.VRModule", "UnityEngine.WindModule", "UnityEngine.XRModule"
		};

		private IEnumerable<IEnumerable<byte>> GetAssemblyReferenceUnityEditor() {
			StringBuilder _MesgBuilder = new StringBuilder(65535);
			_MesgBuilder.AppendLine("Assembly references for mod - unity editor");
			foreach (Assembly _assembly in AppDomain.CurrentDomain.GetAssemblies()) {
				AssemblyName _aName = _assembly.GetName();
				string _aPath = null;
				if (ASM_LIST.Contains(_aName.Name)) {
					try {
						_aPath = _assembly.Location;
					} catch {
						_MesgBuilder.AppendLine($"Unable to resolve .net assembly path for {_aName.Name}");
					}
					if (!string.IsNullOrEmpty(_aPath)) {
						yield return File.ReadAllBytes(_aPath);
					}
				}
			}
			TGZG.公共空间.Log(_MesgBuilder.ToString());
        }

		public IReadOnlyList<ModInfo> GetLoadedMods() => this.m_LoadedMods;

		public ModOperateResult LoadMod(Stream modPackageByteStream) {
			ZipArchive _zipArchive = null;
			MemoryStream _modPackageByteStream_Mem = new MemoryStream(1024 * 1024 * 64 /*预分配64MB内存*/);
			modPackageByteStream.CopyTo(_modPackageByteStream_Mem);
			try {
				//这里可能会卡住很长一段时间，这个方法应该被设计成是异步执行的。
				_zipArchive = new ZipArchive(_modPackageByteStream_Mem, ZipArchiveMode.Read, true, Encoding.UTF8);
			} catch (InvalidDataException) { 
				return ModOperateResult.读取Zip归档失败;
			}
			(ModOperateResult, ModInfo) _ModPackageLoadResult = this.LoadModPackage(_zipArchive, false);
			if ((_ModPackageLoadResult.Item1 & ModOperateResult.失败) == 0 && null != _ModPackageLoadResult.Item2) {
				//计算SHA512校验和
				_modPackageByteStream_Mem.Position = 0;
				_ModPackageLoadResult.Item2.m_ModPackSha512Sum = System.Security.Cryptography.SHA512.Create().ComputeHash(_modPackageByteStream_Mem);
				//编译好脚本！
				string _ScriptsToCompileAtThisSide = this.m_IsServerSide ?
					ModFileTypeConstant.SCRIPT_CSHARP_SERVER_SIDE_ONLY : ModFileTypeConstant.SCRIPT_CSHARP_CLIENT_SIDE_ONLY;
				(ModOperateResult, IEnumerable<Diagnostic>, byte[]) _CompileResult = this.m_WTRCSC.CompileScriptSet(
					_ModPackageLoadResult.Item2.FileTable
					.Where(_fileEntry => _fileEntry.Type == ModFileTypeConstant.SCRIPT_CSHARP ||
						_fileEntry.Type == _ScriptsToCompileAtThisSide)
					.Select(_fileEntry => Encoding.UTF8.GetString(_fileEntry.Content)));
				//编译失败!
				if ((_CompileResult.Item1 & ModOperateResult.失败) != 0) {
					//输出编译诊断信息
					StringBuilder _MesgSB = new StringBuilder(65535);
					_MesgSB.AppendLine("Compiling for script assembly failed.");
					foreach (Diagnostic _DiagInfo in _CompileResult.Item2) {
						_MesgSB.AppendLine(_DiagInfo.ToString());
					}
					TGZG.公共空间.Log(_MesgSB.ToString());
					return _CompileResult.Item1;
				}
				this.m_LoadedMods.Add(_ModPackageLoadResult.Item2);
				//加载程序集，调用模组的回调函数
				Assembly _loadedAssembly = Assembly.Load(_CompileResult.Item3);
				Type _entryType = _loadedAssembly.GetTypes()
					.FirstOrDefault(_type => null != _type.GetCustomAttribute<WTRevModMainClassAttribute>() && _type.GetInterfaces().Contains(typeof(IModBehaviour)));
				if (null == _entryType) {
					return ModOperateResult.缺少模组主类;
				}
				IModBehaviour _modCb = _ModPackageLoadResult.Item2.ModBehaviour = Activator.CreateInstance(_entryType) as IModBehaviour;
				_modCb.PostLoad(_ModPackageLoadResult.Item2);
				return ModOperateResult.成功;
			}
			return _ModPackageLoadResult.Item1;
		}

		/// <summary>
		/// 加载模组包（仅进行基础的模组实例对象创建和加载资产文件）
		/// </summary>
		/// <param name="modPackageZipArchive">模组包的Zip归档对象</param>
		/// <param name="readMetadataOnly">是否仅读取模组包的元数据？</param>
		/// <returns>成功则返回创建的模组实例对象，失败则返回空引用</returns>
		public (ModOperateResult, ModInfo) LoadModPackage(ZipArchive modPackageZipArchive, bool readMetadataOnly) {
			ModInfo _modInstance = null;
			ModOperateResult _错误代码 = default;

			//此处读取元数据文件
			ZipArchiveEntry _metaDataFile = modPackageZipArchive.Entries.FirstOrDefault(_entry => _entry.FullName == "+WTREV_MOD_METADATA");
			Stream _metaDataFileStream = null;
			StreamReader _metaDataFileStreamReader = null;
			string _metadataFileContent = null;
			if (null == _metaDataFile) {
				return (ModOperateResult.没有元数据, null);
			}
			try {
				//此处读取&解析元数据文档!
				_metaDataFileStream = _metaDataFile.Open();
				_metaDataFileStreamReader = new StreamReader(_metaDataFileStream);
				_metadataFileContent = _metaDataFileStreamReader.ReadToEnd();
				_modInstance = JsonConvert.DeserializeObject<ModInfo>(_metadataFileContent);
			} catch (JsonException) {
				//解析失败!
				_错误代码 = ModOperateResult.元数据解析失败;
				return (_错误代码, null);
			} finally {
				_metaDataFileStreamReader.Close();
				_metaDataFileStream.Close();
			}
			if (readMetadataOnly) {
				_错误代码 = ModOperateResult.成功;
				return (_错误代码, _modInstance);
			} else {
				//在Zip归档中查找元数据中有记录的Zip归档文件项。。。
				//此处加载模组包的模组需要的各个资产文件
				List<ModFileInfo> _ModFileNotResolved = new List<ModFileInfo>(_modInstance.FileTable);          //嗯，硬编码预分配内存什么的。。。
				foreach (ZipArchiveEntry _zipArEnt in modPackageZipArchive.Entries) {
					ModFileInfo _modFileMatched = _ModFileNotResolved.FirstOrDefault(_modFileRequired => _modFileRequired.Path == _zipArEnt.FullName);
					//如果这个块没有被执行到，那么模组需要的一个文件项就读取失败了！
					if (null != _modFileMatched) {
						using (Stream _zipArFileEntStream = _zipArEnt.Open()) {
							byte[] _buffer = new byte[_zipArEnt.Length];
							//靠，这样处理4GB以上的文件就会出问题了！
							checked {
								_zipArFileEntStream.Read(_buffer, 0, (int)_zipArEnt.Length);
							}
							_modFileMatched.Content = _buffer;
						}
						_ModFileNotResolved.Remove(_modFileMatched);
					}
				}
				//如果，还是有没有被读取到的模组资产文件。。。
				//那么应该有些文件读取失败了！
				if (_ModFileNotResolved.Count != 0) {
					_错误代码 = ModOperateResult.缺失资产文件;
				}
				_错误代码 = ModOperateResult.成功;
				_modInstance.IsInstance = true;
				return (_错误代码, _modInstance);
			}
		}
	}
}

namespace WTRev.TKTLib.Modding.Interface {
	public interface IModEventCallback {
		/// <summary>
		/// 模组已加载事件回调函数
		/// </summary>
		/// <param name="argument">"通用参数", 当前一般传对 <see cref="WTRev.TKTLib.Modding.InfoCls.ModInfo"/> 类的引用</param>
		void PostLoad(object argument);

		/// <summary>
		/// 模组预卸载事件回调函数。
		/// </summary>
		/// <param name="argument">"通用参数", 当前未使用，传空引用</param>
		void PreUnload(object argument);
	}

	public interface IModBehaviour : IModEventCallback { }
}
