
namespace BeatTogether.DedicatedServer.Interface.Responses
{
    public record SetAllowedRequirementsResponse(bool Success);
    public record GetAllowedRequirementsResponse(
    bool Chroma,
    bool MappingExtensions,
    bool NoodleExtensions)
    {
        public bool Success = true;
    }
}
