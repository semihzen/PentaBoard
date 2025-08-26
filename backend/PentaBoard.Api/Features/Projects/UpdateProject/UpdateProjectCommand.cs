using MediatR;

namespace PentaBoard.Api.Features.Projects.UpdateProject;

public record UpdateProjectCommand(
    Guid Id,
    string? Description,
    DateTime? StartDate,
    DateTime? EndDate,
    List<string>? Tags
) : IRequest<UpdateProjectResult>;

public record UpdateProjectResult(
    Guid Id,
    string Name,
    string Key,
    string? Description,
    string Color,
    DateTime? StartDate,
    DateTime? EndDate,
    IReadOnlyList<string> Tags,   // DTO’da listesi halinde dönüyoruz
    Guid ProjectAdminId,
    Guid CreatedById,
    DateTime CreatedAt
);
