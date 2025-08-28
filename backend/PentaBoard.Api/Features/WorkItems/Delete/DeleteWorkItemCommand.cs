using MediatR;

namespace PentaBoard.Api.Features.WorkItems.Delete;

public sealed record DeleteWorkItemCommand(
    Guid ProjectId,
    Guid BoardId,
    Guid WorkItemId
) : IRequest<bool>;
