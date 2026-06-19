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
        public const string PaymentMethodNotFound = "PAYMENT_METHOD_NOT_FOUND";
        public const string DepositRequestNotFound = "DEPOSIT_REQUEST_NOT_FOUND";
        public const string WithdrawalRequestNotFound = "WITHDRAWAL_REQUEST_NOT_FOUND";

        // ── User / Client / Freelancer ────────────────────────────────────
        public const string UserNotFound         = "USER_NOT_FOUND";
        public const string ClientNotFound       = "CLIENT_NOT_FOUND";
        public const string FreelancerNotFound   = "FREELANCER_NOT_FOUND";
        public const string SavedFreelancerNotFound = "SAVED_FREELANCER_NOT_FOUND";
        public const string Unauthorized         = "UNAUTHORIZED";

        // ── Jobs ──────────────────────────────────────────────────────────
        public const string JobNotFound          = "JOB_NOT_FOUND";

        // ── Contracts / Offers ────────────────────────────────────────────
        public const string InvalidOfferParties  = "INVALID_OFFER_PARTIES";
        public const string MilestonesRequired   = "MILESTONES_REQUIRED";
        public const string ContractNotFound     = "CONTRACT_NOT_FOUND";
        public const string AttachmentNotFound   = "ATTACHMENT_NOT_FOUND";
        public const string FileNotFound         = "FILE_NOT_FOUND";
        public const string MilestoneNotFound    = "MILESTONE_NOT_FOUND";
        public const string DeliveryNotFound     = "DELIVERY_NOT_FOUND";

        // ── Proposals ─────────────────────────────────────────────────────
        public const string ProposalNotFound     = "PROPOSAL_NOT_FOUND";

        // ── Chat / Messaging ──────────────────────────────────────────────
        public const string ChatNotFound         = "CHAT_NOT_FOUND";
        public const string MessageNotFound      = "MESSAGE_NOT_FOUND";
        public const string InvalidFile          = "INVALID_FILE";
        public const string InvalidFileType      = "INVALID_FILE_TYPE";
        public const string FileTooLarge         = "FILE_TOO_LARGE";

        // ── Invitations ───────────────────────────────────────────────────
        public const string InvitationAlreadySent = "INVITATION_ALREADY_SENT";

        // ── Project / Category / Skill ────────────────────────────────────
        public const string CategoryNotFound     = "CATEGORY_NOT_FOUND";
        public const string CategoryAlreadyExists = "CATEGORY_ALREADY_EXISTS";
        public const string SkillNotFound        = "SKILL_NOT_FOUND";
        public const string SkillAlreadyAdded    = "SKILL_ALREADY_ADDED";

        // ── Generic / State ───────────────────────────────────────────────
        public const string InvalidState         = "INVALID_STATE";
        public const string AlreadyReviewed      = "ALREADY_REVIEWED";
        public const string InvalidRating        = "INVALID_RATING";
        public const string ReviewNotFound       = "REVIEW_NOT_FOUND";
        public const string InvitationNotFound   = "INVITATION_NOT_FOUND";
        public const string ProposalAlreadySubmitted = "PROPOSAL_ALREADY_SUBMITTED";
        public const string RevisionLimitExceeded = "REVISION_LIMIT_EXCEEDED";
    }
}
