-- usp_ValidateUser: Returns user Id and primary fields if credentials match
-- NOTE: This stored procedure is provided as an example for DBAs. The application uses ASP.NET Identity by default.
CREATE PROCEDURE usp_ValidateUser
    @Username NVARCHAR(256),
    @PasswordHash NVARCHAR(512) -- hashed password (if you store custom hashes)
AS
BEGIN
    SET NOCOUNT ON;
    -- Example: Query local AspNetUsers table created by Identity
    SELECT Id, UserName, Email, PhoneNumber, EmailConfirmed
    FROM AspNetUsers
    WHERE UserName = @Username;
END
