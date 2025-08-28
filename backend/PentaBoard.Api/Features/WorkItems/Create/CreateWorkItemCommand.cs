using MediatR;
using PentaBoard.Api.Features.WorkItems.Common;

namespace PentaBoard.Api.Features.WorkItems.Create;

public record CreateWorkItemCommand(
    Guid ProjectId,
    Guid BoardId,
    Guid? ColumnId,
    string Title,
    string? Description,
    string Type = "Task",
    byte? Priority = null,
    Guid? AssigneeId = null
) : IRequest<WorkItemDto>;
