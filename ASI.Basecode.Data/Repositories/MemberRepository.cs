using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class MemberRepository : BaseRepository, IMemberRepository
    {
        public MemberRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<Member> GetMembers()
        {
            return this.GetDbSet<Member>();
        }

        public IQueryable<MemberRecord> GetMemberRecords()
        {
            var members = this.GetDbSet<Member>().AsNoTracking();
            var userSummaries = this.GetDbSet<MemberAccountSummary>();

            return from member in members
                   join user in userSummaries on member.UserId equals user.UserId
                   select new MemberRecord
                   {
                       MemberId = member.MemberId,
                       UserId = member.UserId,
                       FullName = user.FullName,
                       Email = user.Email,
                       StudentNumber = member.StudentNumber,
                       Course = member.Course,
                       YearLevel = member.YearLevel,
                       Section = member.Section,
                       MobileNumber = member.MobileNumber,
                       MembershipType = member.MembershipType,
                       Status = member.Status,
                       CreatedBy = member.CreatedBy,
                       CreatedAt = member.CreatedAt,
                       UpdatedBy = member.UpdatedBy,
                       UpdatedAt = member.UpdatedAt
                   };
        }

        public MemberRecord GetMemberRecord(int memberId)
        {
            return this.GetMemberRecords().FirstOrDefault(member => member.MemberId == memberId);
        }

        public Member GetMember(int memberId)
        {
            return this.GetDbSet<Member>().FirstOrDefault(m => m.MemberId == memberId);
        }

        public void AddMember(Member member)
        {
            this.GetDbSet<Member>().Add(member);
            UnitOfWork.SaveChanges();
        }

        public void UpdateMember(Member member)
        {
            this.SetEntityState(member, EntityState.Modified);
            UnitOfWork.SaveChanges();
        }

        public bool UserHasMemberProfile(int userId)
        {
            return this.GetDbSet<Member>().Any(m => m.UserId == userId);
        }

        public bool StudentNumberExists(string studentNumber, int? excludeId = null)
        {
            return this.GetDbSet<Member>().Any(m => m.StudentNumber == studentNumber &&
                                                     (!excludeId.HasValue || m.MemberId != excludeId.Value));
        }
    }
}
