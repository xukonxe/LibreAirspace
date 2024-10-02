using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using WTRev.TKTLib.Modding.InfoCls;

namespace WTRev.TKTLib.Modding.CSC {
	/// <summary>
	/// <para>War Thunder : Revolution C# Compiler</para>
	/// <para>一个简易C#编译器，Roslyn的前端</para>
	/// </summary>
	internal sealed class WTRCSC {
		/// <summary>
		/// 缺省元数据引用
		/// </summary>
		private List<MetadataReference> m_defMdRef;

		/// <summary>
		/// 缺省编译配置
		/// </summary>
		private CSharpCompilationOptions m_defCompilationOptions;

		public WTRCSC(IEnumerable<IEnumerable<byte>> assemblyImageReferences) {
			this.m_defMdRef = new List<MetadataReference>(assemblyImageReferences.Select(_bytes => MetadataReference.CreateFromImage(_bytes)));
			this.m_defCompilationOptions = new CSharpCompilationOptions(
				outputKind: OutputKind.DynamicallyLinkedLibrary,
				optimizationLevel: OptimizationLevel.Debug,
				checkOverflow: false,
				allowUnsafe: true,
				platform: Platform.AnyCpu);
		}

		public (ModOperateResult, IEnumerable<Diagnostic>, byte[]) CompileScriptSet(IEnumerable<string> scriptSet) {
			List<SyntaxTree> _syntaxTrees = new List<SyntaxTree>(scriptSet.Select(_script => SyntaxFactory.ParseSyntaxTree(_script)));
			CSharpCompilation _CSCompilation = CSharpCompilation.Create($"WTRScriptAssembly_{Guid.NewGuid()}", _syntaxTrees, this.m_defMdRef, this.m_defCompilationOptions);
			using (MemoryStream _buffer = new MemoryStream(20 * 1024 * 1024 /* Pre-allocate 20MB of memory */)) {
				EmitResult _EmitResult = _CSCompilation.Emit(_buffer);
				//Anyway, output the diagnostics information.
				if (!_EmitResult.Success) {
					return (ModOperateResult.编译脚本失败, _EmitResult.Diagnostics, null);
				}
				return (ModOperateResult.成功, _EmitResult.Diagnostics, _buffer.ToArray());
			}
		}
	}
}
