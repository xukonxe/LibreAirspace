using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using WTRev.TKTLib.Modding.Interface;

namespace WTRev.TKTLib.Modding.InfoCls {
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public sealed class WTRevModMainClassAttribute : Attribute { }

	public static class ModFileTypeConstant {
		public const string TEXT = "TEXT";

		public const string BINARY = "BINARY";

		public const string SCRIPT_CSHARP = "SCRIPT_CSHARP";

		public const string SCRIPT_CSHARP_CLIENT_SIDE_ONLY = "SCRIPT_CSHARP_CLIENT_SIDE_ONLY";

		public const string SCRIPT_CSHARP_SERVER_SIDE_ONLY = "SCRIPT_CSHARP_SERVER_SIDE_ONLY";
	}

	[JsonObject]
	public sealed class ModFileInfo {
		[JsonProperty(Required = Required.Always, PropertyName = "Path")]
		private string _path;
		[JsonProperty(Required = Required.Always, PropertyName = "Type")]
		private string _type;
		[JsonProperty(Required = Required.Default, PropertyName = "MetaData")]
		private object _metadata;

		/// <summary>
		/// 文件的路径
		/// </summary>
		[JsonIgnore]
		public string Path { get => this._path; }

		/// <summary>
		/// 文件的类型(具体的值的意义可以参考 <see cref="WTRev.TKTLib.Modding.InfoCls.ModFileTypeConstant"/>)
		/// </summary>
		[JsonIgnore]
		public string Type { get => this._type; }

		/// <summary>
		/// 文件的元数据
		/// </summary>
		[JsonIgnore]
		public object MetaData { get => this._metadata; }

		/// <summary>
		/// 文件的内容（二进制数据）
		/// </summary>
		[JsonIgnore]
		public byte[] Content { get; internal set; }
	}

	[JsonObject]
	public class ModInfo {
		[JsonProperty(Required = Required.Always, PropertyName = "Name")]
		private string _name;
		[JsonProperty(Required = Required.Always, PropertyName = "Description")]
		private string _description;
		[JsonProperty(Required = Required.Always, PropertyName = "Author")]
		private string _author;
		[JsonProperty(Required = Required.Always, PropertyName = "Version")]
		private string _version;
		[JsonProperty(Required = Required.Always, PropertyName = "Guid")]
		private string _guid;
		[JsonProperty(Required = Required.Always, PropertyName = "FileTable")]
		private ModFileInfo[] _filetable;

		/// <summary>
		/// 模组包的Sha512校验和。
		/// </summary>
		[JsonIgnore]
		public byte[] m_ModPackSha512Sum;

		/// <summary>
		/// 模组的一般名称
		/// </summary>
		[JsonIgnore]
		public string Name { get => this._name; }

		/// <summary>
		/// 模组的描述
		/// </summary>
		[JsonIgnore]
		public string Description { get => this._description; }

		/// <summary>
		/// 模组的作者
		/// </summary>
		[JsonIgnore]
		public string Author { get => this._author; }

		/// <summary>
		/// 模组的版本
		/// </summary>
		[JsonIgnore]
		public string Version { get => this._version; }

		/// <summary>
		/// 模组的GUID标识符
		/// </summary>
		[JsonIgnore]
		public string Guid { get => this._guid; }

		/// <summary>
		/// 当前的模组信息类实例是否是已加载的模组？
		/// </summary>
		[JsonIgnore]
		public bool IsInstance { get; internal set; }

		#region 大概是只在此对象为对模组实例时才能正常使用的属性
		/// <summary>
		/// 模组包的文件表
		/// </summary>
		[JsonIgnore]
		public ModFileInfo[] FileTable { get => this._filetable; }

		/// <summary>
		/// 对模组的行为类的引用
		/// </summary>
		[JsonIgnore]
		public IModBehaviour ModBehaviour { get; internal set; }
		#endregion

		//TKTek Note @ 2024/09/02 19:52 CST:
		//TODO: 还有一些文件签名等ModInfo类的属性需要被设计，编写好。。。

		public ModInfo() {
			this.IsInstance = false;
		}
	}

	public enum ModOperateResult : uint {
		//这个位域被分为四个字段(首先，计算机计数从0起，0-3位为基础状态，4-15位为致命错误，16-23位为非致命错误, 24-31位保留未来用途, 不使用)
		//00000000
		//基础状态
		成功 = 0b00000000_00000000_000000000000_0000,
		失败 = 0b00000000_00000000_000000000000_0001,

		//致命错误
		读取Zip归档失败 = (1 << 4) | 失败,
		元数据解析失败 = (2 << 4) | 失败,
		编译脚本失败 = (3 << 4) | 失败,
		缺少模组主类 = (4 << 4) | 失败,
		没有元数据 = (5 << 4) | 失败,

		//非致命错误
		缺失资产文件 = 1 << 16,
	}
}
