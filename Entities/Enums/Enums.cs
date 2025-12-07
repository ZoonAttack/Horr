namespace Entities.Enums
{
    /// <summary>
    /// Represents the user role types in the system.
    /// </summary>
    public enum UserRole
    {
        Freelancer,
        Client,
        Specialist
    }

    /// <summary>
    /// Represents the status of a user verification request.
    /// </summary>
    public enum VerificationStatus
    {
        Pending,
        Approved,
        Rejected
    }

    /// <summary>
    /// Represents the status of a client project.
    /// </summary>
    public enum ProjectStatus
    {
        Open,
        InProgress,
        Completed,
        Cancelled,
        Disputed
    }
    /// <summary>
    /// Represents the status of a Freelancer service.
    /// </summary>
    public enum ServiceStatus
    {
        ACTIVE,
        INACTIVE
    }

    /// <summary>
    /// Represents the status of a freelancer proposal.
    /// </summary>
    public enum ProposalStatus
    {
        Pending,
        Accepted,
        Rejected,
        Withdrawn
    }

    /// <summary>
    /// Represents the status of an order.
    /// </summary>
    public enum OrderStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled,
        Refunded
    }

    /// <summary>
    /// Represents the type of an order.
    /// </summary>
    public enum OrderType
    {
        Service,
        Project
    }

    /// <summary>
    /// Represents the status of a delivery.
    /// </summary>
    public enum DeliveryStatus
    {
        Pending,
        Approved,
        Rejected,
        UnderReview
    }

    /// <summary>
    /// Represents the status of a payment.
    /// </summary>
    public enum RequestStatus
    {
        Pending,
        Held,
        Released,
        Refunded,
        Failed,
        Completed
    }

    /// <summary>
    /// Represents the type of a payment.
    /// </summary>
    public enum PaymentType
    {
        Escrow,
        Direct,
        Withdrawal,
        Refund
    }

    /// <summary>
    /// Represents the status of a payment.
    /// </summary>
    public enum PaymentStatus
    {
        Pending,
        Held,
        Released,
        Refunded,
        Failed,
        Completed
    }

    /// <summary>
    /// Represents the status of a transaction.
    /// </summary>
    public enum TransactionStatus
    {
        Pending,
        Completed,
        Failed,
        Cancelled
    }

    /// <summary>
    /// Represents the type of a transaction.
    /// </summary>
    public enum TransactionType
    {
        Deposit,
        Withdrawal,
        Transfer,
        Refund,
        Commission,
        Escrow
    }

    /// <summary>
    /// Represents the status of a specialist review request.
    /// </summary>
    public enum SpecialistReviewStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
    }

    /// <summary>
    /// Represents the status of a contract.
    /// </summary>
    public enum ContractStatus
    {
        Draft,
        Active,
        Completed,
        Terminated,
        Disputed
    }

    /// <summary>
    /// Represents the proficiency level for a skill.
    /// </summary>
    public enum ProficiencyLevel
    {
        Beginner,
        Intermediate,
        Advanced,
        Expert
    }

    /// <summary>
    /// Represents the role in a payment transaction.
    /// </summary>
    public enum PaymentTransactionRole
    {
        Payer,
        Payee,
        Platform,
        Escrow
    }

    public enum PaymentMethodTypes
    {
        InstaPay,
        eWallet
        //BankTransfer
    }

    /// <summary>
    /// Message to server action types.
    /// </summary>
    public enum ActionType : short
    {
        MESSAGE = 0,
        USERNAME,
        UPDATELIST,
        USERCONNECTED,
        USERDISCONNECTED,
        SERVERDISCONNECTED
    }
}
