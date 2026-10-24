using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.IO;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IMemberRepository _memberRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repository, IMemberRepository memberRepository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
            _memberRepository = memberRepository;
        }

        public LoginResult AuthenticateUser(string userId, string password, ref User user)
        {
            user = new User();
            var passwordKey = PasswordManager.EncryptPassword(password);
            user = _repository.GetUsers().Where(x => x.UserId == userId &&
                                                     x.Password == passwordKey).FirstOrDefault();

            return user != null ? LoginResult.Success : LoginResult.Failed;
        }

        public void AddUser(UserViewModel model)
        {
            var user = new User();
            if (!_repository.UserExists(model.UserId))
            {
                _mapper.Map(model, user);
                user.Password = PasswordManager.EncryptPassword(model.Password);
                user.CreatedTime = DateTime.Now;
                user.UpdatedTime = DateTime.Now;
                user.CreatedBy = System.Environment.UserName;
                user.UpdatedBy = System.Environment.UserName;

                _repository.AddUser(user);
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserExists);
            }
        }

        public void RegisterApplicant(UserViewModel model)
        {
            if (_repository.UserExists(model.UserId) || _repository.UserExists("pending:" + model.UserId))
            {
                throw new InvalidDataException("This student number / user ID is already registered or pending review.");
            }

            if (_memberRepository.StudentNumberExists(model.UserId))
            {
                throw new InvalidDataException("A member with this student number already exists.");
            }

            var user = new User
            {
                UserId = "pending:" + model.UserId,
                Name = model.Name,
                Password = PasswordManager.EncryptPassword(model.Password),
                CreatedTime = DateTime.Now,
                UpdatedTime = DateTime.Now,
                CreatedBy = "Self-Registration",
                UpdatedBy = "Self-Registration"
            };
            _repository.AddUser(user);

            var member = new Member
            {
                StudentNumber = model.UserId,
                FullName = model.Name,
                CourseYear = !string.IsNullOrWhiteSpace(model.CourseYear) ? model.CourseYear : "BSIT 1st Year",
                Section = !string.IsNullOrWhiteSpace(model.Section) ? model.Section : "1-A",
                ContactDetails = !string.IsNullOrWhiteSpace(model.Email) ? model.Email : $"{model.UserId}@ustp.edu.ph",
                MembershipType = "Regular",
                Status = "pending"
            };
            _memberRepository.AddMember(member);
        }

        public bool IsPendingApproval(string userId)
        {
            return _repository.GetUsers().Any(x => x.UserId == "pending:" + userId) ||
                   _memberRepository.GetMembers().Any(m => m.StudentNumber == userId && m.Status == "pending");
        }

        public System.Collections.Generic.List<Member> GetPendingApplicants()
        {
            return _memberRepository.GetMembers()
                .Where(m => m.Status == "pending")
                .OrderByDescending(m => m.MemberId)
                .ToList();
        }

        public void ApproveApplicant(string studentNumber)
        {
            var pendingUser = _repository.GetUsers().FirstOrDefault(x => x.UserId == "pending:" + studentNumber);
            if (pendingUser != null)
            {
                pendingUser.UserId = studentNumber;
                pendingUser.UpdatedTime = DateTime.Now;
                pendingUser.UpdatedBy = "Officer";
                _repository.UpdateUser(pendingUser);
            }

            var member = _memberRepository.GetMembers().FirstOrDefault(m => m.StudentNumber == studentNumber);
            if (member != null)
            {
                member.Status = "active";
                _memberRepository.UpdateMember(member);
            }
        }

        public void RejectApplicant(string studentNumber)
        {
            var pendingUser = _repository.GetUsers().FirstOrDefault(x => x.UserId == "pending:" + studentNumber);
            if (pendingUser != null)
            {
                _repository.DeleteUser(pendingUser);
            }

            var member = _memberRepository.GetMembers().FirstOrDefault(m => m.StudentNumber == studentNumber);
            if (member != null)
            {
                member.Status = "rejected";
                _memberRepository.UpdateMember(member);
            }
        }
    }
}
