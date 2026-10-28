using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using HabityFit.API.Common;
using HabityFit.API.Controllers.InvitationsControler;
using HabityFit.BLL._02.UsersManagement.Dto;
using HabityFit.BLL._02.UsersVerificationManagement;
using HabityFit.BLL._02.UsersVerificationManagement.Dto;
namespace HabityFit.Api.Tests.Controllers
{
    public class InvitationsControllerTests
    {
        private readonly Mock<IUsersVerificationBll> _userInvitationBll = new();
        private readonly Mock<ILogger<InvitationsController>> _logger = new();
        private readonly InvitationsController _controller;

        public InvitationsControllerTests()
        {
            _controller = new(_userInvitationBll.Object, _logger.Object);
        }

        #region Register
        [Fact]
        public async Task InvitationsController_Register_WhenEmailIsValid_Returns200()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            var dto = new EmailDto
            {
                Email = "user@test.com"
            };
            _userInvitationBll.Setup(x => x.CreateAndSendValidateAsync(It.IsAny<string>()))
                              .ReturnsAsync(true);
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await _controller.Register(dto);
            //---------------------------------------------------------------------------------------------------------------- Assert
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(200);
            _userInvitationBll.Verify(u => u.CreateAndSendValidateAsync(dto.Email), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("nananana")]
        [InlineData("nananana@")]
        [InlineData("@nanana.com")]
        [InlineData("nananana@nanana")]
        [InlineData("nana nana@nanana.com")]
        public async Task InvitationsController_Register_WhenEmailFormatIsInvalid_Returns400AndDoesNotCallBll(string email)
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            var dto = new EmailDto
            {
                Email = email
            };
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await _controller.Register(dto);
            //---------------------------------------------------------------------------------------------------------------- Assert
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(400);
            _userInvitationBll.Verify(u => u.CreateAndSendValidateAsync(It.IsAny<string>()), Times.Never);
        }


        [Fact]
        public async Task InvitationsController_Register_WhenBllReturnsFalse_Returns500()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            var dto = new EmailDto
            {
                Email = "user@test.com"
            };
            _userInvitationBll.Setup(x => x.CreateAndSendValidateAsync(It.IsAny<string>()))
                              .ReturnsAsync(false);
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await _controller.Register(dto);
            //---------------------------------------------------------------------------------------------------------------- Assert
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task InvitationsController_Register_WhenBllThrowsInvalidOperationException_Returns409()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            var dto = new EmailDto
            {
                Email = "user@test.com"
            };
            _userInvitationBll.Setup(x => x.CreateAndSendValidateAsync(It.IsAny<string>()))
                              .ThrowsAsync(new InvalidOperationException("fake-error"));
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await _controller.Register(dto);
            //---------------------------------------------------------------------------------------------------------------- Assert
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(409);
        }

        [Fact]
        public async Task InvitationsController_Register_WhenBllThrowsUnexpectedException_Returns500()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            var dto = new EmailDto
            {
                Email = "user@test.com"
            };
            _userInvitationBll.Setup(x => x.CreateAndSendValidateAsync(It.IsAny<string>()))
                              .ThrowsAsync(new Exception("fake-error"));
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await _controller.Register(dto);
            //---------------------------------------------------------------------------------------------------------------- Assert
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }
        #endregion

        #region Accept 
        [Fact]
        public async Task InvitationsController_Accept_WhenBllReturnsDto_Returns200WithToken()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            var dto = new AcceptInvitationDto
            {
                Token = Guid.NewGuid(),
                FullName = "fake-fullname",
                Password = "fake-password"
            };
            var respondeDto = new LoginResponseDto
            {
                Token = "fake-token",
                User = new UserReadDto
                {
                    FullName = "Test",
                    Email = "user@test.com",

                }
            };
            _userInvitationBll.Setup(x => x.AcceptInvitationAsync(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<String>()))
                              .ReturnsAsync(respondeDto);
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await _controller.Accept(dto);
            //---------------------------------------------------------------------------------------------------------------- Assert
            var objectResult = result as ObjectResult;
            var apiResponse = objectResult!.Value as ApiResponse;
            objectResult!.StatusCode.Should().Be(200);
            apiResponse!.Result.Should().Be(respondeDto);
            _userInvitationBll.Verify(u => u.AcceptInvitationAsync(dto.Token, dto.FullName, dto.Password), Times.Once);
        }

        [Fact]
        public async Task InvitationsController_Accept_WhenBllReturnsNull_Returns404()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            var dto = new AcceptInvitationDto
            {
                Token = Guid.NewGuid(),
                FullName = "fake-fullname",
                Password = "fake-password"
            };
            _userInvitationBll.Setup(x => x.AcceptInvitationAsync(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<String>()))
                              .ReturnsAsync((LoginResponseDto?)null);
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await _controller.Accept(dto);
            //---------------------------------------------------------------------------------------------------------------- Assert
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task InvitationsController_Accept_WhenBllThrowsInvalidOperationException_Returns409()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            var dto = new AcceptInvitationDto
            {
                Token = Guid.NewGuid(),
                FullName = "fake-fullname",
                Password = "fake-password"
            };
            _userInvitationBll.Setup(x => x.AcceptInvitationAsync(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<String>()))
                              .ThrowsAsync(new InvalidOperationException("fake-error"));
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await _controller.Accept(dto);
            //---------------------------------------------------------------------------------------------------------------- Assert
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(409);
        }

        [Fact]
        public async Task InvitationsController_Accept_WhenBllThrowsUnexpectedException_Returns500()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            var dto = new AcceptInvitationDto
            {
                Token = Guid.NewGuid(),
                FullName = "fake-fullname",
                Password = "fake-password"
            };
            _userInvitationBll.Setup(x => x.AcceptInvitationAsync(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<String>()))
                              .ThrowsAsync(new Exception("fake-error"));
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await _controller.Accept(dto);
            //---------------------------------------------------------------------------------------------------------------- Assert
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }
        #endregion

        #region Activate
        [Fact]
        public void InvitationsController_Activate_ReturnsRedirectWithDeepLink()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            var token = "fake-token-123";
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = _controller.Activate(token);
            //---------------------------------------------------------------------------------------------------------------- Assert
            var redirectResult = result as RedirectResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.Url.Should().Be($"habityfit://activate?token={token}");
        }
        #endregion
    }
}
