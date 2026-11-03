using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IMemberService
    {
        List<MemberViewModel> GetMembers(string search = null);
        MemberViewModel GetMember(int id);
        void UpdateMember(MemberViewModel model, int actorUserId);
        void ToggleStatus(int id, int actorUserId);
        void CreateMemberFromApprovedAccountRequest(int userId, MemberViewModel model, int actorUserId);
    }
}
