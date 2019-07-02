using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Parsers
{
    public interface IBaseEntity
    {
        TextRange StartRange { get; }
        TextRange EndRange { get; set; }
        int Length { get; }
        string DisplayName { get; }
        string Id { get; set; }
        string Value { get; set; }
    }
}