using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using UnityEngine;
using 战雷革命.Configuration.Object;

namespace 战雷革命.Configuration {
    namespace Object {
        //The root object in the JSON document.
        [JsonObject]
        public sealed class LibreAirspaceClientConfiguration {
            [JsonProperty(PropertyName = "KeyBind", Required = Required.Always)]
            internal KeyBind _KeyBind;

            [JsonProperty(PropertyName = "NetworkingConfig", Required = Required.Always)]
            internal NetworkingConfig _NetworkingConfig;

            [JsonIgnore]
            public KeyBind KeyBind { get => this._KeyBind; }

            [JsonIgnore]
            public NetworkingConfig NetworkingConfig { get => this._NetworkingConfig; }
        }

        [JsonObject]
        public sealed class KeyBind {

        }

        [JsonObject]
        public sealed class NetworkingConfig {
            [JsonProperty(PropertyName = "RoomListServerIp", Required = Required.Always)]
            internal string _RoomListServerIp;
            [JsonProperty(PropertyName = "RoomListServerTcpPort", Required = Required.Always)]
            internal ushort _RoomListServerTcpPort;
            [JsonProperty(PropertyName = "DefaultMatchServerKcpPort", Required = Required.Always)]
            internal ushort _DefaultMatchServerKcpPort;

            [JsonIgnore]
            internal IPAddress m_RoomLitServerIp_Resolved;

            [JsonIgnore]
            public IPAddress RoomListServerIp {
                get {
                    IPAddress _parsedIpAddress = null;
                    if (m_RoomLitServerIp_Resolved is null) {
                        if (IPAddress.TryParse(_RoomListServerIp, out _parsedIpAddress)) {
                            m_RoomLitServerIp_Resolved = _parsedIpAddress;
                        } else {
                            TGZG.公共空间.logerror("无法解析房间列表服务器IP地址，回滚至本地回环地址。");
                            m_RoomLitServerIp_Resolved = IPAddress.Loopback;
                        }
                    }
                    return m_RoomLitServerIp_Resolved;
                }
            }
            [JsonIgnore]
            public ushort RoomListServerTcpPort { get => _RoomListServerTcpPort; }
            [JsonIgnore]
            public ushort DefaultMatchServerKcpPort { get => _DefaultMatchServerKcpPort; }
        }
    }

    namespace Handler {

        public static class ClientConfigurationResolver {
            private static LibreAirspaceClientConfiguration CreateDefaultConfiguration() =>
                new LibreAirspaceClientConfiguration() {
                    _KeyBind = new KeyBind(),
                    _NetworkingConfig = new NetworkingConfig() {
                        _RoomListServerIp = "47.97.112.35",
                        _RoomListServerTcpPort = 16313,
                        _DefaultMatchServerKcpPort = 16314
                    },
                };

            /// <summary>
            /// Resolve configuration from configuration file, create and use default configuration if file is not existed
            /// </summary>
            /// <param name="configurationFilePath">Where the configuration file?</param>
            /// <returns>The configuration readed or created</returns>
            private static LibreAirspaceClientConfiguration ResolveCfgFromCfgFileReadOrCreate(string configurationFilePath) {
                if (File.Exists(configurationFilePath)) {
                    LibreAirspaceClientConfiguration _cfg = null;
                    try {
                        _cfg = JsonConvert.DeserializeObject<LibreAirspaceClientConfiguration>(File.ReadAllText(configurationFilePath));
                    } catch (JsonException ex) {
                        TGZG.公共空间.logerror($"无法解析客户端配置文件，回滚至默认配置文件。{Environment.NewLine}" +
                            $"请修复配置文件。");
                        return CreateDefaultConfiguration();
                    }
                    return _cfg;
                } else {
                    TGZG.公共空间.log("没有客户端配置文件，自动创建默认配置文件。");
                    LibreAirspaceClientConfiguration _cfg = CreateDefaultConfiguration();
                    //移除最后一个反斜杠，避免路径错误
                    Directory.CreateDirectory(configurationFilePath.Substring(0, configurationFilePath.LastIndexOf('\\')));
                    File.WriteAllText(configurationFilePath, JsonConvert.SerializeObject(_cfg));
                    return _cfg;
                }
            }

            public static LibreAirspaceClientConfiguration ResolveConfiguration() =>
                ResolveCfgFromCfgFileReadOrCreate(Path.Combine(Application.dataPath, "Config", "LibreAirspaceClientConfiguration.json"));
        }
    }
}
