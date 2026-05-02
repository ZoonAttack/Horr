# Horr Project Context

## Architecture Overview
- **Backend Framework:** ASP.NET Core Web API (C#)
- **Database:** Entity Framework Core (AppDbContext)
- **Pattern:** CQRS implemented via MediatR (Queries, Commands, Handlers separated)
- **Authentication/Authorization:** JWT with Role-based and Policy-based authorization (Client, Freelancer, Admin, Agency).
- **Core Entities:** `User`, `JobPost`, `Proposal`, `Contract`, `WorkDelivery`, `ContractReview`, etc.

## Key Workflows
1. **Job Management:** Clients create `JobPost`s. Freelancers can search and save jobs.
2. **Proposals:** Freelancers submit `Proposal`s to jobs.
3. **Offers / Contracts:** 
   - When an offer is made, a `Contract` is created in a `Draft` state.
   - The Freelancer can Accept/Decline the offer. Accepting changes the `Contract` status to `Active` and `Proposal` status to `Offer`.
4. **Billing & Verification:** Payment integration endpoints exist, and users can deposit/withdraw based on roles.

## Known Limitations / Missing Features (as of current state)
- Clients currently cannot view proposals sent by freelancers on their specific jobs (missing endpoint/service).
- Refactoring ongoing to use string-based IDs in some places versus integer IDs in older entities.

*Note: Update this file when major architectural changes occur.*
