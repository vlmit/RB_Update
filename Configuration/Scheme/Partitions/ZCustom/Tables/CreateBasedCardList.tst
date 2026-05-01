<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="a2795057-4200-4611-9f70-d8fa7d1edd40" Name="CreateBasedCardList" Group="Custom">
	<Description>Список типов для создания на основании</Description>
	<SchemePhysicalColumn ID="6064a9b9-4027-4972-b564-f7113cedc256" Name="ID" Type="Int16 Null" />
	<SchemePhysicalColumn ID="f6398597-ed0f-426b-b868-689901713cb8" Name="Name" Type="String(128) Null" />
	<SchemePrimaryKey ID="3c89f120-d14c-4096-b2c5-86a36f20cf64" Name="pk_CreateBasedCardList">
		<SchemeIndexedColumn Column="6064a9b9-4027-4972-b564-f7113cedc256" />
	</SchemePrimaryKey>
	<SchemeRecord>
		<ID ID="6064a9b9-4027-4972-b564-f7113cedc256">0</ID>
		<Name ID="f6398597-ed0f-426b-b868-689901713cb8">Исходящий АГИП</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="6064a9b9-4027-4972-b564-f7113cedc256">1</ID>
		<Name ID="f6398597-ed0f-426b-b868-689901713cb8">Внутренний АГИП</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="6064a9b9-4027-4972-b564-f7113cedc256">2</ID>
		<Name ID="f6398597-ed0f-426b-b868-689901713cb8">Исходящий отдела кадров АГИП</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="6064a9b9-4027-4972-b564-f7113cedc256">3</ID>
		<Name ID="f6398597-ed0f-426b-b868-689901713cb8">Исходящий советника руководителя АГИП</Name>
	</SchemeRecord>
</SchemeTable>