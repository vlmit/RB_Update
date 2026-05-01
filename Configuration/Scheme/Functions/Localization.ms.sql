CREATE FUNCTION [Localization]
(
	@name nvarchar(256),
	@culture int
)
RETURNS TABLE
AS
RETURN
(
	select top 1 isnull(ls.Value, @name) Value
	from (select 1 Value) t
	outer apply (
		select top 1 le.ID
		from LocalizationEntries le with(nolock)
		where left(@name, 1) = '$'
		    and le.Name = right(@name, case when len(@name) > 0 then len(@name) - 1 else 0 end)
		    and le.Overridden = 0
	) le
	outer apply (
		select top 1 ls.Value
		from LocalizationStrings ls with(nolock)
		where ls.EntryID = le.ID
		  and ls.Culture = @culture
	) ls
);