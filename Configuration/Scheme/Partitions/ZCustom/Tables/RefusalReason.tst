<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="2b80b126-863e-4079-80c4-2e12a6d79a82" Name="RefusalReason" Group="Custom" InstanceType="Cards" ContentType="Entries">
	<Description>Причины отказа</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="2b80b126-863e-0079-2000-0e12a6d79a82" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="2b80b126-863e-0179-4000-0e12a6d79a82" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="0c12a991-c4c5-4139-a37b-3e31ed6a5b34" Name="Name" Type="String(Max) Null">
		<Description>Наименование</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="4767f63f-7a81-4619-9d70-290c631aac6e" Name="Notes" Type="String(128) Null">
		<Description>Примечания</Description>
	</SchemePhysicalColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="2b80b126-863e-0079-5000-0e12a6d79a82" Name="pk_RefusalReason" IsClustered="true">
		<SchemeIndexedColumn Column="2b80b126-863e-0179-4000-0e12a6d79a82" />
	</SchemePrimaryKey>
</SchemeTable>