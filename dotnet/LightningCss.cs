using CsBindgen;

namespace lightningcss
{
	public static class LightningCss
	{
		public static Targets BrowserlistToTargets(ReadOnlySpan<byte> browserlist)
		{
			ReadOnlySpan<CssError> errors = [];
			unsafe
			{
				fixed (byte* browserlistP = browserlist)
				fixed (CssError* cssErrorP = errors)
				{
					CsBindgen.Targets* targetsP = stackalloc CsBindgen.Targets[1];
					NativeMethods.lightningcss_browserslist_to_targets(browserlistP, targetsP, (CssError**)cssErrorP);
					var source = targetsP[0];
					return new()
					{
						Android = source.android,
						Chrome = source.chrome,
						Edge = source.edge,
						Firefox = source.firefox,
						Ie = source.ie,
						IosSafari = source.ios_saf,
						Opera = source.opera,
						Safari = source.safari,
						Samsung = source.samsung,
					};
				}
			}
		}

		public static ToCssResult Transform(TransformOptions options)
		{
			unsafe
			{
				var result = TransformInternal(options);
				return new()
				{
					Code = result.code.GetSpan(),
					Map = result.map.GetSpan(),
					Exports = Convert(result.exports, result.exports_len, x => new CssModuleExport
					{
						Exported = x.exported.GetMemory(),
						Local = x.local.GetMemory(),
						IsReferenced = x.is_referenced,
						Composes = Convert(x.composes, x.composes_len, y => new CssModuleReference
						{
							Name = y.name.GetMemory(),
							Specifier = y.specifier.GetMemory(),
						})
					}),
					References = Convert(result.references, result.references_len, x => new CssModulePlaceholder
					{
						Placeholder = x.placeholder.GetMemory(),
						Reference = new CssModuleReference
						{
							Name = x.reference.name.GetMemory(),
							Specifier = x.reference.specifier.GetMemory(),
						}
					})
				};
			}
		}

		private static CsBindgen.ToCssResult TransformInternal(TransformOptions options)
		{
			ReadOnlySpan<CssError> errors = [];

			unsafe
			{
				fixed (byte* sourcePointer = options.Code)
				fixed (CssError* cssErrorP = errors)
				fixed (byte* fileNamePointer = options.Filename)
				fixed (byte* patternPointer = options.CssModulesPattern)
				fixed (byte* inputSourceMap = options.InputSourceMap)
				fixed (byte* projectRoot = options.ProjectRoot)
				fixed (byte* active = options.PseudoClasses.Active)
				fixed (byte* focus = options.PseudoClasses.Focus)
				fixed (byte* focusVisible = options.PseudoClasses.FocusVisible)
				fixed (byte* focusWithin = options.PseudoClasses.FocusWithin)
				fixed (byte* hover = options.PseudoClasses.Hover)
				fixed (byte** unusedSymbolsP = Fill(options.UnusedSymbols))
				{
					CssError** cssError = (CssError**)cssErrorP;
					var targets = new CsBindgen.Targets
					{
						android = options.Targets.Android,
						chrome = options.Targets.Chrome,
						edge = options.Targets.Edge,
						firefox = options.Targets.Firefox,
						ie = options.Targets.Ie,
						ios_saf = options.Targets.IosSafari,
						opera = options.Targets.Opera,
						safari = options.Targets.Safari,
						samsung = options.Targets.IosSafari,
					};

					var wrapper = NativeMethods.lightningcss_stylesheet_parse(sourcePointer, (nuint)options.Code.Length, new()
					{
						filename = fileNamePointer,
						css_modules_pattern = patternPointer,
						nesting = options.Nesting,
						custom_media = options.CustomMedia,
						css_modules = options.CssModules,
						css_modules_dashed_idents = options.CssModulesDashedIdents,
						error_recovery = options.ErrorRecovery,
					}, cssError);

					NativeMethods.lightningcss_stylesheet_transform(wrapper, new()
					{
						targets = targets,
						unused_symbols = unusedSymbolsP,
						unused_symbols_len = (nuint)options.UnusedSymbols.Length
					}, cssError);

					return NativeMethods.lightningcss_stylesheet_to_css(wrapper, new()
					{
						input_source_map = inputSourceMap,
						input_source_map_len = (nuint)options.InputSourceMap.Length,
						project_root = projectRoot,
						source_map = options.SourceMap,
						analyze_dependencies = options.AnalyzeDependencies,
						minify = options.Minify,
						targets = targets,
						pseudo_classes = new()
						{
							active = active,
							focus = focus,
							focus_visible = focusVisible,
							focus_within = focusWithin,
							hover = hover,
						}
					}, cssError);
				}
			}
		}

		private static ReadOnlySpan<byte> GetSpan(this RawString rawString)
		{
			Span<byte> result = new byte[rawString.len];
			unsafe
			{
				Fill(rawString.text, result);
			}
			return result;
		}

		private static ReadOnlyMemory<byte> GetMemory(this RawString rawString)
		{
			Memory<byte> result = new byte[rawString.len];
			unsafe
			{
				Fill(rawString.text, result.Span);
			}
			return result;
		}

		private static unsafe void Fill<T>(T* pointer, Span<T> span)
		{
			unsafe
			{
				for (int i = 0; i < span.Length; i++)
				{
					span[i] = pointer[i];
				}
			}
		}



		private unsafe static O[] Convert<T, O>(T* pointer, nuint length, Func<T, O> map) where T : struct
		{
			var result = new O[length];
			unsafe
			{
				for (nuint i = 0; i < length; i++)
				{
					result[i] = map(pointer[i]);
				}
			}
			return result;
		}

		private static unsafe byte*[] Fill(ReadOnlySpan<byte[]> source)
		{
			var result = new byte*[source.Length];
			for (int i = 0; i < source.Length; i++)
			{
				fixed (byte* inner = source[i])
				{
					result[i] = inner;
				}
			}
			return result;
		}
	}

	public ref struct TransformOptions
	{
		public ReadOnlySpan<byte> Code;
		public ReadOnlySpan<byte> Filename;
		public bool Nesting;
		public bool CustomMedia;
		public bool CssModules;
		public ReadOnlySpan<byte> CssModulesPattern;
		public bool CssModulesDashedIdents;
		public bool ErrorRecovery;
		public Targets Targets;
		public ReadOnlySpan<byte[]> UnusedSymbols;
		public bool Minify;
		public bool SourceMap;
		public ReadOnlySpan<byte> InputSourceMap;
		public ReadOnlySpan<byte> ProjectRoot;
		public bool AnalyzeDependencies;
		public PseudoClasses PseudoClasses;
	}

	public ref struct ToCssResult
	{
		public ReadOnlySpan<byte> Code;
		public ReadOnlySpan<byte> Map;
		public ReadOnlySpan<CssModuleExport> Exports;
		public ReadOnlySpan<CssModulePlaceholder> References;
	}

	public record struct CssModuleExport
	{
		public ReadOnlyMemory<byte> Exported;
		public ReadOnlyMemory<byte> Local;
		public bool IsReferenced;
		public ReadOnlyMemory<CssModuleReference> Composes;
	}

	public record struct CssModulePlaceholder
	{
		public ReadOnlyMemory<byte> Placeholder;
		public CssModuleReference Reference;
	}

	public record struct CssModuleReference
	{
		public ReadOnlyMemory<byte> Name;
		public ReadOnlyMemory<byte> Specifier;
	}

	public record struct Targets
	{
		public uint Android;
		public uint Chrome;
		public uint Edge;
		public uint Firefox;
		public uint Ie;
		public uint IosSafari;
		public uint Opera;
		public uint Safari;
		public uint Samsung;
	}

	public ref struct PseudoClasses
	{
		public ReadOnlySpan<byte> Hover;
		public ReadOnlySpan<byte> Active;
		public ReadOnlySpan<byte> Focus;
		public ReadOnlySpan<byte> FocusVisible;
		public ReadOnlySpan<byte> FocusWithin;
	}
}

