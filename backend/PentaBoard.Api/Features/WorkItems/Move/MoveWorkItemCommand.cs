// Features/WorkItems/Move/MoveWorkItemCommand.cs
using MediatR;
using PentaBoard.Api.Features.WorkItems.Common;

namespace PentaBoard.Api.Features.WorkItems.Move;

public sealed record MoveWorkItemCommand(
    Guid ProjectId,
    Guid BoardId,
    Guid WorkItemId,
    Guid TargetColumnId
) : IRequest<WorkItemDto>;
