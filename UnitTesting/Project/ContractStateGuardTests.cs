using Xunit;
using FluentAssertions;
using Entities.Project;
using Entities.Enums;
using ServiceImplementation.Helpers;
using ServiceImplementation.Exceptions;
using Entities.Review;
using System;
using System.Collections.Generic;

namespace UnitTesting.Project
{
    public class ContractStateGuardTests
    {
        [Fact]
        public void EnsureCanAcceptOffer_ShouldPass_WhenStatusIsOffer()
        {
            // ARRANGE
            var proposal = new Proposal { Status = ProposalStatus.Offer };

            // ACT
            var act = () => ContractStateGuard.EnsureCanAcceptOffer(proposal);

            // ASSERT
            act.Should().NotThrow();
        }

        [Fact]
        public void EnsureCanAcceptOffer_ShouldThrow_WhenStatusIsNotOffer()
        {
            // ARRANGE
            var proposal = new Proposal { Status = ProposalStatus.Submitted };

            // ACT
            var act = () => ContractStateGuard.EnsureCanAcceptOffer(proposal);

            // ASSERT
            act.Should().Throw<InvalidStateException>();
        }

        [Fact]
        public void EnsureCanDeliverWork_ShouldPass_WhenContractIsNotClosed()
        {
            // ARRANGE
            var contract = new Contract { Status = ContractStatus.Active };

            // ACT
            var act = () => ContractStateGuard.EnsureCanDeliverWork(contract);

            // ASSERT
            act.Should().NotThrow();
        }

        [Fact]
        public void EnsureCanDeliverWork_ShouldThrow_WhenContractIsClosed()
        {
            // ARRANGE
            var contract = new Contract { Status = ContractStatus.Closed };

            // ACT
            var act = () => ContractStateGuard.EnsureCanDeliverWork(contract);

            // ASSERT
            act.Should().Throw<InvalidStateException>();
        }

        [Fact]
        public void EnsureCanSubmitReview_ShouldPass_WhenDeliveriesExistAndNoPriorReview()
        {
            // ARRANGE
            var contract = new Contract
            {
                Id = 1,
                WorkDeliveries = new List<WorkDelivery> { new WorkDelivery() },
                ContractReviews = new List<ContractReview>()
            };

            // ACT
            var act = () => ContractStateGuard.EnsureCanSubmitReview(contract, "user1");

            // ASSERT
            act.Should().NotThrow();
        }

        [Fact]
        public void EnsureCanSubmitReview_ShouldThrow_WhenNoDeliveries()
        {
            // ARRANGE
            var contract = new Contract
            {
                Id = 1,
                WorkDeliveries = new List<WorkDelivery>(),
                ContractReviews = new List<ContractReview>()
            };

            // ACT
            var act = () => ContractStateGuard.EnsureCanSubmitReview(contract, "user1");

            // ASSERT
            act.Should().Throw<InvalidStateException>().WithMessage("*delivered work*");
        }

        [Fact]
        public void EnsureCanSubmitReview_ShouldThrow_WhenReviewAlreadyExists()
        {
            // ARRANGE
            var contract = new Contract
            {
                Id = 1,
                WorkDeliveries = new List<WorkDelivery> { new WorkDelivery() },
                ContractReviews = new List<ContractReview> { new ContractReview { ReviewerId = "user1" } }
            };

            // ACT
            var act = () => ContractStateGuard.EnsureCanSubmitReview(contract, "user1");

            // ASSERT
            act.Should().Throw<ConflictException>().WithMessage("*already reviewed*");
        }
    }
}
