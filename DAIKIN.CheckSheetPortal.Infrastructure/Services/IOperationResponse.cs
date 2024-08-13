using System.Net;

namespace DAIKIN.CheckSheetPortal.Infrastructure
{
    public interface IOperationResponse
    {
        bool IsSuccess { get; }
        bool IsSuccessfulOrNotFound { get; }
        IEnumerable<string> Messages { get; }
        HttpStatusCode StatusCode { get; }
    }

    public interface IOperationResponse<out TPayload> : IOperationResponse
    {
        TPayload Payload { get; }
    }
}
