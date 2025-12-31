-- usp_LogUserLogin: Insert login audit record
CREATE PROCEDURE usp_LogUserLogin
    @UserId NVARCHAR(128),
    @Timestamp DATETIME2,
    @Device NVARCHAR(512) = NULL,
    @Location NVARCHAR(512) = NULL,
    @Success BIT,
    @FailureReason NVARCHAR(512) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO LoginAudits (UserId, Timestamp, Device, Location, Success, FailureReason)
    VALUES (@UserId, @Timestamp, @Device, @Location, @Success, @FailureReason);
END
