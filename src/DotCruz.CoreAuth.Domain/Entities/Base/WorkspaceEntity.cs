using System;

namespace DotCruz.CoreAuth.Domain.Entities.Base
{
    public abstract class WorkspaceEntity : BaseEntity
    {
        public Guid WorkspaceId { get; protected set; }
        public void SetWorkspaceId(Guid workspaceId) => WorkspaceId = workspaceId;
    }
}
