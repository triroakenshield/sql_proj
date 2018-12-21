declare @g geometry;
SET @g = geometry::Point(63, 52, 0); 
select @g.STAsText() as g1, 
 dbo.cs2cs(@g, 
 '+proj=longlat +datum=WGS84 +no_defs',
 '+proj=utm +zone=41 +ellps=WGS84 +datum=WGS84 +units=m +no_defs').STAsText() as g2;