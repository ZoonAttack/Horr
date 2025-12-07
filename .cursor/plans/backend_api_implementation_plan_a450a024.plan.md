---
name: Backend API Implementation Plan
overview: Create a comprehensive, ordered list of categorized tasks for implementing the backend API in C# ASP.NET based on the functional requirements from the project specification. The plan organizes tasks by functional requirement categories and includes foundational infrastructure setup.
todos:
  - id: foundation-db
    content: Configure database connection and Entity Framework Core setup
    status: pending
  - id: foundation-auth
    content: Set up JWT authentication and authorization infrastructure
    status: pending
  - id: foundation-services
    content: Create base services, middleware, and dependency injection setup
    status: pending
  - id: foundation-external
    content: Set up OTP service and payment gateway integration interfaces
    status: pending
  - id: user-registration
    content: Implement user registration, OTP verification, and login endpoints
    status: pending
  - id: freelancer-profile
    content: Implement freelancer profile management and skills endpoints
    status: pending
  - id: services-listings
    content: Implement predefined service creation and management endpoints
    status: pending
  - id: client-projects
    content: Implement client project posting and management endpoints
    status: pending
  - id: proposals
    content: Implement proposal submission, acceptance, and rejection endpoints
    status: pending
  - id: orders
    content: Implement service order creation and management endpoints
    status: pending
  - id: chat-system
    content: Implement chat and messaging endpoints with file attachments
    status: pending
  - id: reviews
    content: Implement review and rating endpoints with validation
    status: pending
  - id: wallets
    content: Implement wallet management and transaction endpoints
    status: pending
  - id: escrow
    content: Implement escrow payment holding and release logic
    status: pending
  - id: payments
    content: Implement payment processing with local payment methods integration
    status: pending
  - id: withdrawals
    content: Implement withdrawal request and processing endpoints
    status: pending
  - id: trust-score
    content: Implement trust score calculation and update system
    status: pending
  - id: search-filtering
    content: Implement search and filtering endpoints for freelancers, projects, and services
    status: pending
  - id: validation-security
    content: Add input validation, security middleware, and rate limiting
    status: pending
  - id: documentation
    content: Set up Swagger/OpenAPI documentation and add XML comments
    status: pending
---

# Backend API Implementation Plan

This plan organizes tasks by functional requirement categories for building the freelancing platform backend API in C# ASP.NET.

## Foundation & Infrastructure Setup

### Database & Configuration

1. Configure database connection string in `appsettings.json` and `appsettings.Development.json`
2. Add Entity Framework Core packages to `Horr.csproj` (Microsoft.EntityFrameworkCore.SqlServer or PostgreSQL)
3. Register `AppDbContext` in `Program.cs` with dependency injection
4. Create and run initial database migration
5. Configure CORS policies for web and mobile clients
6. Set up logging configuration (Serilog or built-in logging)

### Authentication & Authorization Infrastructure

7. Install and configure JWT authentication packages (Microsoft.AspNetCore.Authentication.JwtBearer)
8. Create JWT token service/helper for token generation and validation
9. Configure authentication middleware in `Program.cs`
10. Create authorization policies for role-based access (Client, Freelancer, Specialist)
11. Create custom authorization attributes/requirements if needed

### Core Services & Middleware

12. Create base service interface and implementation pattern
13. Implement global exception handling middleware
14. Implement request validation middleware
15. Configure AutoMapper for entity-to-DTO mapping (if not using extension methods)
16. Set up dependency injection container for services

### External Service Integration

17. Create OTP service interface and implementation (SMS/Email providers)
18. Configure OTP service settings in `appsettings.json`
19. Create payment gateway integration service interface (for Vodafone Cash, InstaPay, local bank wallets)
20. Configure payment gateway settings in `appsettings.json`

---

## 1. User Registration Requirements (URR)

### Authentication Services

21. Create `IAuthService` interface and `AuthService` implementation
22. Implement user registration endpoint (POST `/api/auth/register`) - supports email or phone registration
23. Implement OTP generation and sending logic (SMS/Email)
24. Implement OTP verification endpoint (POST `/api/auth/verify-otp`)
25. Implement login endpoint (POST `/api/auth/login`) with JWT token generation
26. Implement logout endpoint (POST `/api/auth/logout`) - token invalidation
27. Implement password hashing service using BCrypt or ASP.NET Identity PasswordHasher
28. Create `AuthController` with registration, verification, login, logout endpoints

### User Management

29. Create `IUserService` interface and `UserService` implementation
30. Implement user profile retrieval endpoint (GET `/api/users/{id}`)
31. Implement user update endpoint (PUT `/api/users/{id}`)
32. Create `UsersController` for user management endpoints

---

## 2. Freelancer Profile Management Requirements (FPMR)

### Profile Services

33. Create `IFreelancerService` interface and `FreelancerService` implementation
34. Implement freelancer profile creation endpoint (POST `/api/freelancers/profile`)
35. Implement freelancer profile update endpoint (PUT `/api/freelancers/profile`)
36. Implement freelancer profile retrieval endpoint (GET `/api/freelancers/{id}`)
37. Implement public freelancer profile view endpoint (GET `/api/freelancers/{id}/public`) - for clients to view
38. Implement profile picture upload endpoint (POST `/api/freelancers/profile/picture`)
39. Implement profile picture retrieval endpoint (GET `/api/freelancers/{id}/picture`)

### Skills Management

40. Create `ISkillService` interface and `SkillService` implementation
41. Implement add skill to freelancer endpoint (POST `/api/freelancers/{id}/skills`)
42. Implement remove skill from freelancer endpoint (DELETE `/api/freelancers/{id}/skills/{skillId}`)
43. Implement get freelancer skills endpoint (GET `/api/freelancers/{id}/skills`)
44. Implement update skill proficiency endpoint (PUT `/api/freelancers/{id}/skills/{skillId}`)

### Verification Badge

45. Create `IUserVerificationService` interface and `UserVerificationService` implementation
46. Implement verification status check logic (Verified/Unverified badge display)
47. Add verification badge to freelancer profile DTOs
48. Create `FreelancersController` with all profile management endpoints

---

## 2.5. Client Profile Management Requirements (CPMR)

### Profile Services

171. Create `IClientService` interface and `ClientService` implementation
172. Implement client profile creation endpoint (POST `/api/clients/profile`)
173. Implement client profile update endpoint (PUT `/api/clients/profile`)
174. Implement client profile retrieval endpoint (GET `/api/clients/{id}`)
175. Implement public client profile view endpoint (GET `/api/clients/{id}/public`) - for freelancers to view
176. Implement profile picture upload endpoint (POST `/api/clients/profile/picture`)
177. Implement profile picture retrieval endpoint (GET `/api/clients/{id}/picture`)
178. Create `ClientsController` with all profile management endpoints

---

## 3. Projects and Service Listings Requirements (PSLR)

### Service Listings (Freelancer Services)

49. Create `IServiceService` interface and `ServiceService` implementation
50. Implement create predefined service endpoint (POST `/api/services`)
51. Implement update service endpoint (PUT `/api/services/{id}`)
52. Implement get freelancer services endpoint (GET `/api/freelancers/{id}/services`)
53. Implement get all active services endpoint (GET `/api/services`) - for clients to browse
54. Implement get service details endpoint (GET `/api/services/{id}`)
55. Implement deactivate service endpoint (PUT `/api/services/{id}/deactivate`)
56. Create `ServicesController` for service management

### Client Projects

57. Create `IProjectService` interface and `ProjectService` implementation
58. Implement create project request endpoint (POST `/api/projects`)
59. Implement update project endpoint (PUT `/api/projects/{id}`)
60. Implement get client projects endpoint (GET `/api/clients/{id}/projects`)
61. Implement get all open projects endpoint (GET `/api/projects`) - for freelancers to browse
62. Implement get project details endpoint (GET `/api/projects/{id}`)
63. Implement delete project endpoint (DELETE `/api/projects/{id}`) - soft delete
64. Create `ProjectsController` for project management

### Service Orders (Predefined Services)

65. Create `IOrderService` interface and `OrderService` implementation
66. Implement request predefined service endpoint (POST `/api/orders/service`) - creates order for service
67. Implement get client orders endpoint (GET `/api/clients/{id}/orders`)
68. Implement get freelancer orders endpoint (GET `/api/freelancers/{id}/orders`)
69. Implement get order details endpoint (GET `/api/orders/{id}`)
70. Create `OrdersController` for order management

### Proposals

71. Create `IProposalService` interface and `ProposalService` implementation
72. Implement submit proposal endpoint (POST `/api/projects/{projectId}/proposals`)
73. Implement get project proposals endpoint (GET `/api/projects/{projectId}/proposals`)
74. Implement get freelancer proposals endpoint (GET `/api/freelancers/{id}/proposals`)
75. Implement accept proposal endpoint (PUT `/api/proposals/{id}/accept`)
76. Implement reject proposal endpoint (PUT `/api/proposals/{id}/reject`)
77. Implement withdraw proposal endpoint (PUT `/api/proposals/{id}/withdraw`)
78. Create `ProposalsController` for proposal management

### Project Approval

79. Implement project delivery submission endpoint (POST `/api/projects/{id}/deliver`)
80. Implement approve project delivery endpoint (PUT `/api/deliveries/{id}/approve`)
81. Implement reject project delivery endpoint (PUT `/api/deliveries/{id}/reject`)
82. Create `DeliveriesController` for delivery management

---

## 4. Project Chat Requirements (PCR)

### Chat Services

83. Create `IChatService` interface and `ChatService` implementation
84. Implement create chat endpoint (POST `/api/chats`) - between client and freelancer for a project/order
85. Implement get user chats endpoint (GET `/api/chats`) - returns all chats for authenticated user
86. Implement get chat details endpoint (GET `/api/chats/{id}`)
87. Implement send message endpoint (POST `/api/chats/{chatId}/messages`)
88. Implement get chat messages endpoint (GET `/api/chats/{chatId}/messages`) - with pagination
89. Implement file attachment upload endpoint (POST `/api/chats/{chatId}/messages/attachments`)
90. Implement file download endpoint (GET `/api/messages/{messageId}/attachments/{fileId}`)
91. Implement chat access validation - ensure only involved parties can access
92. Create `ChatsController` for chat management
93. Create `MessagesController` for message management
94. Set up SignalR hub for real-time chat (optional but recommended)

---

## 5. Rating and Review Requirements (RRR)

### Review Services

95. Create `IReviewService` interface and `ReviewService` implementation
96. Implement create review endpoint (POST `/api/reviews`) - only after project completion
97. Implement validation to ensure review can only be created after project/order completion
98. Implement get freelancer reviews endpoint (GET `/api/freelancers/{id}/reviews`)
99. Implement calculate and update average rating logic
100. Implement get review details endpoint (GET `/api/reviews/{id}`)
101. Implement update review endpoint (PUT `/api/reviews/{id}`) - if allowed
102. Implement delete review endpoint (DELETE `/api/reviews/{id}`) - if allowed
103. Add average rating to freelancer profile DTOs
104. Create `ReviewsController` for review management

---

## 6. Financial Management Requirements (FMR)

### Wallet Services

105. Create `IWalletService` interface and `WalletService` implementation
106. Implement auto-create wallet on user registration
107. Implement get wallet balance endpoint (GET `/api/wallets/balance`)
108. Implement get wallet details endpoint (GET `/api/wallets/{id}`)
109. Implement get wallet transactions endpoint (GET `/api/wallets/{id}/transactions`) - with filtering
110. Implement calculate pending payments logic
111. Implement calculate total earnings logic
112. Add wallet summary DTO (balance, earnings, pending payments)
113. Create `WalletsController` for wallet management

### Escrow Management

114. Create `IEscrowService` interface and `EscrowService` implementation
115. Implement hold payment in escrow logic - when order/project is created
116. Implement release escrow payment logic - when project is approved
117. Implement refund escrow payment logic - when project is cancelled/rejected
118. Implement get escrow status endpoint (GET `/api/payments/{id}/escrow-status`)

---

## 7. Payment Requirements (PR)

### Payment Services

119. Create `IPaymentService` interface and `PaymentService` implementation
120. Implement create payment request endpoint (POST `/api/payments`) - for client to fund wallet
121. Implement process payment endpoint (POST `/api/payments/{id}/process`) - integrate with payment gateways
122. Implement Vodafone Cash payment integration
123. Implement InstaPay payment integration
124. Implement local bank wallet payment integration
125. Implement payment callback/webhook endpoint (POST `/api/payments/webhook`) - for payment gateway callbacks
126. Implement get payment status endpoint (GET `/api/payments/{id}`)
127. Implement get payment history endpoint (GET `/api/payments`) - for authenticated user
128. Create `PaymentsController` for payment processing

### Withdrawal Services

129. Create `IWithdrawalService` interface and `WithdrawalService` implementation
130. Implement create withdrawal request endpoint (POST `/api/withdrawals`) - only after project approval
131. Implement validate withdrawal eligibility logic - check available balance
132. Implement process withdrawal endpoint (PUT `/api/withdrawals/{id}/process`)
133. Implement get withdrawal requests endpoint (GET `/api/withdrawals`)
134. Implement get withdrawal status endpoint (GET `/api/withdrawals/{id}`)
135. Create `WithdrawalsController` for withdrawal management

### Payment Methods

136. Create `IPaymentMethodService` interface and `PaymentMethodService` implementation
137. Implement add payment method endpoint (POST `/api/payment-methods`)
138. Implement get user payment methods endpoint (GET `/api/payment-methods`)
139. Implement update payment method endpoint (PUT `/api/payment-methods/{id}`)
140. Implement delete payment method endpoint (DELETE `/api/payment-methods/{id}`)
141. Create `PaymentMethodsController` for payment method management

---

## 8. Trust Score System Requirements (TSSR)

### Trust Score Services

142. Create `ITrustScoreService` interface and `TrustScoreService` implementation
143. Implement trust score calculation algorithm:

    - Punctuality and deadline adherence (weighted factor)
    - Number of completed projects (weighted factor)
    - Client ratings and feedback (weighted factor)
    - Verification level achieved (weighted factor)

144. Implement update trust score on project completion
145. Implement update trust score on review submission
146. Implement update trust score on deadline adherence check
147. Implement get trust score endpoint (GET `/api/users/{id}/trust-score`)
148. Implement background job/service to recalculate trust scores periodically
149. Add trust score to user profile DTOs
150. Implement trust score influence on search results visibility

---

## Additional Features & Polish

### Search & Filtering

151. Implement search freelancers endpoint (GET `/api/freelancers/search`) - with trust score sorting
152. Implement search projects endpoint (GET `/api/projects/search`) - with filters
153. Implement search services endpoint (GET `/api/services/search`) - with filters

### Notifications (Optional but Recommended)

154. Create notification service interface and implementation
155. Implement project proposal received notification
156. Implement proposal accepted/rejected notification
157. Implement project delivery notification
158. Implement payment received notification
159. Create `NotificationsController` for notification management

### Data Validation & Security

160. Add FluentValidation or Data Annotations validation for all DTOs
161. Implement input sanitization for user-generated content
162. Add rate limiting middleware for API endpoints
163. Implement request logging and audit trail

### Testing & Documentation

164. Create API documentation using Swagger/OpenAPI
165. Configure Swagger UI in `Program.cs`
166. Add XML documentation comments to all controllers and services
167. Create unit tests for core services (optional but recommended)
168. Create integration tests for critical endpoints (optional but recommended)

### Database Optimization

169. Add database indexes for frequently queried fields (if not already in entities)
170. Implement pagination for all list endpoints
171. Implement caching strategy for frequently accessed data (optional)

---

## Implementation Order Recommendation

**Phase 1: Foundation (Tasks 1-20)**

- Set up database, authentication, core services, and external integrations

**Phase 2: User Management (Tasks 21-48)**

- Implement registration, authentication, and profile management

**Phase 3: Core Marketplace (Tasks 49-82)**

- Implement services, projects, proposals, and orders

**Phase 4: Communication (Tasks 83-94)**

- Implement chat and messaging system

**Phase 5: Reviews & Trust (Tasks 95-103, 142-150)**

- Implement reviews and trust score system

**Phase 6: Financial System (Tasks 104-141)**

- Implement wallets, payments, escrow, and withdrawals

**Phase 7: Polish & Optimization (Tasks 151-171)**

- Add searc