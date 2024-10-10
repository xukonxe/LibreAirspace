using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using WTRev.TKTLib.Modding.ModManager;
using static TGZG.公共空间;
using static 战雷革命.公共空间;

namespace 战雷革命 {
	public static partial class 公共空间 {
		//==========端口定义=========== 
		//16312:服务器<=>房间服务器
		//16313:客户端<=>房间服务器
		//16314:客户端<=>服务器
		//============================

		//16312:服务器 <=> 房间服务器 <=> 客户端
		//         ^ 16312     16313 ^
		//        ||      16314      ||
		//        ||=================||

		//============================
		//public static 房间服务器信道类 信道_房间列表 = new("127.0.0.1:16313", 版本);
		//public static 房间服务器信道类 信道_房间列表 = new("47.97.112.35:16313", 版本);
		//public static 游戏服务器信道类 信道_游戏 = new("");
		public static ushort 服务器端口 => 16314;
		public static ushort 房间列表端口 => 16313;
		public static 房间服务器信道类 信道_房间列表;
		public static 游戏服务器信道类 信道_游戏;
		public static string 版本 => 主程序类.版本;
		public static string 发布日期 => 主程序类.发布日期;

		public static 自由空域主程序 主程序类 => _主程序类缓存 ?? (_主程序类缓存 = GameObject.Find("Main").GetComponent<自由空域主程序>());
		public static ModManager 模组管理器类;        //这个字段的值由 模组系统初始化.cs 源代码文件里的程序设置。
		private static 自由空域主程序 _主程序类缓存;
	}
	//程序入口物体应添加跨场景脚本，以便在多场景间切换时保持生命周期
	public class 自由空域主程序 : MonoBehaviour {
		public string 版本;
		public string 发布日期;
		void Start() {
			模组系统初始化();

			信道_房间列表 = new($"47.97.112.35:{房间列表端口}", 版本);
			//信道_房间列表 = new($"127.0.0.1:{房间列表端口}", 版本);
			信道_游戏 = new(版本);
			初始化主线程();
			初始化每帧();
			切换场景("主界面");
		}
	}
}
