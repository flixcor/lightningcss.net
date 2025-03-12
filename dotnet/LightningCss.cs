using CsBindgen;
using System.Runtime.InteropServices;
using System.Text;

namespace lightningcss
{
    public static class LightningCss
    {
        public static Targets BrowserlistToTargets(byte[] browserlist)
        {
            CsBindgen.Targets[] targets = [new CsBindgen.Targets()];
            CssError[] errors = [];
            unsafe
            {
                fixed (byte* browserlistP = browserlist)
                fixed (CsBindgen.Targets* targetsP = targets)
                fixed (CssError* cssErrorP = errors)
                {
                    NativeMethods.lightningcss_browserslist_to_targets(browserlistP, targetsP, (CssError**)cssErrorP);
                }
            }
            var source = targets[0];
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

        public static ToCssResult Transform(ReadOnlySpan<byte> source, ParseOptions parseOptions, TransformOptions transformOptions, ToCssOptions toCssOptions)
        {
            CssError[] errors = [];

            unsafe
            {
                fixed (byte* sourcePointer = source)
                fixed (CssError* cssErrorP = errors)
                fixed (byte* fileNamePointer = parseOptions.Filename)
                fixed (byte* patternPointer = parseOptions.CssModulesPattern)
                fixed (byte** unusedSymbolsP = Fill(transformOptions.UnusedSymbols))
                {
                    CssError** cssError = (CssError**)cssErrorP;

                    var wrapper = NativeMethods.lightningcss_stylesheet_parse(sourcePointer, (nuint)source.Length, new()
                    {
                        filename = fileNamePointer,
                        css_modules_pattern = patternPointer,
                        nesting = parseOptions.Nesting,
                        custom_media = parseOptions.CustomMedia,
                        css_modules = parseOptions.CssModules,
                        css_modules_dashed_idents = parseOptions.CssModulesDashedIdents,
                        error_recovery = parseOptions.ErrorRecovery,
                    }, cssError);

                    NativeMethods.lightningcss_stylesheet_transform(wrapper, new()
                    {
                        targets = new()
                        {
                            android = transformOptions.Targets.Android,
                            chrome = transformOptions.Targets.Chrome,
                            edge = transformOptions.Targets.Edge,
                            firefox = transformOptions.Targets.Firefox,
                            ie = transformOptions.Targets.Ie,
                            ios_saf = transformOptions.Targets.IosSafari,
                            opera = transformOptions.Targets.Opera,
                            safari = transformOptions.Targets.Safari,
                            samsung = transformOptions.Targets.IosSafari,
                        },
                        unused_symbols = unusedSymbolsP,
                        unused_symbols_len = (nuint)transformOptions.UnusedSymbols.Length
                    }, cssError);

                    var result = NativeMethods.lightningcss_stylesheet_to_css(wrapper, new(), cssError);

                    return new()
                    {
                        Code = ConvertToBytes(result.code),
                        Map = ConvertToBytes(result.map),
                        Exports = Convert(result.exports, result.exports_len, x => new CssModuleExport
                        {
                            Exported = ConvertToBytes(x.exported),
                            Local = ConvertToBytes(x.local),
                            IsReferenced = x.is_referenced,
                            Composes = Convert(x.composes, x.composes_len, y => new CssModuleReference
                            {
                                Name = ConvertToBytes(y.name),
                                Specifier = ConvertToBytes(y.specifier),
                            })
                        }),
                        References = Convert(result.references, result.references_len, x => new CssModulePlaceholder
                        {
                            Placeholder = ConvertToBytes(x.placeholder),
                            Reference = new CssModuleReference
                            {
                                Name = ConvertToBytes(x.reference.name),
                                Specifier = ConvertToBytes(x.reference.specifier),
                            }
                        })
                    };
                }
            }
        }

        private static byte[] ConvertToBytes(RawString rawString)
        {
            if (rawString.len == 0) return [];
            var result = new byte[rawString.len];
            unsafe
            {
                for (nuint i = 0; i < rawString.len; i++)
                {
                    result[i] = rawString.text[i];
                }
            }
            return result;
        }

        private unsafe static string ConvertToString(RawString rawString)
        {
            return Encoding.UTF8.GetString(rawString.text, (int)rawString.len);
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

        private static unsafe byte*[] Fill(byte[][] source)
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

    public record struct ParseOptions
    {
        public byte[] Filename;
        public bool Nesting;
        public bool CustomMedia;
        public bool CssModules;
        public byte[] CssModulesPattern;
        public bool CssModulesDashedIdents;
        public bool ErrorRecovery;
    }

    public record struct ToCssResult
    {
        public byte[] Code;
        public byte[] Map;
        public CssModuleExport[] Exports;
        public CssModulePlaceholder[] References;
    }

    public record struct CssModuleExport
    {
        public byte[] Exported;
        public byte[] Local;
        public bool IsReferenced;
        public CssModuleReference[] Composes;
    }

    public record struct CssModulePlaceholder
    {
        public byte[] Placeholder;
        public CssModuleReference Reference;
    }

    public record struct CssModuleReference
    {
        public byte[] Name;
        public byte[] Specifier;
    }

    public record struct TransformOptions
    {
        public Targets Targets;
        public byte[][] UnusedSymbols;
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

    public record struct ToCssOptions
    {
        public bool minify;
        public bool source_map;
        public byte[] input_source_map;
        public byte[] project_root;
        public Targets targets;
        public bool analyze_dependencies;
        public PseudoClasses pseudo_classes;
    }

    public record struct PseudoClasses
    {
        public byte[] hover;
        public byte[] active;
        public byte[] focus;
        public byte[] focus_visible;
        public byte[] focus_within;
    }
}

