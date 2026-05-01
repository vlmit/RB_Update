<?xml version="1.0" encoding="utf-8"?>
<SchemeProcedure ID="287a8a96-f1f3-4986-ae97-2342c7af11ce" Name="ObtainWriterLock" Group="System">
	<Description>Перед выполнением хранимой процедуры следует указать максимальное время ожидания блокировки (LOCK_TIMEOUT для SQL Server или lock_timeout для PostgreSQL).</Description>
	<Definition Dbms="SqlServer" IsExternal="true" />
	<Definition Dbms="PostgreSql" IsExternal="true" />
</SchemeProcedure>