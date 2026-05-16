using System;
using GMLSSystem.Models;
using GMLSSystem.Models.Enums;
using Xunit;

namespace GMLSSystem.Tests.Models
{
    public class ContractTests
    {
        [Fact]
        public void CanCreateServiceRequest_ActiveContract_ReturnsTrue()
        {
            // Arrange
            var contract = new Contract { Status = ContractStatus.Active };

            // Act
            var result = contract.canCreateServiceRequest();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanCreateServiceRequest_DraftContract_ReturnsFalse()
        {
            // Arrange
            var contract = new Contract { Status = ContractStatus.Draft };

            // Act
            var result = contract.canCreateServiceRequest();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanCreateServiceRequest_ExpiredContract_ReturnsFalse()
        {
            // Arrange
            var contract = new Contract { Status = ContractStatus.Expired };

            // Act
            var result = contract.canCreateServiceRequest();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanCreateServiceRequest_OnHoldContract_ReturnsFalse()
        {
            // Arrange
            var contract = new Contract { Status = ContractStatus.OnHold };

            // Act
            var result = contract.canCreateServiceRequest();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsActive_ActiveWithinDates_ReturnsTrue()
        {
            // Arrange
            var contract = new Contract
            {
                Status = ContractStatus.Active,
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow.AddDays(10)
            };

            // Act
            var result = contract.IsActive();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsActive_ActiveButExpired_ReturnsFalse()
        {
            // Arrange
            var contract = new Contract
            {
                Status = ContractStatus.Active,
                StartDate = DateTime.UtcNow.AddDays(-20),
                EndDate = DateTime.UtcNow.AddDays(-5)
            };

            // Act
            var result = contract.IsActive();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ContractNumber_IsGeneratedOnCreation()
        {
            // Arrange & Act
            var contract = new Contract();

            // Assert
            Assert.NotNull(contract.ContractNumber);
            Assert.StartsWith("CTR-", contract.ContractNumber);
        }
    }
}