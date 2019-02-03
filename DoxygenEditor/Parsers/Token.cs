namespace DoxygenEditor.Parsers
{
    public class Token
    {
        public string Value { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
        public string Type { get; set; }
        public bool IsValid { get; set; }

        public override string ToString()
        {
            return $"({Type}) {Offset}:{Length}";
        }
    }
}
