<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="1c49f2d9-8688-4044-a7f5-7daedac250b2" Name="SubStageState" Group="Custom">
	<Description>Состояния этапов маршрута</Description>
	<SchemePhysicalColumn ID="3afc027c-da47-4ede-b398-80b4ee5928d9" Name="ID" Type="Int16 Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="8a0dde5d-5e3d-430c-a0ab-a16d12be5307" Name="df_SubStageState_ID" Value="0" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="6b9087c0-b2a3-447f-9fa2-58486b2b3de0" Name="Name" Type="String(128) Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="12d89d74-3dc3-4662-9ee3-643fedde4eb5" Name="df_SubStageState_Name" Value="Не запущен" />
	</SchemePhysicalColumn>
	<SchemePrimaryKey ID="b2965d90-030d-483c-a208-917f4568a047" Name="pk_SubStageState">
		<SchemeIndexedColumn Column="3afc027c-da47-4ede-b398-80b4ee5928d9" />
	</SchemePrimaryKey>
	<SchemeRecord>
		<ID ID="3afc027c-da47-4ede-b398-80b4ee5928d9">0</ID>
		<Name ID="6b9087c0-b2a3-447f-9fa2-58486b2b3de0">Не запущен</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="3afc027c-da47-4ede-b398-80b4ee5928d9">1</ID>
		<Name ID="6b9087c0-b2a3-447f-9fa2-58486b2b3de0">Активен</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="3afc027c-da47-4ede-b398-80b4ee5928d9">2</ID>
		<Name ID="6b9087c0-b2a3-447f-9fa2-58486b2b3de0">Завершен</Name>
	</SchemeRecord>
</SchemeTable>