<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="cc017f2f-e99d-4e89-be52-5e9ed5056c91" Name="CommissionControl" Group="Custom">
	<Description>Статус контроля</Description>
	<SchemePhysicalColumn ID="c62d365b-f051-4a3d-a8b7-2d02f8e477bb" Name="ID" Type="Int16 Null" />
	<SchemePhysicalColumn ID="b04d4c60-ca70-4eb5-b836-3fb1e2dfcf30" Name="Name" Type="String(128) Null" />
	<SchemePrimaryKey ID="4b8e6453-8de1-47ff-b232-5f76e33fe500" Name="pk_CommissionControl">
		<SchemeIndexedColumn Column="c62d365b-f051-4a3d-a8b7-2d02f8e477bb" />
	</SchemePrimaryKey>
	<SchemeRecord>
		<ID ID="c62d365b-f051-4a3d-a8b7-2d02f8e477bb">0</ID>
		<Name ID="b04d4c60-ca70-4eb5-b836-3fb1e2dfcf30">Нет контроля</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="c62d365b-f051-4a3d-a8b7-2d02f8e477bb">1</ID>
		<Name ID="b04d4c60-ca70-4eb5-b836-3fb1e2dfcf30">На контроле</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="c62d365b-f051-4a3d-a8b7-2d02f8e477bb">2</ID>
		<Name ID="b04d4c60-ca70-4eb5-b836-3fb1e2dfcf30">Снято с контроля</Name>
	</SchemeRecord>
</SchemeTable>