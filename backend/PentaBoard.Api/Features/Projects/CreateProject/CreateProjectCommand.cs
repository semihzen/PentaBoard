using MediatR;

namespace PentaBoard.Api.Features.Projects.CreateProject;

public record CreateProjectCommand(
    string Name,
    string? Description,
    string Color,
    DateTime? StartDate,
    DateTime? EndDate,
    List<string>? Tags,

    // Admin başka bir System Admin adına atamak isterse:
    Guid? ProjectAdminId    // System Admin ise null gönderir, backend kendisini atar
) : IRequest<CreateProjectResult>;

public record CreateProjectResult(
    Guid Id,
    string Name,
    string Key
);
