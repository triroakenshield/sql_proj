-- Enable show advanced options on the server
sp_configure 'show advanced options',1
RECONFIGURE
GO
-- Enable clr on the server
sp_configure 'clr enabled',1
RECONFIGURE
GO

IF object_id(N'cs2cs') IS NOT NULL
DROP FUNCTION cs2cs
go

IF object_id(N'affine') IS NOT NULL
DROP FUNCTION affine
go

IF object_id(N'reaffine') IS NOT NULL
DROP FUNCTION reaffine
go

DROP ASSEMBLY [DotSpatial.Projections]
go

-- Import the assembly
CREATE ASSEMBLY [DotSpatial.Projections]
FROM 'C:\work\DotSpatial.Projections.dll'
WITH PERMISSION_SET = UNSAFE;
go

CREATE FUNCTION [dbo].cs2cs(@ngeom geometry, @sproj [nvarchar](max), @tproj [nvarchar](max)) 
RETURNS geometry 
as EXTERNAL NAME [DotSpatial.Projections].[DotSpatial.Projections.Functions].cs2cs
go

CREATE FUNCTION [dbo].affine(@ngeom geometry, @a float, @b float, @c float, @d float, @e float, @f float) 
RETURNS geometry 
as EXTERNAL NAME [DotSpatial.Projections].[DotSpatial.Projections.Functions].affine
go

CREATE FUNCTION [dbo].reaffine(@ngeom geometry, @a float, @b float, @c float, @d float, @e float, @f float) 
RETURNS geometry 
as EXTERNAL NAME [DotSpatial.Projections].[DotSpatial.Projections.Functions].reaffine
go
