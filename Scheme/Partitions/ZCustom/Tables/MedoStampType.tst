<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="307d2356-4fbe-41c4-b114-3497f2ac64ef" Name="MedoStampType" Group="Custom">
	<SchemePhysicalColumn ID="cac5c352-662d-4547-9f79-e77306a827e5" Name="ID" Type="Int32 Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="fa0e0a6f-eab9-4dc8-a481-0ed43c955627" Name="df_MedoStampType_ID" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="e893f1b3-1548-4d5e-ae40-286478ce6208" Name="Name" Type="String(128) Null" />
	<SchemePrimaryKey ID="d65e0e60-932e-4d65-81bf-97a9e16fd28d" Name="pk_MedoStampType">
		<SchemeIndexedColumn Column="cac5c352-662d-4547-9f79-e77306a827e5" />
	</SchemePrimaryKey>
	<SchemeRecord>
		<ID ID="cac5c352-662d-4547-9f79-e77306a827e5">0</ID>
		<Name ID="e893f1b3-1548-4d5e-ae40-286478ce6208">Регистрационный штамп</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="cac5c352-662d-4547-9f79-e77306a827e5">1</ID>
		<Name ID="e893f1b3-1548-4d5e-ae40-286478ce6208">Штамп подписи</Name>
	</SchemeRecord>
</SchemeTable>