<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="4bf059f1-0fef-4789-886e-4ac5ec01ffb3" Name="NPAType" Group="Custom" InstanceType="Cards" ContentType="Entries">
	<Description>Вид НПА</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="4bf059f1-0fef-0089-2000-0ac5ec01ffb3" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="4bf059f1-0fef-0189-4000-0ac5ec01ffb3" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="d1408cd4-2992-4121-8b8e-a3852e297d87" Name="Name" Type="String(Max) Null" />
	<SchemePhysicalColumn ID="6d789ed3-4b27-4cd0-a01d-26a431d457be" Name="Index" Type="String(Max) Null">
		<Description>Индекс</Description>
	</SchemePhysicalColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="4bf059f1-0fef-0089-5000-0ac5ec01ffb3" Name="pk_NPAType" IsClustered="true">
		<SchemeIndexedColumn Column="4bf059f1-0fef-0189-4000-0ac5ec01ffb3" />
	</SchemePrimaryKey>
</SchemeTable>