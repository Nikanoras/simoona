using System.Collections.Generic;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Domain.Exceptions.Exceptions;

namespace Shrooms.Domain.ServiceValidators.Validators.Kudos
{
    public class KudosServiceValidator : IKudosServiceValidator
    {
        public void ValidateUser(ApplicationUser user)
        {
            if (user != null)
            {
                return;
            }

            throw new ValidationException(ErrorCodes.UserNotFound, "User not found");
        }

        public void ValidateKudosType(KudosType kudosType)
        {
            if (kudosType != null)
            {
                return;
            }

            throw new ValidationException(ErrorCodes.KudosTypeNotFound, "Kudos type not found");
        }

        public void ValidateKudosMinusPermission(bool hasPermission)
        {
            if (hasPermission)
            {
                return;
            }

            throw new UnauthorizedException();
        }

        public void ValidateUserAvailableKudos(decimal userRemainingKudos, decimal totalKudosPoints)
        {
            if (userRemainingKudos >= totalKudosPoints)
            {
                return;
            }

            throw new ValidationException(ErrorCodes.InsufficientKudos, "Insufficient kudos points");
        }

        public void ValidateUserAvailableKudosToSendPerMonth(decimal totalKudosPointsInLog, decimal kudosAvailableToSendThisMonth)
        {
            if (kudosAvailableToSendThisMonth >= totalKudosPointsInLog)
            {
                return;
            }

            throw new ValidationException(ErrorCodes.InsufficientKudos, "Insufficient kudos points");
        }

        public void ValidateSendingToSameUserAsReceiving(string sendingUserId, string receivingUserId)
        {
            if (sendingUserId != receivingUserId)
            {
                return;
            }

            throw new ValidationException(ErrorCodes.CanNotSendKudosToSelf, "Kudos receiver can not be a sender");
        }

        public void CheckForEmptyUserList(List<ApplicationUser> recievingUsers)
        {
            if (recievingUsers.Count != 0)
            {
                return;
            }

            throw new ValidationException(ErrorCodes.UserNotFound, "User not found");
        }

        public void CheckIfUserExists(bool userExists)
        {
            if (userExists)
            {
                return;
            }

            throw new ValidationException(ErrorCodes.UserNotFound, "User not found");
        }
    }
}
