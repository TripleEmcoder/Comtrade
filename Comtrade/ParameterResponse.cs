using System.Text.Json.Serialization;

namespace Comtrade
{
    public class ParameterResponse
    {
        public bool More { get; set; }

        public IList<ParameterResult> Results { get; set; }
    }

    public class ParameterResult
    {
        [JsonPropertyName("id")]
        public string Code { get; set; }
        
        [JsonPropertyName("text")]
        public string Description { get; set; }

        [JsonPropertyName("parent")]
        public string ParentCode { get; set; }

        private const string Separator = " - ";

        public override string ToString()
            => Description.StartsWith(Code + Separator) ? Description : Code + Separator + Description;
    }
}