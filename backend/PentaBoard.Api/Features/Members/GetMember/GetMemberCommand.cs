using System;

namespace PentaBoard.Api.Features.Members.GetMember;

// İstek yapan (RequestorId) ve üyeleri istenen proje (ProjectId)
public sealed record GetMemberCommand(Guid ProjectId, Guid RequestorId);
