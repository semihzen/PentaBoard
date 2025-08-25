using MediatR;

namespace PentaBoard.Api.Features.Projects.GetProjects;

public record GetProjectsQuery : IRequest<List<GetProjectsResult>>;

public record GetProjectsResult(
    Guid Id,
    string Name,
    string Key,
    string? Description,
    string Color,
    DateTime? StartDate,
    DateTime? EndDate,
    List<string>? Tags,
    Guid ProjectAdminId,
    Guid CreatedById,
    DateTime CreatedAt
);
