using MediatR;
using System;

namespace PentaBoard.Api.Features.Projects.DeleteProject;

public record DeleteProjectCommand(Guid ProjectId) : IRequest<bool>;
