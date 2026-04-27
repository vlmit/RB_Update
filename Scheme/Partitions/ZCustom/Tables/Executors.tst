<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="4b29bf7c-c26a-4fe6-a0e4-23681e254798" Name="Executors" Group="Custom" InstanceType="Cards" ContentType="Collections">
	<Description>Для поля "Исполнители"</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="4b29bf7c-c26a-00e6-2000-03681e254798" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="4b29bf7c-c26a-01e6-4000-03681e254798" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="4b29bf7c-c26a-00e6-3100-03681e254798" Name="RowID" Type="Guid Not Null" />
	<SchemeComplexColumn ID="eaa7be4f-201c-419f-ab6d-5965c1d316da" Name="User" Type="Reference(Typified) Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="eaa7be4f-201c-009f-4000-0965c1d316da" Name="UserID" Type="Guid Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="6761fa85-e4c8-4f7e-89bd-2a9271c6eda2" Name="UserName" Type="String(128) Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964" />
	</SchemeComplexColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="4b29bf7c-c26a-00e6-5000-03681e254798" Name="pk_Executors">
		<SchemeIndexedColumn Column="4b29bf7c-c26a-00e6-3100-03681e254798" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="4b29bf7c-c26a-00e6-7000-03681e254798" Name="idx_Executors_ID" IsClustered="true">
		<SchemeIndexedColumn Column="4b29bf7c-c26a-01e6-4000-03681e254798" />
	</SchemeIndex>
</SchemeTable>