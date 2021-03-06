/* Code Generate 'AddEntityAttribute(...)' for migrations. 
This will list all of the attributes order by entity type and order 
Just pick the top few that you need for your migration depending
*/

begin

declare
@crlf varchar(2) = char(13) + char(10),
@entityTypeNameSearch nvarchar(max) = '%GroupType%', -- < Change this 
@entityTypeQualifierValueSearch nvarchar(max) = '%GroupTypePurpose%', -- < Change this 
@daysAgo int = 1 --< Change this to how far back to look for newly added attributes (or null to not limit)


SELECT 
		'            RockMigrationHelper.AddEntityAttribute("'+    
		CONCAT([e].name, '","',
        CONVERT(nvarchar(50), ft.Guid), '","' , 
		a.EntityTypeQualifierColumn , '","' ,
		a.EntityTypeQualifierValue , '","' ,
		a.Name , '","","' ,
		a.Description , '",' ,
		CONVERT(varchar(5), a.[order]) , ',@"' ,
		REPLACE(a.DefaultValue, '"', '""') , '","' ,
        CONVERT(nvarchar(50), a.Guid)+  '","' ,
		a.[Key] , '");') + @crlf [Up.AddEntityAttribute]
  FROM [Attribute] [a]
  inner join [EntityType] [e] on [e].Id = [a].EntityTypeId
  inner join [FieldType] [ft] on [ft].Id = [a].FieldTypeId
  where e.Name like @entityTypeNameSearch
  and a.EntityTypeQualifierColumn like @entityTypeQualifierValueSearch
  and ((a.CreatedDateTime is not null and DATEDIFF(hour, a.CreatedDateTime, SYSDATETIME()) < (@daysAgo*24)) or @daysAgo is null)
order by [e].[name], [a].[Order] 


--RockMigrationHelper.UpdateAttributeQualifier( "90C34D24-7CFB-4A52-B39C-DFF05A40997C", "fieldtype", "ddl", "6AF5B4B1-8195-4516-B1FE-79705847B8D0" );
SELECT 
		@crlf+'            // ' + a.[Key] + @crlf +
		'            RockMigrationHelper.UpdateAttributeQualifier("'+    
		CONCAT(CONVERT(nvarchar(50), a.Guid), '","',
        aq.[Key], '",@"' , 
		aq.[Value] , '","' ,
        CONVERT(nvarchar(50), aq.Guid)+ '");') + @crlf [Up.UpdateAttributeQualifier]
  FROM [Attribute] [a]
  inner join [AttributeQualifier] aq on aq.AttributeId = a.Id
  inner join [EntityType] [e] on [e].Id = [a].EntityTypeId
  inner join [FieldType] [ft] on [ft].Id = [a].FieldTypeId
  where e.Name like @entityTypeNameSearch
  and a.EntityTypeQualifierColumn like @entityTypeQualifierValueSearch
  and ((a.CreatedDateTime is not null and DATEDIFF(hour, a.CreatedDateTime, SYSDATETIME()) < (@daysAgo*24)) or @daysAgo is null)
order by [e].[name], [a].[Order]

    -- attributes values
	--    public void AddAttributeValue( string attributeGuid, int entityId, string value, string guid )

    SELECT 
        '            RockMigrationHelper.AddAttributeValue("'+     
        CONVERT(nvarchar(50), a.Guid)+
		'",0,@"'+ 
        ISNULL(av.Value,'') + '","'+ 
        CONVERT(nvarchar(50), a.Guid)+ '"); // '+ a.[Name] + 
        @crlf [Up.AddAttributeValue]
    from [AttributeValue] [av]
    join Attribute a on a.id = av.AttributeId
	inner join [EntityType] [e] on [e].Id = [a].EntityTypeId
	inner join [FieldType] [ft] on [ft].Id = [a].FieldTypeId
    where e.[Name] like @entityTypeNameSearch
	and a.EntityTypeQualifierColumn like @entityTypeQualifierValueSearch
	and ((a.CreatedDateTime is not null and DATEDIFF(hour, a.CreatedDateTime, SYSDATETIME()) < (@daysAgo*24)) or @daysAgo is null)
    order by [e].[name], [a].[Order] 

SELECT 
        '            RockMigrationHelper.DeleteAttribute("' +    
        CONVERT(nvarchar(50), a.Guid)+ '");    // ' + 
		[e].name + ': ' +
		a.Name + @crlf [Down.DeleteAttribute]
  FROM [Attribute] [a]
  inner join [EntityType] [e] on [e].Id = [a].EntityTypeId
  inner join [FieldType] [ft] on [ft].Id = [a].FieldTypeId
  where e.Name like @entityTypeNameSearch
  and a.EntityTypeQualifierColumn like @entityTypeQualifierValueSearch
  and ((a.CreatedDateTime is not null and DATEDIFF(hour, a.CreatedDateTime, SYSDATETIME()) < (@daysAgo*24)) or @daysAgo is null)
order by [e].[name], [a].[Order] 



end