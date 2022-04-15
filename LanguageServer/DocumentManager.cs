using System.Collections.Concurrent;

namespace akanevrc.TeuchiUdon.Server
{
    public class DocumentManager
    {
        private ConcurrentDictionary<string, string> Documents { get; } = new();

        public void Update(string documentPath, string text)
        {
            Documents.AddOrUpdate(documentPath, text, (k, v) => text);
        }

        public string Get(string documentPath)
        {
            return Documents.TryGetValue(documentPath, out var text) ? text : "";
        }
    }
}
