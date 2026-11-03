using ASI.Basecode.Data.Models;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IMemberRepository
    {
        IQueryable<MemberRecord> GetMemberRecords();
        MemberRecord GetMemberRecord(int memberId);
        Member GetMember(int memberId);
        void AddMember(Member member);
        void UpdateMember(Member member);
        bool UserHasMemberProfile(int userId);
        bool StudentNumberExists(string studentNumber, int? excludeId = null);
    }
}
