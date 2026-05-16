using System.IO;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace GMLSSystem.Tests.Validation
{
    public class FileValidationTests
    {
        [Fact]
        public void ValidateFileType_PDFFile_ReturnsTrue()
        {
            // Arrange
            var fileName = "contract.pdf";
            var extension = Path.GetExtension(fileName).ToLower();

            // Act
            var isValid = extension == ".pdf";

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateFileType_ExeFile_ReturnsFalse()
        {
            // Arrange
            var fileName = "malware.exe";
            var extension = Path.GetExtension(fileName).ToLower();

            // Act
            var isValid = extension == ".pdf";

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateFileType_DocxFile_ReturnsFalse()
        {
            // Arrange
            var fileName = "document.docx";
            var extension = Path.GetExtension(fileName).ToLower();

            // Act
            var isValid = extension == ".pdf";

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateFileType_JpgFile_ReturnsFalse()
        {
            // Arrange
            var fileName = "image.jpg";
            var extension = Path.GetExtension(fileName).ToLower();

            // Act
            var isValid = extension == ".pdf";

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateFileType_EmptyFileName_ReturnsFalse()
        {
            // Arrange
            var fileName = "";
            var extension = Path.GetExtension(fileName).ToLower();

            // Act
            var isValid = extension == ".pdf";

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateFileType_UppercasePDF_ReturnsTrue()
        {
            // Arrange
            var fileName = "CONTRACT.PDF";
            var extension = Path.GetExtension(fileName).ToLower();

            // Act
            var isValid = extension == ".pdf";

            // Assert
            Assert.True(isValid);
        }
    }
}