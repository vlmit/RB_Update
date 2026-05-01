<?xml version="1.0" encoding="utf-8"?>
<SchemeProcedure Partition="dd8eeaba-9042-4fb5-9e8e-f7544463464f" ID="e93e2332-b48d-4ffe-8f06-ba1145ef3767" Name="ObtainProcessLock" Group="WorkflowEngine">
	<Description>Перед выполнением хранимой процедуры следует указать максимальное время ожидания блокировки (LOCK_TIMEOUT для SQL Server или lock_timeout для PostgreSQL).</Description>
	<Definition Dbms="SqlServer" IsExternal="true" />
	<Definition Dbms="PostgreSql" IsExternal="true" />
</SchemeProcedure>