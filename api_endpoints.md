# API Endpoints Documentation

This document lists all the API endpoints available in the system, grouped by controller. It includes the HTTP methods, routes, action summaries, parameters, request body models, and expected responses.

---

## Table of Contents
1. [AdminBillingController (`api/admin/billing`)](#1-adminbillingcontroller-apiadminbilling)
2. [AuthController (`api/Auth`)](#2-authcontroller-apiauth)
3. [ExperienceController (`api/Experience`)](#3-experiencecontroller-apiexperience)
4. [PortfolioController (`api/Portfolio`)](#4-portfoliocontroller-apiportfolio)
5. [UserProfileController (`api/UserProfile`)](#5-userprofilecontroller-apiuserprofile)
6. [BillingController (`api/Billing`)](#6-billingcontroller-apibilling)
7. [CategoriesController (`api/Categories`)](#7-categoriescontroller-apicategories)
8. [ChatController (`api/chat`)](#8-chatcontroller-apichat)
9. [ClientController (`api/client`)](#9-clientcontroller-apiclient)
10. [ContractsController (`api/Contracts`)](#10-contractscontroller-apicontracts)
11. [DeliveriesController (`api/deliveries`)](#11-deliveriescontroller-apideliveries)
12. [DisputesController (`api/disputes`)](#12-disputescontroller-apidisputes)
13. [FreelancerDiscoveryController (`api/client/freelancers`)](#13-freelancerdiscoverycontroller-apiclientfreelancers)
14. [JobsController (`api/Jobs`)](#14-jobscontroller-apijobs)
15. [MilestonesController (`api/Milestones`)](#15-milestonescontroller-apimilestones)
16. [ProposalsController (`api/Proposals`)](#16-proposalscontroller-apiproposals)
17. [RecommendationsController (`api/Recommendations`)](#17-recommendationscontroller-apirecommendations)
18. [RevisionsController (`api/Revisions`)](#18-revisionscontroller-apirevisions)
19. [ServicesController (`api/Services`)](#19-servicescontroller-apiservices)
20. [SkillsController (`api/Skills`)](#20-skillscontroller-apiskills)
21. [VerificationController (`api/Verification`)](#21-verificationcontroller-apiverification)

---

## 1. AdminBillingController (`api/admin/billing`)
**Authorization**: `AdminOnly`

### GET `/api/admin/billing/deposit-requests/pending`
* **Action Method**: `GetPendingDeposits`
* **Summary**: Retrieves a list of all pending deposit requests.
* **Request Body / Parameters**: None
* **Expected Responses**:
  * `200 OK`: Returns `IEnumerable<DepositRequestDto>`

### PATCH `/api/admin/billing/deposit-requests/{id}/review`
* **Action Method**: `ReviewDeposit`
* **Summary**: Approves or rejects a pending deposit request.
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (string)
  * **Request Body (JSON)**: `ReviewDepositRequestCommandDto`
    ```json
    {
      "status": 0, // DepositStatus Enum: e.g. Approved, Rejected
      "adminNote": "string"
    }
    ```
* **Expected Responses**:
  * `200 OK`: Returns `DepositRequestDto`

### GET `/api/admin/billing/withdrawal-requests/pending`
* **Action Method**: `GetPendingWithdrawals`
* **Summary**: Retrieves a list of all pending withdrawal requests.
* **Request Body / Parameters**: None
* **Expected Responses**:
  * `200 OK`: Returns `IEnumerable<WithdrawalRequestDto>`

### PATCH `/api/admin/billing/withdrawal-requests/{id}/review`
* **Action Method**: `ReviewWithdrawal`
* **Summary**: Approves or rejects a pending withdrawal request.
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (string)
  * **Request Body (JSON)**: `ReviewWithdrawalRequestCommandDto`
    ```json
    {
      "status": 0, // WithdrawalStatus Enum: e.g. Approved, Rejected
      "adminNote": "string"
    }
    ```
* **Expected Responses**:
  * `200 OK`: Returns `WithdrawalRequestDto`

---

## 2. AuthController (`api/Auth`)
**Authorization**: Guest / Authorized (endpoint-dependent)

### POST `/api/Auth/change-password`
* **Action Method**: `ChangePassword`
* **Summary**: Changes the password for the currently logged-in user.
* **Request Body / Parameters**:
  * **Request Body (JSON)**: `ChangePasswordRequestDTO`
    ```json
    {
      "currentPassword": "string",
      "newPassword": "string"
    }
    ```
* **Expected Responses**:
  * `200 OK`: Success
  * `400 Bad Request`: Validation or execution failure
  * `401 Unauthorized`: User ID claim missing or invalid

### POST `/api/Auth/register`
* **Action Method**: `Register`
* **Summary**: Registers a new user. Email confirmation is sent from within the service.
* **Request Body / Parameters**:
  * **Request Body (JSON)**: `RegisterRequestDto`
    ```json
    {
      "email": "string",
      "password": "string",
      "fullName": "string",
      "role": "string" // e.g. Client, Freelancer
    }
    ```
* **Expected Responses**:
  * `200 OK`: Registration successful
  * `400 Bad Request`: Registration failure (returns validation errors)

### PATCH `/api/Auth/change-email`
* **Action Method**: `ChangeEmail`
* **Summary**: Changes the email of a user using a verification token.
* **Request Body / Parameters**:
  * **Query Parameters**:
    * `userId` (string)
    * `newEmail` (string)
    * `token` (string)
* **Expected Responses**:
  * `200 OK`: Email changed successfully
  * `400 Bad Request`: Action failed
  * `401 Unauthorized`: User not found or deleted

### POST `/api/Auth/confirm-email`
* **Action Method**: `ConfirmEmail`
* **Summary**: Confirms a user's email address using a verification token.
* **Request Body / Parameters**:
  * **Query Parameters**:
    * `userId` (string)
    * `token` (string)
* **Expected Responses**:
  * `200 OK`: Email confirmed successfully
  * `400 Bad Request`: Confirmation failed

### POST `/api/Auth/resend-confirmation-email`
* **Action Method**: `ResendConfirmationEmail`
* **Summary**: Resends the email confirmation link to the user.
* **Request Body / Parameters**:
  * **Query Parameters**:
    * `email` (string)
* **Expected Responses**:
  * `200 OK`: Confirmation email resent
  * `400 Bad Request`: Resend process failed

### POST `/api/Auth/login`
* **Action Method**: `Login`
* **Summary**: Authenticates a user, sets a secure HTTP-Only refresh token cookie, and returns a JWT.
* **Request Body / Parameters**:
  * **Request Body (JSON)**: `LoginRequestDTO`
    ```json
    {
      "email": "string",
      "password": "string"
    }
    ```
* **Expected Responses**:
  * `200 OK`: Returns JWT token string
  * `400 Bad Request`: Invalid credentials or login failure

### POST `/api/Auth/logout`
* **Action Method**: `Logout`
* **Summary**: Invalidates refresh token and deletes the HTTP-Only refresh cookie.
* **Request Body / Parameters**: None (reads `refreshToken` from Cookie)
* **Expected Responses**:
  * `200 OK`: Successfully logged out

### POST `/api/Auth/refresh-token`
* **Action Method**: `RefreshToken`
* **Summary**: Refreshes the JWT token using the refresh token stored in cookies.
* **Request Body / Parameters**: None (reads `refreshToken` from Cookie)
* **Expected Responses**:
  * `200 OK`: Returns new JWT token string
  * `401 Unauthorized`: Refresh token is missing or invalid

---

## 3. ExperienceController (`api/Experience`)
**Authorization**: `[Authorize]`

### GET `/api/Experience`
* **Action Method**: `GetExperience`
* **Summary**: Retrieves professional experience records for the logged-in freelancer.
* **Request Body / Parameters**: None
* **Expected Responses**:
  * `200 OK`: Returns experience records list
  * `400 Bad Request`: Service failed to fetch records

### DELETE `/api/Experience/{id}`
* **Action Method**: `DeleteExperience`
* **Summary**: Soft deletes a professional experience record by ID.
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (string)
* **Expected Responses**:
  * `204 No Content`: Successfully deleted
  * `400 Bad Request`: Deletion failed

---

## 4. PortfolioController (`api/Portfolio`)
**Authorization**: `[Authorize]`

### GET `/api/Portfolio`
* **Action Method**: `GetPortfolio`
* **Summary**: Retrieves the portfolio items for the logged-in freelancer.
* **Request Body / Parameters**: None
* **Expected Responses**:
  * `200 OK`: Returns `IEnumerable<PortfolioItemDto>`
  * `404 Not Found`: Freelancer profile not found or deleted

### GET `/api/Portfolio/{id}`
* **Action Method**: `GetById`
* **Summary**: Retrieves a specific portfolio item by ID.
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (string)
* **Expected Responses**:
  * `200 OK`: Returns `PortfolioItemDto`
  * `404 Not Found`: Freelancer profile or portfolio item missing/deleted

### POST `/api/Portfolio`
* **Action Method**: `Create`
* **Summary**: Uploads new portfolio items with media files (images/videos/thumbnail).
* **Request Body / Parameters**:
  * **Content-Type**: `multipart/form-data`
  * **Form Fields**:
    * `title` (string, Required)
    * `description` (string, Required)
    * `role` (string, Optional)
    * `visitLink` (string, Optional)
    * `thumbnail` (IFormFile, Required)
    * `images` (List<IFormFile>, Optional)
    * `videos` (List<IFormFile>, Optional)
* **Expected Responses**:
  * `201 CreatedAtAction`: Returns created `PortfolioItemDto`
  * `400 Bad Request`: Validation errors
  * `404 Not Found`: Freelancer profile not found

### PUT `/api/Portfolio/{id}`
* **Action Method**: `Update`
* **Summary**: Updates an existing portfolio item, replacing its media files.
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (string)
  * **Content-Type**: `multipart/form-data`
  * **Form Fields**: Same as Create (but `thumbnail` is optional here)
* **Expected Responses**:
  * `200 OK`: Returns updated `PortfolioItemDto`
  * `400 Bad Request`: Validation errors
  * `404 Not Found`: Profile or portfolio item not found

### DELETE `/api/Portfolio/{id}`
* **Action Method**: `Delete`
* **Summary**: Soft deletes a portfolio item.
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (string)
* **Expected Responses**:
  * `204 No Content`: Successfully deleted
  * `404 Not Found`: Profile or portfolio item not found

---

## 5. UserProfileController (`api/UserProfile`)
**Authorization**: `[Authorize]`

### GET `/api/UserProfile`
* **Action Method**: `GetProfile`
* **Summary**: Retrieves the full profile details of the logged-in user.
* **Request Body / Parameters**: None
* **Expected Responses**:
  * `200 OK`: Returns profile details
  * `404 Not Found`: Profile retrieval error

### GET `/api/UserProfile/freelancer-details`
* **Action Method**: `GetFreelancerDetails`
* **Summary**: Retrieves freelancer-specific details of the logged-in user.
* **Request Body / Parameters**: None
* **Expected Responses**:
  * `200 OK`: Returns freelancer details
  * `400 Bad Request`: Service error

### GET `/api/UserProfile/public/{userIdHash}`
* **Action Method**: `GetPublicProfile`
* **Summary**: Retrieves public profile details by hashed user ID. (Anonymous allowed)
* **Request Body / Parameters**:
  * **Path Parameter**: `userIdHash` (string)
* **Expected Responses**:
  * `200 OK`: Returns public details
  * `404 Not Found`: Profile not found

### PATCH `/api/UserProfile/name`
* **Action Method**: `UpdateName`
* **Summary**: Updates the logged-in user's full name.
* **Request Body / Parameters**:
  * **Request Body (JSON)**: `fullname` (string)
* **Expected Responses**:
  * `200 OK`: Returns updated name details
  * `400 Bad Request`: Invalid request state
  * `404 Not Found`: User not found

### PATCH `/api/UserProfile/email`
* **Action Method**: `UpdateEmail`
* **Summary**: Starts the process to update the user's email address.
* **Request Body / Parameters**:
  * **Request Body (JSON)**: `email` (string)
* **Expected Responses**:
  * `200 OK`: Returns status update details
  * `400 Bad Request`: Invalid request state
  * `404 Not Found`: User not found

### PATCH `/api/UserProfile/title`
* **Action Method**: `UpdateTitle`
* **Summary**: Updates the freelancer's professional title.
* **Request Body / Parameters**:
  * **Request Body (JSON)**: `title` (string)
* **Expected Responses**:
  * `200 OK`: Returns updated title
  * `404 Not Found`: User not found

### PATCH `/api/UserProfile/bio`
* **Action Method**: `UpdateBio`
* **Summary**: Updates the freelancer's biography.
* **Request Body / Parameters**:
  * **Request Body (JSON)**: `bio` (string or null)
* **Expected Responses**:
  * `200 OK`: Returns updated bio
  * `404 Not Found`: User not found

### PATCH `/api/UserProfile/experience-level`
* **Action Method**: `UpdateExperienceLevel`
* **Summary**: Updates the freelancer's experience level.
* **Request Body / Parameters**:
  * **Request Body (JSON)**: `ExperienceUpdateDto`
    ```json
    {
      "experienceLevel": 0 // Experience level code
    }
    ```
* **Expected Responses**:
  * `200 OK`: Returns updated experience level data
  * `400 Bad Request`: Error processing update

### POST `/api/UserProfile/payment-method`
* **Action Method**: `CreatePaymentMethod`
* **Summary**: Configures a new payment/billing method for the user.
* **Request Body / Parameters**:
  * **Request Body (JSON)**: `PaymentMethodCreateDTO`
    ```json
    {
      "provider": "string",
      "accountNumber": "string",
      "details": "string"
    }
    ```
* **Expected Responses**:
  * `200 OK`: Returns created payment method details
  * `404 Not Found`: User not found

### PUT `/api/UserProfile/payment-method/{id}`
* **Action Method**: `UpdatePaymentMethod`
* **Summary**: Updates an existing billing method.
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (string)
  * **Request Body (JSON)**: `PaymentMethodUpdateDTO`
    ```json
    {
      "provider": "string",
      "accountNumber": "string",
      "details": "string"
    }
    ```
* **Expected Responses**:
  * `200 OK`: Returns updated details
  * `404 Not Found`: User not found

### DELETE `/api/UserProfile/payment-method/{id}`
* **Action Method**: `DeletePaymentMethod`
* **Summary**: Deletes a billing payment method.
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (string)
* **Expected Responses**:
  * `204 No Content`: Successfully deleted
  * `404 Not Found`: User not found

### PATCH `/api/UserProfile/location`
* **Action Method**: `UpdateLocation`
* **Summary**: Updates the location settings.
* **Request Body / Parameters**:
  * **Request Body (JSON)**: `LocationUpdateDto`
    ```json
    {
      "country": "string",
      "city": "string",
      "timeZone": "string"
    }
    ```
* **Expected Responses**:
  * `200 OK`: Success
  * `400 Bad Request`: Invalid request state
  * `404 Not Found`: User not found

### PATCH `/api/UserProfile/freelancer-details`
* **Action Method**: `UpdateFreelancerDetails`
* **Summary**: Updates overall freelancer configuration metrics.
* **Request Body / Parameters**:
  * **Request Body (JSON)**: `FreelancerUpdateDTO`
    ```json
    {
      "hourlyRate": 0.0,
      "skills": ["string"]
    }
    ```
* **Expected Responses**:
  * `200 OK`: Returns updated freelancer details
  * `400 Bad Request`: Model invalidity or service error

---

## 6. BillingController (`api/Billing`)
**Authorization**: `[Authorize]`

### POST `/api/Billing/deposit-requests`
* **Action Method**: `SubmitDeposit`
* **Summary**: Submits a manual deposit request with a receipt attachment. (Clients only)
* **Request Body / Parameters**:
  * **Content-Type**: `multipart/form-data`
  * **Form Fields**: maps to `SubmitDepositRequestCommand`
    * `amount` (decimal, Required)
    * `receiptFile` (IFormFile, Required)
* **Expected Responses**:
  * `201 CreatedAtAction`: Returns `DepositRequestDto`
  * `400 Bad Request`: Execution failure
  * `401 Unauthorized`: Client identity missing

### GET `/api/Billing/deposit-requests/my-requests`
* **Action Method**: `GetMyDeposits`
* **Summary**: Retrieves deposit history for the logged-in client. (Clients only)
* **Request Body / Parameters**:
  * **Query Parameters**:
    * `page` (int, default 1)
    * `pageSize` (int, default 10)
* **Expected Responses**:
  * `200 OK`: Returns `PagedResult<DepositRequestDto>`
  * `400 Bad Request`: Failure fetching history
  * `401 Unauthorized`: Client identity missing

### POST `/api/Billing/withdrawal-requests`
* **Action Method**: `SubmitWithdrawal`
* **Summary**: Requests a payout/withdrawal. (Freelancers only)
* **Request Body / Parameters**:
  * **Request Body (JSON)**: `SubmitWithdrawalRequestCommand`
    ```json
    {
      "amount": 0.0,
      "payoutDetails": "string"
    }
    ```
* **Expected Responses**:
  * `201 CreatedAtAction`: Returns `WithdrawalRequestDto`
  * `400 Bad Request`: Error submitting request
  * `401 Unauthorized`: Freelancer identity missing

### GET `/api/Billing/withdrawal-requests/my-requests`
* **Action Method**: `GetMyWithdrawals`
* **Summary**: Retrieves withdrawal requests history. (Freelancers only)
* **Request Body / Parameters**:
  * **Query Parameters**:
    * `page` (int, default 1)
    * `pageSize` (int, default 10)
* **Expected Responses**:
  * `200 OK`: Returns `PagedResult<WithdrawalRequestDto>`
  * `400 Bad Request`: Failure fetching history
  * `401 Unauthorized`: Freelancer identity missing

### GET `/api/Billing/wallet-balance`
* **Action Method**: `GetWalletBalance`
* **Summary**: Returns current wallet balances.
* **Request Body / Parameters**: None
* **Expected Responses**:
  * `200 OK`: Returns `WalletBalanceDto`
  * `400 Bad Request`: Error fetching balance
  * `401 Unauthorized`: User identity missing

---

## 7. CategoriesController (`api/Categories`)
**Authorization**: Guest / Admin (endpoint-dependent)

### GET `/api/Categories`
* **Action Method**: `GetAll`
* **Summary**: Retrieves all active categories. (Anonymous allowed)
* **Request Body / Parameters**: None
* **Expected Responses**:
  * `200 OK`: Returns list of categories

### GET `/api/Categories/{id}`
* **Action Method**: `GetById`
* **Summary**: Retrieves a category by ID. (Anonymous allowed)
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (string)
* **Expected Responses**:
  * `200 OK`: Returns category details
  * `404 Not Found`: Category not found

### POST `/api/Categories`
* **Action Method**: `Create`
* **Summary**: Creates a new category. (Admin only)
* **Request Body / Parameters**:
  * **Request Body (JSON)**: `CreateCategoryDto`
    ```json
    {
      "name": "string",
      "description": "string"
    }
    ```
* **Expected Responses**:
  * `201 CreatedAtAction`: Returns category details
  * `400 Bad Request`: Creation failed

### PUT `/api/Categories/{id}`
* **Action Method**: `Update`
* **Summary**: Modifies a category's properties. (Admin only)
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (string)
  * **Request Body (JSON)**: `UpdateCategoryDto`
    ```json
    {
      "name": "string",
      "description": "string"
    }
    ```
* **Expected Responses**:
  * `200 OK`: Returns updated category details
  * `400 Bad Request`: Update failed

### DELETE `/api/Categories/{id}`
* **Action Method**: `Delete`
* **Summary**: Soft deletes a category. (Admin only)
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (string)
* **Expected Responses**:
  * `200 OK`: Soft deletion successful
  * `400 Bad Request`: Deletion failed

---

## 8. ChatController (`api/chat`)
**Authorization**: `[Authorize]`

### GET `/api/chat`
* **Action Method**: `GetChats`
* **Summary**: Retrieves lists of user chat threads.
* **Request Body / Parameters**:
  * **Query Parameters**:
    * `role` (UserRole enum, default Client: e.g. Client, Freelancer)
* **Expected Responses**:
  * `200 OK`: Returns `IEnumerable<ChatSummaryDto>`
  * `400 Bad Request`: Fetch failure
  * `401 Unauthorized`: User ID missing

### GET `/api/chat/{chatId}/messages`
* **Action Method**: `GetMessages`
* **Summary**: Retrieves history of text and media messages in a conversation thread.
* **Request Body / Parameters**:
  * **Path Parameter**: `chatId` (string)
  * **Query Parameters**:
    * `page` (int, default 1)
    * `pageSize` (int, default 30)
* **Expected Responses**:
  * `200 OK`: Returns `PagedResult<MessageDto>`
  * `403 Forbidden`: Unauthorized to view this chat
  * `404 Not Found`: Conversation not found

### POST `/api/chat/{chatId}/messages/text`
* **Action Method**: `SendTextMessage`
* **Summary**: Sends a new text message.
* **Request Body / Parameters**:
  * **Path Parameter**: `chatId` (string)
  * **Request Body (JSON)**: `SendTextMessageRequest`
    ```json
    {
      "text": "string"
    }
    ```
* **Expected Responses**:
  * `201 Created`: Returns `MessageDto`
  * `400 Bad Request`: Invalid request
  * `403 Forbidden`: Unauthorized to chat in this channel
  * `404 Not Found`: Conversation not found

### POST `/api/chat/{chatId}/messages/file`
* **Action Method**: `SendFileMessage`
* **Summary**: Uploads and sends a file inside a chat thread.
* **Request Body / Parameters**:
  * **Path Parameter**: `chatId` (string)
  * **Content-Type**: `multipart/form-data`
  * **Form Fields**:
    * `file` (IFormFile, Required)
* **Expected Responses**:
  * `201 Created`: Returns `MessageDto`
  * `400 Bad Request`: Missing file or invalid format/size
  * `403 Forbidden`: Unauthorized to access this chat
  * `404 Not Found`: Conversation not found

---

## 9. ClientController (`api/client`)
**Authorization**: `ClientOnly` Policy

### GET `/api/client/me`
* **Action Method**: `GetMe`
* **Summary**: Retrieves profile info of the logged-in client.
* **Request Body / Parameters**: None
* **Expected Responses**:
  * `200 OK`: Returns client profile data
  * `400 Bad Request`: Fetch failed

### GET `/api/client/onboarding`
* **Action Method**: `GetOnboarding`
* **Summary**: Retrieves onboarding profile configuration status.
* **Request Body / Parameters**: None
* **Expected Responses**:
  * `200 OK`: Returns onboarding information
  * `400 Bad Request`: Fetch failed

### GET `/api/client/jobs`
* **Action Method**: `GetClientJobs`
* **Summary**: Retrieves all jobs posted by this client.
* **Request Body / Parameters**: None
* **Expected Responses**:
  * `200 OK`: Returns jobs list
  * `400 Bad Request`: Fetch failed

### GET `/api/client/proposals`
* **Action Method**: `GetClientProposals`
* **Summary**: Retrieves all job proposals submitted to this client's jobs.
* **Request Body / Parameters**: None
* **Expected Responses**:
  * `200 OK`: Returns `IEnumerable<ClientProposalSummaryDto>`
  * `400 Bad Request`: Fetch failed

---

## 10. ContractsController (`api/Contracts`)
**Authorization**: `[Authorize]`

### GET `/api/Contracts/my-contracts`
* **Action Method**: `GetMyContracts`
* **Summary**: Retrieves lists of user contract agreements.
* **Request Body / Parameters**:
  * **Query Parameters**:
    * `status` (ContractStatus enum, Optional)
    * `page` (int, default 1)
    * `pageSize` (int, default 10)
* **Expected Responses**:
  * `200 OK`: Returns contract details
  * `400 Bad Request`: Fetch failed

### GET `/api/Contracts/{id}`
* **Action Method**: `GetContractById`
* **Summary**: Retrieves a single contract detail.
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (int)
* **Expected Responses**:
  * `200 OK`: Returns `ContractReadDTO`
  * `404 Not Found`: Contract not found

### POST `/api/Contracts/{id}/accept-offer`
* **Action Method**: `AcceptOffer`
* **Summary**: Freelancer accepts a contract offer.
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (int)
* **Expected Responses**:
  * `201 Created`: Returns boolean success status
  * `400 Bad Request`: Error accepting offer

### POST `/api/Contracts/{id}/decline-offer`
* **Action Method**: `DeclineOffer`
* **Summary**: Freelancer declines a contract offer.
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (int)
* **Expected Responses**:
  * `204 No Content`: Offer declined successfully
  * `400 Bad Request`: Error declining offer

### POST `/api/Contracts/{id}/revoke-offer`
* **Action Method**: `RevokeOffer`
* **Summary**: Client revokes a pending contract offer. (Clients only)
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (int)
* **Expected Responses**:
  * `204 No Content`: Offer revoked successfully
  * `400 Bad Request`: Error revoking offer

### POST `/api/Contracts/{id}/deliver-work`
* **Action Method**: `DeliverWork`
* **Summary**: Freelancer submits deliverables/work.
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (int)
  * **Content-Type**: `multipart/form-data`
  * **Form Fields**:
    * `note` (string, Optional)
    * `files` (List<IFormFile>, Required)
* **Expected Responses**:
  * `201 Created`: Returns `WorkDeliveryDto`
  * `400 Bad Request`: Submission failed

### GET `/api/Contracts/{id}/deliveries/{deliveryId}/attachments/{attachmentId}/download`
* **Action Method**: `DownloadAttachment`
* **Summary**: Downloads a file submitted with a work delivery.
* **Request Body / Parameters**:
  * **Path Parameters**:
    * `id` (int)
    * `deliveryId` (int)
    * `attachmentId` (Guid)
* **Expected Responses**:
  * `200 OK`: Returns physical file binary payload
  * `403 Forbidden`: Unauthorized to download this file
  * `404 Not Found`: File or delivery does not exist

### POST `/api/Contracts/{id}/complete`
* **Action Method**: `CompleteContract`
* **Summary**: Client accepts work delivery and completes the contract.
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (int)
* **Expected Responses**:
  * `204 No Content`: Contract completed successfully
  * `400 Bad Request`: Process failed

### POST `/api/Contracts/{id}/reject`
* **Action Method**: `RejectContract`
* **Summary**: Rejects a work delivery.
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (int)
* **Expected Responses**:
  * `204 No Content`: Work rejected successfully
  * `400 Bad Request`: Process failed

### POST `/api/Contracts/{id}/reviews`
* **Action Method**: `SubmitReview`
* **Summary**: Submits a contract review.
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (int)
  * **Request Body (JSON)**: `ContractReviewCreateDTO`
    ```json
    {
      "rating": 5, // e.g. 1 to 5
      "comment": "string"
    }
    ```
* **Expected Responses**:
  * `201 Created`: Returns `ContractReviewReadDTO`
  * `400 Bad Request`: Process failed
  * `409 Conflict`: Review already submitted for this contract

### POST `/api/Contracts/create-offer`
* **Action Method**: `CreateDirectOffer`
* **Summary**: Posts a direct job offer to a freelancer. (Clients only)
* **Request Body / Parameters**:
  * **Request Body (JSON)**: `CreateDirectOfferCommand`
    ```json
    {
      "freelancerId": "string",
      "title": "string",
      "description": "string",
      "budget": 0.0,
      "deadline": "2026-06-13T00:00:00Z"
    }
    ```
* **Expected Responses**:
  * `201 Created`: Returns `ContractDto`
  * `400 Bad Request`: Creation failed

### GET `/api/Contracts/{id}/deliveries`
* **Action Method**: `GetContractDeliveries`
* **Summary**: Retrieves all work deliveries of a contract.
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (int)
* **Expected Responses**:
  * `200 OK`: Returns `IEnumerable<ContractDeliveryDto>`

### GET `/api/Contracts/{id}/escrow`
* **Action Method**: `GetEscrowSummary`
* **Summary**: Retrieves status of funds held in escrow.
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (int)
* **Expected Responses**:
  * `200 OK`: Returns `EscrowSummaryDto`

---

## 11. DeliveriesController (`api/deliveries`)
**Authorization**: `[Authorize]`

### POST `/api/deliveries/submit`
* **Action Method**: `Submit`
* **Summary**: Freelancer uploads final delivery. (Freelancers only)
* **Request Body / Parameters**:
  * **Request Body (JSON)**: `SubmitDeliveryRequest`
    ```json
    {
      "contractId": 0,
      "contractMilestoneId": "00000000-0000-0000-0000-000000000000",
      "deliveryNote": "string",
      "attachments": [
        {
          "fileName": "string",
          "fileUrl": "string"
        }
      ]
    }
    ```
* **Expected Responses**:
  * `201 Created`: Returns `ContractDeliveryDto`

### POST `/api/deliveries/{deliveryId}/approve`
* **Action Method**: `Approve`
* **Summary**: Approves a work delivery. (Clients only)
* **Request Body / Parameters**:
  * **Path Parameter**: `deliveryId` (Guid)
* **Expected Responses**:
  * `200 OK`: Returns `ContractDeliveryDto`

### POST `/api/deliveries/{deliveryId}/revision`
* **Action Method**: `RequestRevision`
* **Summary**: Requests changes on a delivery. (Clients only)
* **Request Body / Parameters**:
  * **Path Parameter**: `deliveryId` (Guid)
  * **Request Body (JSON)**: `RequestRevisionRequest`
    ```json
    {
      "reason": "string"
    }
    ```
* **Expected Responses**:
  * `201 Created`: Returns `RevisionRequestDto`

### POST `/api/deliveries/{deliveryId}/dispute`
* **Action Method**: `OpenDispute`
* **Summary**: Opens a formal dispute on a delivery. (Clients or Freelancers)
* **Request Body / Parameters**:
  * **Path Parameter**: `deliveryId` (Guid)
  * **Request Body (JSON)**: `OpenDisputeRequest`
    ```json
    {
      "contractId": 0,
      "reason": "string"
    }
    ```
* **Expected Responses**:
  * `201 Created`: Returns `DisputeDto`

---

## 12. DisputesController (`api/disputes`)
**Authorization**: `Admin` role only

### POST `/api/disputes/{disputeId}/resolve`
* **Action Method**: `Resolve`
* **Summary**: Decides the outcome of a dispute.
* **Request Body / Parameters**:
  * **Path Parameter**: `disputeId` (Guid)
  * **Request Body (JSON)**: `ResolveDisputeRequest`
    ```json
    {
      "decision": 0, // DisputeDecision Enum
      "adminDecision": "string"
    }
    ```
* **Expected Responses**:
  * `200 OK`: Returns resolved `DisputeDto`

---

## 13. FreelancerDiscoveryController (`api/client/freelancers`)
**Authorization**: `ClientOnly` Policy

### GET `/api/client/freelancers/search`
* **Action Method**: `SearchFreelancers`
* **Summary**: Filtered keyword search for freelancers.
* **Request Body / Parameters**:
  * **Query Parameters**:
    * `searchQuery` (string, Optional)
    * `skillIds` (List of strings, Optional)
    * `minHourlyRate` (decimal, Optional)
    * `maxHourlyRate` (decimal, Optional)
    * `minYearsExperience` (int, Optional)
    * `minTrustScore` (decimal, Optional)
    * `isVerified` (bool, Optional)
    * `sortBy` (string, default: "TrustScore")
    * `sortDescending` (bool, default: true)
    * `page` (int, default: 1)
    * `pageSize` (int, default: 10)
* **Expected Responses**:
  * `200 OK`: Returns `PagedResult<FreelancerSearchResultDTO>`
  * `400 Bad Request`: Process error

### POST `/api/client/freelancers/{freelancerId}/save`
* **Action Method**: `SaveFreelancer`
* **Summary**: Adds a freelancer to the client's bookmarks.
* **Request Body / Parameters**:
  * **Path Parameter**: `freelancerId` (string)
* **Expected Responses**:
  * `200 OK`: Returns confirmation message
  * `400 Bad Request`: Save failed

### DELETE `/api/client/freelancers/{freelancerId}/unsave`
* **Action Method**: `UnsaveFreelancer`
* **Summary**: Removes freelancer from saved bookmarks list.
* **Request Body / Parameters**:
  * **Path Parameter**: `freelancerId` (string)
* **Expected Responses**:
  * `200 OK`: Returns confirmation message
  * `400 Bad Request`: Unsave failed

### GET `/api/client/freelancers/saved`
* **Action Method**: `GetSavedFreelancers`
* **Summary**: Lists all saved freelancers.
* **Request Body / Parameters**:
  * **Query Parameters**:
    * `page` (int, default: 1)
    * `pageSize` (int, default: 10)
* **Expected Responses**:
  * `200 OK`: Returns `PagedResult<FreelancerSearchResultDTO>`
  * `400 Bad Request`: Fetch failed

---

## 14. JobsController (`api/Jobs`)
**Authorization**: Guest / Authorized (endpoint-dependent)

### GET `/api/Jobs/jobs`
* **Action Method**: `GetJobs`
* **Summary**: Search active job listings. (Anonymous allowed)
* **Request Body / Parameters**:
  * **Query Parameters**: Maps to `SearchJobsQuery` model (filters like keywords, category, budget, etc.)
* **Expected Responses**:
  * `200 OK`: Returns `SearchJobsQueryResponse`
  * `400 Bad Request`: Search failed

### POST `/api/Jobs/create-job`
* **Action Method**: `CreateJob`
* **Summary**: Creates a job post. (Clients only)
* **Request Body / Parameters**:
  * **Request Body (JSON)**: `JobDetailsDto`
    ```json
    {
      "title": "string",
      "description": "string",
      "budget": 0.0,
      "categoryId": "string",
      "requiredSkills": ["string"]
    }
    ```
* **Expected Responses**:
  * `201 CreatedAtAction`: Returns created `JobDetailsDto`
  * `400 Bad Request`: Insufficient funds or execution error

### GET `/api/Jobs/jobs/{id}`
* **Action Method**: `GetJob`
* **Summary**: Retrieves detailed job description by ID. (Anonymous allowed)
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (string)
* **Expected Responses**:
  * `200 OK`: Returns `JobDetailsDto`
  * `400 Bad Request`: Job not found or error

### POST `/api/Jobs/{id}/save-job`
* **Action Method**: `SaveJob`
* **Summary**: Saves job to freelancer bookmarks. (Freelancers only)
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (string)
* **Expected Responses**:
  * `200 OK`: Returns updated details
  * `400 Bad Request`: Process failed

### DELETE `/api/Jobs/{id}/unsave-job`
* **Action Method**: `UnsaveJob`
* **Summary**: Removes job from freelancer bookmarks. (Freelancers only)
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (string)
* **Expected Responses**:
  * `200 OK`: Returns updated details
  * `400 Bad Request`: Process failed

### GET `/api/Jobs/{id}/proposals`
* **Action Method**: `GetJobProposals`
* **Summary**: Retrieves proposals submitted to this job. (Clients only)
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (string)
  * **Query Parameters**:
    * `page` (int, default: 1)
    * `pageSize` (int, default: 10)
* **Expected Responses**:
  * `200 OK`: Returns paged proposals list
  * `400 Bad Request`: Process failed

### PUT `/api/Jobs/update-job/{id}`
* **Action Method**: `UpdateJob`
* **Summary**: Updates job properties. (Clients only)
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (string)
  * **Request Body (JSON)**: `JobDetailsDto`
    ```json
    {
      "title": "string",
      "description": "string",
      "budget": 0.0,
      "categoryId": "string",
      "requiredSkills": ["string"]
    }
    ```
* **Expected Responses**:
  * `200 OK`: Returns updated `JobDetailsDto`
  * `400 Bad Request`: Update failed

### DELETE `/api/Jobs/delete-job/{id}`
* **Action Method**: `DeleteJob`
* **Summary**: Deletes a job posting. (Clients only)
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (string)
* **Expected Responses**:
  * `200 OK`: Deleted successfully
  * `400 Bad Request`: Deletion failed

---

## 15. MilestonesController (`api/Milestones`)
**Authorization**: `[Authorize]`

### POST `/api/Milestones/{milestoneId}/fund`
* **Action Method**: `Fund`
* **Summary**: Transfers funds from the client's balance to the milestone escrow. (Clients only)
* **Request Body / Parameters**:
  * **Path Parameter**: `milestoneId` (Guid)
* **Expected Responses**:
  * `200 OK`: Funding successful
  * `400 Bad Request`: Parsing client identity error or processing failures

---

## 16. ProposalsController (`api/Proposals`)
**Authorization**: `[Authorize]`

### POST `/api/Proposals`
* **Action Method**: `Create`
* **Summary**: Creates and submits a new proposal.
* **Request Body / Parameters**:
  * **Request Body (JSON)**: `ProposalCreateDTO`
    ```json
    {
      "jobId": "string",
      "coverLetter": "string",
      "bidAmount": 0.0,
      "estimatedDurationDays": 0
    }
    ```
* **Expected Responses**:
  * `201 CreatedAtAction`: Returns `ProposalReadDTO`
  * `400 Bad Request`: Invalid configuration
  * `409 Conflict`: Proposal already submitted for this job

### GET `/api/Proposals/my-proposals`
* **Action Method**: `GetMyProposals`
* **Summary**: Retrieves proposals submitted by the logged-in freelancer.
* **Request Body / Parameters**: None
* **Expected Responses**:
  * `200 OK`: Returns `MyProposalsResponseDto`
  * `400 Bad Request`: Fetch failed

### DELETE `/api/Proposals/{id}/withdraw`
* **Action Method**: `Withdraw`
* **Summary**: Withdraws a submitted proposal.
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (int)
* **Expected Responses**:
  * `204 No Content`: Proposal withdrawn successfully
  * `400 Bad Request`: Withdrawal failed

### POST `/api/Proposals/{id}/reject`
* **Action Method**: `Reject`
* **Summary**: Rejects a submitted proposal. (Clients only)
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (int)
* **Expected Responses**:
  * `204 No Content`: Proposal rejected successfully
  * `400 Bad Request`: Rejection failed

---

## 17. RecommendationsController (`api/Recommendations`)
**Authorization**: `[Authorize]` (endpoint-dependent)

### GET `/api/Recommendations/jobs`
* **Action Method**: `GetRecommendedJobs`
* **Summary**: Gets job suggestions tailored to the freelancer. (Freelancers only)
* **Request Body / Parameters**: None
* **Expected Responses**:
  * `200 OK`: Returns list of recommended jobs

### GET `/api/Recommendations/freelancers`
* **Action Method**: `GetRecommendedFreelancers`
* **Summary**: Gets freelancer recommendations for the client's jobs. (Clients only)
* **Request Body / Parameters**: None
* **Expected Responses**:
  * `200 OK`: Returns list of suggested freelancers

### POST `/api/Recommendations/track`
* **Action Method**: `Track`
* **Summary**: Tracks page clicks, views, or user interactions to train recommendation models.
* **Request Body / Parameters**:
  * **Request Body (JSON)**: `TrackInteractionDTO`
    ```json
    {
      "interactionType": "string", // e.g. View, Click
      "targetId": "string"
    }
    ```
* **Expected Responses**:
  * `200 OK`: Interaction tracked successfully

---

## 18. RevisionsController (`api/Revisions`)
**Authorization**: `[Authorize]`

### GET `/api/Revisions/open`
* **Action Method**: `GetOpenRevisions`
* **Summary**: Retrieves active open revision requests. (Specialists only)
* **Request Body / Parameters**: None
* **Expected Responses**:
  * `200 OK`: Returns `IEnumerable<RevisionRequestDto>`

---

## 19. ServicesController (`api/Services`)
**Authorization**: `[Authorize]`

### POST `/api/Services`
* **Action Method**: `Create`
* **Summary**: Posts a new service listing with details and supporting media.
* **Request Body / Parameters**:
  * **Content-Type**: `multipart/form-data`
  * **Form Fields**: maps to `ServiceCreateDTO dto`
    * `title` (string, Required)
    * `description` (string, Required)
    * `price` (decimal, Required)
    * `deliveryDays` (int, Required)
    * `categoryId` (string, Required)
    * `coverImageFileName` (string, Optional)
  * **Form Files**:
    * `images` (List<IFormFile>, Optional)
    * `video` (IFormFile, Optional)
    * `documents` (List<IFormFile>, Optional)
* **Expected Responses**:
  * `201 CreatedAtAction`: Returns `ServiceCatalogItemDto`
  * `400 Bad Request`: Process error

### PUT `/api/Services/{id}`
* **Action Method**: `Update`
* **Summary**: Updates an existing service catalog listing.
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (string)
  * **Content-Type**: `multipart/form-data`
  * **Form Fields**: maps to `ServiceUpdateDTO dto` (same fields as Create, but editable)
  * **Form Files**: Same as Create
* **Expected Responses**:
  * `200 OK`: Returns updated `ServiceCatalogItemDto`
  * `400 Bad Request`: Validation errors
  * `404 Not Found`: Service listing not found

### DELETE `/api/Services/{id}`
* **Action Method**: `Delete`
* **Summary**: Deletes a service listing (soft or hard delete).
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (string)
* **Expected Responses**:
  * `204 No Content`: Deleted successfully
  * `404 Not Found`: Service listing not found

### GET `/api/Services/my-services`
* **Action Method**: `GetMyServices`
* **Summary**: Returns all services owned by the current freelancer.
* **Request Body / Parameters**: None
* **Expected Responses**:
  * `200 OK`: Returns `ServiceGroupedDto`

### GET `/api/Services/{id}`
* **Action Method**: `GetById`
* **Summary**: Retrieves details of a specific service listing by ID.
* **Request Body / Parameters**:
  * **Path Parameter**: `id` (string)
* **Expected Responses**:
  * `200 OK`: Returns `ServiceCatalogItemDto`
  * `404 Not Found`: Service listing not found

---

## 20. SkillsController (`api/Skills`)
**Authorization**: `[Authorize]`

### GET `/api/Skills`
* **Action Method**: `GetAllSkills`
* **Summary**: Returns a list of all available skills.
* **Request Body / Parameters**: None
* **Expected Responses**:
  * `200 OK`: Returns `IEnumerable<SkillDto>`

### GET `/api/Skills/category/{categoryId}`
* **Action Method**: `GetSkillsByCategory`
* **Summary**: Filter skills by category ID.
* **Request Body / Parameters**:
  * **Path Parameter**: `categoryId` (string)
* **Expected Responses**:
  * `200 OK`: Returns `IEnumerable<SkillDto>`

### GET `/api/Skills/my-skills`
* **Action Method**: `GetMySkills`
* **Summary**: Lists skills registered to the freelancer.
* **Request Body / Parameters**: None
* **Expected Responses**:
  * `200 OK`: Returns `IEnumerable<FreelancerSkillDto>`
  * `400 Bad Request` / `404 Not Found`: Freelancer profile does not exist

### POST `/api/Skills/my-skills`
* **Action Method**: `AddMySkill`
* **Summary**: Assigns a new skill to the freelancer.
* **Request Body / Parameters**:
  * **Request Body (JSON)**: `AddFreelancerSkillDto`
    ```json
    {
      "skillId": "string",
      "yearsOfExperience": 0
    }
    ```
* **Expected Responses**:
  * `201 CreatedAtAction`: Returns `FreelancerSkillDto`
  * `400 Bad Request`: Assigning failed
  * `404 Not Found`: Skill or profile not found
  * `409 Conflict`: Skill already assigned to the freelancer

### DELETE `/api/Skills/my-skills/{skillId}`
* **Action Method**: `DeleteMySkill`
* **Summary**: Disassociates a skill from the freelancer.
* **Request Body / Parameters**:
  * **Path Parameter**: `skillId` (string)
* **Expected Responses**:
  * `204 No Content`: Disassociation successful
  * `404 Not Found`: Freelancer profile not found

---

## 21. VerificationController (`api/Verification`)
**Authorization**: `[Authorize]` (endpoint-dependent)

### POST `/api/Verification/submit`
* **Action Method**: `Submit`
* **Summary**: Submits verification documents (Front ID, Back ID, and Selfie). (Freelancers only)
* **Request Body / Parameters**:
  * **Content-Type**: `multipart/form-data`
  * **Form Files**:
    * `frontImage` (IFormFile, Required)
    * `backImage` (IFormFile, Required)
    * `selfie` (IFormFile, Required)
* **Expected Responses**:
  * `201 CreatedAtAction`: Returns `VerificationRequestDto`
  * `400 Bad Request`: Validation or file format issues
  * `404 Not Found`: User missing
  * `409 Conflict`: Already verified or has a pending verification request

### GET `/api/Verification/my-status`
* **Action Method**: `GetMyStatus`
* **Summary**: Returns verification progress/status for the logged-in freelancer. (Freelancers only)
* **Request Body / Parameters**: None
* **Expected Responses**:
  * `200 OK`: Returns `VerificationRequestDto` (or `null` if no request submitted)
  * `404 Not Found`: User profile missing

### GET `/api/Verification/pending`
* **Action Method**: `GetPending`
* **Summary**: Retrieves all pending verification requests. (Admins only)
* **Request Body / Parameters**: None
* **Expected Responses**:
  * `200 OK`: Returns list of pending requests

### GET `/api/Verification/all`
* **Action Method**: `GetAll`
* **Summary**: Retrieves history of all verification requests. (Admins only)
* **Request Body / Parameters**: None
* **Expected Responses**:
  * `200 OK`: Returns list of all requests

### POST `/api/Verification/review`
* **Action Method**: `Review`
* **Summary**: Approves or Rejects a verification request. (Admins only)
* **Request Body / Parameters**:
  * **Request Body (JSON)**: `ReviewVerificationDto`
    ```json
    {
      "requestId": "00000000-0000-0000-0000-000000000000",
      "approved": true,
      "rejectionReason": "string" // Required if approved is false
    }
    ```
* **Expected Responses**:
  * `200 OK`: Returns reviewed `VerificationRequestDto`
  * `400 Bad Request`: Missing reason or request already reviewed
  * `404 Not Found`: Verification request not found
