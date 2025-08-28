namespace PentaBoard.Api.Features.WorkItems.Common;

public sealed record WorkItemDto(
    Guid Id,
    Guid ProjectId,
    Guid BoardId,
    Guid BoardColumnId,
    string Title,
    string? Description,
    string Type,
    byte? Priority,
    int OrderKey,
    Guid? AssigneeId
);
