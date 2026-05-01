<?xml version="1.0" encoding="utf-8"?>
<SchemeProcedure Partition="d1b372f3-7565-4309-9037-5e5a0969d94e" ID="eee906f2-ac9c-4cd4-aae2-150756cb4f2b" Name="KrPermissionsObtainReadersLock" Group="Kr">
	<Description>Перед выполнением хранимой процедуры следует указать максимальное время ожидания блокировки (LOCK_TIMEOUT для SQL Server или lock_timeout для PostgreSQL).</Description>
	<Definition Dbms="SqlServer" IsExternal="true" />
	<Definition Dbms="PostgreSql" IsExternal="true" />
</SchemeProcedure>