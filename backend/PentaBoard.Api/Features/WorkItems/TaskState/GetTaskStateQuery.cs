// Features/WorkItems/TaskState/GetTaskStateQuery.cs
using MediatR;

namespace PentaBoard.Api.Features.WorkItems.TaskState;

public sealed record GetTaskStateQuery(
    Guid ProjectId,
    Guid BoardId,
    int Days // 7/14/30...
) : IRequest<TaskStateDto>;
