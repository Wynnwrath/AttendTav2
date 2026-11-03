using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository _repository;

        public MemberService(IMemberRepository repository)
        {
            _repository = repository;
        }

        public List<MemberViewModel> GetMembers(string search = null)
        {
            var query = _repository.GetMemberRecords();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.FullName.Contains(search) ||
                                         m.Email.Contains(search) ||
                                         m.StudentNumber.Contains(search));
            }

            return query.OrderBy(member => member.FullName)
                .ThenBy(member => member.MemberId)
                .Select(ToViewModel)
                .ToList();
        }

        public MemberViewModel GetMember(int id)
        {
            var member = _repository.GetMemberRecord(id);
            return member == null ? null : ToViewModel(member);
        }

        public void CreateMemberFromApprovedAccountRequest(int userId, MemberViewModel model, int actorUserId)
        {
            if (userId <= 0 || actorUserId <= 0)
            {
                throw new InvalidDataException("A valid linked user and approving actor are required to create a member.");
            }

            if (_repository.UserHasMemberProfile(userId))
            {
                throw new InvalidDataException("This user already has a member profile.");
            }

            if (_repository.StudentNumberExists(model.StudentNumber))
            {
                throw new InvalidDataException("Student number already exists.");
            }

            var member = new Member
            {
                UserId = userId,
                StudentNumber = model.StudentNumber,
                Course = model.Course,
                YearLevel = model.YearLevel,
                Section = model.Section,
                MobileNumber = model.MobileNumber,
                MembershipType = model.MembershipType,
                Status = "active",
                CreatedBy = actorUserId,
                CreatedAt = DateTime.UtcNow
            };

            _repository.AddMember(member);
        }

        public void UpdateMember(MemberViewModel model, int actorUserId)
        {
            var member = _repository.GetMember(model.MemberId);
            if (member == null)
            {
                throw new InvalidDataException("Member not found.");
            }

            if (_repository.StudentNumberExists(model.StudentNumber, model.MemberId))
            {
                throw new InvalidDataException("Student number already exists.");
            }

            if (actorUserId <= 0)
            {
                throw new InvalidDataException("A valid authenticated user is required to update a member.");
            }

            member.StudentNumber = model.StudentNumber;
            member.Course = model.Course;
            member.YearLevel = model.YearLevel;
            member.Section = model.Section;
            member.MobileNumber = model.MobileNumber;
            member.MembershipType = model.MembershipType;
            member.Status = model.Status;
            member.UpdatedBy = actorUserId;
            member.UpdatedAt = DateTime.UtcNow;
            _repository.UpdateMember(member);
        }

        public void ToggleStatus(int id, int actorUserId)
        {
            var member = _repository.GetMember(id);
            if (member == null)
            {
                throw new InvalidDataException("Member not found.");
            }

            if (actorUserId <= 0)
            {
                throw new InvalidDataException("A valid authenticated user is required to update a member.");
            }

            member.Status = (member.Status ?? "active") == "active" ? "inactive" : "active";
            member.UpdatedBy = actorUserId;
            member.UpdatedAt = DateTime.UtcNow;
            _repository.UpdateMember(member);
        }

        private static MemberViewModel ToViewModel(MemberRecord member)
        {
            return new MemberViewModel
            {
                MemberId = member.MemberId,
                UserId = member.UserId,
                FullName = member.FullName,
                Email = member.Email,
                StudentNumber = member.StudentNumber,
                Course = member.Course,
                YearLevel = member.YearLevel,
                Section = member.Section,
                MobileNumber = member.MobileNumber,
                MembershipType = member.MembershipType,
                Status = member.Status
            };
        }
    }
}
