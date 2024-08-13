using DAIKIN.CheckSheetPortal.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DAIKIN.CheckSheetPortal.Services
{
    public class OperationResponse : IOperationResponse
    {
        private static HttpStatusCode[] SuccessStatus =
        {
            HttpStatusCode.OK,
            HttpStatusCode.Accepted
        };
        protected OperationResponse()
        {
        }
        public static IOperationResponse FromResponse<T>(IOperationResponse<T> response)
        {
            return new OperationResponse { StatusCode = response.StatusCode, Messages = response.Messages };
        }
        public static IOperationResponse FromResponse(IOperationResponse response)
        {
            return new OperationResponse { StatusCode = response.StatusCode, Messages = response.Messages };
        }
        public static IOperationResponse Success()
        {
            return new OperationResponse { StatusCode = HttpStatusCode.OK };
        }
        public static IOperationResponse Success(params string[] messages)
        {
            return new OperationResponse { StatusCode = HttpStatusCode.OK, Messages = messages };
        }
        public static IOperationResponse Accepted(params string[] messages)
        {
            return new OperationResponse { StatusCode = HttpStatusCode.Accepted, Messages = messages };
        }
        public static IOperationResponse Error(params string[] messages)
        {
            return new OperationResponse { StatusCode = HttpStatusCode.BadRequest, Messages = messages };
        }
        public static IOperationResponse ServerError(params string[] messages)
        {
            return new OperationResponse { StatusCode = HttpStatusCode.InternalServerError, Messages = messages };
        }
        public static IOperationResponse NotFound(params string[] messages)
        {
            return new OperationResponse { StatusCode = HttpStatusCode.NotFound, Messages = messages };
        }
        public static IOperationResponse NotImplemented(params string[] messages)
        {
            return new OperationResponse { StatusCode = HttpStatusCode.NotImplemented, Messages = messages };
        }
        public static IOperationResponse ServiceUnavailable(params string[] messages)
        {
            return new OperationResponse { StatusCode = HttpStatusCode.ServiceUnavailable, Messages = messages };
        }
        public static IOperationResponse BadRequest(params string[] messages)
        {
            return new OperationResponse { StatusCode = HttpStatusCode.BadRequest, Messages = messages };
        }
        public IEnumerable<string> Messages { get; protected set; }
        public HttpStatusCode StatusCode { get; protected set; }
        public bool IsSuccess => SuccessStatus.Contains(StatusCode);
        public bool IsSuccessfulOrNotFound => IsSuccess || StatusCode == HttpStatusCode.NotFound;
        public static bool EnsureOperationResponse(IOperationResponse operationResponse)
        {
            return operationResponse?.IsSuccess ?? false;
        }
    }
    public class OperationResponse<TPayload> : OperationResponse, IOperationResponse<TPayload>
    {
        protected OperationResponse()
        {
        }
        public static IOperationResponse<TPayload> FromResponse(IOperationResponse<TPayload> response)
        {
            return new OperationResponse<TPayload> { StatusCode = response.StatusCode, Messages = response.Messages, Payload = response.Payload };
        }
        public new static IOperationResponse<TPayload> FromResponse(IOperationResponse response)
        {
            return new OperationResponse<TPayload> { StatusCode = response.StatusCode, Messages = response.Messages };
        }
        public static IOperationResponse<TPayload> Success(TPayload payload, long recordcount = 0)
        {
            return new OperationResponse<TPayload> { StatusCode = HttpStatusCode.OK, Payload = payload, RecordCount = recordcount };
        }
        public static IOperationResponse<TPayload> Success(TPayload payload, params string[] messages)
        {
            return new OperationResponse<TPayload> { StatusCode = HttpStatusCode.OK, Messages = messages, Payload = payload };
        }
        public static IOperationResponse<TPayload> Accepted(TPayload payload)
        {
            return new OperationResponse<TPayload> { StatusCode = HttpStatusCode.Accepted, Payload = payload };
        }
        public static IOperationResponse<TPayload> Accepted(TPayload payload, params string[] messages)
        {
            return new OperationResponse<TPayload> { StatusCode = HttpStatusCode.Accepted, Messages = messages, Payload = payload };
        }
        public new static IOperationResponse<TPayload> Error(params string[] messages)
        {
            return new OperationResponse<TPayload> { StatusCode = HttpStatusCode.BadRequest, Messages = messages };
        }
        public static IOperationResponse<TPayload> Error(TPayload payload, params string[] messages)
        {
            return new OperationResponse<TPayload> { StatusCode = HttpStatusCode.BadRequest, Messages = messages, Payload = payload };
        }
        public new static IOperationResponse<TPayload> ServerError(params string[] messages)
        {
            return new OperationResponse<TPayload> { StatusCode = HttpStatusCode.InternalServerError, Messages = messages };
        }
        public new static IOperationResponse<TPayload> NotFound(params string[] messages)
        {
            return new OperationResponse<TPayload> { StatusCode = HttpStatusCode.NotFound, Messages = messages };
        }
        public static bool EnsureOperationResponse(IOperationResponse<TPayload> operationResponse)
        {
            return operationResponse?.IsSuccess ?? false;
        }
        public static bool EnsureOperationResponseValue(IOperationResponse<TPayload> operationResponse)
        {
            return EnsureOperationResponse(operationResponse) && operationResponse.Payload != null;
        }
        public TPayload Payload { get; private set; }
        public long RecordCount { get; private set; }
    }
}
