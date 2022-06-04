
namespace BeatTogether.DedicatedServer.Interface.Requests
{
    public record SetAllowedRequirementsRequest(
        bool AllowChroma,
        bool AllowMappingExtensions,
        bool AllowNoodleExtensions);
    public record GetAllowedRequirementsRequest();
}
