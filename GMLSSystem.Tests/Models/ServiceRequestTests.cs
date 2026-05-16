using System;
using GMLSSystem.Models;
using GMLSSystem.Models.Enums;
using Xunit;

namespace GMLSSystem.Tests.Models
{
    public class ServiceRequestTests
    {
        [Fact]
        public void ServiceRequest_GenerateRequestNumber_CreatesValidNumber()
        {
            // Arrange & Act
            var request = new ServiceRequest();

            // Assert
            Assert.NotNull(request.RequestNumber);
            Assert.StartsWith("SRQ-", request.RequestNumber);
        }

        [Fact]
        public void ServiceRequest_DefaultStatus_IsPending()
        {
            // Arrange & Act
            var request = new ServiceRequest();

            // Assert
            Assert.Equal(RequestStatus.Pending, request.Status);
        }

        [Fact]
        public void ServiceRequest_DefaultDate_IsCurrentDateTime()
        {
            // Arrange & Act
            var request = new ServiceRequest();

            // Assert
            Assert.True(request.RequestDate <= DateTime.UtcNow);
            Assert.True(request.RequestDate >= DateTime.UtcNow.AddSeconds(-5));
        }
    }
}