namespace ServiceImplementation.Helpers
{
    public static class ErrorCodes
    {
        // ── Auth ──────────────────────────────────────────────────────────
        public const string EmailNotConfirmed    = "EMAIL_NOT_CONFIRMED";
        public const string InvalidCredentials   = "INVALID_CREDENTIALS";
        public const string EmailAlreadyInUse    = "EMAIL_ALREADY_IN_USE";
        public const string AccountDeleted       = "ACCOUNT_DELETED";
        public const string RegistrationFailed   = "REGISTRATION_FAILED";
        public const string RoleAssignmentFailed = "ROLE_ASSIGNMENT_FAILED";
        public const string EmailConfirmFailed   = "EMAIL_CONFIRM_FAILED";
        public const string EmailSendFailed      = "EMAIL_SEND_FAILED";
        public const string AlreadyConfirmed     = "ALREADY_CONFIRMED";
        public const string TokenInvalid         = "TOKEN_INVALID";
        public const string TokenExpired         = "TOKEN_EXPIRED";
        public const string PasswordChangeFailed = "PASSWORD_CHANGE_FAILED";

        // ── Billing / Wallet ─────────────────────────────────────────────
        public const string InsufficientBalance  = "INSUFFICIENT_BALANCE";
        public const string InvalidAmount        = "INVALID_AMOUNT";
        public const string MissingReceiptNumber = "MISSING_RECEIPT_NUMBER";
        public const string MissingReceiptPhoto  = "MISSING_RECEIPT_PHOTO";
        public const string MissingPaymentDetails = "MISSING_PAYMENT_DETAILS";

        // ── User / Client / Freelancer ────────────────────────────────────
        public const string UserNotFound         = "USER_NOT_FOUND";
        public const string ClientNotFound       = "CLIENT_NOT_FOUND";
        public const string FreelancerNotFound   = "FREELANCER_NOT_FOUND";
        public const string Unauthorized         = "UNAUTHORIZED";

        // ── Jobs ──────────────────────────────────────────────────────────
        public const string JobNotFound          = "JOB_NOT_FOUND";

        // ── Contracts / Offers ────────────────────────────────────────────
        public const string InvalidOfferParties  = "INVALID_OFFER_PARTIES";
        public const string MilestonesRequired   = "MILESTONES_REQUIRED";
        public const string ContractNotFound     = "CONTRACT_NOT_FOUND";
    }
}
