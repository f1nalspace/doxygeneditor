using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP.DoxygenEditor.Collections;
using TSP.DoxygenEditor.Lexers;
using TSP.DoxygenEditor.Parsers;
using TSP.DoxygenEditor.Symbols;

namespace TSP.DoxygenEditor.Languages.Doxygen
{
    public class DoxygenConfigParser : BaseParser<DoxygenConfigEntity, DoxygenToken>
    {
        public DoxygenConfigParser(ISymbolTableId id) : base(id)
        {
        }

        private void ParseValues(LinkedListStream<IBaseToken> stream)
        {
            DoxygenToken token = stream.Peek<DoxygenToken>();
            Debug.Assert(token != null && token.Kind == DoxygenTokenKind.ConfigKey);
            DoxygenToken keyToken = token;
            string key = keyToken.Value;
            stream.Next();

            token = stream.Peek<DoxygenToken>();
            if (token == null)
                return;

            DoxygenConfigEntityKind kind = DoxygenConfigEntityKind.None;
            if (token.Kind == DoxygenTokenKind.ConfigOpAssign)
                kind = DoxygenConfigEntityKind.ConfigSet;
            else if (token.Kind == DoxygenTokenKind.ConfigOpAddAssign)
                kind = DoxygenConfigEntityKind.ConfigAdd;
            else
                return;

            stream.Next();

            DoxygenConfigEntity entity = new DoxygenConfigEntity(kind, keyToken)
            {
                Id = key,
                Value = key,
            };

            while (!stream.IsEOF)
            {
                token = stream.Peek<DoxygenToken>();
                if (token == null)
                    break;
                if (token.Kind == DoxygenTokenKind.ConfigValue)
                {
                    string value = token.Value;
                    entity.AddSettingsValue(value);
                    stream.Next();
                }
                else if (token.Kind == DoxygenTokenKind.ConfigOpAddLine)
                    stream.Next();
                else
                    break;
            }

            DoxygenConfigNode node = new DoxygenConfigNode(Top, entity);
            Add(node);

            LocalSymbolTable.AddSymbol(new SourceSymbol(LanguageKind.DoxygenConfig, SourceSymbolKind.DoxygenConfigValue, key, keyToken.Range, node));
        }

        protected override ParseTokenResult ParseToken(string source, LinkedListStream<IBaseToken> stream)
        {
            DoxygenToken token = stream.Peek<DoxygenToken>();
            if (token != null)
            {
                if (token.Kind == DoxygenTokenKind.ConfigKey)
                {
                    ParseValues(stream);
                    return ParseTokenResult.AlreadyAdvanced;
                }
            }
            return ParseTokenResult.ReadNext;
        }
    }
}
