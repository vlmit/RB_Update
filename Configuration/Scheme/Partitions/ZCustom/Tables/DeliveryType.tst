<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="366fa575-2ada-446b-afa1-a68eca0ab8db" Name="DeliveryType" Group="Custom" InstanceType="Cards" ContentType="Entries">
	<Description>Тип доставки</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="366fa575-2ada-006b-2000-068eca0ab8db" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="366fa575-2ada-016b-4000-068eca0ab8db" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="d1d10a19-8cb9-42f2-b242-69ae0e8f2ecd" Name="Name" Type="String(128) Null" />
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="366fa575-2ada-006b-5000-068eca0ab8db" Name="pk_DeliveryType" IsClustered="true">
		<SchemeIndexedColumn Column="366fa575-2ada-016b-4000-068eca0ab8db" />
	</SchemePrimaryKey>
</SchemeTable>