-- usp_GetUserRole: Returns roles and permissions for a user
CREATE PROCEDURE usp_GetUserRole
    @UserId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.Id, r.Name
    FROM RoleMasters r
    INNER JOIN UserRoleMappings m ON m.RoleMasterId = r.Id
    WHERE m.UserId = @UserId;
END
