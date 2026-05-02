using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceImplementation.Helpers
{
    public static class ErrorCodes
    {
        public const string EmailNotConfirmed = "EMAIL_NOT_CONFIRMED";
        public const string InvalidCredentials = "INVALID_CREDENTIALS";
        public const string EmailAlreadyInUse = "EMAIL_ALREADY_IN_USE";
        public const string AccountDeleted = "ACCOUNT_DELETED";
        public const string InsufficientBalance = "INSUFFICIENT_BALANCE";
    }

}
