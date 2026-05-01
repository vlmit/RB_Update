<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="6000e7dd-0ef7-4a4e-b6a8-f01d8545b15b" Name="TypeOfDocument" Group="Custom" InstanceType="Cards" ContentType="Entries">
	<Description>Вид документа</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="6000e7dd-0ef7-004e-2000-001d8545b15b" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="6000e7dd-0ef7-014e-4000-001d8545b15b" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="34b4147b-9834-4190-ba6e-ae8b5b838d36" Name="Name" Type="String(Max) Null" />
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="6000e7dd-0ef7-004e-5000-001d8545b15b" Name="pk_TypeOfDocument" IsClustered="true">
		<SchemeIndexedColumn Column="6000e7dd-0ef7-014e-4000-001d8545b15b" />
	</SchemePrimaryKey>
</SchemeTable>