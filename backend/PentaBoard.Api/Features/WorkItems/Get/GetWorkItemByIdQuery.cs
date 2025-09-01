// Features/WorkItems/Get/GetWorkItemByIdQuery.cs
using MediatR;
using PentaBoard.Api.Features.WorkItems.Common; // WorkItemDto

namespace PentaBoard.Api.Features.WorkItems.Get;

public sealed record GetWorkItemByIdQuery(
    Guid ProjectId,
    Guid BoardId,
    Guid WorkItemId
) : IRequest<WorkItemDto?>;
