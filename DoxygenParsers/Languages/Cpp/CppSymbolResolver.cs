using System.Collections.Generic;
using System.Linq;
using TSP.DoxygenEditor.Collections;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.Symbols;

namespace TSP.DoxygenEditor.Languages.Cpp
{
    public class CppSymbolResolver : BaseSymbolResolver<CppEntity, CppToken>
    {
        public CppSymbolResolver(SymbolTable localSymbolTable) : base(localSymbolTable)
        {
        }

        public override void ResolveTokens(IEnumerable<CppToken> tokens)
        {
            // Resolve references
            LinkedListStream<CppToken> stream = new LinkedListStream<CppToken>(tokens);
            while (!stream.IsEOF)
            {
                CppToken token = stream.Peek<CppToken>();
                if (token != null)
                {
                    if (token.Kind == CppTokenKind.IdentLiteral)
                    {
                        string value = token.Value;
                        SourceSymbol sourceSymbol = _localSymbolTable.GetSource(value);
                        if (sourceSymbol != null)
                        {
                            ReferenceSymbolKind refKind = ReferenceSymbolKind.Any;
                            if (sourceSymbol.Kind == SourceSymbolKind.CppMacro)
                            {
                                token.Kind = CppTokenKind.PreprocessorDefineUsage;
                                refKind = ReferenceSymbolKind.CppMacroUsage;
                            }
                            else if (sourceSymbol.Kind == SourceSymbolKind.CppFunctionDefinition || sourceSymbol.Kind == SourceSymbolKind.CppFunctionBody)
                            {
                                token.Kind = CppTokenKind.FunctionIdent;
                                refKind = ReferenceSymbolKind.CppFunction;
                            }
                            else if (
                                sourceSymbol.Kind == SourceSymbolKind.CppType ||
                                sourceSymbol.Kind == SourceSymbolKind.CppStruct ||
                                sourceSymbol.Kind == SourceSymbolKind.CppClass ||
                                sourceSymbol.Kind == SourceSymbolKind.CppEnum)
                            {
                                token.Kind = CppTokenKind.UserTypeIdent;
                                refKind = ReferenceSymbolKind.CppType;
                            }
                            else if (sourceSymbol.Kind == SourceSymbolKind.CppMember)
                            {
                                token.Kind = CppTokenKind.MemberIdent;
                                refKind = ReferenceSymbolKind.CppMember;
                            }
                            _localSymbolTable.AddReference(new ReferenceSymbol(token.Lang, refKind, token.Value, token.Range, null));
                        }
                        else
                        {
                            IEnumerable<ReferenceSymbol> references = _localSymbolTable.GetReferences(value);
                            if (references.Count() > 0)
                            {
                                ReferenceSymbol reference = references.First();
                                switch (reference.Kind)
                                {
                                    case ReferenceSymbolKind.CppMacroMatch:
                                    case ReferenceSymbolKind.CppMacroUsage:
                                        token.Kind = CppTokenKind.PreprocessorDefineUsage;
                                        break;
                                    case ReferenceSymbolKind.CppFunction:
                                        token.Kind = CppTokenKind.FunctionIdent;
                                        break;
                                    case ReferenceSymbolKind.CppType:
                                        token.Kind = CppTokenKind.UserTypeIdent;
                                        break;
                                    case ReferenceSymbolKind.CppMember:
                                        token.Kind = CppTokenKind.MemberIdent;
                                        break;
                                }
                            }
                        }
                    }
                }
                stream.Next();
            }
        }
    }
}
