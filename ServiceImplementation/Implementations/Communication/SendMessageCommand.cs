using MediatR;
using Microsoft.AspNetCore.Http;
using ServiceContracts.DTOs.Chat;
using ServiceContracts.DTOs.Responses;
using System.Collections.Generic;

namespace ServiceImplementation.Implementations.Communication
{
    public record SendMessageCommand(
        string ChatId,
        string SenderId,
        string Body,
        List<IFormFile>? Files = null,
        int? ContractId = null,
        string? ReceiverId = null
    ) : IRequest<Result<MessageDto>>;
}
