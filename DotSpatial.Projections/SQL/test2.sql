declare @g geometry;
SET @g = geometry::Point(20000, 20000, 0); 
SELECT  @g.STAsText() as g1, [dbo].cs2cs(dbo.reaffine(@g, 0.999152,-0.035328,-11079788.502,0.035328, 0.999152,-6660043.491), 
'+proj=tmerc +lat_0=0 +lon_0=63 +k=1 +x_0=11499824.25 +y_0=285.47 +ellps=krass +units=m +no_defs', 
'+proj=longlat +ellps=WGS84 +datum=WGS84 +no_defs').STAsText() as g2