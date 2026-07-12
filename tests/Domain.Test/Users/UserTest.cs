using CommonTestUtilities.Entities.Users;
using DotCruz.CoreAuth.Domain.Entities.Users;
using DotCruz.CoreAuth.Domain.Enums.Users;
using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using FluentAssertions;
using System;

namespace Domain.Test.Users;

public class UserTest
{
    [Fact]
    public void Success_On_Create()
    {
        var user = UserBuilder.Build();

        user.Should().NotBeNull();
        user.Name.Should().NotBeEmpty();
        user.Email.Should().NotBeEmpty();
        user.PasswordHash.Should().NotBeEmpty();
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void Success_Create_PendingActivation()
    {
        var user = UserBuilder.Build(passwordHashed: null, status: UserStatus.PendingActivation);

        user.Should().NotBeNull();
        user.PasswordHash.Should().BeNull();
        user.Status.Should().Be(UserStatus.PendingActivation);
    }

    [Fact]
    public void Success_Activate()
    {
        var user = UserBuilder.Build(passwordHashed: null, status: UserStatus.PendingActivation);

        user.Activate("new-password-hash");

        user.Status.Should().Be(UserStatus.Active);
        user.PasswordHash.Should().Be("new-password-hash");
    }

    [Fact]
    public void Error_Activate_Already_Active()
    {
        var user = UserBuilder.Build(status: UserStatus.Active);

        Action act = () => user.Activate("new-password-hash");

        act.Should().Throw<ErrorOnValidationException>()
            .And.GetErrorsMessages().Should().Contain(ResourceMessagesException.TOKEN_INVALID);
    }

    [Fact]
    public void Success_On_Update()
    {
        var user = UserBuilder.Build(type: UserType.TenantUser);

        var newData = UserBuilder.Build(type: UserType.TenantUser);

        user.Update(newData.Name, newData.Email, newData.PasswordHash, newData.Type, newData.TenantId);

        user.Name.Should().Be(newData.Name);
        user.Email.Should().Be(newData.Email);
        user.PasswordHash.Should().Be(newData.PasswordHash);
        user.Type.Should().Be(newData.Type);
    }

    [Fact]
    public void Success_On_Update_Only_Name()
    {
        var user = UserBuilder.Build();

        var newData = UserBuilder.Build();

        var originalName = user.Name;
        var originalEmail = user.Email;
        var originalPasswordHash = user.PasswordHash;
        var originalType = user.Type;

        user.Update(newData.Name, null, null, null);

        user.Name.Should().Be(newData.Name);
        user.Name.Should().NotBe(originalName);
        user.Email.Should().Be(originalEmail);
        user.PasswordHash.Should().Be(originalPasswordHash);
        user.Type.Should().Be(originalType);
    }

    [Fact]
    public void Error_Name_Empty()
    {
        Action act = () => UserBuilder.Build(name: string.Empty);

        act.Should().Throw<ErrorOnValidationException>()
            .And.GetErrorsMessages().Should().Contain(ResourceMessagesException.NAME_EMPTY)
            .And.HaveCount(1);
    }

    [Fact]
    public void Error_Name_Max_Length()
    {
        Action act = () => UserBuilder.Build(nameLength: 201);

        var expectedMessage = string.Format(ResourceMessagesException.NAME_MAX_LENGTH, 200);

        act.Should().Throw<ErrorOnValidationException>()
            .And.GetErrorsMessages().Should().Contain(expectedMessage)
            .And.HaveCount(1);
    }

    [Fact]
    public void Error_Email_Empty()
    {
        Action act = () => UserBuilder.Build(email: string.Empty);

        act.Should().Throw<ErrorOnValidationException>()
            .And.GetErrorsMessages().Should().Contain(ResourceMessagesException.EMAIL_EMPTY)
            .And.HaveCount(1);
    }

    [Fact]
    public void Error_Email_Max_Length()
    {
        Action act = () => UserBuilder.Build(emailLength: 201);

        var expectedMessage = string.Format(ResourceMessagesException.EMAIL_MAX_LENGTH, 200);

        act.Should().Throw<ErrorOnValidationException>()
            .And.GetErrorsMessages().Should().Contain(expectedMessage)
            .And.HaveCount(1);
    }

    [Fact]
    public void Error_Password_Empty()
    {
        Action act = () => UserBuilder.Build(passwordHashed: string.Empty, status: UserStatus.Active);

        act.Should().Throw<ErrorOnValidationException>()
            .And.GetErrorsMessages().Should().Contain(ResourceMessagesException.PASSWORD_EMPTY)
            .And.HaveCount(1);
    }

    [Fact]
    public void Error_TenantId_Empty_For_TenantUser()
    {
        Action act = () => new User("Nome", "email@teste.com", UserType.TenantUser, Guid.Empty);

        act.Should().Throw<ErrorOnValidationException>()
            .And.GetErrorsMessages().Should().Contain(ResourceMessagesException.TENANT_ID_REQUIRED)
            .And.HaveCount(1);
    }

    [Fact]
    public void Success_On_Create_With_TenantId()
    {
        var tenantId = Guid.NewGuid();
        var user = new User("Nome", "email@teste.com", UserType.TenantUser, tenantId);

        user.Should().NotBeNull();
        user.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public void Success_On_Update_To_Global_Clears_TenantId()
    {
        var tenantId = Guid.NewGuid();
        var user = new User("Nome", "email@teste.com", UserType.TenantUser, tenantId);

        user.Update(null, null, null, UserType.SuperAdmin, Guid.Empty);

        user.TenantId.Should().Be(Guid.Empty);
    }
}
