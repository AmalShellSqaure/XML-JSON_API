using System.Collections.Concurrent;
using XML_JSON_API.Models;

namespace XML_JSON_API.Repositories
{
    public class MemoryRepository
    {
        public ConcurrentDictionary<Guid, FileStatus> Files { get; } = new();
    }
}