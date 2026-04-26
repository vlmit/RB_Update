<?xml version="1.0" encoding="utf-8"?>
<SchemeProcedure ID="5db01958-f973-44f0-af0f-14e640661447" Name="ObtainReaderLock" Group="System">
	<Description>Перед выполнением хранимой процедуры следует указать максимальное время ожидания блокировки (LOCK_TIMEOUT для SQL Server или lock_timeout для PostgreSQL).</Description>
	<Definition Dbms="SqlServer" IsExternal="true" />
	<Definition Dbms="PostgreSql" IsExternal="true" />
</SchemeProcedure>